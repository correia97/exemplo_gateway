using FluentValidation.TestHelper;
using OpenCode.Music.Api.Dtos;
using OpenCode.Music.Api.Validators;

namespace OpenCode.Api.Tests.Validators;

public class TrackValidatorTests
{
    [Fact]
    public void CreateTrack_Should_Pass_When_Valid()
    {
        var validator = new CreateTrackValidator();
        var request = new CreateTrackRequest("Come Together", 1, TimeSpan.FromSeconds(259), null, 1, false);
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTrack_Should_Fail_When_Name_Empty()
    {
        var validator = new CreateTrackValidator();
        var request = new CreateTrackRequest("", 1, null, null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateTrack_Should_Fail_When_TrackNumber_Zero()
    {
        var validator = new CreateTrackValidator();
        var request = new CreateTrackRequest("Song", 0, null, null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TrackNumber);
    }

    [Fact]
    public void CreateTrack_Should_Fail_When_Duration_Negative()
    {
        var validator = new CreateTrackValidator();
        var request = new CreateTrackRequest("Song", 1, TimeSpan.FromSeconds(-1), null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    [Fact]
    public void CreateTrack_Should_Fail_When_Duration_Exceeds_TwoHours()
    {
        var validator = new CreateTrackValidator();
        var request = new CreateTrackRequest("Song", 1, TimeSpan.FromHours(2).Add(TimeSpan.FromSeconds(1)), null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    [Fact]
    public void CreateTrack_Should_Pass_When_Duration_Null()
    {
        var validator = new CreateTrackValidator();
        var request = new CreateTrackRequest("Song", 1, null, null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateTrack_Should_Pass_When_Valid()
    {
        var validator = new UpdateTrackValidator();
        var request = new UpdateTrackRequest("Updated Song", 2, TimeSpan.FromSeconds(180), "Lyrics", 1, true);
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateTrack_Should_Fail_When_Name_Empty()
    {
        var validator = new UpdateTrackValidator();
        var request = new UpdateTrackRequest("", 1, null, null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateTrack_Should_Fail_When_Duration_Zero()
    {
        var validator = new CreateTrackValidator();
        var request = new CreateTrackRequest("Song", 1, TimeSpan.Zero, null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    [Fact]
    public void CreateTrack_Should_Fail_When_Duration_Exactly_TwoHours()
    {
        var validator = new CreateTrackValidator();
        var request = new CreateTrackRequest("Song", 1, TimeSpan.FromHours(2), null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    [Fact]
    public void UpdateTrack_Should_Fail_When_Name_Exceeds_MaxLength()
    {
        var validator = new UpdateTrackValidator();
        var request = new UpdateTrackRequest(new string('A', 201), 1, null, null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateTrack_Should_Fail_When_TrackNumber_Zero()
    {
        var validator = new UpdateTrackValidator();
        var request = new UpdateTrackRequest("Song", 0, null, null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TrackNumber);
    }

    [Fact]
    public void UpdateTrack_Should_Fail_When_Duration_Negative()
    {
        var validator = new UpdateTrackValidator();
        var request = new UpdateTrackRequest("Song", 1, TimeSpan.FromSeconds(-1), null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }

    [Fact]
    public void UpdateTrack_Should_Fail_When_Duration_Exceeds_TwoHours()
    {
        var validator = new UpdateTrackValidator();
        var request = new UpdateTrackRequest("Song", 1, TimeSpan.FromHours(2).Add(TimeSpan.FromSeconds(1)), null, null, false);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Duration);
    }
}
