# Event Sourcing Guide

This guide demonstrates how to implement event-sourced aggregates in the RIVORA Framework.

## Overview

Event Sourcing persists domain events instead of current state. The current state of an aggregate is reconstructed by replaying its events. This provides a complete audit trail and enables temporal queries.

## Step 1: Define Domain Events

Events are immutable records that describe something that happened:

```csharp
public record AccountOpened(
    Guid AccountId,
    Guid CustomerId,
    string AccountType,
    decimal InitialDeposit,
    DateTime OpenedAt) : IDomainEvent;

public record MoneyDeposited(
    Guid AccountId,
    decimal Amount,
    string Description,
    DateTime DepositedAt) : IDomainEvent;

public record MoneyWithdrawn(
    Guid AccountId,
    decimal Amount,
    string Description,
    DateTime WithdrawnAt) : IDomainEvent;

public record AccountClosed(
    Guid AccountId,
    string Reason,
    DateTime ClosedAt) : IDomainEvent;
```

::: tip
Name events in past tense -- they describe facts that already happened.
:::

## Step 2: Create the Aggregate

The aggregate raises events to change state and applies them to update internal properties:

```csharp
public class BankAccount : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public string AccountType { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public AccountStatus Status { get; private set; }

    // Public constructor -- creates a new account
    public BankAccount(Guid customerId, string accountType, decimal initialDeposit)
    {
        if (initialDeposit < 0)
            throw new ArgumentException("Initial deposit cannot be negative.");

        RaiseEvent(new AccountOpened(
            Guid.NewGuid(), customerId, accountType, initialDeposit, DateTime.UtcNow));
    }

    // Private constructor -- used for rehydration from events
    private BankAccount() { }

    public void Deposit(decimal amount, string description)
    {
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Cannot deposit to a closed account.");
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive.");

        RaiseEvent(new MoneyDeposited(Id, amount, description, DateTime.UtcNow));
    }

    public void Withdraw(decimal amount, string description)
    {
        if (Status != AccountStatus.Active)
            throw new InvalidOperationException("Cannot withdraw from a closed account.");
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive.");
        if (amount > Balance)
            throw new InvalidOperationException("Insufficient funds.");

        RaiseEvent(new MoneyWithdrawn(Id, amount, description, DateTime.UtcNow));
    }

    public void Close(string reason)
    {
        if (Status == AccountStatus.Closed)
            throw new InvalidOperationException("Account is already closed.");
        if (Balance != 0)
            throw new InvalidOperationException("Account balance must be zero before closing.");

        RaiseEvent(new AccountClosed(Id, reason, DateTime.UtcNow));
    }

    // Event handlers -- update internal state
    private void Apply(AccountOpened e)
    {
        Id = e.AccountId;
        CustomerId = e.CustomerId;
        AccountType = e.AccountType;
        Balance = e.InitialDeposit;
        Status = AccountStatus.Active;
    }

    private void Apply(MoneyDeposited e)
    {
        Balance += e.Amount;
    }

    private void Apply(MoneyWithdrawn e)
    {
        Balance -= e.Amount;
    }

    private void Apply(AccountClosed e)
    {
        Status = AccountStatus.Closed;
    }
}

public enum AccountStatus { Active, Closed }
```

## Step 3: Use the Event Store

```csharp
public class BankAccountService
{
    private readonly IEventStore _eventStore;

    public BankAccountService(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<BankAccount> OpenAccountAsync(
        Guid customerId, string accountType, decimal initialDeposit, CancellationToken ct)
    {
        var account = new BankAccount(customerId, accountType, initialDeposit);

        await _eventStore.SaveEventsAsync(
            account.Id,
            account.UncommittedEvents,
            expectedVersion: 0,
            ct);

        account.ClearUncommittedEvents();
        return account;
    }

    public async Task<BankAccount> GetAccountAsync(Guid accountId, CancellationToken ct)
    {
        var events = await _eventStore.GetEventsAsync(accountId, ct);

        if (!events.Any())
            throw new KeyNotFoundException($"Account {accountId} not found.");

        var account = new BankAccount();
        account.LoadFromHistory(events);
        return account;
    }

    public async Task DepositAsync(
        Guid accountId, decimal amount, string description, CancellationToken ct)
    {
        var account = await GetAccountAsync(accountId, ct);
        account.Deposit(amount, description);

        await _eventStore.SaveEventsAsync(
            accountId,
            account.UncommittedEvents,
            expectedVersion: account.Version,
            ct);

        account.ClearUncommittedEvents();
    }

    public async Task WithdrawAsync(
        Guid accountId, decimal amount, string description, CancellationToken ct)
    {
        var account = await GetAccountAsync(accountId, ct);
        account.Withdraw(amount, description);

        await _eventStore.SaveEventsAsync(
            accountId,
            account.UncommittedEvents,
            expectedVersion: account.Version,
            ct);

        account.ClearUncommittedEvents();
    }
}
```

## Step 4: Register Services

```csharp
// In Program.cs
builder.Services.AddRvrEventSourcing(options =>
{
    options.UseInMemoryStore();  // Development
    // options.UseSqlServerStore(connectionString);  // Production
});
```

## Building Read Models (Projections)

Event sourcing separates writes (events) from reads (projections). Build read models by subscribing to events:

```csharp
public class AccountBalanceProjection
{
    private readonly IProjectionStore _store;

    public async Task HandleAsync(MoneyDeposited @event)
    {
        var balance = await _store.GetAsync<AccountBalanceView>(@event.AccountId);
        balance.Balance += @event.Amount;
        balance.LastTransactionAt = @event.DepositedAt;
        await _store.SaveAsync(balance);
    }

    public async Task HandleAsync(MoneyWithdrawn @event)
    {
        var balance = await _store.GetAsync<AccountBalanceView>(@event.AccountId);
        balance.Balance -= @event.Amount;
        balance.LastTransactionAt = @event.WithdrawnAt;
        await _store.SaveAsync(balance);
    }
}

// Read model -- optimized for queries
public class AccountBalanceView
{
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
    public DateTime LastTransactionAt { get; set; }
}
```

## Temporal Queries

Replay events up to a specific point in time to see historical state:

```csharp
public async Task<BankAccount> GetAccountAtAsync(Guid accountId, DateTime pointInTime, CancellationToken ct)
{
    var allEvents = await _eventStore.GetEventsAsync(accountId, ct);

    var eventsUpToDate = allEvents
        .Where(e => e.Timestamp <= pointInTime)
        .ToList();

    var account = new BankAccount();
    account.LoadFromHistory(eventsUpToDate);
    return account;

    // account.Balance reflects the balance at `pointInTime`
}
```
