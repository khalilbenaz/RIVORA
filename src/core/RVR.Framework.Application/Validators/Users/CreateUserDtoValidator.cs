using FluentValidation;
using RVR.Framework.Application.DTOs.Users;

namespace RVR.Framework.Application.Validators.Users;

/// <summary>
/// Validator pour CreateUserDto
/// </summary>
public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Le nom d'utilisateur est obligatoire.")
            .MinimumLength(3).WithMessage("Le nom d'utilisateur doit contenir au moins 3 caractères.")
            .MaximumLength(50).WithMessage("Le nom d'utilisateur ne peut pas dépasser 50 caractères.")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Le nom d'utilisateur ne peut contenir que des lettres, chiffres, tirets et underscores.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email est obligatoire.")
            .EmailAddress().WithMessage("L'email n'est pas valide.")
            .MaximumLength(256).WithMessage("L'email ne peut pas dépasser 256 caractères.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est obligatoire.")
            .MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caractères.")
            .Matches(@"[A-Z]").WithMessage("Le mot de passe doit contenir au moins une lettre majuscule.")
            .Matches(@"[a-z]").WithMessage("Le mot de passe doit contenir au moins une lettre minuscule.")
            .Matches(@"[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre.")
            .Matches(@"[\W_]").WithMessage("Le mot de passe doit contenir au moins un caractère spécial.");

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
