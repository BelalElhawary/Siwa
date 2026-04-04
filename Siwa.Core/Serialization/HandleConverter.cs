using System.Text.Json;
using System.Text.Json.Serialization;
using Siwa.Core.Assets;

namespace Siwa.Core.Serialization;

public class HandleConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Handle<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type elementType = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter)Activator.CreateInstance(
            typeof(HandleConverter<>).MakeGenericType(elementType))!;
    }
}

public class HandleConverter<T> : JsonConverter<Handle<T>> where T : struct
{
    public override Handle<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        long packed = reader.GetInt64();
        // Unpack: Upper 32 bits = Index, Lower 32 bits = Generation
        uint index = (uint)(packed >> 32);
        uint generation = (uint)(packed & 0xFFFFFFFF);
        return new Handle<T>(index, generation);
    }

    public override void Write(Utf8JsonWriter writer, Handle<T> value, JsonSerializerOptions options)
    {
        // Pack: Shift Index left and OR with Generation
        long packed = ((long)value.Index << 32) | value.Generation;
        writer.WriteNumberValue(packed);
    }
}