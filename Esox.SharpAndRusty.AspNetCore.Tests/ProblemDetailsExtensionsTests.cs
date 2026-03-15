using Esox.SharpAndRusty.AspNetCore;
using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Esox.SharpAndRusty.AspNetCore.Tests;

public class ProblemDetailsExtensionsTests
{
    #region ToProblemDetails Tests

    [Fact]
    public void Error_NotFound_ToProblemDetails_Returns404()
    {
        // Arrange
        var error = Error.New("Resource not found", ErrorKind.NotFound);

        // Act
        var actionResult = error.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Not Found", problemDetails.Title);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Contains("Resource not found", problemDetails.Detail);
    }

    [Fact]
    public void Error_PermissionDenied_ToProblemDetails_Returns403()
    {
        // Arrange
        var error = Error.New("Access denied", ErrorKind.PermissionDenied);

        // Act
        var actionResult = error.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Forbidden", problemDetails.Title);
    }

    [Fact]
    public void Error_InvalidInput_ToProblemDetails_Returns400()
    {
        // Arrange
        var error = Error.New("Invalid data", ErrorKind.InvalidInput);

        // Act
        var actionResult = error.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Bad Request", problemDetails.Title);
    }

    [Fact]
    public void Error_Timeout_ToProblemDetails_Returns408()
    {
        // Arrange
        var error = Error.New("Request timeout", ErrorKind.Timeout);

        // Act
        var actionResult = error.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status408RequestTimeout, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Request Timeout", problemDetails.Title);
    }

    [Fact]
    public void Error_AlreadyExists_ToProblemDetails_Returns409()
    {
        // Arrange
        var error = Error.New("Resource already exists", ErrorKind.AlreadyExists);

        // Act
        var actionResult = error.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Conflict", problemDetails.Title);
    }

    [Fact]
    public void Error_ResourceExhausted_ToProblemDetails_Returns429()
    {
        // Arrange
        var error = Error.New("Too many requests", ErrorKind.ResourceExhausted);

        // Act
        var actionResult = error.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status429TooManyRequests, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Too Many Requests", problemDetails.Title);
    }

    [Fact]
    public void Error_Io_ToProblemDetails_Returns500()
    {
        // Arrange
        var error = Error.New("I/O error", ErrorKind.Io);

        // Act
        var actionResult = error.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Internal Server Error", problemDetails.Title);
    }

    [Fact]
    public void Error_NotSupported_ToProblemDetails_Returns501()
    {
        // Arrange
        var error = Error.New("Not supported", ErrorKind.NotSupported);

        // Act
        var actionResult = error.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status501NotImplemented, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Not Implemented", problemDetails.Title);
    }

    [Fact]
    public void Error_ConnectionRefused_ToProblemDetails_Returns503()
    {
        // Arrange
        var error = Error.New("Service unavailable", ErrorKind.ConnectionRefused);

        // Act
        var actionResult = error.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Service Unavailable", problemDetails.Title);
    }

    #endregion

    #region ToProblemDetailsObject Tests

    [Fact]
    public void Error_ToProblemDetailsObject_IncludesDetailMessage()
    {
        // Arrange
        var error = Error.New("Field validation failed", ErrorKind.InvalidInput);

        // Act
        var problemDetails = error.ToProblemDetailsObject();

        // Assert
        Assert.Contains("Field validation failed", problemDetails.Detail);
    }

    [Fact]
    public void Error_ToProblemDetailsObject_SetsCorrectType()
    {
        // Arrange
        var error = Error.New("Not found", ErrorKind.NotFound);

        // Act
        var problemDetails = error.ToProblemDetailsObject();

        // Assert
        Assert.NotNull(problemDetails.Type);
        Assert.Contains("rfc7231", problemDetails.Type);
    }

    [Fact]
    public void Error_WithContext_ToProblemDetailsObject_IncludesFullMessage()
    {
        // Arrange
        var error = Error.New("Failed to save: Database error", ErrorKind.InvalidOperation);

        // Act
        var problemDetails = error.ToProblemDetailsObject();

        // Assert
        Assert.Contains("Failed to save", problemDetails.Detail);
    }

    #endregion

    #region ErrorKind.ToStatusCode Tests

    [Theory]
    [InlineData(ErrorKind.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorKind.PermissionDenied, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorKind.ConnectionRefused, StatusCodes.Status503ServiceUnavailable)]
    [InlineData(ErrorKind.ConnectionReset, StatusCodes.Status503ServiceUnavailable)]
    [InlineData(ErrorKind.Timeout, StatusCodes.Status408RequestTimeout)]
    [InlineData(ErrorKind.Interrupted, 499)] // Client Closed Request
    [InlineData(ErrorKind.InvalidInput, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorKind.NotSupported, StatusCodes.Status501NotImplemented)]
    [InlineData(ErrorKind.InvalidOperation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorKind.Io, StatusCodes.Status500InternalServerError)]
    [InlineData(ErrorKind.ParseError, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorKind.ResourceExhausted, StatusCodes.Status429TooManyRequests)]
    [InlineData(ErrorKind.AlreadyExists, StatusCodes.Status409Conflict)]
    [InlineData(ErrorKind.InvalidState, StatusCodes.Status400BadRequest)]
    public void ErrorKind_ToStatusCode_ReturnsCorrectStatusCode(ErrorKind kind, int expectedStatusCode)
    {
        // Act
        var statusCode = kind.ToStatusCode();

        // Assert
        Assert.Equal(expectedStatusCode, statusCode);
    }

    #endregion

    #region ErrorKind.ToTitle Tests

    [Theory]
    [InlineData(ErrorKind.NotFound, "Not Found")]
    [InlineData(ErrorKind.PermissionDenied, "Forbidden")]
    [InlineData(ErrorKind.ConnectionRefused, "Service Unavailable")]
    [InlineData(ErrorKind.ConnectionReset, "Service Unavailable")]
    [InlineData(ErrorKind.Timeout, "Request Timeout")]
    [InlineData(ErrorKind.Interrupted, "Request Cancelled")]
    [InlineData(ErrorKind.InvalidInput, "Bad Request")]
    [InlineData(ErrorKind.NotSupported, "Not Implemented")]
    [InlineData(ErrorKind.InvalidOperation, "Bad Request")]
    [InlineData(ErrorKind.Io, "Internal Server Error")]
    [InlineData(ErrorKind.ParseError, "Bad Request")]
    [InlineData(ErrorKind.ResourceExhausted, "Too Many Requests")]
    [InlineData(ErrorKind.AlreadyExists, "Conflict")]
    [InlineData(ErrorKind.InvalidState, "Invalid State")]
    public void ErrorKind_ToTitle_ReturnsCorrectTitle(ErrorKind kind, string expectedTitle)
    {
        // Act
        var title = kind.ToTitle();

        // Assert
        Assert.Equal(expectedTitle, title);
    }

    #endregion

    #region ErrorKind.ToRfc7807Type Tests

    [Fact]
    public void ErrorKind_ToRfc7807Type_ReturnsValidUri()
    {
        // Arrange
        var kind = ErrorKind.NotFound;

        // Act
        var type = kind.ToRfc7807Type();

        // Assert
        Assert.NotNull(type);
        Assert.Contains("rfc7231", type);
    }

    [Fact]
    public void ErrorKind_ToRfc7807Type_IncludesStatusCodeInfo()
    {
        // Arrange
        var kind = ErrorKind.InvalidInput;

        // Act
        var type = kind.ToRfc7807Type();

        // Assert
        Assert.Contains("rfc7231", type);
    }

    #endregion

    #region ExtendedStatusCodes Tests

    [Fact]
    public void ExtendedStatusCodes_Status499ClientClosedRequest_HasCorrectValue()
    {
        // Assert
        Assert.Equal(499, ExtendedStatusCodes.Status499ClientClosedRequest);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Error_WithMetadata_ToProblemDetails_IncludesMetadata()
    {
        // Arrange
        var error = Error.New("Invalid", ErrorKind.InvalidInput)
            .WithMetadata("field", "email")
            .WithMetadata("value", "invalid-email");

        // Act
        var actionResult = error.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.NotNull(problemDetails.Detail);
    }

    [Fact]
    public void Error_WithStackTrace_ToProblemDetails_IncludesStackTrace()
    {
        // Arrange
        var error = Error.New("Operation failed", ErrorKind.InvalidOperation).CaptureStackTrace();

        // Act
        var actionResult = error.ToProblemDetails();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.NotNull(problemDetails);
    }

    #endregion
}
