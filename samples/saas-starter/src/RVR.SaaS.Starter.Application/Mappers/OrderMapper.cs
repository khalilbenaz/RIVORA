using Riok.Mapperly.Abstractions;
using RVR.SaaS.Starter.Application.DTOs;
using RVR.SaaS.Starter.Domain.Entities;

namespace RVR.SaaS.Starter.Application.Mappers;

[Mapper]
public partial class OrderMapper
{
    public partial OrderDto OrderToDto(Order order);

    public partial Order DtoToOrder(OrderDto dto);
}
