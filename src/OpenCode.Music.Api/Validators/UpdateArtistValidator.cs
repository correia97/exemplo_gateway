using FluentValidation;
using OpenCode.Music.Api.Dtos;

namespace OpenCode.Music.Api.Validators;

public class UpdateArtistValidator : AbstractValidator<UpdateArtistRequest>
{
    public UpdateArtistValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Artist name is required")
            .MaximumLength(200).WithMessage("Artist name must not exceed 200 characters");

        RuleFor(x => x.Biography)
            .MaximumLength(4000).WithMessage("Biography must not exceed 4000 characters");
    }
}