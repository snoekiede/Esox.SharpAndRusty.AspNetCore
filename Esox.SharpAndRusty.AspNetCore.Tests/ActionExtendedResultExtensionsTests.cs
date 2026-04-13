using Esox.SharpAndRusty.AspNetCore;
using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Esox.SharpAndRusty.AspNetCore.Tests;

public class ActionExtendedResultExtensionsTests
{
    #region ExtendedResult<T, E> ToActionResult Tests

    [Fact]
    public void ExtendedResult_Ok_ToActionResult_ReturnsOkObjectResult()
    {
        // Arrange
        var result = ExtendedResult<string, string>.Ok("success");

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("success", okResult.Value);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [Fact]
    public void ExtendedResult_Err_ToActionResult_ReturnsObjectResultWithDefaultStatusCode()
    {
        // Arrange
        var result = ExtendedResult<string, string>.Err("error");

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal("error", objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public void ExtendedResult_Err_ToActionResult_ReturnsObjectResultWithCustomStatusCode()
    {
        // Arrange
        var result = ExtendedResult<string, string>.Err("error");

        // Act
        var actionResult = result.ToActionResult(StatusCodes.Status500InternalServerError);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal("error", objectResult.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
    }

    [Fact]
    public void ExtendedResult_WithError_ToActionResult_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.New("Resource not found", ErrorKind.NotFound);
        var result = ExtendedResult<string, Error>.Err(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Not Found", problemDetails.Title);
    }

    [Fact]
    public void ExtendedResult_WithComplexType_ToActionResult_SerializesCorrectly()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        var result = ExtendedResult<object, string>.Ok(data);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(data, okResult.Value);
    }

    #endregion

    #region ExtendedResult ToCreatedResult Tests

    [Fact]
    public void ExtendedResult_Ok_ToCreatedResult_ReturnsCreatedResult()
    {
        // Arrange
        var result = ExtendedResult<string, string>.Ok("created");

        // Act
        var actionResult = result.ToCreatedResult("/api/resource/1");

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal("created", createdResult.Value);
        Assert.Equal("/api/resource/1", createdResult.Location);
    }

    [Fact]
    public void ExtendedResult_Err_ToCreatedResult_ReturnsObjectResult()
    {
        // Arrange
        var result = ExtendedResult<string, string>.Err("error");

        // Act
        var actionResult = result.ToCreatedResult("/api/resource/1");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal("error", objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public void ExtendedResult_Ok_ToCreatedResult_WithLocationSelector_ReturnsCreatedResult()
    {
        // Arrange
        var user = new { Id = 42, Name = "John" };
        var result = ExtendedResult<object, Error>.Ok(user);

        // Act
        var actionResult = result.ToCreatedResult(u => $"/api/users/{((dynamic)u).Id}");

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal(user, createdResult.Value);
        Assert.Equal("/api/users/42", createdResult.Location);
    }

    [Fact]
    public void ExtendedResult_Err_ToCreatedResult_WithLocationSelector_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.New("Validation failed", ErrorKind.InvalidInput);
        var result = ExtendedResult<object, Error>.Err(error);

        // Act
        var actionResult = result.ToCreatedResult(u => $"/api/users/{((dynamic)u).Id}");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    #endregion

    #region ExtendedResult ToNoContentResult Tests

    [Fact]
    public void ExtendedResult_Ok_ToNoContentResult_ReturnsNoContentResult()
    {
        // Arrange
        var result = ExtendedResult<Unit, string>.Ok(Unit.Value);

        // Act
        var actionResult = result.ToNoContentResult();

        // Assert
        Assert.IsType<NoContentResult>(actionResult);
    }

    [Fact]
    public void ExtendedResult_Err_ToNoContentResult_ReturnsObjectResult()
    {
        // Arrange
        var result = ExtendedResult<Unit, string>.Err("error");

        // Act
        var actionResult = result.ToNoContentResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal("error", objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public void ExtendedResult_WithError_ToNoContentResult_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.New("Operation failed", ErrorKind.InvalidOperation);
        var result = ExtendedResult<Unit, Error>.Err(error);

        // Act
        var actionResult = result.ToNoContentResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Bad Request", problemDetails.Title);
    }

    [Fact]
    public void ExtendedResult_Err_ToNoContentResult_WithCustomStatusCode_ReturnsObjectResultWithCustomStatusCode()
    {
        // Arrange
        var result = ExtendedResult<Unit, string>.Err("error");

        // Act
        var actionResult = result.ToNoContentResult(StatusCodes.Status422UnprocessableEntity);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);
    }

    #endregion

    #region ExtendedResult ToAcceptedResult Tests

    [Fact]
    public void ExtendedResult_Ok_ToAcceptedResult_WithLocation_ReturnsAcceptedResult()
    {
        // Arrange
        var result = ExtendedResult<string, Error>.Ok("accepted");

        // Act
        var actionResult = result.ToAcceptedResult("/api/jobs/123");

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(actionResult);
        Assert.Equal("accepted", acceptedResult.Value);
        Assert.Equal("/api/jobs/123", acceptedResult.Location);
    }

    [Fact]
    public void ExtendedResult_Ok_ToAcceptedResult_WithoutLocation_ReturnsAcceptedResult()
    {
        // Arrange
        var result = ExtendedResult<string, Error>.Ok("accepted");

        // Act
        var actionResult = result.ToAcceptedResult();

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(actionResult);
        Assert.Equal("accepted", acceptedResult.Value);
        Assert.Equal(string.Empty, acceptedResult.Location);
    }

    [Fact]
    public void ExtendedResult_Err_ToAcceptedResult_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.New("Job creation failed", ErrorKind.InvalidOperation);
        var result = ExtendedResult<string, Error>.Err(error);

        // Act
        var actionResult = result.ToAcceptedResult("/api/jobs/123");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    #endregion

    #region ExtendedResult ToActionResult with Custom Handlers Tests

    [Fact]
    public void ExtendedResult_Ok_ToActionResult_WithCustomHandlers_CallsOnSuccess()
    {
        // Arrange
        var result = ExtendedResult<string, string>.Ok("success");
        var wasCalled = false;

        // Act
        var actionResult = result.ToActionResult(
            onSuccess: value =>
            {
                wasCalled = true;
                return new OkObjectResult(new { data = value, custom = true });
            },
            onFailure: error => new BadRequestObjectResult(error)
        );

        // Assert
        Assert.True(wasCalled);
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        dynamic? resultValue = okResult.Value;
        Assert.NotNull(resultValue);
        Assert.Equal("success", resultValue!.data);
        Assert.True(resultValue!.custom);
    }

    [Fact]
    public void ExtendedResult_Err_ToActionResult_WithCustomHandlers_CallsOnFailure()
    {
        // Arrange
        var result = ExtendedResult<string, string>.Err("error");
        var wasCalled = false;

        // Act
        var actionResult = result.ToActionResult(
            onSuccess: value => new OkObjectResult(value),
            onFailure: error =>
            {
                wasCalled = true;
                return new BadRequestObjectResult(new { error, custom = true });
            }
        );

        // Assert
        Assert.True(wasCalled);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        dynamic? resultValue = badRequestResult.Value;
        Assert.NotNull(resultValue);
        Assert.Equal("error", resultValue!.error);
        Assert.True(resultValue!.custom);
    }

    [Fact]
    public void ExtendedResult_ToActionResult_WithCustomHandlers_SupportsComplexTransformations()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        var result = ExtendedResult<object, Error>.Ok(data);

        // Act
        var actionResult = result.ToActionResult(
            onSuccess: value => new CreatedResult("/api/test/1", value),
            onFailure: error => error.ToProblemDetails()
        );

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal(data, createdResult.Value);
        Assert.Equal("/api/test/1", createdResult.Location);
    }

    #endregion

    #region ExtendedResult Error Kind Mapping Tests

    [Theory]
    [InlineData(ErrorKind.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorKind.PermissionDenied, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorKind.InvalidInput, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorKind.Timeout, StatusCodes.Status408RequestTimeout)]
    [InlineData(ErrorKind.AlreadyExists, StatusCodes.Status409Conflict)]
    public void ExtendedResult_WithError_ToActionResult_MapsErrorKindToCorrectStatusCode(
        ErrorKind errorKind,
        int expectedStatusCode)
    {
        // Arrange
        var error = Error.New("Test error", errorKind);
        var result = ExtendedResult<string, Error>.Err(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(expectedStatusCode, objectResult.StatusCode);
    }

    #endregion

    #region ExtendedResult Edge Cases and Validation Tests

    [Fact]
    public void ExtendedResult_WithNullValue_ToActionResult_HandlesNull()
    {
        // Arrange
        var result = ExtendedResult<string?, string>.Ok(null);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Null(okResult.Value);
    }

    [Fact]
    public void ExtendedResult_WithEmptyString_ToActionResult_HandlesEmptyString()
    {
        // Arrange
        var result = ExtendedResult<string, string>.Ok(string.Empty);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(string.Empty, okResult.Value);
    }

    [Fact]
    public void ExtendedResult_ToCreatedResult_WithEmptyLocation_ReturnsCreatedResultWithEmptyLocation()
    {
        // Arrange
        var result = ExtendedResult<string, string>.Ok("created");

        // Act
        var actionResult = result.ToCreatedResult(string.Empty);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal(string.Empty, createdResult.Location);
    }

    [Fact]
    public void ExtendedResult_WithNestedError_ToActionResult_ReturnsCorrectProblemDetails()
    {
        // Arrange
        var error = Error.New("Parent error", ErrorKind.InvalidOperation)
            .WithContext("Additional context");
        var result = ExtendedResult<string, Error>.Err(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Parent error", problemDetails.Detail);
    }

    #endregion

    #region ExtendedResult with Collections Tests

    [Fact]
    public void ExtendedResult_WithList_ToActionResult_ReturnsOkWithList()
    {
        // Arrange
        var list = new List<string> { "item1", "item2", "item3" };
        var result = ExtendedResult<List<string>, Error>.Ok(list);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedList = Assert.IsType<List<string>>(okResult.Value);
        Assert.Equal(3, returnedList.Count);
        Assert.Equal("item1", returnedList[0]);
    }

    [Fact]
    public void ExtendedResult_WithEmptyList_ToActionResult_ReturnsOkWithEmptyList()
    {
        // Arrange
        var list = new List<string>();
        var result = ExtendedResult<List<string>, Error>.Ok(list);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedList = Assert.IsType<List<string>>(okResult.Value);
        Assert.Empty(returnedList);
    }

    #endregion

    #region ExtendedResult Integration Tests

    [Fact]
    public void ExtendedResult_CombinedWithErrorChaining_ToActionResult_WorksCorrectly()
    {
        // Arrange
        var error1 = Error.New("First error", ErrorKind.InvalidInput);
        var error2 = error1.WithContext("Second error context");
        var result = ExtendedResult<string, Error>.Err(error2);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public void ExtendedResult_MultipleConversions_MaintainCorrectBehavior()
    {
        // Arrange
        var result = ExtendedResult<int, Error>.Ok(42);

        // Act
        var actionResult1 = result.ToActionResult();
        var actionResult2 = result.ToCreatedResult(x => $"/api/items/{x}");
        var actionResult3 = result.ToAcceptedResult($"/api/jobs/42");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult1);
        Assert.Equal(42, okResult.Value);

        var createdResult = Assert.IsType<CreatedResult>(actionResult2);
        Assert.Equal(42, createdResult.Value);
        Assert.Equal("/api/items/42", createdResult.Location);

        var acceptedResult = Assert.IsType<AcceptedResult>(actionResult3);
        Assert.Equal(42, acceptedResult.Value);
        Assert.Equal("/api/jobs/42", acceptedResult.Location);
    }

    #endregion

    #region ExtendedResult Type Inference Tests

    [Fact]
    public void ExtendedResult_GenericTypeInference_WorksWithToActionResult()
    {
        // Arrange
        var result = CreateExtendedResult(42);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(42, okResult.Value);
    }

    [Fact]
    public void ExtendedResult_GenericTypeInference_WorksWithToCreatedResult()
    {
        // Arrange
        var result = CreateExtendedResult("created");

        // Act
        var actionResult = result.ToCreatedResult(x => $"/api/{x}");

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal("created", createdResult.Value);
    }

    private static ExtendedResult<T, Error> CreateExtendedResult<T>(T value)
        => ExtendedResult<T, Error>.Ok(value);

    #endregion
}
