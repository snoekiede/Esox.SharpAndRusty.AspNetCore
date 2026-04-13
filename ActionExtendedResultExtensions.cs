using Esox.SharpAndRusty.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Esox.SharpAndRusty.AspNetCore;

/// <summary>
/// Provides extension methods for converting ExtendedResult types to ASP.NET Core action results.
/// </summary>
public static class ActionExtendedResultExtensions
{
    #region ExtendedResult to ActionResult

    /// <summary>
    /// Converts an ExtendedResult to an IActionResult, mapping Ok to 200 OK and Err to a problem details response.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="result">The extended result to convert.</param>
    /// <param name="statusCode">The status code to use for errors (default: 400 Bad Request).</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToActionResult<T, E>(
        this ExtendedResult<T, E> result,
        int statusCode = StatusCodes.Status400BadRequest)
    {
        return result.Match<IActionResult>(
            success: value => new OkObjectResult(value),
            failure: error => new ObjectResult(error) { StatusCode = statusCode }
        );
    }

    /// <summary>
    /// Converts an ExtendedResult with Error type to an IActionResult with automatic status code mapping.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The extended result to convert.</param>
    /// <returns>An IActionResult with appropriate status code based on ErrorKind.</returns>
    public static IActionResult ToActionResult<T>(this ExtendedResult<T, Error> result)
    {
        return result.Match<IActionResult>(
            success: value => new OkObjectResult(value),
            failure: error => error.ToProblemDetails()
        );
    }

    /// <summary>
    /// Converts an ExtendedResult to a CreatedResult (201) on success, or problem details on error.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="result">The extended result to convert.</param>
    /// <param name="location">The URI of the created resource.</param>
    /// <param name="errorStatusCode">The status code to use for errors (default: 400).</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToCreatedResult<T, E>(
        this ExtendedResult<T, E> result,
        string location,
        int errorStatusCode = StatusCodes.Status400BadRequest)
    {
        return result.Match<IActionResult>(
            success: value => new CreatedResult(location, value),
            failure: error => new ObjectResult(error) { StatusCode = errorStatusCode }
        );
    }

    /// <summary>
    /// Converts an ExtendedResult to a CreatedResult with automatic location generation using a function.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The extended result to convert.</param>
    /// <param name="locationSelector">Function to generate the location URI from the success value.</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToCreatedResult<T>(
        this ExtendedResult<T, Error> result,
        Func<T, string> locationSelector)
    {
        return result.Match<IActionResult>(
            success: value => new CreatedResult(locationSelector(value), value),
            failure: error => error.ToProblemDetails()
        );
    }

    /// <summary>
    /// Converts an ExtendedResult to a NoContentResult (204) on success, or problem details on error.
    /// </summary>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="result">The extended result to convert.</param>
    /// <param name="errorStatusCode">The status code to use for errors (default: 400).</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToNoContentResult<E>(
        this ExtendedResult<Unit, E> result,
        int errorStatusCode = StatusCodes.Status400BadRequest)
    {
        return result.Match<IActionResult>(
            success: _ => new NoContentResult(),
            failure: error => new ObjectResult(error) { StatusCode = errorStatusCode }
        );
    }

    /// <summary>
    /// Converts an ExtendedResult with Unit success type to a NoContentResult with automatic error handling.
    /// </summary>
    /// <param name="result">The extended result to convert.</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToNoContentResult(this ExtendedResult<Unit, Error> result)
    {
        return result.Match<IActionResult>(
            success: _ => new NoContentResult(),
            failure: error => error.ToProblemDetails()
        );
    }

    /// <summary>
    /// Converts an ExtendedResult to an AcceptedResult (202) on success, or problem details on error.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The extended result to convert.</param>
    /// <param name="location">The URI where the status of the operation can be monitored.</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToAcceptedResult<T>(
        this ExtendedResult<T, Error> result,
        string? location = null)
    {
        return result.Match<IActionResult>(
            success: value => location != null
                ? new AcceptedResult(location, value)
                : new AcceptedResult(string.Empty, value),
            failure: error => error.ToProblemDetails()
        );
    }

    /// <summary>
    /// Converts an ExtendedResult to an IActionResult with custom success and failure handlers.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E">The error type.</typeparam>
    /// <param name="result">The extended result to convert.</param>
    /// <param name="onSuccess">Function to create result for success case.</param>
    /// <param name="onFailure">Function to create result for failure case.</param>
    /// <returns>An IActionResult representing the result.</returns>
    public static IActionResult ToActionResult<T, E>(
        this ExtendedResult<T, E> result,
        Func<T, IActionResult> onSuccess,
        Func<E, IActionResult> onFailure)
    {
        return result.Match(
            success: onSuccess,
            failure: onFailure
        );
    }

    #endregion
}
