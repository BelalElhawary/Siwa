using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using Siwa.Core.Components;
using Texture = Siwa.Core.Components.Texture;

namespace Siwa.Core.Helper;

public static class OpenGlExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Texture NewTexture(this GL gl, string path, uint slot = 0, PixelFormat format = PixelFormat.Rgba)
    {
        Texture texture;

        byte[] buffer = [];
        float width = 0, height = 0;
        
        InternalFormat internalFormat = (format == PixelFormat.Red) 
            ? InternalFormat.R8 
            : InternalFormat.Rgba;
        
        if(format == PixelFormat.Rgba)
            buffer = ImageLoader.LoadRgba32(path, out width, out height);
        else if(format == PixelFormat.Red)
            buffer = ImageLoader.LoadL8(path, out width, out height);
        
        texture.Id = gl.GenTexture();
        texture.Slot = (TextureUnit)(33984 + slot);
        gl.ActiveTexture(texture.Slot);
        gl.BindTexture(TextureTarget.Texture2D, texture.Id);
        
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BindTexture(this GL gl, Texture texture)
    {
        gl.ActiveTexture(texture.Slot);
        gl.BindTexture(TextureTarget.Texture2D, texture.Id);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vao NewVao(this GL gl)
    {
        Vao vao;
        vao.Id = gl.GenVertexArray();
        return vao;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void LinkVboToVao(this GL gl, Vbo vbo, uint layout, int size, VertexAttribPointerType type, uint stride, void* offset)
    {
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.Id);
        gl.VertexAttribPointer(layout, size, type, false, stride, offset);
        gl.EnableVertexAttribArray(layout);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vbo NewVbo(this GL gl, float[] vertices)
    {
        Vbo vbo;
        vbo.Id = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo.Id);
        fixed(float* buffer = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), buffer, BufferUsageARB.StaticDraw);
        return vbo;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Ebo NewEbo(this GL gl, uint[] indices)
    {
        Ebo ebo;
        ebo.Id = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo.Id);
        fixed(uint* buffer = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buffer, BufferUsageARB.StaticDraw);
        return ebo;
    }
}