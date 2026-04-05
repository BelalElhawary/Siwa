using System.Numerics;
using System.Text.Json;
using Arch.Core;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using Siwa.Core.Assets.Kinds;
using Siwa.Core.Data;
using Siwa.Core.Serialization;
using Siwa.Core.Testing;
using File = System.IO.File;
using Texture = Siwa.Core.Components.Texture;
using Shader = Siwa.Core.Components.Shader;

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
    private AssetLoader(GL gl, Assimp assimp, string rootProject)
    {
        Gl = gl;
        Assimp = assimp;
        _rootProject = rootProject;
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
        LoadDefaultAssets();
        SaveTestingAssets();
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

    public void SaveWorld(string worldName, World world)
    {
        var json = JsonSerializer.Serialize(world, SerializationManager.Options);
        File.WriteAllText(Path.Combine(AssetsFolder, worldName + ".world"), json);
    }
    
    public World LoadWorld(string worldName)
    {
        var json = File.ReadAllText(Path.Combine(AssetsFolder, worldName + ".world"));
        var world = JsonSerializer.Deserialize<World>(json, SerializationManager.Options);
        return world ?? throw new ArgumentNullException(nameof(world));
    }

    private void SaveTestingAssets()
    {
        var unlitAssets = new UnlitAssets(SerializationManager.Options);
        var litAssets = new LitAssets(SerializationManager.Options);
    }

    private void LoadDefaultAssets()
    {
        var unlitShaderAsset = new ShaderAsset
        {
            Readonly = true,
            Name = "UnlitShader",
            FilePath = "Engine/Shaders/UnlitShader",
            FragmentShaderPath = "D:\\SiwaProject\\Shaders\\unlit.frag",
            VertexShaderPath = "D:\\SiwaProject\\Shaders\\unlit.vert",
            Handle = AssetPool<Shader>.Registry.Reserve().ToRaw()
        };
        var litShaderAsset = new ShaderAsset
        {
            Readonly = true,
            Name = "LitShader",
            FilePath = "Engine/Shaders/LitShader",
            FragmentShaderPath = "D:\\SiwaProject\\Shaders\\shader.frag",
            VertexShaderPath = "D:\\SiwaProject\\Shaders\\shader.vert",
            Handle = AssetPool<Shader>.Registry.Reserve().ToRaw()
        };
        var nullTextureAsset = new TextureAsset
        {
            Readonly = true,
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            FilePath = "Engine/Textures/NullTexture",
            Name = "NullTexture",
            ImagePath = "D:\\SiwaProject\\Engine\\null_texture.png",
            PixelFormat = PixelFormat.Rgba,
            Slot = 0,
        };
        var modelIcon = new TextureAsset
        {
            Readonly = true,
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Engine\\deployed_code_50dp_E3E3E3_FILL0_wght400_GRAD0_opsz48.png",
            FilePath = "Engine/Textures/ModelIconTexture",
            PixelFormat = PixelFormat.Rgba,
            FlipVertically = false,
            Slot = 0,
            Name = "ModelIcon"
        };
        var textureIcon = new TextureAsset
        {
            Readonly = true,
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Engine\\texture_50dp_E3E3E3_FILL0_wght400_GRAD0_opsz48.png",
            FilePath = "Engine/Textures/TextureIconTexture",
            PixelFormat = PixelFormat.Rgba,
            FlipVertically = false,
            Slot = 0,
            Name = "TextureIcon"
        };
        var shaderIcon = new TextureAsset
        {
            Readonly = true,
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Engine\\docs_50dp_E3E3E3_FILL0_wght400_GRAD0_opsz48.png",
            FilePath = "Engine/Textures/ShaderIconTexture",
            PixelFormat = PixelFormat.Rgba,
            FlipVertically = false,
            Slot = 0,
            Name = "ShaderIcon"
        };
        var materialIcon = new TextureAsset
        {
            Readonly = true,
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Engine\\ev_shadow_50dp_E3E3E3_FILL0_wght400_GRAD0_opsz48.png",
            FilePath = "Engine/Textures/MaterialIconTexture",
            PixelFormat = PixelFormat.Rgba,
            FlipVertically = false,
            Slot = 0,
            Name = "MaterialIcon"
        };
        var nullLitMaterialAsset = new LitMaterialAsset
        {
            Readonly = true,
            Handle = AssetPool<LitMaterial>.Registry.Reserve().ToRaw(),
            FilePath = "Engine/Textures/NullLitMaterial",
            Name = "NullLitMaterial",
            AlbedoTexture = nullTextureAsset.Handle.ToHandle<Texture>(),
            Shader = litShaderAsset.Handle.ToHandle<Shader>(),
            Color = new Vector4(1,1,1,1)
        };
        var nullUnlitMaterialAsset = new UnlitMaterialAsset
        {
            Readonly = true,
            Handle = AssetPool<UnlitMaterial>.Registry.Reserve().ToRaw(),
            FilePath = "Engine/Materials/NullUnlitMaterial",
            Name = "NullUnlitMaterial",
            LightColor = new Vector4(1, 0, 1, 1),
            Shader =  unlitShaderAsset.Handle.ToHandle<Shader>()
        };
        
        _assets.AddRange(unlitShaderAsset, litShaderAsset, nullUnlitMaterialAsset, nullLitMaterialAsset, nullTextureAsset, modelIcon, textureIcon, shaderIcon, materialIcon);
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
        var deserialized = JsonSerializer.Deserialize<T>(File.ReadAllText(file.FullName), SerializationManager.Options);
        if (deserialized is not null)
        {
            deserialized.FilePath = file.FullName;
            deserialized.OnRestore();
            return deserialized;
        }
        throw new Exception($"Failed to deserialize the file {file.Name} at location {file.DirectoryName}.");
    }
}