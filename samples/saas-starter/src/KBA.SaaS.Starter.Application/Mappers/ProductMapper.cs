using Riok.Mapperly.Abstractions;
using KBA.SaaS.Starter.Application.DTOs;
using KBA.SaaS.Starter.Domain.Entities;

namespace KBA.SaaS.Starter.Application.Mappers;

[Mapper]
public partial class ProductMapper
{
    public partial ProductDto ProductToDto(Product product);

    public partial Product DtoToProduct(ProductDto dto);
}
