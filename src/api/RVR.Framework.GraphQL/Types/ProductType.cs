namespace RVR.Framework.GraphQL.Types;

using RVR.Framework.Domain.Entities.Products;

/// <summary>
/// GraphQL type definition for the Product entity.
/// </summary>
public class ProductType : ObjectType<Product>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
    {
        descriptor.Description("Represents a product.");
        descriptor.Field(p => p.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(p => p.Name).Type<NonNullType<StringType>>();
    }
}
