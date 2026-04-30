using FluentValidation.TestHelper;
using OpenCode.Music.Api.Dtos;
using OpenCode.Music.Api.Validators;

namespace OpenCode.Api.Tests.Validators;

public class AlbumValidatorTests
{
    [Fact]
    public void CreateAlbum_Should_Pass_When_Valid()
    {
        var validator = new CreateAlbumValidator();
        var request = new CreateAlbumRequest("Abbey Road", new DateOnly(1969, 9, 26), "https://example.com/abbey.jpg", 1);
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateAlbum_Should_Fail_When_Title_Empty()
    {
        var validator = new CreateAlbumValidator();
        var request = new CreateAlbumRequest("", null, null, 1);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreateAlbum_Should_Fail_When_CoverUrl_Invalid()
    {
        var validator = new CreateAlbumValidator();
        var request = new CreateAlbumRequest("Test", null, "invalid-url", 1);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CoverUrl);
    }

    [Fact]
    public void CreateAlbum_Should_Fail_When_ArtistId_Zero()
    {
        var validator = new CreateAlbumValidator();
        var request = new CreateAlbumRequest("Test", null, null, 0);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ArtistId);
    }

    [Fact]
    public void UpdateAlbum_Should_Pass_When_Valid()
    {
        var validator = new UpdateAlbumValidator();
        var request = new UpdateAlbumRequest("Updated Title", new DateOnly(2020, 1, 1), null);
        var result = validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateAlbum_Should_Fail_When_Title_Empty()
    {
        var validator = new UpdateAlbumValidator();
        var request = new UpdateAlbumRequest("", null, null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void UpdateAlbum_Should_Fail_When_CoverUrl_Invalid()
    {
        var validator = new UpdateAlbumValidator();
        var request = new UpdateAlbumRequest("Title", null, "bad-url");
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CoverUrl);
    }

    [Fact]
    public void CreateAlbum_Should_Fail_When_Title_Exceeds_MaxLength()
    {
        var validator = new CreateAlbumValidator();
        var request = new CreateAlbumRequest(new string('A', 201), null, null, 1);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreateAlbum_Should_Fail_When_CoverUrl_Exceeds_MaxLength()
    {
        var validator = new CreateAlbumValidator();
        var request = new CreateAlbumRequest("Test", null, "https://" + new string('a', 490) + ".com", 1);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CoverUrl);
    }

    [Fact]
    public void UpdateAlbum_Should_Fail_When_Title_Exceeds_MaxLength()
    {
        var validator = new UpdateAlbumValidator();
        var request = new UpdateAlbumRequest(new string('A', 201), null, null);
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void UpdateAlbum_Should_Fail_When_CoverUrl_Exceeds_MaxLength()
    {
        var validator = new UpdateAlbumValidator();
        var request = new UpdateAlbumRequest("Test", null, "https://" + new string('a', 490) + ".com");
        var result = validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CoverUrl);
    }
}
