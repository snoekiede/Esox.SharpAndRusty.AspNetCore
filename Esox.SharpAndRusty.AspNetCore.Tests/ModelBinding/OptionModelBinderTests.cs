using Esox.SharpAndRusty.AspNetCore.ModelBinding;
using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Esox.SharpAndRusty.AspNetCore.Tests.ModelBinding;

public class OptionModelBinderTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullInnerBinder_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new OptionModelBinder(null!));
    }

    [Fact]
    public void Constructor_WithValidInnerBinder_DoesNotThrow()
    {
        // Arrange
        var innerBinder = new TestModelBinder(null);

        // Act
        var binder = new OptionModelBinder(innerBinder);

        // Assert
        Assert.NotNull(binder);
    }

    #endregion

    #region BindModelAsync Tests

    [Fact]
    public async Task BindModelAsync_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var innerBinder = new TestModelBinder(null);
        var binder = new OptionModelBinder(innerBinder);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => binder.BindModelAsync(null!));
    }

    [Fact]
    public async Task BindModelAsync_WhenInnerBinderSucceeds_ReturnsSome()
    {
        // Arrange
        var innerBinder = new TestModelBinder("test-value");
        var binder = new OptionModelBinder(innerBinder);
        var context = CreateModelBindingContext<Option<string>>();

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<string>.Some>(context.Result.Model);
        Assert.Equal("test-value", option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WhenInnerBinderFails_ReturnsNone()
    {
        // Arrange
        var innerBinder = new TestModelBinder(null, fail: true);
        var binder = new OptionModelBinder(innerBinder);
        var context = CreateModelBindingContext<Option<string>>();

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        Assert.IsType<Option<string>.None>(context.Result.Model);
    }

    [Fact]
    public async Task BindModelAsync_WithIntegerValue_ReturnsSome()
    {
        // Arrange
        var innerBinder = new TestModelBinder(42);
        var binder = new OptionModelBinder(innerBinder);
        var context = CreateModelBindingContext<Option<int>>();

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<int>.Some>(context.Result.Model);
        Assert.Equal(42, option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithNullValue_ReturnsSomeWithNull()
    {
        // Arrange
        var innerBinder = new TestModelBinder(null!);
        var binder = new OptionModelBinder(innerBinder);
        var context = CreateModelBindingContext<Option<string>>();

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<string>.Some>(context.Result.Model);
        Assert.Null(option.Value);
    }

    [Fact]
    public async Task BindModelAsync_WithComplexType_ReturnsSome()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };
        var innerBinder = new TestModelBinder(person);
        var binder = new OptionModelBinder(innerBinder);
        var context = CreateModelBindingContext<Option<Person>>();

        // Act
        await binder.BindModelAsync(context);

        // Assert
        Assert.True(context.Result.IsModelSet);
        var option = Assert.IsType<Option<Person>.Some>(context.Result.Model);
        Assert.Equal("John", option.Value.Name);
        Assert.Equal(30, option.Value.Age);
    }

    #endregion

    #region Helper Methods

    private static ModelBindingContext CreateModelBindingContext<T>()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

        var metadataProvider = new EmptyModelMetadataProvider();
        var modelMetadata = metadataProvider.GetMetadataForType(typeof(T));

        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = actionContext,
            ModelMetadata = modelMetadata,
            ModelName = "test",
            ValueProvider = new SimpleValueProvider(),
            ModelState = new ModelStateDictionary()
        };

        return bindingContext;
    }

    private class TestModelBinder : IModelBinder
    {
        private readonly object? _value;
        private readonly bool _fail;

        public TestModelBinder(object? value, bool fail = false)
        {
            _value = value;
            _fail = fail;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (_fail)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(_value);
            }
            return Task.CompletedTask;
        }
    }

    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    #endregion
}

public class OptionModelBinderProviderTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Act
        var provider = new OptionModelBinderProvider();

        // Assert
        Assert.NotNull(provider);
    }

    #endregion

    #region GetBinder Tests

    [Fact]
    public void GetBinder_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
    }

    [Fact]
    public void GetBinder_WithOptionType_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<string>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithNonOptionType_ReturnsNull()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<string>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.Null(binder);
    }

    [Fact]
    public void GetBinder_WithOptionInt_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<int>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithOptionComplexType_ReturnsOptionModelBinder()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Option<Person>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<OptionModelBinder>(binder);
    }

    [Fact]
    public void GetBinder_WithResult_ReturnsNull()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<Result<string, string>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.Null(binder);
    }

    [Fact]
    public void GetBinder_WithList_ReturnsNull()
    {
        // Arrange
        var provider = new OptionModelBinderProvider();
        var context = CreateProviderContext<List<string>>();

        // Act
        var binder = provider.GetBinder(context);

        // Assert
        Assert.Null(binder);
    }

    #endregion

    #region Helper Methods

    private static ModelBinderProviderContext CreateProviderContext<T>()
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var modelMetadata = metadataProvider.GetMetadataForType(typeof(T));

        return new TestModelBinderProviderContext(modelMetadata, metadataProvider);
    }

    private class TestModelBinderProviderContext : ModelBinderProviderContext
    {
        private readonly ModelMetadata _metadata;
        private readonly IModelMetadataProvider _metadataProvider;

        public TestModelBinderProviderContext(ModelMetadata metadata, IModelMetadataProvider metadataProvider)
        {
            _metadata = metadata;
            _metadataProvider = metadataProvider;
        }

        public override BindingInfo BindingInfo => new BindingInfo();
        public override ModelMetadata Metadata => _metadata;
        public override IModelMetadataProvider MetadataProvider => _metadataProvider;

        public override IModelBinder CreateBinder(ModelMetadata metadata)
        {
            return new TestModelBinder(null);
        }

        private class TestModelBinder : IModelBinder
        {
            public TestModelBinder(object? value) { }

            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }
        }
    }

    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    #endregion
}
