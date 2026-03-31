using System.Numerics;
using System.Text.Json.Serialization;
using Siwa.Core.Helper;

namespace Siwa.Core.Assets.Kinds;

public struct UnlitMaterial
{
    public Handle<Components.Shader> Shader;
    public Vector4 Color;
}

public class UnlitMaterialAsset : Asset
{
    [JsonInclude] public Handle<Components.Shader> Shader;
    [JsonInclude] public Vector4 LightColor;

    public override void OnRestore()
    {
        AssetPool<UnlitMaterial>.Registry.Restore(Handle.ToHandle<UnlitMaterial>());
    }

    public override void OnLoad()
    {
        ref var material = ref AssetPool<UnlitMaterial>.Registry.Get(Handle.ToHandle<UnlitMaterial>());
        material.Shader = Shader;
        material.Color =  LightColor;
    }
}