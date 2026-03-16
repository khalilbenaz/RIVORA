using Riok.Mapperly.Abstractions;
using KBA.SaaS.Starter.Application.DTOs;
using KBA.SaaS.Starter.Domain.Entities;

namespace KBA.SaaS.Starter.Application.Mappers;

[Mapper]
public partial class FeatureFlagMapper
{
    public partial FeatureFlagDto FeatureFlagToDto(FeatureFlag featureFlag);

    public partial FeatureFlag DtoToFeatureFlag(FeatureFlagDto dto);
}
