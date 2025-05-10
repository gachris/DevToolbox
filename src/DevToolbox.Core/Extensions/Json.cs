using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DevToolbox.Core.Extensions;

/// <summary>
/// Provides extension methods for asynchronously serializing objects to JSON and deserializing JSON strings to objects.
/// </summary>
public static class Json
{
    /// <summary>
    /// Asynchronously deserializes the JSON string to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize into.</typeparam>
    /// <param name="value">The JSON string to deserialize.</param>
    /// <returns>
    /// A task that completes with the deserialized object of type <typeparamref name="T"/>,
    /// or <c>null</c> if deserialization fails or the input is <c>null</c>.
    /// </returns>
    public static async Task<T?> ToObjectAsync<T>(string value)
    {
        return await Task.Run(() => JsonConvert.DeserializeObject<T>(value));
    }

    /// <summary>
    /// Asynchronously serializes the specified object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize. May be <c>null</c>.</param>
    /// <returns>
    /// A task that completes with the JSON string representation of the object,
    /// or an empty JSON fragment for <c>null</c>.
    /// </returns>
    public static async Task<string> StringifyAsync(object? value)
    {
        return await Task.Run(() => JsonConvert.SerializeObject(value));
    }
}