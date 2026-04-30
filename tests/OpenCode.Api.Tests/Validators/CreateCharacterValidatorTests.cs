using FluentValidation.TestHelper;
using OpenCode.DragonBall.Api.Dtos;
using OpenCode.DragonBall.Api.Validators;

namespace OpenCode.Api.Tests.Validators;

public class CreateCharacterValidatorTests
{
    private readonly CreateCharacterValidator _validator = new();

    [Fact]
    public void Should_Pass_When_All_Fields_Valid()
    {
        var request = new CreateCharacterRequest("Goku", "Saiyan", "60.000.000", null, "Main protagonist", "https://example.com/goku.png", 1);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Name_Empty()
    {
        var request = new CreateCharacterRequest("", "Saiyan", "60.000.000", null, null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_When_Name_Exceeds_MaxLength()
    {
        var request = new CreateCharacterRequest(new string('A', 101), "Saiyan", "60.000.000", null, null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_When_Race_Empty()
    {
        var request = new CreateCharacterRequest("Goku", "", "60.000.000", null, null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Race);
    }

    [Fact]
    public void Should_Fail_When_Ki_Empty()
    {
        var request = new CreateCharacterRequest("Goku", "Saiyan", "", null, null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Ki);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_MaxLength()
    {
        var request = new CreateCharacterRequest("Goku", "Saiyan", "60.000.000", null, new string('A', 2001), null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_PictureUrl_Invalid()
    {
        var request = new CreateCharacterRequest("Goku", "Saiyan", "60.000.000", null, null, "not-a-url", null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PictureUrl);
    }

    [Fact]
    public void Should_Pass_When_PictureUrl_Null()
    {
        var request = new CreateCharacterRequest("Goku", "Saiyan", "60.000.000", null, null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
