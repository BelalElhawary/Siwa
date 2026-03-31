using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Silk.NET.OpenGL;
using Siwa.Core.Importers;
using Texture = Siwa.Core.Components.Texture;

namespace Siwa.Core.Assets.Kinds;

public class TextureAsset : Asset
{
    [JsonInclude] public string ImagePath;
    [JsonInclude] public bool FlipVertically = true;
    [JsonInclude] public uint Slot;
    [JsonInclude] public PixelFormat PixelFormat;
    
    
    public override void OnRestore()
    {
        AssetPool<Texture>.Registry.Restore(Handle.ToHandle<Texture>());
    }

    public override void OnLoad()
    {
        var gl = AssetLoader.Instance.Gl;
        var newTexture = NewTexture(gl, ImagePath, Slot, PixelFormat, FlipVertically);
        ref var texture = ref AssetPool<Texture>.Registry.Get(Handle.ToHandle<Texture>());
        texture.Handle = newTexture.Handle;
        texture.Slot = newTexture.Slot;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe Texture NewTexture(GL gl, string path, uint slot = 0, PixelFormat format = PixelFormat.Rgba, bool flipY = true)
    {
        Texture texture;

        byte[] buffer = [];
        float width = 0, height = 0;
        
        InternalFormat internalFormat = (format == PixelFormat.Red) 
            ? InternalFormat.R8 
            : InternalFormat.Rgba;
        
        if(format == PixelFormat.Rgba)
            buffer = ImageImporter.Rgba32(path, flipY, out width, out height);
        else if(format == PixelFormat.Red)
            buffer = ImageImporter.L8(path, flipY, out width, out height);
        
        texture.Handle = gl.GenTexture();
        texture.Slot = (TextureUnit)(33984 + slot);
        gl.ActiveTexture(texture.Slot);
        gl.BindTexture(TextureTarget.Texture2D, texture.Handle);
        
        gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

        int filter = (int)GLEnum.Linear;
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, in filter);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, in filter);

        int wrapMode = (int)TextureWrapMode.Repeat;
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, in wrapMode);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, in wrapMode);
        
        fixed (byte* ptr = buffer)
            gl.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, (uint)width, (uint)height, 0, format, PixelType.UnsignedByte, ptr);
        gl.GenerateMipmap(TextureTarget.Texture2D);
        
        // unbind
        gl.BindTexture(TextureTarget.Texture2D, 0);

        return texture;
    }
    
    protected override void OnUnload()
    {
        ref var texture = ref AssetPool<Texture>.Registry.Get(Handle.ToHandle<Texture>());
        AssetLoader.Instance.Gl.DeleteTexture(texture.Handle);
    }
}