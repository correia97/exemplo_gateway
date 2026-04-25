using FluentValidation;
using OpenCode.Music.Api.Dtos;

namespace OpenCode.Music.Api.Validators;

public class CreateAlbumValidator : AbstractValidator<CreateAlbumRequest>
{
    public CreateAlbumValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Album title is required")
            .MaximumLength(200).WithMessage("Album title must not exceed 200 characters");

        RuleFor(x => x.CoverUrl)
            .MaximumLength(500).WithMessage("Cover URL must not exceed 500 characters")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.CoverUrl))
            .WithMessage("Cover URL must be a valid absolute URL");

        RuleFor(x => x.ArtistId)
            .GreaterThan(0).WithMessage("ArtistId must be a positive integer");
    }
}