using FluentValidation;
using OpenCode.Music.Api.Dtos;

namespace OpenCode.Music.Api.Validators;

public class UpdateTrackValidator : AbstractValidator<UpdateTrackRequest>
{
    public UpdateTrackValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Track name is required")
            .MaximumLength(200).WithMessage("Track name must not exceed 200 characters");

        RuleFor(x => x.TrackNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Track number must be 1 or greater");

        RuleFor(x => x.Duration)
            .GreaterThan(TimeSpan.Zero).WithMessage("Duration must be positive")
            .LessThan(TimeSpan.FromHours(2)).WithMessage("Duration must be less than 2 hours")
            .When(x => x.Duration.HasValue);

        RuleFor(x => x.Lyrics)
            .MaximumLength(10000).WithMessage("Lyrics must not exceed 10000 characters");
    }
}