using RVR.Framework.Application.DTOs.Products;
using RVR.Framework.Domain.Entities.Products;
using RVR.Framework.Domain.Repositories;

namespace RVR.Framework.Application.Services;

/// <summary>
/// Service de gestion des produits
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUserContext _currentUserContext;

    /// <summary>
    /// Constructeur
    /// </summary>
    public ProductService(IProductRepository productRepository, ICurrentUserContext currentUserContext)
    {
        _productRepository = productRepository;
        _currentUserContext = currentUserContext;
    }

    /// <inheritdoc />
    public async Task<ProductDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        return product == null ? null : MapToDto(product);
    }

    /// <inheritdoc />
    public async Task<List<ProductDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetListAsync(cancellationToken);
        return products.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<List<ProductDto>> GetActiveListAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetActiveProductsAsync(cancellationToken);
        return products.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var product = new Product(
            tenantId: _currentUserContext.TenantId,
            name: dto.Name,
            price: dto.Price,
            stock: dto.Stock
        );

        if (!string.IsNullOrWhiteSpace(dto.Description))
            product.Update(dto.Name, dto.Description, dto.Price);

        if (!string.IsNullOrWhiteSpace(dto.SKU))
            product.SetSKU(dto.SKU);

        if (!string.IsNullOrWhiteSpace(dto.Category))
            product.SetCategory(dto.Category);

        var createdProduct = await _productRepository.InsertAsync(product, cancellationToken);
        return MapToDto(createdProduct);
    }

    /// <inheritdoc />
    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"Produit avec l'id {id} non trouvé.");

        product.Update(dto.Name, dto.Description, dto.Price);
        product.SetSKU(dto.SKU);
        product.SetCategory(dto.Category);

        var updatedProduct = await _productRepository.UpdateAsync(product, cancellationToken);
        return MapToDto(updatedProduct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null)
            throw new KeyNotFoundException($"Produit avec l'id {id} non trouvé.");

        await _productRepository.DeleteAsync(product, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<ProductDto>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom de recherche ne peut pas être vide.", nameof(name));

        var products = await _productRepository.SearchByNameAsync(name, cancellationToken);
        return products.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Convertit une entité Product en ProductDto
    /// </summary>
    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto(
            Id: product.Id,
            TenantId: product.TenantId,
            Name: product.Name,
            Description: product.Description,
            Price: product.Price,
            Stock: product.Stock,
            IsActive: product.IsActive,
            SKU: product.SKU,
            Category: product.Category,
            CreatedAt: product.CreatedAt,
            CreatorId: product.CreatorId,
            UpdatedAt: product.UpdatedAt,
            LastModifierId: product.LastModifierId
        );
    }
}
