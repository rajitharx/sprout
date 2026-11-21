using Sprout.Api.Models;
using Xunit;

namespace Sprout.Api.Tests;

public class ApiErrorsTests
{
    [Fact]
    public void ValidationError_FromDictionary_CarriesFieldErrors()
    {
        var fields = new Dictionary<string, string[]> { ["label"] = ["Required."] };

        var response = ApiErrors.ValidationError(fields);

        Assert.Equal("VALIDATION_ERROR", response.Error.Code);
        Assert.Equal(["Required."], response.Error.ValidationErrors!["label"]);
    }

    [Fact]
    public void ValidationError_FromSingleField_WrapsMessageUnderThatField()
    {
        var response = ApiErrors.ValidationError("id", "Task ID is required.");

        Assert.Equal("VALIDATION_ERROR", response.Error.Code);
        Assert.Equal(["Task ID is required."], response.Error.ValidationErrors!["id"]);
    }

    [Fact]
    public void NotFound_NamesTheResource()
    {
        var response = ApiErrors.NotFound("Task");

        Assert.Equal("NOT_FOUND", response.Error.Code);
        Assert.Equal("Task not found.", response.Error.Message);
        Assert.Null(response.Error.ValidationErrors);
    }

    [Fact]
    public void ConflictError_UsesGivenMessage()
    {
        var response = ApiErrors.ConflictError("Already exists.");

        Assert.Equal("CONFLICT", response.Error.Code);
        Assert.Equal("Already exists.", response.Error.Message);
    }

    [Fact]
    public void InternalError_DefaultsToGenericMessage()
    {
        var response = ApiErrors.InternalError();

        Assert.Equal("INTERNAL_ERROR", response.Error.Code);
        Assert.Equal("An unexpected error occurred.", response.Error.Message);
    }

    [Fact]
    public void InternalError_UsesCustomMessageWhenGiven()
    {
        Assert.Equal("boom", ApiErrors.InternalError("boom").Error.Message);
    }
}
