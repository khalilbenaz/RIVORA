using Riok.Mapperly.Abstractions;
using KBA.SaaS.Starter.Application.DTOs;
using KBA.SaaS.Starter.Domain.Entities;

namespace KBA.SaaS.Starter.Application.Mappers;

[Mapper]
public partial class OrderMapper
{
    public partial OrderDto OrderToDto(Order order);

    public partial Order DtoToOrder(OrderDto dto);
}
