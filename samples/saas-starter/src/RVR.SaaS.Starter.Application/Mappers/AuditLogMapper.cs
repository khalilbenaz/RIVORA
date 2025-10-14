using Riok.Mapperly.Abstractions;
using RVR.SaaS.Starter.Application.DTOs;
using RVR.SaaS.Starter.Domain.Entities;

namespace RVR.SaaS.Starter.Application.Mappers;

[Mapper]
public partial class AuditLogMapper
{
    public partial AuditLogDto AuditLogToDto(AuditLog auditLog);

    public partial AuditLog DtoToAuditLog(AuditLogDto dto);
}
