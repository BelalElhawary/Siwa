using System.Text.Json;
using Silk.NET.OpenGL;
using Siwa.Core.Assets;
using Siwa.Core.Assets.Kinds;
using Texture = Siwa.Core.Components.Texture;

namespace Siwa;

public class EngineAssets
{
    public EngineAssets(JsonSerializerOptions options)
    {
        var modelIcon = new TextureAsset
        {
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Engine\\deployed_code_50dp_E3E3E3_FILL0_wght400_GRAD0_opsz48.png",
            PixelFormat = PixelFormat.Rgba,
            FlipVertically = false,
            Slot = 0,
            Name = "ModelIcon"
        };
        
        var textureIcon = new TextureAsset
        {
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Engine\\texture_50dp_E3E3E3_FILL0_wght400_GRAD0_opsz48.png",
            PixelFormat = PixelFormat.Rgba,
            FlipVertically = false,
            Slot = 0,
            Name = "TextureIcon"
        };
        
        var shaderIcon = new TextureAsset
        {
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Engine\\docs_50dp_E3E3E3_FILL0_wght400_GRAD0_opsz48.png",
            PixelFormat = PixelFormat.Rgba,
            FlipVertically = false,
            Slot = 0,
            Name = "ShaderIcon"
        };
        
        var materialIcon = new TextureAsset
        {
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Engine\\ev_shadow_50dp_E3E3E3_FILL0_wght400_GRAD0_opsz48.png",
            PixelFormat = PixelFormat.Rgba,
            FlipVertically = false,
            Slot = 0,
            Name = "MaterialIcon"
        };


        File.WriteAllText("D:\\SiwaProject\\Assets\\modelIcon.texture", JsonSerializer.Serialize(modelIcon, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\textureIcon.texture", JsonSerializer.Serialize(textureIcon, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\shaderIcon.texture", JsonSerializer.Serialize(shaderIcon, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\materialIcon.texture", JsonSerializer.Serialize(materialIcon, options));
    }
}