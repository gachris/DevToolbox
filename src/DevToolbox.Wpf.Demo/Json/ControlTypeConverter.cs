using System.Text.Json;
using System.Text.Json.Serialization;
using DevToolbox.Wpf.Demo.ViewModels;

namespace DevToolbox.Wpf.Demo.Json;

public class ControlTypeConverter : JsonConverter<ControlType>
{
    public override ControlType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            throw new JsonException("Cannot convert null value to ControlType.");
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token type '{reader.TokenType}' for ControlType.");
        }

        var controlTypeStr = reader.GetString();
        if (Enum.TryParse(controlTypeStr, out ControlType controlType))
        {
            return controlType;
        }

        throw new JsonException($"Invalid ControlType value: {controlTypeStr}");
    }

    public override void Write(Utf8JsonWriter writer, ControlType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
