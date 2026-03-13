using KBA.Framework.Application.DTOs.Users;
using KBA.Framework.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBA.Framework.Api.Controllers;

/// <summary>
/// Contrôleur pour la gestion des utilisateurs
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
[Tags("Users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Constructeur
    /// </summary>
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère tous les utilisateurs
    /// </summary>
    /// <returns>Liste des utilisateurs</returns>
    /// <response code="200">Retourne la liste des utilisateurs</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userService.GetListAsync(cancellationToken);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des utilisateurs");
            return StatusCode(500, "Une erreur s'est produite lors de la récupération des utilisateurs.");
        }
    }

    /// <summary>
    /// Récupère un utilisateur par son identifiant
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>L'utilisateur ou 404</returns>
    /// <response code="200">Retourne l'utilisateur</response>
    /// <response code="404">Utilisateur non trouvé</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetAsync(id, cancellationToken);
            if (user == null)
            {
                return NotFound($"Utilisateur avec l'id {id} non trouvé.");
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur {UserId}", id);
            return StatusCode(500, "Une erreur s'est produite lors de la récupération de l'utilisateur.");
        }
    }

    /// <summary>
    /// Récupère un utilisateur par nom d'utilisateur
    /// </summary>
    /// <param name="userName">Nom d'utilisateur</param>
    /// <returns>L'utilisateur</returns>
    /// <response code="200">Retourne l'utilisateur</response>
    /// <response code="404">Utilisateur non trouvé</response>
    [HttpGet("by-username/{userName}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetByUserName(string userName, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.GetByUserNameAsync(userName, cancellationToken);
            if (user == null)
            {
                return NotFound($"Utilisateur avec le nom '{userName}' non trouvé.");
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur {UserName}", userName);
            return StatusCode(500, "Une erreur s'est produite lors de la récupération de l'utilisateur.");
        }
    }

    /// <summary>
    /// Crée un nouvel utilisateur
    /// </summary>
    /// <param name="dto">Données de l'utilisateur</param>
    /// <returns>L'utilisateur créé</returns>
    /// <response code="201">Utilisateur créé avec succès</response>
    /// <response code="400">Données invalides</response>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Opération invalide lors de la création de l'utilisateur");
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation échouée lors de la création de l'utilisateur");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'utilisateur");
            return StatusCode(500, "Une erreur s'est produite lors de la création de l'utilisateur.");
        }
    }

    /// <summary>
    /// Met à jour un utilisateur
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur</param>
    /// <param name="dto">Données de mise à jour</param>
    /// <returns>L'utilisateur mis à jour</returns>
    /// <response code="200">Utilisateur mis à jour avec succès</response>
    /// <response code="400">Données invalides</response>
    /// <response code="404">Utilisateur non trouvé</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.UpdateAsync(id, dto, cancellationToken);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Utilisateur {UserId} non trouvé", id);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation échouée lors de la mise à jour de l'utilisateur {UserId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de l'utilisateur {UserId}", id);
            return StatusCode(500, "Une erreur s'est produite lors de la mise à jour de l'utilisateur.");
        }
    }

    /// <summary>
    /// Supprime un utilisateur
    /// </summary>
    /// <param name="id">Identifiant de l'utilisateur</param>
    /// <response code="204">Utilisateur supprimé avec succès</response>
    /// <response code="404">Utilisateur non trouvé</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Utilisateur {UserId} non trouvé", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'utilisateur {UserId}", id);
            return StatusCode(500, "Une erreur s'est produite lors de la suppression de l'utilisateur.");
        }
    }
}
