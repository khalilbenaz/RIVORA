using Riok.Mapperly.Abstractions;
using RVR.SaaS.Starter.Application.DTOs;
using RVR.SaaS.Starter.Domain.Entities;

namespace RVR.SaaS.Starter.Application.Mappers;

[Mapper]
public partial class ProductMapper
{
    public partial ProductDto ProductToDto(Product product);

    public partial Product DtoToProduct(ProductDto dto);
}
