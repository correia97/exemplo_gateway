using FluentValidation;
using OpenCode.DragonBall.Api.Dtos;

namespace OpenCode.DragonBall.Api.Validators;

public class CreateCharacterValidator : AbstractValidator<CreateCharacterRequest>
{
    public CreateCharacterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Character name is required")
            .MaximumLength(100).WithMessage("Character name must not exceed 100 characters");

        RuleFor(x => x.Race)
            .NotEmpty().WithMessage("Character race is required")
            .MaximumLength(50).WithMessage("Character race must not exceed 50 characters");

        RuleFor(x => x.Ki)
            .NotEmpty().WithMessage("Character ki is required")
            .MaximumLength(50).WithMessage("Character ki must not exceed 50 characters");

        RuleFor(x => x.MaxKi)
            .MaximumLength(50).WithMessage("Maximum ki must not exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.PictureUrl)
            .MaximumLength(500).WithMessage("Picture URL must not exceed 500 characters")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.PictureUrl))
            .WithMessage("Picture URL must be a valid absolute URL");
    }
}
