using System.Drawing;
using System.Numerics;
using System.Text.Json;
using Silk.NET.OpenGL;
using Siwa.Core.Assets;
using Siwa.Core.Assets.Kinds;
using Siwa.Core.Helper;
using Shader = Siwa.Core.Components.Shader;
using Texture = Siwa.Core.Components.Texture;

namespace Siwa.Core.Testing;

public class LitAssets
{
    public LitAssets(JsonSerializerOptions options)
    {
        var clothB = new TextureAsset
        {
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Assets\\objs\\Textures\\KARPENTER_GRASSHOPPER_cloth_b.jpg",
            Name = "Cloth_B",
            PixelFormat = PixelFormat.Red,
            Slot = 1
        };

        var clothD1 = new TextureAsset
        {
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Assets\\objs\\Textures\\KARPENTER_GRASSHOPPER_cloth_d1.jpg",
            Name = "Cloth_D1",
            PixelFormat = PixelFormat.Rgba,
            Slot = 0
        };

        var clothD2 = new TextureAsset
        {
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Assets\\objs\\Textures\\KARPENTER_GRASSHOPPER_cloth_d2.jpg",
            Name = "Cloth_D2",
            PixelFormat = PixelFormat.Rgba,
            Slot = 0
        };

        var woodB = new TextureAsset
        {
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Assets\\objs\\Textures\\KARPENTER_GRASSHOPPER_wood_b.jpg",
            Name = "Wood_B",
            PixelFormat = PixelFormat.Red,
            Slot = 1
        };

        var woodD = new TextureAsset
        {
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Assets\\objs\\Textures\\KARPENTER_GRASSHOPPER_wood_d.jpg",
            Name = "Wood_D",
            PixelFormat = PixelFormat.Rgba,
            Slot = 0
        };

        var popCat = new TextureAsset
        {
            Handle = AssetPool<Texture>.Registry.Reserve().ToRaw(),
            ImagePath = "D:\\SiwaProject\\Assets\\objs\\Textures\\pop-cat.jpg",
            Name = "PopCat",
            PixelFormat = PixelFormat.Rgba,
            Slot = 0,
            FlipVertically = false
        };

        var litShaderHandle = new Handle<Shader>(1, 0);
        
        var material = new LitMaterialAsset
        {
            Name = "cloth_material",
            Handle = AssetPool<LitMaterial>.Registry.Reserve().ToRaw(),
            Color = Color.White.ToVector4(),
            Shader = litShaderHandle,
            AlbedoTexture = clothD1.Handle.ToHandle<Texture>(),
            SpecularTexture = clothB.Handle.ToHandle<Texture>(),
            LightRange = 0,
            LightFalloff = 0,
            LightPosition = new Vector3()
        };

        LitMaterialAsset material1 = new LitMaterialAsset
        {
            Name = "cloth_material1",
            Handle = AssetPool<LitMaterial>.Registry.Reserve().ToRaw(),
            Color = Color.White.ToVector4(),
            Shader = litShaderHandle,
            AlbedoTexture = clothD2.Handle.ToHandle<Texture>(),
            SpecularTexture = clothB.Handle.ToHandle<Texture>(),
            LightRange = 0,
            LightFalloff = 0,
            LightPosition = new Vector3()
        };
        LitMaterialAsset material2 = new LitMaterialAsset
        {
            Name = "wood",
            Handle = AssetPool<LitMaterial>.Registry.Reserve().ToRaw(),
            Color = Color.White.ToVector4(),
            Shader = litShaderHandle,
            AlbedoTexture = woodD.Handle.ToHandle<Texture>(),
            SpecularTexture = woodB.Handle.ToHandle<Texture>(),
            LightRange = 0,
            LightFalloff = 0,
            LightPosition = new Vector3()
        };

        ModelAsset modelAsset = new ModelAsset
        {
            Model = "D:\\SiwaProject\\Assets\\objs\\uploads_files_6722647_KARPENTER+GRASSHOPPER+SET1.obj",
            Name = "Table",
            Handle = AssetPool<Model>.Registry.Register(new Model()).ToRaw(),
            MaterialAssets = [
                new MaterialHandle
                {
                    Handle = material.Handle,
                    Type = MaterialType.Lit
                }, 
                new MaterialHandle
                {
                    Handle = material1.Handle,
                    Type = MaterialType.Lit
                }, 
                new MaterialHandle
                {
                    Handle = material2.Handle,
                    Type = MaterialType.Lit 
                }
            ]
        };
        
        ModelAsset armadillo = new ModelAsset
        {
            Model = "D:\\SiwaProject\\Assets\\objs\\armadillo.obj",
            Name = "armadillo",
            Handle = AssetPool<Model>.Registry.Register(new Model()).ToRaw(),
            MaterialAssets = [
                new MaterialHandle
                {
                    Handle = material.Handle,
                    Type = MaterialType.Lit
                }
            ]
        };


        File.WriteAllText("D:\\SiwaProject\\Assets\\testing_lit.model", JsonSerializer.Serialize(modelAsset, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\testing_lit.lit", JsonSerializer.Serialize(material, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\testing1_lit.lit", JsonSerializer.Serialize(material1, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\testing2_lit.lit", JsonSerializer.Serialize(material2, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\clothB_lit.texture", JsonSerializer.Serialize(clothB, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\clothD1_lit.texture", JsonSerializer.Serialize(clothD1, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\clothD2_lit.texture", JsonSerializer.Serialize(clothD2, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\woodB_lit.texture", JsonSerializer.Serialize(woodB, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\woodD_lit.texture", JsonSerializer.Serialize(woodD, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\popcat.texture", JsonSerializer.Serialize(popCat, options));
        File.WriteAllText("D:\\SiwaProject\\Assets\\armadillo.model", JsonSerializer.Serialize(armadillo, options));
    }
}