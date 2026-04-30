using FluentValidation.TestHelper;
using OpenCode.Music.Api.Dtos;
using OpenCode.Music.Api.Validators;

namespace OpenCode.Api.Tests.Validators;

public class ArtistValidatorTests
{
    [Fact]
    public void CreateArtist_Should_Pass_When_Valid()
    {
        var validator = new CreateArtistValidator();
        var request = new CreateArtistRequest("The Beatles", "British rock band", new List<int> { 1, 2 });
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateArtist_Should_Fail_When_Name_Empty()
    {
        var validator = new CreateArtistValidator();
        var request = new CreateArtistRequest("", null, null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateArtist_Should_Fail_When_Name_Exceeds_MaxLength()
    {
        var validator = new CreateArtistValidator();
        var request = new CreateArtistRequest(new string('A', 201), null, null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateArtist_Should_Fail_When_Biography_Exceeds_MaxLength()
    {
        var validator = new CreateArtistValidator();
        var request = new CreateArtistRequest("Test", new string('A', 4001), null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Biography);
    }

    [Fact]
    public void UpdateArtist_Should_Pass_When_Valid()
    {
        var validator = new UpdateArtistValidator();
        var request = new UpdateArtistRequest("Queen", "Legendary rock band", new List<int> { 1 });
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateArtist_Should_Fail_When_Name_Empty()
    {
        var validator = new UpdateArtistValidator();
        var request = new UpdateArtistRequest("", null, null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateArtist_Should_Fail_When_Name_Exceeds_MaxLength()
    {
        var validator = new UpdateArtistValidator();
        var request = new UpdateArtistRequest(new string('A', 201), null, null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateArtist_Should_Fail_When_Biography_Exceeds_MaxLength()
    {
        var validator = new UpdateArtistValidator();
        var request = new UpdateArtistRequest("Queen", new string('A', 4001), null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Biography);
    }
}
