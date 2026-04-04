using System.Text.Json;
using System.Text.Json.Serialization;

namespace Siwa.Core.Serialization;

public static class SerializationManager
{
    private static bool _initialized;
    public static readonly JsonSerializerOptions Options = new();
    public static void Initialize()
    {
        if (_initialized) return;
        Options.WriteIndented = true;
        Options.IncludeFields = true;
        Options.Converters.Add(new MaterialHandleConverter());
        Options.Converters.Add(new HandleConverterFactory());
        Options.Converters.Add(new RawHandleConverter());
        Options.Converters.Add(new WorldConverter());
        _initialized = true;
    }
    
    public static void RegisterConverter(JsonConverter converter)
        => Options.Converters.Add(converter);
}