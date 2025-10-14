using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using RVR.Framework.Domain.Entities.Identity;
using RVR.Framework.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace RVR.Framework.Benchmarks;

/// <summary>
/// Benchmarks for authentication operations including JWT token generation/validation
/// and BCrypt password hashing at various work factors.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AuthBenchmarks
{
    private JwtTokenService _jwtService = null!;
    private User _testUser = null!;
    private string _validToken = null!;
    private string _testPassword = null!;
    private string _bcryptHash10 = null!;
    private string _bcryptHash12 = null!;
    private string _bcryptHash14 = null!;

    private const string SecretKey = "ThisIsAVeryLongSecretKeyForBenchmarkingJwtTokenGeneration2024!@#$";

    [GlobalSetup]
    public void Setup()
    {
        var configData = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = SecretKey,
            ["JwtSettings:Issuer"] = "RVR.Framework.Benchmark",
            ["JwtSettings:Audience"] = "RVR.Framework.Benchmark.Client",
            ["JwtSettings:ExpirationMinutes"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _jwtService = new JwtTokenService(configuration);

        _testUser = new User(Guid.NewGuid(), "benchmarkuser", "benchmark@test.com");
        _testUser.UpdatePersonalInfo("Benchmark", "User", "+1234567890");

        _validToken = _jwtService.GenerateAccessToken(_testUser);
        _testPassword = "BenchmarkP@ssw0rd!2024";

        // Pre-compute hashes at different work factors
        _bcryptHash10 = BCrypt.Net.BCrypt.EnhancedHashPassword(_testPassword, 10);
        _bcryptHash12 = BCrypt.Net.BCrypt.EnhancedHashPassword(_testPassword, 12);
        _bcryptHash14 = BCrypt.Net.BCrypt.EnhancedHashPassword(_testPassword, 14);
    }

    // ─── JWT Token Generation ──────────────────────────────────────────

    [Benchmark(Description = "JWT - GenerateAccessToken")]
    public string JwtGenerate()
    {
        return _jwtService.GenerateAccessToken(_testUser);
    }

    [Benchmark(Description = "JWT - GenerateRefreshToken")]
    public string JwtRefreshToken()
    {
        return _jwtService.GenerateRefreshToken();
    }

    // ─── JWT Token Validation ──────────────────────────────────────────

    [Benchmark(Description = "JWT - ValidateToken (valid)")]
    public ClaimsPrincipal? JwtValidate()
    {
        return _jwtService.ValidateToken(_validToken);
    }

    [Benchmark(Description = "JWT - ValidateExpiredToken")]
    public ClaimsPrincipal? JwtValidateExpired()
    {
        return _jwtService.ValidateExpiredToken(_validToken);
    }

    // ─── BCrypt Hash Verification (Different Work Factors) ─────────────

    [Benchmark(Description = "BCrypt - Verify WorkFactor=10")]
    public bool BcryptVerify_WorkFactor10()
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(_testPassword, _bcryptHash10);
    }

    [Benchmark(Description = "BCrypt - Verify WorkFactor=12")]
    public bool BcryptVerify_WorkFactor12()
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(_testPassword, _bcryptHash12);
    }

    [Benchmark(Description = "BCrypt - Verify WorkFactor=14")]
    public bool BcryptVerify_WorkFactor14()
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(_testPassword, _bcryptHash14);
    }

    // ─── Password Hashing Throughput ───────────────────────────────────

    [Benchmark(Description = "BCrypt - HashPassword WorkFactor=10")]
    public string BcryptHash_WorkFactor10()
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(_testPassword, 10);
    }

    [Benchmark(Description = "BCrypt - HashPassword WorkFactor=12")]
    public string BcryptHash_WorkFactor12()
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(_testPassword, 12);
    }

    [Benchmark(Description = "BCrypt - HashPassword WorkFactor=14")]
    public string BcryptHash_WorkFactor14()
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(_testPassword, 14);
    }

    [Benchmark(Description = "PasswordHasher service - HashPassword")]
    public string PasswordHasherService_Hash()
    {
        var logger = NullLogger<Security.Services.PasswordHasher>.Instance;
        var options = Microsoft.Extensions.Options.Options.Create(new Security.Services.PasswordHasherOptions());
        var hasher = new Security.Services.PasswordHasher(logger, options);
        return hasher.HashPassword(_testPassword);
    }

    [Benchmark(Description = "PasswordHasher service - VerifyPassword")]
    public bool PasswordHasherService_Verify()
    {
        var logger = NullLogger<Security.Services.PasswordHasher>.Instance;
        var options = Microsoft.Extensions.Options.Options.Create(new Security.Services.PasswordHasherOptions());
        var hasher = new Security.Services.PasswordHasher(logger, options);
        return hasher.VerifyPassword(_testPassword, _bcryptHash12);
    }
}
