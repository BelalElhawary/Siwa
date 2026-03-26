using System.Runtime.CompilerServices;
using System.Text.Json;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using Siwa.Core.Assets;
using File = System.IO.File;

namespace Siwa.Core.Helper;

public static class AssetTypes
{
    public const string Material = ".material";
    public const string Obj = ".obj";
    public const string Model = ".model";
}

public sealed class AssetLoader
{
    private AssetLoader(GL gl, Assimp assimp)
    {
        Gl = gl;
        Assimp = assimp;
    }
    
    public static AssetLoader Instance = null!;
    public static void Initialize(GL gl, Assimp assimp)
    {
        Instance = new AssetLoader(gl, assimp);
    }
    
    public readonly GL Gl;
    public readonly Assimp Assimp;
    
    public readonly Dictionary<Guid, RawHandle> AssetGuidLookupDictionary = new();
    public readonly List<Asset> Assets = new();
    
    private readonly string[] _extensions = [
        AssetTypes.Material,
        AssetTypes.Model
    ];
    
    private const string AssetsFolder = "Assets";
    private static readonly string AssetsPath = Path.Combine(AppContext.BaseDirectory,  AssetsFolder);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Handle<T> GetAssetHandle<T>(string guid) where T : Asset
    {
        return AssetGuidLookupDictionary[new Guid(guid)].ToHandle<T>();
    }
    
    public void LoadAssetFiles()
    {
        var files = Directory.GetFiles(AssetsPath, "*.*", SearchOption.AllDirectories)
            .Where(f => _extensions.Any(extension =>
                string.Equals(Path.GetExtension(f), extension, StringComparison.OrdinalIgnoreCase)))
            .Select(f => new FileInfo(f))
            .ToArray();
        ResolveAssetFiles(files);
    }

    private void ResolveAssetFiles(FileInfo[] files)
    {
        Assets.AddRange(files.Select(ResolveAssetFile));
        foreach (var asset in Assets)
        {
            asset.OnLoad();
        }
    }

    private Asset ResolveAssetFile(FileInfo file)
    {
        switch (file.Extension)
        {
            case AssetTypes.Material:
                return LoadAssetFile<MaterialAsset>(file);
            case AssetTypes.Model:
                return LoadAssetFile<ModelAsset>(file);
            default:
                throw new Exception($"Unknown file extension {file.Extension}");
                break;
        }
    }

    private T LoadAssetFile<T>(FileInfo file) where T : Asset
    {
        var deserialized = JsonSerializer.Deserialize<T>(File.ReadAllText(file.FullName));
        if (deserialized is not null)
        {
            deserialized.Handle = AssetPool<T>.Registry.Register(deserialized).ToRaw();
            AssetGuidLookupDictionary[deserialized.Id] = deserialized.Handle;
            return deserialized;
        }
        throw new Exception($"Failed to deserialize the file {file.Name} at location {file.DirectoryName}.");
    }
}