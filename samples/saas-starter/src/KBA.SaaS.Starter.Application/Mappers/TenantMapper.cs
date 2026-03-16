using Riok.Mapperly.Abstractions;
using KBA.SaaS.Starter.Application.DTOs;
using KBA.SaaS.Starter.Domain.Entities;

namespace KBA.SaaS.Starter.Application.Mappers;

[Mapper]
public partial class TenantMapper
{
    public partial TenantDto TenantToDto(Tenant tenant);

    public partial Tenant DtoToTenant(TenantDto dto);
}
