using Riok.Mapperly.Abstractions;
using RVR.SaaS.Starter.Application.DTOs;
using RVR.SaaS.Starter.Domain.Entities;

namespace RVR.SaaS.Starter.Application.Mappers;

[Mapper]
public partial class TenantMapper
{
    public partial TenantDto TenantToDto(Tenant tenant);

    public partial Tenant DtoToTenant(TenantDto dto);
}
