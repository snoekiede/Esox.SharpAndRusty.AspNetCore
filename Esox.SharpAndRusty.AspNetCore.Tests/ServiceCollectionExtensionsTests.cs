using Esox.SharpAndRusty.AspNetCore;
using Esox.SharpAndRusty.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Esox.SharpAndRusty.AspNetCore.Tests;

public class ServiceCollectionExtensionsTests
{
    #region AddSharpAndRusty Tests

    [Fact]
    public void AddSharpAndRusty_WithoutConfiguration_AddsServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddSharpAndRusty();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddSharpAndRusty_WithConfiguration_CallsConfigureAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuredCalled = false;

        // Act
        services.AddSharpAndRusty(options =>
        {
            configuredCalled = true;
            options.EnableOptionModelBinding = false;
        });

        // Assert
        Assert.True(configuredCalled);
    }

    [Fact]
    public void AddSharpAndRusty_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddSharpAndRusty();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddSharpAndRusty_CanBeChained()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services
            .AddSharpAndRusty()
            .AddLogging();

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region UseResultMiddleware Tests

    [Fact]
    public void UseResultMiddleware_WithoutOptions_AddsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var appBuilder = new TestApplicationBuilder(services.BuildServiceProvider());

        // Act
        var result = appBuilder.UseResultMiddleware();

        // Assert
        Assert.Same(appBuilder, result);
        Assert.True(appBuilder.MiddlewareAdded);
    }

    [Fact]
    public void UseResultMiddleware_WithOptions_AddsMiddlewareWithOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var appBuilder = new TestApplicationBuilder(services.BuildServiceProvider());
        var options = new ResultMiddlewareOptions { IncludeStackTrace = true };

        // Act
        var result = appBuilder.UseResultMiddleware(options);

        // Assert
        Assert.Same(appBuilder, result);
        Assert.True(appBuilder.MiddlewareAdded);
    }

    [Fact]
    public void UseResultMiddleware_ReturnsSameApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var appBuilder = new TestApplicationBuilder(services.BuildServiceProvider());

        // Act
        var result = appBuilder.UseResultMiddleware();

        // Assert
        Assert.Same(appBuilder, result);
    }

    #endregion

    #region UseResultMiddlewareDevelopment Tests

    [Fact]
    public void UseResultMiddlewareDevelopment_AddsMiddlewareWithDevelopmentOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var appBuilder = new TestApplicationBuilder(services.BuildServiceProvider());

        // Act
        var result = appBuilder.UseResultMiddlewareDevelopment();

        // Assert
        Assert.Same(appBuilder, result);
        Assert.True(appBuilder.MiddlewareAdded);
    }

    [Fact]
    public void UseResultMiddlewareDevelopment_ReturnsSameApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var appBuilder = new TestApplicationBuilder(services.BuildServiceProvider());

        // Act
        var result = appBuilder.UseResultMiddlewareDevelopment();

        // Assert
        Assert.Same(appBuilder, result);
    }

    #endregion

    #region UseResultMiddlewareProduction Tests

    [Fact]
    public void UseResultMiddlewareProduction_AddsMiddlewareWithProductionOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var appBuilder = new TestApplicationBuilder(services.BuildServiceProvider());

        // Act
        var result = appBuilder.UseResultMiddlewareProduction();

        // Assert
        Assert.Same(appBuilder, result);
        Assert.True(appBuilder.MiddlewareAdded);
    }

    [Fact]
    public void UseResultMiddlewareProduction_ReturnsSameApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var appBuilder = new TestApplicationBuilder(services.BuildServiceProvider());

        // Act
        var result = appBuilder.UseResultMiddlewareProduction();

        // Assert
        Assert.Same(appBuilder, result);
    }

    #endregion

    #region Extension Method Chaining Tests

    [Fact]
    public void UseResultMiddleware_Methods_CanBeChained()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var appBuilder = new TestApplicationBuilder(services.BuildServiceProvider());

        // Act
        var result = appBuilder
            .UseResultMiddleware()
            .UseResultMiddlewareDevelopment()
            .UseResultMiddlewareProduction();

        // Assert
        Assert.NotNull(result);
        Assert.True(appBuilder.MiddlewareAdded);
    }

    #endregion

    #region Helper Classes

    private class TestApplicationBuilder : IApplicationBuilder
    {
        private readonly List<Func<RequestDelegate, RequestDelegate>> _components = new();

        public TestApplicationBuilder(IServiceProvider serviceProvider)
        {
            ApplicationServices = serviceProvider;
            Properties = new Dictionary<string, object?>();
            ServerFeatures = new Microsoft.AspNetCore.Http.Features.FeatureCollection();
        }

        public bool MiddlewareAdded => _components.Count > 0;

        public IServiceProvider ApplicationServices { get; set; }
        public IFeatureCollection ServerFeatures { get; }
        public IDictionary<string, object?> Properties { get; }

        public RequestDelegate Build()
        {
            RequestDelegate app = context => Task.CompletedTask;
            foreach (var component in _components.AsEnumerable().Reverse())
            {
                app = component(app);
            }
            return app;
        }

        public IApplicationBuilder New()
        {
            return new TestApplicationBuilder(ApplicationServices);
        }

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            _components.Add(middleware);
            return this;
        }
    }

    #endregion
}

public class SharpAndRustyOptionsTests
{
    #region Property Tests

    [Fact]
    public void EnableOptionModelBinding_DefaultValue_IsTrue()
    {
        // Arrange
        var options = new SharpAndRustyOptions();

        // Assert
        Assert.True(options.EnableOptionModelBinding);
    }

    [Fact]
    public void EnableResultModelBinding_DefaultValue_IsFalse()
    {
        // Arrange
        var options = new SharpAndRustyOptions();

        // Assert
        Assert.False(options.EnableResultModelBinding);
    }

    [Fact]
    public void EnableOptionModelBinding_CanBeSet()
    {
        // Arrange
        var options = new SharpAndRustyOptions { EnableOptionModelBinding = false };

        // Assert
        Assert.False(options.EnableOptionModelBinding);
    }

    [Fact]
    public void EnableResultModelBinding_CanBeSet()
    {
        // Arrange
        var options = new SharpAndRustyOptions { EnableResultModelBinding = true };

        // Assert
        Assert.True(options.EnableResultModelBinding);
    }

    [Fact]
    public void Options_WithBothPropertiesEnabled_MaintainsValues()
    {
        // Arrange
        var options = new SharpAndRustyOptions
        {
            EnableOptionModelBinding = true,
            EnableResultModelBinding = true
        };

        // Assert
        Assert.True(options.EnableOptionModelBinding);
        Assert.True(options.EnableResultModelBinding);
    }

    [Fact]
    public void Options_WithBothPropertiesDisabled_MaintainsValues()
    {
        // Arrange
        var options = new SharpAndRustyOptions
        {
            EnableOptionModelBinding = false,
            EnableResultModelBinding = false
        };

        // Assert
        Assert.False(options.EnableOptionModelBinding);
        Assert.False(options.EnableResultModelBinding);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesInstanceWithDefaultValues()
    {
        // Act
        var options = new SharpAndRustyOptions();

        // Assert
        Assert.NotNull(options);
        Assert.True(options.EnableOptionModelBinding);
        Assert.False(options.EnableResultModelBinding);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Options_PropertiesAreIndependent()
    {
        // Arrange
        var options = new SharpAndRustyOptions();

        // Act
        options.EnableOptionModelBinding = false;

        // Assert
        Assert.False(options.EnableOptionModelBinding);
        Assert.False(options.EnableResultModelBinding); // Should remain unchanged
    }

    [Fact]
    public void Options_CanBeModifiedAfterCreation()
    {
        // Arrange
        var options = new SharpAndRustyOptions();

        // Act
        options.EnableOptionModelBinding = false;
        options.EnableResultModelBinding = true;

        // Assert
        Assert.False(options.EnableOptionModelBinding);
        Assert.True(options.EnableResultModelBinding);
    }

    #endregion
}
