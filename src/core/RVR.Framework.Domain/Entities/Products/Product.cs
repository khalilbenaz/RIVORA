namespace RVR.Framework.Domain.Entities.Products;

/// <summary>
/// Entité métier exemple : Produit
/// </summary>
public class Product : AggregateRoot<Guid>
{
    /// <summary>
    /// Identifiant du tenant
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Nom du produit
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Description du produit
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Prix unitaire
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Stock disponible
    /// </summary>
    public int Stock { get; private set; }

    /// <summary>
    /// Indique si le produit est actif
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// SKU (Stock Keeping Unit)
    /// </summary>
    public string? SKU { get; private set; }

    /// <summary>
    /// Catégorie du produit
    /// </summary>
    public string? Category { get; private set; }

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private Product() { }

    /// <summary>
    /// Crée un nouveau produit
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="name">Nom du produit</param>
    /// <param name="price">Prix</param>
    /// <param name="stock">Stock initial</param>
    /// <param name="userId">Identifiant de l'utilisateur créateur</param>
    public Product(Guid? tenantId, string name, decimal price, int stock = 0, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du produit ne peut pas être vide.", nameof(name));

        if (price < 0)
            throw new ArgumentException("Le prix ne peut pas être négatif.", nameof(price));

        if (stock < 0)
            throw new ArgumentException("Le stock ne peut pas être négatif.", nameof(stock));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name.Trim();
        Price = price;
        Stock = stock;
        IsActive = true;
        SetCreationInfo(userId);
    }

    /// <summary>
    /// Met à jour les informations du produit
    /// </summary>
    /// <param name="name">Nom</param>
    /// <param name="description">Description</param>
    /// <param name="price">Prix</param>
    /// <param name="userId">Identifiant de l'utilisateur qui modifie</param>
    public void Update(string name, string? description, decimal price, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom du produit ne peut pas être vide.", nameof(name));

        if (price < 0)
            throw new ArgumentException("Le prix ne peut pas être négatif.", nameof(price));

        Name = name.Trim();
        Description = description?.Trim();
        Price = price;
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Ajuste le stock
    /// </summary>
    /// <param name="quantity">Quantité à ajouter (positive) ou retirer (négative)</param>
    /// <param name="userId">Identifiant de l'utilisateur qui modifie</param>
    public void AdjustStock(int quantity, Guid? userId = null)
    {
        var newStock = Stock + quantity;
        if (newStock < 0)
            throw new InvalidOperationException("Le stock ne peut pas être négatif.");

        Stock = newStock;
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Active le produit
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur qui active</param>
    public void Activate(Guid? userId = null)
    {
        IsActive = true;
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Désactive le produit
    /// </summary>
    /// <param name="userId">Identifiant de l'utilisateur qui désactive</param>
    public void Deactivate(Guid? userId = null)
    {
        IsActive = false;
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Définit le SKU
    /// </summary>
    /// <param name="sku">SKU</param>
    /// <param name="userId">Identifiant de l'utilisateur qui modifie</param>
    public void SetSKU(string? sku, Guid? userId = null)
    {
        SKU = sku?.Trim();
        SetModificationInfo(userId);
    }

    /// <summary>
    /// Définit la catégorie
    /// </summary>
    /// <param name="category">Catégorie</param>
    /// <param name="userId">Identifiant de l'utilisateur qui modifie</param>
    public void SetCategory(string? category, Guid? userId = null)
    {
        Category = category?.Trim();
        SetModificationInfo(userId);
    }
}
