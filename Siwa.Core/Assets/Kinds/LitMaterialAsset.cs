using System.Numerics;
using System.Text.Json.Serialization;
using Siwa.Core.Components;
using Siwa.Core.Helper;

namespace Siwa.Core.Assets.Kinds;

public struct LitMaterial
{
    public Handle<Shader> Shader;
    public Handle<Texture> AlbedoTexture;
    public Handle<Texture> SpecularTexture;
    public Vector4 Color;
    public Vector3 LightPosition;
    public float LightFalloff;
    public float LightRange;
}

public class LitMaterialAsset : Asset
{
    [JsonInclude] public Handle<Shader> Shader;
    [JsonInclude] public Handle<Texture> AlbedoTexture;
    [JsonInclude] public Handle<Texture> SpecularTexture;
    [JsonInclude] public Vector4 Color;
    [JsonInclude] public Vector3 LightPosition;
    [JsonInclude] public float LightFalloff;
    [JsonInclude] public float LightRange;
    
    public override void OnRestore()
    {
        AssetPool<LitMaterial>.Registry.Restore(Handle.ToHandle<LitMaterial>());
    }

    public override void OnLoad()
    {
        ref var material = ref AssetPool<LitMaterial>.Registry.Get(Handle.ToHandle<LitMaterial>());
        material.Shader = Shader;
        material.Color = Color;
        material.LightPosition = LightPosition;
        material.LightFalloff = LightFalloff;
        material.LightRange = LightRange;
        material.AlbedoTexture = AlbedoTexture;
        material.SpecularTexture = SpecularTexture;
    }
}