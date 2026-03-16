using Riok.Mapperly.Abstractions;
using KBA.SaaS.Starter.Application.DTOs;
using KBA.SaaS.Starter.Domain.Entities;

namespace KBA.SaaS.Starter.Application.Mappers;

[Mapper]
public partial class UserMapper
{
    public partial UserDto UserToDto(User user);

    public partial User DtoToUser(UserDto dto);
}
