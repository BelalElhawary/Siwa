using System.Text.Json.Serialization;
using Silk.NET.OpenGL;
using Siwa.Core.Assets;
using Texture = Siwa.Core.Components.Texture;

namespace Siwa.Core.Helper;

public class MaterialAsset : Asset
{
    [JsonInclude]
    public string? AlbedoPath;
    [JsonInclude]
    public string? SpecularPath;
    [JsonInclude]
    public int MaterialIndex = -1;
    
    [JsonIgnore]
    public Texture Albedo;
    [JsonInclude]
    public Texture Specular;

    public override void OnLoad()
    {
        if (AlbedoPath != null && File.Exists(AlbedoPath))
            Albedo = AssetLoader.Instance.Gl.NewTexture(AlbedoPath);
        if (SpecularPath != null && File.Exists(SpecularPath))
            Specular = AssetLoader.Instance.Gl.NewTexture(SpecularPath, 1, PixelFormat.Red);
        AssetPool<MaterialAsset>.Registry.Register(this);
    }

    protected override void OnUnload()
    {
        AssetLoader.Instance.Gl.DeleteTexture(Albedo.Id);
        AssetLoader.Instance.Gl.DeleteTexture(Specular.Id);
        AssetPool<MaterialAsset>.Registry.Unload(Handle.ToHandle<MaterialAsset>());
    }
}