using Esox.SharpAndRusty.AspNetCore.Middleware;
using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace Esox.SharpAndRusty.AspNetCore.Tests.Middleware;

public class ResultMiddlewareTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullNext_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = NullLogger<ResultMiddleware>.Instance;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ResultMiddleware(null!, logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ResultMiddleware(next, null!));
    }

    [Fact]
    public void Constructor_WithNullOptions_DoesNotThrow()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var logger = NullLogger<ResultMiddleware>.Instance;

        // Act
        var middleware = new ResultMiddleware(next, logger, null);

        // Assert
        Assert.NotNull(middleware);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var logger = NullLogger<ResultMiddleware>.Instance;
        var options = new ResultMiddlewareOptions();

        // Act
        var middleware = new ResultMiddleware(next, logger, options);

        // Assert
        Assert.NotNull(middleware);
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ => { nextCalled = true; return Task.CompletedTask; };
        var logger = NullLogger<ResultMiddleware>.Instance;
        var middleware = new ResultMiddleware(next, logger);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_HandlesException()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Test error");
        var logger = new TestLogger<ResultMiddleware>();
        var middleware = new ResultMiddleware(next, logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.True(logger.ErrorLogged);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionHandled_WritesProblemDetailsResponse()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Test error");
        var logger = NullLogger<ResultMiddleware>.Instance;
        var middleware = new ResultMiddleware(next, logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        Assert.NotEmpty(responseBody);
        var problemDetails = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(problemDetails);
        Assert.True(problemDetails.ContainsKey("title"));
    }

    [Fact]
    public async Task InvokeAsync_WithWriteIndented_WritesFormattedJson()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Test error");
        var logger = NullLogger<ResultMiddleware>.Instance;
        var options = new ResultMiddlewareOptions { WriteIndented = true };
        var middleware = new ResultMiddleware(next, logger, options);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        Assert.Contains("\n", responseBody); // Indented JSON should have newlines
    }

    [Fact]
    public async Task InvokeAsync_WithHandleExceptionFilter_FiltersExceptions()
    {
        // Arrange
        var exceptionThrown = false;
        RequestDelegate next = _ => { exceptionThrown = true; throw new ArgumentException("Test error"); };
        var logger = NullLogger<ResultMiddleware>.Instance;
        var options = new ResultMiddlewareOptions
        {
            HandleException = ex => ex is InvalidOperationException
        };
        var middleware = new ResultMiddleware(next, logger, options);
        var context = new DefaultHttpContext();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => middleware.InvokeAsync(context));
        Assert.True(exceptionThrown);
    }

    [Fact]
    public async Task InvokeAsync_AddsTraceIdToResponse()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Test error");
        var logger = NullLogger<ResultMiddleware>.Instance;
        var middleware = new ResultMiddleware(next, logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "test-trace-id";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AddsRequestPathToResponse()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Test error");
        var logger = NullLogger<ResultMiddleware>.Instance;
        var middleware = new ResultMiddleware(next, logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("/api/test", responseBody);
    }

    [Fact]
    public async Task InvokeAsync_WithNotSupportedException_Maps501()
    {
        // Arrange
        RequestDelegate next = _ => throw new NotSupportedException("Not supported");
        var logger = NullLogger<ResultMiddleware>.Instance;
        var middleware = new ResultMiddleware(next, logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status501NotImplemented, context.Response.StatusCode);
    }

    #endregion

    #region Helper Classes

    private class TestLogger<T> : ILogger<T>
    {
        public bool ErrorLogged { get; private set; }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Error)
            {
                ErrorLogged = true;
            }
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            public void Dispose() { }
        }
    }

    #endregion
}

public class ResultMiddlewareOptionsTests
{
    #region Property Tests

    [Fact]
    public void IncludeStackTrace_DefaultValue_IsFalse()
    {
        // Arrange
        var options = new ResultMiddlewareOptions();

        // Assert
        Assert.False(options.IncludeStackTrace);
    }

    [Fact]
    public void IncludeFileInfo_DefaultValue_IsFalse()
    {
        // Arrange
        var options = new ResultMiddlewareOptions();

        // Assert
        Assert.False(options.IncludeFileInfo);
    }

    [Fact]
    public void WriteIndented_DefaultValue_IsFalse()
    {
        // Arrange
        var options = new ResultMiddlewareOptions();

        // Assert
        Assert.False(options.WriteIndented);
    }

    [Fact]
    public void HandleException_DefaultValue_ReturnsTrue()
    {
        // Arrange
        var options = new ResultMiddlewareOptions();

        // Assert
        Assert.True(options.HandleException(new Exception()));
    }

    [Fact]
    public void CustomMetadataProvider_DefaultValue_IsNull()
    {
        // Arrange
        var options = new ResultMiddlewareOptions();

        // Assert
        Assert.Null(options.CustomMetadataProvider);
    }

    [Fact]
    public void IncludeStackTrace_CanBeSet()
    {
        // Arrange
        var options = new ResultMiddlewareOptions { IncludeStackTrace = true };

        // Assert
        Assert.True(options.IncludeStackTrace);
    }

    [Fact]
    public void HandleException_CanBeSet()
    {
        // Arrange
        var options = new ResultMiddlewareOptions
        {
            HandleException = ex => ex is InvalidOperationException
        };

        // Assert
        Assert.True(options.HandleException(new InvalidOperationException()));
        Assert.False(options.HandleException(new ArgumentException()));
    }

    #endregion

    #region Development Tests

    [Fact]
    public void Development_ReturnsOptionsForDevelopment()
    {
        // Act
        var options = ResultMiddlewareOptions.Development();

        // Assert
        Assert.True(options.IncludeStackTrace);
        Assert.True(options.IncludeFileInfo);
        Assert.True(options.WriteIndented);
    }

    [Fact]
    public void Development_HandleException_ReturnsTrue()
    {
        // Act
        var options = ResultMiddlewareOptions.Development();

        // Assert
        Assert.True(options.HandleException(new Exception()));
    }

    #endregion

    #region Production Tests

    [Fact]
    public void Production_ReturnsOptionsForProduction()
    {
        // Act
        var options = ResultMiddlewareOptions.Production();

        // Assert
        Assert.False(options.IncludeStackTrace);
        Assert.False(options.IncludeFileInfo);
        Assert.False(options.WriteIndented);
    }

    [Fact]
    public void Production_HasCustomMetadataProvider()
    {
        // Act
        var options = ResultMiddlewareOptions.Production();

        // Assert
        Assert.NotNull(options.CustomMetadataProvider);
    }

    [Fact]
    public void Production_CustomMetadataProvider_AddsRequestId()
    {
        // Arrange
        var options = ResultMiddlewareOptions.Production();
        var context = new DefaultHttpContext { TraceIdentifier = "test-trace" };
        var error = Error.New("Test", ErrorKind.InvalidOperation);

        // Act
        var updatedError = options.CustomMetadataProvider!(context, error);

        // Assert
        Assert.NotNull(updatedError);
    }

    #endregion
}
