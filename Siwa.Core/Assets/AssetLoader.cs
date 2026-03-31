using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using Siwa.Core.Assets.Kinds;
using Siwa.Core.Data;
using Siwa.Core.Helper;
using File = System.IO.File;

namespace Siwa.Core.Assets;

public static class AssetTypes
{
    public const string LitMaterial = ".lit";
    public const string UnlitMaterial = ".unlit";
    public const string Shader = ".shader";
    public const string Model = ".model";
    public const string Texture = ".texture";
    
}

public sealed class AssetLoader
{
    private readonly JsonSerializerOptions _options;
    
    private AssetLoader(GL gl, Assimp assimp, string rootProject)
    {
        Gl = gl;
        Assimp = assimp;
        _rootProject = rootProject;
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new Vector4Converter());
        _options.Converters.Add(new Vector3Converter());
        _options.Converters.Add(new MaterialHandleConverter());
    }
    
    public static AssetLoader Instance = null!;
    public static void Initialize(GL gl, Assimp assimp, string root)
    {
        Instance = new AssetLoader(gl, assimp, root);
    }
    
    public readonly GL Gl;
    public readonly Assimp Assimp;
    
    private readonly List<Asset> _assets = new();
    
    public List<Asset> GetAssets() => _assets;
    public List<T> GetAllAssets<T>() where T : Asset => _assets.OfType<T>().ToList();
    public T? GetAsset<T>(string assetName) where T : Asset 
        => _assets.OfType<T>().FirstOrDefault(x => x.Name == assetName);
    public T? GetAsset<T>(RawHandle handle) where T : Asset 
        => _assets.OfType<T>().FirstOrDefault(x => x.Handle.Index == handle.Index && x.Handle.Generation == handle.Generation);
    public T? GetAsset<T>(long uid) where T : Asset 
        => GetAsset<T>(RawHandle.FromLong(uid));
    
    private readonly List<string> _extensions = [
        AssetTypes.LitMaterial,
        AssetTypes.UnlitMaterial,
        AssetTypes.Model,
        AssetTypes.Shader,
        AssetTypes.Texture
    ];

    private readonly string _rootProject;
    private string AssetsFolder => Path.Combine(_rootProject, "Assets");

    public void LoadAssetFiles()
    {
        var files = Directory.GetFiles(AssetsFolder, "*.*", SearchOption.AllDirectories)
            .Where(f => _extensions.Any(extension =>
                string.Equals(Path.GetExtension(f), extension, StringComparison.OrdinalIgnoreCase)))
            .Select(f => new FileInfo(f))
            .ToArray();
        ResolveAssetFiles(files);
        Images.Load();
    }

    private void ResolveAssetFiles(FileInfo[] files)
    {
        foreach (var file in files)
        {
            var asset = ResolveAssetFile(file);
            _assets.Add(asset);
        }
        foreach (var asset in _assets)
        {
            asset.OnLoad();
        }
    }

    private Asset ResolveAssetFile(FileInfo file)
    {
        switch (file.Extension)
        {
            case AssetTypes.UnlitMaterial:
                return LoadAssetFile<UnlitMaterialAsset>(file);
            case AssetTypes.LitMaterial:
                return LoadAssetFile<LitMaterialAsset>(file);
            case AssetTypes.Model:
                return LoadAssetFile<ModelAsset>(file);
            case AssetTypes.Shader:
                return LoadAssetFile<ShaderAsset>(file);
            case AssetTypes.Texture:
                return LoadAssetFile<TextureAsset>(file);
            default:
                throw new Exception($"Unknown file extension {file.Extension}");
        }
    }

    private T LoadAssetFile<T>(FileInfo file) where T : Asset
    {
        var deserialized = JsonSerializer.Deserialize<T>(File.ReadAllText(file.FullName), _options);
        if (deserialized is not null)
        {
            deserialized.FilePath = file.FullName;
            deserialized.OnRestore();
            return deserialized;
        }
        throw new Exception($"Failed to deserialize the file {file.Name} at location {file.DirectoryName}.");
    }
}

public class Vector4Converter : JsonConverter<Vector4>
{
    public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Simple logic: read as an object with X, Y, Z, W properties
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        return new Vector4(
            root.GetProperty("X").GetSingle(),
            root.GetProperty("Y").GetSingle(),
            root.GetProperty("Z").GetSingle(),
            root.GetProperty("W").GetSingle()
        );
    }

    public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Z", value.Z);
        writer.WriteNumber("W", value.W);
        writer.WriteEndObject();
    }
}

public class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Simple logic: read as an object with X, Y, Z, W properties
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        return new Vector3(
            root.GetProperty("X").GetSingle(),
            root.GetProperty("Y").GetSingle(),
            root.GetProperty("Z").GetSingle()
        );
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Z", value.Z);
        writer.WriteEndObject();
    }
}

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