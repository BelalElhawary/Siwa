using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Siwa.Core.Assets;

public sealed class AssetPool<T> where T : struct
{
    public static readonly AssetPool<T> Registry = new();
        
    private T[] _assets = new T[16];
    private uint[] _generations = new uint[16];
    private readonly Stack<uint> _freeIndices = new();
    private uint _nextIndex;
        
    private void EnsureCapacity()
    {
        var newSize = _assets.Length * 2;
        Array.Resize(ref _assets, newSize);
        Array.Resize(ref _generations, newSize);
    }
        
    public void Unload(Handle<T> handle)
    {
        if (_generations[handle.Index] != handle.Generation) return;
        _generations[handle.Index]++; 
        _assets[handle.Index] = default;
        _freeIndices.Push(handle.Index);
    }
        
    public Handle<T> Register(T asset)
    {
        var index = _freeIndices.Count > 0 ? _freeIndices.Pop() : _nextIndex++;
        if (index >= _assets.Length) EnsureCapacity();
        _assets[index] = asset;
        return new Handle<T>(index, _generations[index]);
    }
    
    public Handle<T> Reserve()
    {
        var index = _freeIndices.Count > 0 ? _freeIndices.Pop() : _nextIndex++;
        if (index >= _assets.Length) EnsureCapacity();
        _assets[index] = new T();
        return new Handle<T>(index, _generations[index]);
    }
    
    public void Restore(Handle<T> handle)
    {
        while (handle.Index >= _assets.Length) EnsureCapacity();
    
        _assets[handle.Index] = new T();
        _generations[handle.Index] = handle.Generation;
        if (handle.Index >= _nextIndex) _nextIndex = handle.Index + 1;
    }
        
    public ref T Get(Handle<T> handle)
    {
        Debug.Assert(_generations[handle.Index] == handle.Generation);
        return ref _assets[handle.Index];
    }
}

[JsonConverter(typeof(HandleConverterFactory))]
public readonly struct Handle<T>(uint index, uint generation)
    where T : struct
{
    [JsonInclude] public uint Index { get; init; } = index;
    [JsonInclude] public uint Generation { get; init; } = generation;
    
    public static Handle<T> FromLong(long uid)
    {
        uint index = (uint)(uid >> 32);
        uint generation = (uint)(uid & 0xFFFFFFFF);
        return new Handle<T>(index, generation);
    }

    public RawHandle ToRaw() => new(Index, Generation);
    public static long ToLong(Handle<T> handle)
    {
        return ((long)handle.Index << 32) | handle.Generation;
    }
}

[JsonConverter(typeof(RawHandleConverter))]
public readonly struct RawHandle(uint index, uint generation)
{
    [JsonInclude] public uint Index { get; init; } = index;
    [JsonInclude] public uint Generation { get; init; } = generation;
    public Handle<T> ToHandle<T>() where T : struct => new(Index, Generation);

    public static RawHandle FromLong(long uid)
    {
        uint index = (uint)(uid >> 32);
        uint generation = (uint)(uid & 0xFFFFFFFF);
        return new RawHandle(index, generation);
    }
    
    public static long ToLong(RawHandle handle)
    {
        return ((long)handle.Index << 32) | handle.Generation;
    }
}

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