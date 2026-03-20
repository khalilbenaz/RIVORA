using RVR.Framework.Application.DTOs.Users;
using RVR.Framework.Application.Services;
using RVR.Framework.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RVR.Framework.Api.Controllers;

/// <summary>
/// Contrôleur pour l'initialisation du système
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Initialization")]
public class InitController : ControllerBase
{
    private static readonly SemaphoreSlim _initLock = new(1, 1);

    private readonly IUserService _userService;
    private readonly ILogger<InitController> _logger;

    public InitController(IUserService userService, ILogger<InitController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Initialise le premier utilisateur administrateur du système
    /// </summary>
    /// <param name="dto">Informations du premier administrateur</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>L'utilisateur créé</returns>
    /// <response code="200">Utilisateur administrateur créé avec succès</response>
    /// <response code="400">Données invalides ou un utilisateur existe déjà</response>
    /// <remarks>
    /// Cet endpoint ne peut être utilisé que si aucun utilisateur n'existe dans le système.
    /// Il crée le premier utilisateur administrateur avec tous les privilèges.
    /// 
    /// Exemple de requête:
    /// 
    ///     POST /api/init/first-admin
    ///     {
    ///         "userName": "admin",
    ///         "email": "admin@RIVORA-framework.com",
    ///         "password": "Admin@123",
    ///         "firstName": "Admin",
    ///         "lastName": "System"
    ///     }
    /// 
    /// </remarks>
    [HttpPost("first-admin")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> InitializeFirstAdmin([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        if (!await _initLock.WaitAsync(TimeSpan.FromSeconds(5)))
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Initialization already in progress." });
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifier qu'aucun utilisateur n'existe déjà
            var existingUsers = await _userService.GetListAsync(cancellationToken);
            if (existingUsers.Any())
            {
                _logger.LogWarning("Tentative d'initialisation alors que des utilisateurs existent déjà");
                return BadRequest(new { message = "Le système est déjà initialisé. Des utilisateurs existent déjà dans la base de données." });
            }

            var user = await _userService.CreateAsync(dto, cancellationToken);
            _logger.LogInformation("Premier utilisateur administrateur créé avec succès: {UserName}", LogSanitizer.Sanitize(dto.UserName));

            return Ok(new
            {
                user,
                message = "Premier utilisateur administrateur créé avec succès. Vous pouvez maintenant vous connecter via /api/auth/login"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Opération invalide lors de l'initialisation");
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation échouée lors de l'initialisation");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation du premier administrateur");
            return StatusCode(500, new { message = "Une erreur s'est produite lors de l'initialisation." });
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Vérifie si le système nécessite une initialisation
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Statut d'initialisation du système</returns>
    /// <response code="200">Retourne le statut d'initialisation</response>
    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetInitStatus(CancellationToken cancellationToken)
    {
        try
        {
            var existingUsers = await _userService.GetListAsync(cancellationToken);
            var needsInitialization = !existingUsers.Any();

            return Ok(new
            {
                needsInitialization,
                userCount = existingUsers.Count,
                message = needsInitialization
                    ? "Le système nécessite une initialisation. Veuillez créer le premier utilisateur administrateur."
                    : "Le système est déjà initialisé."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification du statut d'initialisation");
            return StatusCode(500, new { message = "Une erreur s'est produite lors de la vérification du statut." });
        }
    }
}
