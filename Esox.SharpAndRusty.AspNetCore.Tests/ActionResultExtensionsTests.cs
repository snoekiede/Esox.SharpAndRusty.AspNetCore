using Esox.SharpAndRusty.AspNetCore;
using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Esox.SharpAndRusty.AspNetCore.Tests;

public class ActionResultExtensionsTests
{
    #region Result<T, E> ToActionResult Tests

    [Fact]
    public void Result_Ok_ToActionResult_ReturnsOkObjectResult()
    {
        // Arrange
        var result = Result<string, string>.Ok("success");

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("success", okResult.Value);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [Fact]
    public void Result_Err_ToActionResult_ReturnsObjectResultWithDefaultStatusCode()
    {
        // Arrange
        var result = Result<string, string>.Err("error");

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal("error", objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public void Result_Err_ToActionResult_ReturnsObjectResultWithCustomStatusCode()
    {
        // Arrange
        var result = Result<string, string>.Err("error");

        // Act
        var actionResult = result.ToActionResult(StatusCodes.Status500InternalServerError);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal("error", objectResult.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
    }

    [Fact]
    public void Result_WithError_ToActionResult_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.New("Resource not found", ErrorKind.NotFound);
        var result = Result<string, Error>.Err(error);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Not Found", problemDetails.Title);
    }

    #endregion

    #region Result ToCreatedResult Tests

    [Fact]
    public void Result_Ok_ToCreatedResult_ReturnsCreatedResult()
    {
        // Arrange
        var result = Result<string, string>.Ok("created");

        // Act
        var actionResult = result.ToCreatedResult("/api/resource/1");

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal("created", createdResult.Value);
        Assert.Equal("/api/resource/1", createdResult.Location);
    }

    [Fact]
    public void Result_Err_ToCreatedResult_ReturnsObjectResult()
    {
        // Arrange
        var result = Result<string, string>.Err("error");

        // Act
        var actionResult = result.ToCreatedResult("/api/resource/1");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal("error", objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public void Result_Ok_ToCreatedResult_WithLocationSelector_ReturnsCreatedResult()
    {
        // Arrange
        var result = Result<int, Error>.Ok(42);

        // Act
        var actionResult = result.ToCreatedResult(id => $"/api/resource/{id}");

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal(42, createdResult.Value);
        Assert.Equal("/api/resource/42", createdResult.Location);
    }

    #endregion

    #region Result ToNoContentResult Tests

    [Fact]
    public void Result_Ok_ToNoContentResult_ReturnsNoContentResult()
    {
        // Arrange
        var result = Result<Unit, string>.Ok(Unit.Value);

        // Act
        var actionResult = result.ToNoContentResult();

        // Assert
        Assert.IsType<NoContentResult>(actionResult);
    }

    [Fact]
    public void Result_Err_ToNoContentResult_ReturnsObjectResult()
    {
        // Arrange
        var result = Result<Unit, string>.Err("error");

        // Act
        var actionResult = result.ToNoContentResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal("error", objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public void Result_WithError_ToNoContentResult_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.New("Invalid data", ErrorKind.InvalidInput);
        var result = Result<Unit, Error>.Err(error);

        // Act
        var actionResult = result.ToNoContentResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    #endregion

    #region Result ToAcceptedResult Tests

    [Fact]
    public void Result_Ok_ToAcceptedResult_WithLocation_ReturnsAcceptedResult()
    {
        // Arrange
        var result = Result<string, Error>.Ok("processing");

        // Act
        var actionResult = result.ToAcceptedResult("/api/status/123");

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(actionResult);
        Assert.Equal("processing", acceptedResult.Value);
        Assert.Equal("/api/status/123", acceptedResult.Location);
    }

    [Fact]
    public void Result_Ok_ToAcceptedResult_WithoutLocation_ReturnsAcceptedResult()
    {
        // Arrange
        var result = Result<string, Error>.Ok("processing");

        // Act
        var actionResult = result.ToAcceptedResult();

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(actionResult);
        Assert.Equal("processing", acceptedResult.Value);
        Assert.Equal(string.Empty, acceptedResult.Location);
    }

    [Fact]
    public void Result_Err_ToAcceptedResult_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.New("Invalid request", ErrorKind.InvalidInput);
        var result = Result<string, Error>.Err(error);

        // Act
        var actionResult = result.ToAcceptedResult("/api/status/123");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    #endregion

    #region Option<T> ToActionResult Tests

    [Fact]
    public void Option_Some_ToActionResult_ReturnsOkObjectResult()
    {
        // Arrange
        var option = new Option<string>.Some("value");

        // Act
        var actionResult = option.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("value", okResult.Value);
    }

    [Fact]
    public void Option_None_ToActionResult_ReturnsNotFoundResult()
    {
        // Arrange
        var option = new Option<string>.None();

        // Act
        var actionResult = option.ToActionResult();

        // Assert
        Assert.IsType<NotFoundResult>(actionResult);
    }

    [Fact]
    public void Option_Some_ToActionResult_WithMessage_ReturnsOkObjectResult()
    {
        // Arrange
        var option = new Option<string>.Some("value");

        // Act
        var actionResult = option.ToActionResult("Not found message");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("value", okResult.Value);
    }

    [Fact]
    public void Option_None_ToActionResult_WithMessage_ReturnsNotFoundObjectResult()
    {
        // Arrange
        var option = new Option<string>.None();

        // Act
        var actionResult = option.ToActionResult("Resource not found");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
        var value = notFoundResult.Value;
        Assert.NotNull(value);
    }

    [Fact]
    public void Option_Some_ToActionResult_WithFunctions_CallsSomeFunction()
    {
        // Arrange
        var option = new Option<string>.Some("value");

        // Act
        var actionResult = option.ToActionResult(
            value => new OkObjectResult(value.ToUpper()),
            () => new NotFoundResult()
        );

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("VALUE", okResult.Value);
    }

    [Fact]
    public void Option_None_ToActionResult_WithFunctions_CallsNoneFunction()
    {
        // Arrange
        var option = new Option<string>.None();

        // Act
        var actionResult = option.ToActionResult(
            value => new OkObjectResult(value),
            () => new BadRequestResult()
        );

        // Assert
        Assert.IsType<BadRequestResult>(actionResult);
    }

    #endregion

    #region Either<L, R> ToActionResult Tests

    [Fact]
    public void Either_Left_ToActionResult_ReturnsOkObjectResult()
    {
        // Arrange
        var either = new Either<string, string>.Left("success");

        // Act
        var actionResult = either.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("success", okResult.Value);
    }

    [Fact]
    public void Either_Right_ToActionResult_ReturnsObjectResultWithDefaultStatusCode()
    {
        // Arrange
        var either = new Either<string, string>.Right("error");

        // Act
        var actionResult = either.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal("error", objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    [Fact]
    public void Either_Right_ToActionResult_ReturnsObjectResultWithCustomStatusCode()
    {
        // Arrange
        var either = new Either<string, string>.Right("error");

        // Act
        var actionResult = either.ToActionResult(StatusCodes.Status500InternalServerError);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal("error", objectResult.Value);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
    }

    [Fact]
    public void Either_WithError_ToActionResult_ReturnsProblemDetails()
    {
        // Arrange
        var error = Error.New("Operation failed", ErrorKind.InvalidOperation);
        var either = new Either<string, Error>.Right(error);

        // Act
        var actionResult = either.ToActionResult();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    #endregion

    #region Validation<T, E> ToActionResult Tests

    [Fact]
    public void Validation_Valid_ToActionResult_ReturnsOkObjectResult()
    {
        // Arrange
        var validation = Validation<string, string>.Valid("success");

        // Act
        var actionResult = validation.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal("success", okResult.Value);
    }

    [Fact]
    public void Validation_Invalid_ToActionResult_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var validation = Validation<string, string>.Invalid(new[] { "error1", "error2" });

        // Act
        var actionResult = validation.ToActionResult();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        var value = badRequestResult.Value;
        Assert.NotNull(value);
    }

    [Fact]
    public void Validation_WithError_ToActionResult_ReturnsValidationProblemDetails()
    {
        // Arrange
        var error1 = Error.New("Field1 invalid", ErrorKind.InvalidInput);
        var error2 = Error.New("Field2 invalid", ErrorKind.InvalidInput);
        var validation = Validation<string, Error>.Invalid(new[] { error1, error2 });

        // Act
        var actionResult = validation.ToActionResult();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
        Assert.Equal("One or more validation errors occurred.", problemDetails.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Equal(2, problemDetails.Errors.Count);
    }

    [Fact]
    public void Validation_Valid_ToValidationResult_ReturnsNull()
    {
        // Arrange
        var validation = Validation<object, string>.Valid(new object());

        // Act
        var actionResult = validation.ToValidationResult(
            e => "field",
            e => e
        );

        // Assert
        Assert.Null(actionResult);
    }

    [Fact]
    public void Validation_Invalid_ToValidationResult_ReturnsValidationProblemDetails()
    {
        // Arrange
        var validation = Validation<object, string>.Invalid(new[] { "error1", "error2" });

        // Act
        var actionResult = validation.ToValidationResult(
            e => "field",
            e => e
        );

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
        Assert.True(problemDetails.Errors.ContainsKey("field"));
        Assert.Equal(2, problemDetails.Errors["field"].Length);
    }

    [Fact]
    public void Validation_Invalid_ToValidationResult_GroupsByKey()
    {
        // Arrange
        var validation = Validation<object, (string field, string message)>.Invalid(new[]
        {
            ("email", "Invalid email"),
            ("password", "Too short"),
            ("email", "Already exists")
        });

        // Act
        var actionResult = validation.ToValidationResult(
            e => e.field,
            e => e.message
        );

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
        Assert.True(problemDetails.Errors.ContainsKey("email"));
        Assert.True(problemDetails.Errors.ContainsKey("password"));
        Assert.Equal(2, problemDetails.Errors["email"].Length);
        Assert.Single(problemDetails.Errors["password"]);
    }

    #endregion

    #region Complex Type Tests

    [Fact]
    public void Result_WithComplexType_ToActionResult_SerializesCorrectly()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };
        var result = Result<Person, Error>.Ok(person);

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedPerson = Assert.IsType<Person>(okResult.Value);
        Assert.Equal("John", returnedPerson.Name);
        Assert.Equal(30, returnedPerson.Age);
    }

    [Fact]
    public void Option_WithComplexType_ToActionResult_SerializesCorrectly()
    {
        // Arrange
        var person = new Person { Name = "Jane", Age = 25 };
        var option = new Option<Person>.Some(person);

        // Act
        var actionResult = option.ToActionResult();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var returnedPerson = Assert.IsType<Person>(okResult.Value);
        Assert.Equal("Jane", returnedPerson.Name);
        Assert.Equal(25, returnedPerson.Age);
    }

    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    #endregion
}
