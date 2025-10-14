using FluentValidation;
using RVR.Framework.Application.DTOs.Auth;

namespace RVR.Framework.Application.Validators.Auth;

/// <summary>
/// Validator pour LoginDto
/// </summary>
public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Le nom d'utilisateur est obligatoire.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est obligatoire.");
    }
}
