using System;
using System.IO;
using System.Threading.Tasks;
using DevToolbox.Core.Extensions;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DevToolbox.WinUI.Helpers;

/// <summary>
/// Provides extension methods for saving and retrieving settings and files
/// using <see cref="StorageFolder"/>, <see cref="StorageFile"/>, and
/// <see cref="ApplicationDataContainer"/> in a WinUI application.
/// </summary>
public static class SettingsStorageExtensions
{
    private const string FileExtension = ".json";

    /// <summary>
    /// Determines whether roaming storage is available by checking the quota.
    /// </summary>
    /// <param name="appData">
    /// The <see cref="ApplicationData"/> instance to evaluate.
    /// </param>
    /// <returns>
    /// <c>true</c> if roaming storage quota is zero (available); otherwise, <c>false</c>.
    /// </returns>
    public static bool IsRoamingStorageAvailable(this ApplicationData appData)
    {
        return appData.RoamingStorageQuota == 0;
    }

    /// <summary>
    /// Asynchronously saves an object as JSON to the specified folder.
    /// </summary>
    /// <typeparam name="T">The type of the object to save.</typeparam>
    /// <param name="folder">
    /// The <see cref="StorageFolder"/> in which to create or replace the file.
    /// </param>
    /// <param name="name">The base name of the file (without extension).</param>
    /// <param name="content">The object to serialize to JSON.</param>
    /// <returns>A task that completes when the file has been written.</returns>
    public static async Task SaveAsync<T>(this StorageFolder folder, string name, T content) where T : notnull
    {
        var file = await folder.CreateFileAsync(GetFileName(name), CreationCollisionOption.ReplaceExisting);
        var fileContent = await Json.StringifyAsync(content);

        await FileIO.WriteTextAsync(file, fileContent);
    }

    /// <summary>
    /// Asynchronously reads and deserializes a JSON file to an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize.</typeparam>
    /// <param name="folder">
    /// The <see cref="StorageFolder"/> containing the JSON file.
    /// </param>
    /// <param name="name">The base name of the file (without extension).</param>
    /// <returns>
    /// A task that completes with the deserialized object,
    /// or <c>null</c> if the file does not exist or deserialization fails.
    /// </returns>
    public static async Task<T?> ReadAsync<T>(this StorageFolder folder, string name)
    {
        if (!File.Exists(Path.Combine(folder.Path, GetFileName(name))))
        {
            return default;
        }

        var file = await folder.GetFileAsync($"{name}.json");
        var fileContent = await FileIO.ReadTextAsync(file);

        return await Json.ToObjectAsync<T>(fileContent);
    }

    /// <summary>
    /// Asynchronously saves an object as a JSON string in the application settings container.
    /// </summary>
    /// <typeparam name="T">The type of the value to save.</typeparam>
    /// <param name="settings">
    /// The <see cref="ApplicationDataContainer"/> representing the settings.
    /// </param>
    /// <param name="key">The key under which to store the JSON string.</param>
    /// <param name="value">The object to serialize and save.</param>
    /// <returns>A task that completes when the setting has been written.</returns>
    public static async Task SaveAsync<T>(this ApplicationDataContainer settings, string key, T value) where T : notnull
    {
        settings.SaveString(key, await Json.StringifyAsync(value));
    }

    /// <summary>
    /// Saves a raw string value in the application settings container.
    /// </summary>
    /// <param name="settings">
    /// The <see cref="ApplicationDataContainer"/> representing the settings.
    /// </param>
    /// <param name="key">The key under which to store the string.</param>
    /// <param name="value">The string value to save.</param>
    public static void SaveString(this ApplicationDataContainer settings, string key, string value)
    {
        settings.Values[key] = value;
    }

    /// <summary>
    /// Asynchronously reads and deserializes a JSON string from the application settings container.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize.</typeparam>
    /// <param name="settings">
    /// The <see cref="ApplicationDataContainer"/> representing the settings.
    /// </param>
    /// <param name="key">The key under which the JSON string is stored.</param>
    /// <returns>
    /// A task that completes with the deserialized object,
    /// or <c>null</c> if the key does not exist or deserialization fails.
    /// </returns>
    public static async Task<T?> ReadAsync<T>(this ApplicationDataContainer settings, string key)
    {
        if (settings.Values.TryGetValue(key, out var obj))
        {
            return await Json.ToObjectAsync<T>((string)obj);
        }

        return default;
    }

    /// <summary>
    /// Asynchronously saves a byte array to a file in the specified folder.
    /// </summary>
    /// <param name="folder">
    /// The <see cref="StorageFolder"/> in which to create or replace the file.
    /// </param>
    /// <param name="content">The byte array to write to the file.</param>
    /// <param name="fileName">The name of the file, including extension.</param>
    /// <param name="options">
    /// Specifies how to behave if the file already exists.
    /// </param>
    /// <returns>
    /// A task that completes with the <see cref="StorageFile"/> representing the saved file.
    /// </returns>
    public static async Task<StorageFile> SaveFileAsync(this StorageFolder folder, byte[] content, string fileName, CreationCollisionOption options = CreationCollisionOption.ReplaceExisting)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentException("File name is null or empty. Specify a valid file name", nameof(fileName));
        }

        var storageFile = await folder.CreateFileAsync(fileName, options);
        await FileIO.WriteBytesAsync(storageFile, content);
        return storageFile;
    }

    /// <summary>
    /// Asynchronously reads a file from the specified folder as a byte array.
    /// </summary>
    /// <param name="folder">
    /// The <see cref="StorageFolder"/> containing the file.
    /// </param>
    /// <param name="fileName">The name of the file to read.</param>
    /// <returns>
    /// A task that completes with the file's byte content,
    /// or <c>null</c> if the file does not exist.
    /// </returns>
    public static async Task<byte[]?> ReadFileAsync(this StorageFolder folder, string fileName)
    {
        var item = await folder.TryGetItemAsync(fileName).AsTask().ConfigureAwait(false);

        if (item != null && item.IsOfType(StorageItemTypes.File))
        {
            var storageFile = await folder.GetFileAsync(fileName);
            var content = await storageFile.ReadBytesAsync();
            return content;
        }

        return null;
    }

    /// <summary>
    /// Asynchronously reads the contents of a <see cref="StorageFile"/> as a byte array.
    /// </summary>
    /// <param name="file">The <see cref="StorageFile"/> to read.</param>
    /// <returns>
    /// A task that completes with the file's byte content,
    /// or <c>null</c> if the file reference is <c>null</c>.
    /// </returns>
    public static async Task<byte[]?> ReadBytesAsync(this StorageFile file)
    {
        if (file != null)
        {
            using var stream = await file.OpenReadAsync();
            using var reader = new DataReader(stream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)stream.Size);
            var bytes = new byte[stream.Size];
            reader.ReadBytes(bytes);
            return bytes;
        }

        return null;
    }

    /// <summary>
    /// Combines the base file name with the JSON file extension.
    /// </summary>
    /// <param name="name">The base name of the file.</param>
    /// <returns>The file name with the ".json" extension appended.</returns>
    private static string GetFileName(string name)
    {
        return string.Concat(name, FileExtension);
    }
}
