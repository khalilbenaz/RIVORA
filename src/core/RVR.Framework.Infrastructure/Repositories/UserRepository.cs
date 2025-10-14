// ---------------------------------------------------------------------------
// EXAMPLE IMPLEMENTATION - NOT CORE FRAMEWORK CODE
// ---------------------------------------------------------------------------
// This repository is a sample/reference implementation provided as part of
// the Rivora Framework starter template. It is business-specific and should
// be overridden or replaced in your own application's Infrastructure layer.
//
// The generic Repository<TEntity, TKey> base class (see Repository.cs) is
// the actual framework code and is intended to be reused as-is.
// ---------------------------------------------------------------------------

using RVR.Framework.Domain.Entities.Identity;
using RVR.Framework.Domain.Repositories;
using RVR.Framework.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace RVR.Framework.Infrastructure.Repositories;

/// <summary>
/// <b>Example implementation</b> – Repository for <see cref="User"/> entities.
/// <para>
/// This class is provided as a reference/starter implementation and is <b>not</b>
/// part of the core Rivora Framework. Consumers of the framework should create
/// their own user repository in their application's Infrastructure project,
/// inheriting from <see cref="Repository{TEntity, TKey}"/> and implementing
/// <see cref="IUserRepository"/>.
/// </para>
/// </summary>
public class UserRepository : Repository<User, Guid>, IUserRepository
{
    #region Compiled Queries (business-specific, kept with the repository)

    private static readonly Func<RVRDbContext, string, Task<User?>> GetUserByNormalizedUserNameQuery =
        EF.CompileAsyncQuery((RVRDbContext ctx, string normalizedUserName) =>
            ctx.Users.FirstOrDefault(u => u.NormalizedUserName == normalizedUserName));

    private static readonly Func<RVRDbContext, string, Task<User?>> GetUserByNormalizedEmailQuery =
        EF.CompileAsyncQuery((RVRDbContext ctx, string normalizedEmail) =>
            ctx.Users.AsNoTracking().FirstOrDefault(u => u.NormalizedEmail == normalizedEmail));

    private static readonly Func<RVRDbContext, Guid, Task<User?>> GetUserByIdQuery =
        EF.CompileAsyncQuery((RVRDbContext ctx, Guid id) =>
            ctx.Users.AsNoTracking().FirstOrDefault(u => u.Id == id));

    private static readonly Func<RVRDbContext, IAsyncEnumerable<User>> GetActiveUsersQuery =
        EF.CompileAsyncQuery((RVRDbContext ctx) =>
            ctx.Users.AsNoTracking().Where(u => u.IsActive).AsQueryable());

    #endregion

    /// <summary>
    /// Constructeur
    /// </summary>
    public UserRepository(RVRDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var normalizedUserName = userName.ToUpperInvariant();
        // Uses compiled query - no AsNoTracking because this may be used for authentication
        // where tracking is needed to update the user
        return await GetUserByNormalizedUserNameQuery(_context, normalizedUserName);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToUpperInvariant();
        return await GetUserByNormalizedEmailQuery(_context, normalizedEmail);
    }

    /// <summary>
    /// Retrieves a user by their identifier (read-only via compiled query).
    /// </summary>
    public override async Task<User?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetUserByIdQuery(_context, id);
    }

    /// <summary>
    /// Retrieves all active users.
    /// </summary>
    public async Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<User>();
        await foreach (var user in GetActiveUsersQuery(_context).WithCancellation(cancellationToken))
        {
            result.Add(user);
        }
        return result;
    }
}
