using FluentValidation;
using OpenCode.Music.Api.Dtos;

namespace OpenCode.Music.Api.Validators;

public class CreateArtistValidator : AbstractValidator<CreateArtistRequest>
{
    public CreateArtistValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Artist name is required")
            .MaximumLength(200).WithMessage("Artist name must not exceed 200 characters");

        RuleFor(x => x.Biography)
            .MaximumLength(4000).WithMessage("Biography must not exceed 4000 characters");
    }
}