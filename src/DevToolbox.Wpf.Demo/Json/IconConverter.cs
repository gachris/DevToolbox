using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevToolbox.Wpf.Fonts;

namespace DevToolbox.Wpf.Demo.JsonHelpers;

public class IconConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var iconName = reader.GetString();
        if (string.IsNullOrEmpty(iconName))
        {
            return null;
        }

        var field = typeof(Symbol).GetField(iconName);
        if (field != null)
        {
            return (string?)field.GetValue(null);
        }
        else
        {
            throw new JsonException($"Unknown icon name: {iconName}");
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
