using FluentValidation;
using OpenCode.Music.Api.Dtos;

namespace OpenCode.Music.Api.Validators;

public class UpdateGenreValidator : AbstractValidator<UpdateGenreRequest>
{
    public UpdateGenreValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Genre name is required")
            .MaximumLength(100).WithMessage("Genre name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
    }
}