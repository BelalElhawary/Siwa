using System.Text.Json;
using System.Text.Json.Serialization;
using Siwa.Core.Assets;

namespace Siwa.Core.Serialization;

public class RawHandleConverter : JsonConverter<RawHandle>
{
    public override RawHandle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        long packed = reader.GetInt64();
        // Unpack: Upper 32 bits = Index, Lower 32 bits = Generation
        uint index = (uint)(packed >> 32);
        uint generation = (uint)(packed & 0xFFFFFFFF);
        return new RawHandle(index, generation);
    }

    public override void Write(Utf8JsonWriter writer, RawHandle value, JsonSerializerOptions options)
    {
        // Pack: Shift Index left and OR with Generation
        long packed = ((long)value.Index << 32) | value.Generation;
        writer.WriteNumberValue(packed);
    }
}