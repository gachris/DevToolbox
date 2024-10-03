using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevToolbox.Wpf.Demo.JsonHelpers;

public class XamlConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var path = reader.GetString();
        ArgumentNullException.ThrowIfNull(path, nameof(path));
        
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path.TrimStart('\\', '/'));

        // Read XAML content from file
        string xamlContent = File.ReadAllText(fullPath);

        return xamlContent;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
