using FluentValidation;
using OpenCode.DragonBall.Api.Dtos;

namespace OpenCode.DragonBall.Api.Validators;

public class UpdateCharacterValidator : AbstractValidator<UpdateCharacterRequest>
{
    public UpdateCharacterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Character name is required")
            .MaximumLength(100).WithMessage("Character name must not exceed 100 characters");

        RuleFor(x => x.IntroductionPhase)
            .MaximumLength(100).WithMessage("Introduction phase must not exceed 100 characters");

        RuleFor(x => x.PictureUrl)
            .MaximumLength(500).WithMessage("Picture URL must not exceed 500 characters")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.PictureUrl))
            .WithMessage("Picture URL must be a valid absolute URL");
    }
}