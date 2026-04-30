using FluentValidation.TestHelper;
using OpenCode.Music.Api.Dtos;
using OpenCode.Music.Api.Validators;

namespace OpenCode.Api.Tests.Validators;

public class GenreValidatorTests
{
    [Fact]
    public void CreateGenre_Should_Pass_When_Valid()
    {
        var validator = new CreateGenreValidator();
        var request = new CreateGenreRequest("Rock", "Rock music genre");
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateGenre_Should_Fail_When_Name_Empty()
    {
        var validator = new CreateGenreValidator();
        var request = new CreateGenreRequest("", null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateGenre_Should_Fail_When_Name_Exceeds_MaxLength()
    {
        var validator = new CreateGenreValidator();
        var request = new CreateGenreRequest(new string('A', 101), null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateGenre_Should_Pass_When_Valid()
    {
        var validator = new UpdateGenreValidator();
        var request = new UpdateGenreRequest("Jazz", "Smooth jazz");
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateGenre_Should_Fail_When_Name_Empty()
    {
        var validator = new UpdateGenreValidator();
        var request = new UpdateGenreRequest("", null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateGenre_Should_Fail_When_Description_Exceeds_MaxLength()
    {
        var validator = new CreateGenreValidator();
        var request = new CreateGenreRequest("Rock", new string('A', 1001));
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void UpdateGenre_Should_Fail_When_Name_Exceeds_MaxLength()
    {
        var validator = new UpdateGenreValidator();
        var request = new UpdateGenreRequest(new string('A', 101), null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateGenre_Should_Fail_When_Description_Exceeds_MaxLength()
    {
        var validator = new UpdateGenreValidator();
        var request = new UpdateGenreRequest("Jazz", new string('A', 1001));
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
