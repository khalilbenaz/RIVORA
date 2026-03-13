using KBA.Framework.Application.DTOs.Products;
using KBA.Framework.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBA.Framework.Api.Controllers;

/// <summary>
/// Contrôleur pour la gestion des produits
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
[Tags("Products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// Constructeur
    /// </summary>
    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère tous les produits
    /// </summary>
    /// <returns>Liste des produits</returns>
    /// <response code="200">Retourne la liste des produits</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productService.GetListAsync(cancellationToken);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des produits");
            return StatusCode(500, "Une erreur s'est produite lors de la récupération des produits.");
        }
    }

    /// <summary>
    /// Récupère les produits actifs
    /// </summary>
    /// <returns>Liste des produits actifs</returns>
    /// <response code="200">Retourne la liste des produits actifs</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductDto>>> GetActive(CancellationToken cancellationToken)
    {
        try
        {
            var products = await _productService.GetActiveListAsync(cancellationToken);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des produits actifs");
            return StatusCode(500, "Une erreur s'est produite lors de la récupération des produits actifs.");
        }
    }

    /// <summary>
    /// Récupère un produit par son identifiant
    /// </summary>
    /// <param name="id">Identifiant du produit</param>
    /// <param name="cancellationToken">Token d'annulation</param>
    /// <returns>Le produit ou 404</returns>
    /// <response code="200">Retourne le produit</response>
    /// <response code="404">Produit non trouvé</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.GetAsync(id, cancellationToken);
            if (product == null)
            {
                return NotFound($"Produit avec l'id {id} non trouvé.");
            }
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du produit {ProductId}", id);
            return StatusCode(500, "Une erreur s'est produite lors de la récupération du produit.");
        }
    }

    /// <summary>
    /// Recherche des produits par nom
    /// </summary>
    /// <param name="name">Nom à rechercher</param>
    /// <returns>Liste des produits correspondants</returns>
    /// <response code="200">Retourne la liste des produits</response>
    /// <response code="400">Paramètre invalide</response>
    [HttpGet("search/{name}")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ProductDto>>> Search(string name, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Le nom de recherche ne peut pas être vide.");
            }

            var products = await _productService.SearchByNameAsync(name, cancellationToken);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la recherche de produits avec le nom {Name}", name);
            return StatusCode(500, "Une erreur s'est produite lors de la recherche.");
        }
    }

    /// <summary>
    /// Crée un nouveau produit
    /// </summary>
    /// <param name="dto">Données du produit</param>
    /// <returns>Le produit créé</returns>
    /// <response code="201">Produit créé avec succès</response>
    /// <response code="400">Données invalides</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _productService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation échouée lors de la création du produit");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du produit");
            return StatusCode(500, "Une erreur s'est produite lors de la création du produit.");
        }
    }

    /// <summary>
    /// Met à jour un produit
    /// </summary>
    /// <param name="id">Identifiant du produit</param>
    /// <param name="dto">Données de mise à jour</param>
    /// <returns>Le produit mis à jour</returns>
    /// <response code="200">Produit mis à jour avec succès</response>
    /// <response code="400">Données invalides</response>
    /// <response code="404">Produit non trouvé</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _productService.UpdateAsync(id, dto, cancellationToken);
            return Ok(product);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Produit {ProductId} non trouvé", id);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation échouée lors de la mise à jour du produit {ProductId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du produit {ProductId}", id);
            return StatusCode(500, "Une erreur s'est produite lors de la mise à jour du produit.");
        }
    }

    /// <summary>
    /// Supprime un produit
    /// </summary>
    /// <param name="id">Identifiant du produit</param>
    /// <response code="204">Produit supprimé avec succès</response>
    /// <response code="404">Produit non trouvé</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _productService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Produit {ProductId} non trouvé", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du produit {ProductId}", id);
            return StatusCode(500, "Une erreur s'est produite lors de la suppression du produit.");
        }
    }
}
