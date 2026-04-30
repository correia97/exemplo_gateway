using FluentValidation.TestHelper;
using OpenCode.DragonBall.Api.Dtos;
using OpenCode.DragonBall.Api.Validators;

namespace OpenCode.Api.Tests.Validators;

public class UpdateCharacterValidatorTests
{
    private readonly UpdateCharacterValidator _validator = new();

    [Fact]
    public void Should_Pass_When_All_Fields_Valid()
    {
        var request = new UpdateCharacterRequest("Vegeta", "Saiyan", "80.000.000", null, "Prince of Saiyans", "https://example.com/vegeta.png", 1);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Name_Empty()
    {
        var request = new UpdateCharacterRequest("", "Saiyan", "60.000.000", null, null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_When_PictureUrl_Invalid()
    {
        var request = new UpdateCharacterRequest("Goku", "Saiyan", "60.000.000", null, null, "invalid", null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PictureUrl);
    }

    [Fact]
    public void Should_Pass_When_PictureUrl_Empty()
    {
        var request = new UpdateCharacterRequest("Goku", "Saiyan", "60.000.000", null, null, "", null);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Name_Exceeds_MaxLength()
    {
        var request = new UpdateCharacterRequest(new string('A', 101), "Saiyan", "60.000.000", null, null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_When_Race_Empty()
    {
        var request = new UpdateCharacterRequest("Goku", "", "60.000.000", null, null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Race);
    }

    [Fact]
    public void Should_Fail_When_Race_Exceeds_MaxLength()
    {
        var request = new UpdateCharacterRequest("Goku", new string('A', 51), "60.000.000", null, null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Race);
    }

    [Fact]
    public void Should_Fail_When_Ki_Empty()
    {
        var request = new UpdateCharacterRequest("Goku", "Saiyan", "", null, null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Ki);
    }

    [Fact]
    public void Should_Fail_When_Ki_Exceeds_MaxLength()
    {
        var request = new UpdateCharacterRequest("Goku", "Saiyan", new string('A', 51), null, null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Ki);
    }

    [Fact]
    public void Should_Fail_When_MaxKi_Exceeds_MaxLength()
    {
        var request = new UpdateCharacterRequest("Goku", "Saiyan", "60.000.000", new string('A', 51), null, null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.MaxKi);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_MaxLength()
    {
        var request = new UpdateCharacterRequest("Goku", "Saiyan", "60.000.000", null, new string('A', 2001), null, null);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
