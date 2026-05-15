using System.Text.Json;
using Esox.SharpAndRusty.AspNetCore;
using Esox.SharpAndRusty.Types;
using Xunit;

namespace Esox.SharpAndRusty.AspNetCore.Tests;

public class JsonConvertersTests
{
    private readonly JsonSerializerOptions _options;

    public JsonConvertersTests()
    {
        _options = new JsonSerializerOptions();
        // Use factories instead of direct converters
        _options.Converters.Add(new OptionJsonConverterFactory());
        _options.Converters.Add(new ResultJsonConverterFactory());
        _options.Converters.Add(new ExtendedResultJsonConverterFactory());
    }

    #region Option<T> Serialization

    [Fact]
    public void Option_Some_SerializesToValue()
    {
        // Arrange
        var option = new Option<string>.Some("hello world");

        // Act
        var json = JsonSerializer.Serialize(option, _options);

        // Assert
        Assert.Equal("\"hello world\"", json);
    }

    [Fact]
    public void Option_None_SerializesToNull()
    {
        // Arrange
        var option = new Option<string>.None();

        // Act
        var json = JsonSerializer.Serialize(option, _options);

        // Assert
        Assert.Equal("null", json);
    }

    [Fact]
    public void Option_Some_Int_SerializesCorrectly()
    {
        // Arrange
        var option = new Option<int>.Some(42);

        // Act
        var json = JsonSerializer.Serialize(option, _options);

        // Assert
        Assert.Equal("42", json);
    }

    [Fact]
    public void Option_None_Int_SerializesToNull()
    {
        // Arrange
        var option = new Option<int>.None();

        // Act
        var json = JsonSerializer.Serialize(option, _options);

        // Assert
        Assert.Equal("null", json);
    }

    [Fact]
    public void Option_Some_Object_SerializesCorrectly()
    {
        // Arrange
        var user = new { Name = "John", Age = 30 };
        var option = new Option<object>.Some(user);

        // Act
        var json = JsonSerializer.Serialize(option, _options);

        // Assert
        Assert.Contains("\"Name\":\"John\"", json);
        Assert.Contains("\"Age\":30", json);
    }

    [Fact]
    public void Option_Deserialize_Null_To_None()
    {
        // Arrange
        var json = "null";

        // Act
        var option = JsonSerializer.Deserialize<Option<string>>(json, _options);

        // Assert
        Assert.IsType<Option<string>.None>(option);
    }

    [Fact]
    public void Option_Deserialize_Value_To_Some()
    {
        // Arrange
        var json = "\"test value\"";

        // Act
        var option = JsonSerializer.Deserialize<Option<string>>(json, _options);

        // Assert
        var some = Assert.IsType<Option<string>.Some>(option);
        Assert.Equal("test value", some.Value);
    }

    #endregion

    #region Result<T, E> Serialization

    [Fact]
    public void Result_Ok_SerializesToValue()
    {
        // Arrange
        var result = Result<string, string>.Ok("success");

        // Act
        var json = JsonSerializer.Serialize(result, _options);

        // Assert
        Assert.Equal("\"success\"", json);
    }

    [Fact]
    public void Result_Err_ThrowsJsonException()
    {
        // Arrange
        var result = Result<string, string>.Err("error");

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Serialize(result, _options));
        Assert.Contains("error", exception.Message);
    }

    [Fact]
    public void Result_Ok_Int_SerializesCorrectly()
    {
        // Arrange
        var result = Result<int, string>.Ok(123);

        // Act
        var json = JsonSerializer.Serialize(result, _options);

        // Assert
        Assert.Equal("123", json);
    }

    [Fact]
    public void Result_Deserialize_Value_To_Ok()
    {
        // Arrange
        var json = "\"deserialized value\"";

        // Act
        var result = JsonSerializer.Deserialize<Result<string, string>>(json, _options);

        // Assert
        Assert.Equal(Result<string, string>.Ok("deserialized value"), result);
    }

    #endregion

    #region ExtendedResult<T, E> Serialization

    [Fact]
    public void ExtendedResult_Ok_SerializesToValue()
    {
        // Arrange
        var result = ExtendedResult<string, string>.Ok("success");

        // Act
        var json = JsonSerializer.Serialize(result, _options);

        // Assert
        Assert.Equal("\"success\"", json);
    }

    [Fact]
    public void ExtendedResult_Err_ThrowsJsonException()
    {
        // Arrange
        var result = ExtendedResult<string, string>.Err("error");

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Serialize(result, _options));
        Assert.Contains("error", exception.Message);
    }

    [Fact]
    public void ExtendedResult_Ok_Object_SerializesCorrectly()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        var result = ExtendedResult<object, string>.Ok(data);

        // Act
        var json = JsonSerializer.Serialize(result, _options);

        // Assert
        Assert.Contains("\"Id\":1", json);
        Assert.Contains("\"Name\":\"Test\"", json);
    }

    [Fact]
    public void ExtendedResult_Deserialize_Value_To_Ok()
    {
        // Arrange
        var json = "\"extended result\"";

        // Act
        var result = JsonSerializer.Deserialize<ExtendedResult<string, string>>(json, _options);

        // Assert
        Assert.Equal(ExtendedResult<string, string>.Ok("extended result"), result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Complex_Object_With_Option_SerializesCorrectly()
    {
        // Arrange
        var obj = new
        {
            Id = 1,
            Name = "Test User",
            Email = new Option<string>.Some("test@example.com"),
            Phone = new Option<string>.None()
        };

        // Act
        var json = JsonSerializer.Serialize(obj, _options);

        // Assert
        Assert.Contains("\"Id\":1", json);
        Assert.Contains("\"Name\":\"Test User\"", json);
        Assert.Contains("\"Email\":\"test@example.com\"", json);
        Assert.Contains("\"Phone\":null", json);
    }

    [Fact]
    public void ApiResponse_With_Result_SerializesCorrectly()
    {
        // Arrange
        var response = new
        {
            Success = true,
            Data = Result<User, string>.Ok(new User { Id = 1, Name = "John Doe" }),
            Timestamp = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(response, _options);

        // Assert
        Assert.Contains("\"Success\":true", json);
        Assert.Contains("\"Data\":{", json);
        Assert.Contains("\"Id\":1", json);
        Assert.Contains("\"Name\":\"John Doe\"", json);
    }

    #endregion
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}