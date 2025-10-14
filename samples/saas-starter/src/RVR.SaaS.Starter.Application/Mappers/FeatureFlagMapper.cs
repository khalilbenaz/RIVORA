using Riok.Mapperly.Abstractions;
using RVR.SaaS.Starter.Application.DTOs;
using RVR.SaaS.Starter.Domain.Entities;

namespace RVR.SaaS.Starter.Application.Mappers;

[Mapper]
public partial class FeatureFlagMapper
{
    public partial FeatureFlagDto FeatureFlagToDto(FeatureFlag featureFlag);

    public partial FeatureFlag DtoToFeatureFlag(FeatureFlagDto dto);
}
