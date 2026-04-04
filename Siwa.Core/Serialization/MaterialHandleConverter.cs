using System.Text.Json;
using System.Text.Json.Serialization;
using Siwa.Core.Assets;
using Siwa.Core.Assets.Kinds;

namespace Siwa.Core.Serialization;

public class MaterialHandleConverter : JsonConverter<MaterialHandle>
{
    public override MaterialHandle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Simple logic: read as an object with X, Y, Z, W properties
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        return new MaterialHandle
        {
            Handle = RawHandle.FromLong(root.GetProperty("Handle").GetInt64()),
            Type = (MaterialType)root.GetProperty("Type").GetByte()
        };
    }

    public override void Write(Utf8JsonWriter writer, MaterialHandle value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("Handle", RawHandle.ToLong(value.Handle));
        writer.WriteNumber("Type", (byte)value.Type);
        writer.WriteEndObject();
    }
}