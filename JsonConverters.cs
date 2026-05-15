using System.Text.Json;
using System.Text.Json.Serialization;
using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Mvc;

namespace Esox.SharpAndRusty.AspNetCore;

/// <summary>
/// Factory for creating Option{T} JSON converters.
/// </summary>
public class OptionJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsGenericType)
        {
            var genericType = typeToConvert.GetGenericTypeDefinition();
            // Handle Option<T>.Some and Option<T>.None
            if (genericType == typeof(Option<>.Some) || genericType == typeof(Option<>.None))
            {
                return true;
            }
        }
        return false;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var elementType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionJsonConverter<>).MakeGenericType(elementType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// <summary>
/// Factory for creating Result{T, E} JSON converters.
/// </summary>
public class ResultJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        return typeToConvert.GetGenericTypeDefinition() == typeof(Result<,>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArgs = typeToConvert.GetGenericArguments();
        var converterType = typeof(ResultJsonConverter<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// <summary>
/// Factory for creating ExtendedResult{T, E} JSON converters.
/// </summary>
public class ExtendedResultJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        return typeToConvert.GetGenericTypeDefinition() == typeof(ExtendedResult<,>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArgs = typeToConvert.GetGenericArguments();
        var converterType = typeof(ExtendedResultJsonConverter<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

/// <summary>
/// JSON converter for Option<T> types.
/// Serializes Some(value) as the value itself, None as null.
/// </summary>
public class OptionJsonConverter<T> : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        var genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(Option<>.Some) || genericType == typeof(Option<>.None);
    }

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // For deserialization, null -> None, value -> Some(value)
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new Option<T>.None();
        }

        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new Option<T>.Some(value!);
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        // Use reflection to check the type and get the value
        var valueType = value.GetType();
        if (valueType.IsGenericType)
        {
            var genericType = valueType.GetGenericTypeDefinition();
            if (genericType == typeof(Option<>.Some))
            {
                // Get the Value property
                var valueProperty = valueType.GetProperty("Value");
                var innerValue = valueProperty?.GetValue(value);
                JsonSerializer.Serialize(writer, innerValue, options);
                return;
            }
            else if (genericType == typeof(Option<>.None))
            {
                writer.WriteNullValue();
                return;
            }
        }

        throw new JsonException("Value is not a valid Option type.");
    }
}

/// <summary>
/// JSON converter for Result<T, E> types.
/// Serializes Ok(value) as the value itself, Err(error) throws an exception.
/// </summary>
public class ResultJsonConverter<T, E> : JsonConverter<Result<T, E>>
{
    public override Result<T, E> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // For deserialization, assume it's always an Ok value
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return Result<T, E>.Ok(value!);
    }

    public override void Write(Utf8JsonWriter writer, Result<T, E> value, JsonSerializerOptions options)
    {
        value.Match<IActionResult>(
            success: val => 
            {
                JsonSerializer.Serialize(writer, val, options);
                return null!;
            },
            failure: error => throw new JsonException($"Cannot serialize Result in error state: {error}")
        );
    }
}

/// <summary>
/// JSON converter for ExtendedResult<T, E> types.
/// Serializes Ok(value) as the value itself, Err(error) throws an exception.
/// </summary>
public class ExtendedResultJsonConverter<T, E> : JsonConverter<ExtendedResult<T, E>>
{
    public override ExtendedResult<T, E> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // For deserialization, assume it's always an Ok value
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return ExtendedResult<T, E>.Ok(value!);
    }

    public override void Write(Utf8JsonWriter writer, ExtendedResult<T, E> value, JsonSerializerOptions options)
    {
        value.Match<IActionResult>(
            success: val => 
            {
                JsonSerializer.Serialize(writer, val, options);
                return null!;
            },
            failure: error => throw new JsonException($"Cannot serialize ExtendedResult in error state: {error}")
        );
    }
}
