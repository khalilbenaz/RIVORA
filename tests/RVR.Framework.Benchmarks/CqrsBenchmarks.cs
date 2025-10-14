using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FluentValidation;
using FluentValidation.Results;
using RVR.Framework.Application.DTOs.Users;
using RVR.Framework.Application.Validators.Users;
using RVR.Framework.Domain.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace RVR.Framework.Benchmarks;

/// <summary>
/// Benchmarks for CQRS pipeline components including MediatR dispatch latency,
/// FluentValidation execution time, and Result pattern overhead.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CqrsBenchmarks
{
    private IServiceProvider _serviceProvider = null!;
    private MediatR.IMediator _mediator = null!;
    private CreateUserDtoValidator _validator = null!;
    private CreateUserDto _validDto = null!;
    private CreateUserDto _invalidDto = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Register MediatR with our sample handler
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SamplePingRequest).Assembly));

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<MediatR.IMediator>();

        _validator = new CreateUserDtoValidator();
        _validDto = new CreateUserDto("benchmarkuser", "user@benchmark.com", "P@ssw0rd!2024", "John", "Doe", "+1234567890");
        _invalidDto = new CreateUserDto("", "not-an-email", "weak", null, null, null);
    }

    // ─── MediatR Dispatch Latency ──────────────────────────────────────

    [Benchmark(Description = "MediatR - Send request (simple handler)")]
    public async Task<string> MediatR_Send()
    {
        return await _mediator.Send(new SamplePingRequest { Message = "Benchmark" });
    }

    [Benchmark(Description = "MediatR - Publish notification")]
    public async Task MediatR_Publish()
    {
        await _mediator.Publish(new SampleNotification { Message = "Benchmark" });
    }

    // ─── FluentValidation Execution Time ───────────────────────────────

    [Benchmark(Description = "FluentValidation - Valid DTO")]
    public ValidationResult Validate_ValidDto()
    {
        return _validator.Validate(_validDto);
    }

    [Benchmark(Description = "FluentValidation - Invalid DTO (all rules fail)")]
    public ValidationResult Validate_InvalidDto()
    {
        return _validator.Validate(_invalidDto);
    }

    [Benchmark(Description = "FluentValidation - ValidateAsync Valid DTO")]
    public async Task<ValidationResult> ValidateAsync_ValidDto()
    {
        return await _validator.ValidateAsync(_validDto);
    }

    [Benchmark(Description = "FluentValidation - ValidateAsync Invalid DTO")]
    public async Task<ValidationResult> ValidateAsync_InvalidDto()
    {
        return await _validator.ValidateAsync(_invalidDto);
    }

    // ─── Result Pattern Overhead ───────────────────────────────────────

    [Benchmark(Description = "Result.Success() - no value")]
    public Result ResultSuccess()
    {
        return Result.Success();
    }

    [Benchmark(Description = "Result.Failure() - with error")]
    public Result ResultFailure()
    {
        return Result.Failure("Something went wrong", "ERR_001");
    }

    [Benchmark(Description = "Result<T>.Success() - with value")]
    public Result<string> ResultSuccess_WithValue()
    {
        return Result.Success("benchmark-result-value");
    }

    [Benchmark(Description = "Result<T>.Failure() - with error")]
    public Result<string> ResultFailure_WithValue()
    {
        return Result<string>.Failure("Something went wrong", "ERR_001");
    }

    [Benchmark(Description = "Result<T> - implicit conversion from T")]
    public Result<int> ResultImplicitConversion()
    {
        return 42;
    }

    [Benchmark(Description = "Result<T> chain - Success then access Value")]
    public string ResultChainSuccess()
    {
        var result = Result.Success("hello");
        return result.IsSuccess ? result.Value : string.Empty;
    }
}

// ─── Sample MediatR types for benchmarking ─────────────────────────────

public class SamplePingRequest : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
}

public class SamplePingHandler : IRequestHandler<SamplePingRequest, string>
{
    public Task<string> Handle(SamplePingRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Pong: {request.Message}");
    }
}

public class SampleNotification : INotification
{
    public string Message { get; set; } = string.Empty;
}

public class SampleNotificationHandler : INotificationHandler<SampleNotification>
{
    public Task Handle(SampleNotification notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
