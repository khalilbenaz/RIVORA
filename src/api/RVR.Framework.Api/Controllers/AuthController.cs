using RVR.Framework.Application.DTOs.Auth;
using RVR.Framework.Application.Services;
using RVR.Framework.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace RVR.Framework.Api.Controllers;

/// <summary>
/// Contrôleur pour l'authentification
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authentifie un utilisateur et retourne un token JWT
    /// </summary>
    /// <param name="loginDto">Informations de connexion</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Token JWT et informations utilisateur</returns>
    /// <response code="200">Authentification réussie</response>
    /// <response code="401">Identifiants invalides</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("strict")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(loginDto, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Échec d'authentification pour l'utilisateur: {UserName}", LogSanitizer.Sanitize(loginDto.UserName));
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'authentification de l'utilisateur: {UserName}", LogSanitizer.Sanitize(loginDto.UserName));
            return StatusCode(500, "Une erreur s'est produite lors de l'authentification.");
        }
    }

    /// <summary>
    /// Rafraîchit un token JWT
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Nouveau token JWT</returns>
    /// <response code="200">Token rafraîchi avec succès</response>
    /// <response code="401">Refresh token invalide</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(refreshToken, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Échec du rafraîchissement du token");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
            return StatusCode(500, "Une erreur s'est produite lors du rafraîchissement du token.");
        }
    }

    /// <summary>
    /// Déconnecte un utilisateur
    /// </summary>
    /// <param name="refreshToken">Refresh token à révoquer</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <response code="200">Déconnexion réussie</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            await _authService.LogoutAsync(refreshToken, cancellationToken);
            return Ok(new { message = "Déconnexion réussie." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la déconnexion");
            return StatusCode(500, "Une erreur s'est produite lors de la déconnexion.");
        }
    }
}
