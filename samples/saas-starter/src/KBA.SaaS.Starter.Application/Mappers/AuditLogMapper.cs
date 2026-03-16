using Riok.Mapperly.Abstractions;
using KBA.SaaS.Starter.Application.DTOs;
using KBA.SaaS.Starter.Domain.Entities;

namespace KBA.SaaS.Starter.Application.Mappers;

[Mapper]
public partial class AuditLogMapper
{
    public partial AuditLogDto AuditLogToDto(AuditLog auditLog);

    public partial AuditLog DtoToAuditLog(AuditLogDto dto);
}
