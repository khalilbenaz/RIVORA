namespace RVR.Framework.GraphQL.Types;

using RVR.Framework.Domain.Entities.Identity;

/// <summary>
/// GraphQL type definition for the User entity.
/// </summary>
public class UserType : ObjectType<User>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Description("Represents a user in the system.");
        descriptor.Field(u => u.PasswordHash).Ignore(); // Never expose password hash
        descriptor.Field(u => u.Id).Type<NonNullType<UuidType>>();
        descriptor.Field(u => u.UserName).Type<NonNullType<StringType>>();
        descriptor.Field(u => u.Email).Type<NonNullType<StringType>>();
    }
}
