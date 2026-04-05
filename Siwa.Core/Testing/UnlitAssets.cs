using System.Drawing;
using System.Text.Json;
using Siwa.Core.Assets;
using Siwa.Core.Assets.Kinds;
using Siwa.Core.Components;
using Siwa.Core.Helper;

namespace Siwa.Core.Testing;

public class UnlitAssets
{
    public UnlitAssets(JsonSerializerOptions options)
    {
        var unlitShaderHandle = new Handle<Shader>(0, 0);
        UnlitMaterialAsset material = new UnlitMaterialAsset
        {
            Name = "lamb_material_1",
            Handle = AssetPool<UnlitMaterial>.Registry.Reserve().ToRaw(),
            LightColor = Color.White.ToVector4(),
            Shader = unlitShaderHandle
        };

        UnlitMaterialAsset material1 = new UnlitMaterialAsset
        {
            Name = "lamb_material_2",
            Handle = AssetPool<UnlitMaterial>.Registry.Reserve().ToRaw(),
            LightColor = Color.White.ToVector4(),
            Shader = unlitShaderHandle
        };

        ModelAsset modelAsset = new ModelAsset
        {
            Model = "D:\\SiwaProject\\Assets\\Engine\\Light_LightBulb.obj",
            Name = "Light_LightBulb",
            Handle = AssetPool<Model>.Registry.Reserve().ToRaw(),
            MaterialAssets =
            [
                new MaterialHandle
                {
                    Handle = material.Handle,
                    Type = MaterialType.Unlit
                },
                new MaterialHandle
                {
                    Handle = material1.Handle,
                    Type = MaterialType.Unlit
                }
            ]
        };
        
        File.WriteAllText("D:\\SiwaProject\\Assets\\testing.model", JsonSerializer.Serialize(modelAsset, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\testing.unlit", JsonSerializer.Serialize(material, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\testing1.unlit", JsonSerializer.Serialize(material1, options));
    }
}