using FluentValidation;
using RVR.Framework.Application.DTOs.Users;

namespace RVR.Framework.Application.Validators.Users;

/// <summary>
/// Validator pour UpdateUserDto
/// </summary>
public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Le prénom ne peut pas dépasser 100 caractères.")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Le numéro de téléphone n'est pas valide.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
