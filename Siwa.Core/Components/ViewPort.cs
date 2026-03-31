using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;

namespace Siwa.Core.Components;

public unsafe class ViewPort(GL gl)
{
    private readonly uint _fbo = gl.GenFramebuffer();
    private readonly uint _rbo = gl.GenRenderbuffer();

    public readonly uint TextureColorBuffer = gl.GenTexture();
    public uint Width;
    public uint Height;
    public bool IsFocused = false;
    
    public void OnLoad()
    {
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        gl.BindTexture(TextureTarget.Texture2D, TextureColorBuffer);
        
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, Width, Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, null);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

        // 3. Attach texture to framebuffer
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureColorBuffer, 0);
        
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, Width, Height);
        gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _rbo);

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0); // Unbind
    }
    
    public void Rescale(uint width, uint height)
    {
        Width = width;
        Height = height;
        gl.BindTexture(TextureTarget.Texture2D, TextureColorBuffer);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, null);
    
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Depth24Stencil8, width, height);
    }

    public void OnRender()
    {
        // --- PHASE 1: RENDER SCENE TO TEXTURE ---
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo); 
        gl.ClearColor(0.529f, 0.811f, 0.921f, 1.0f);
        gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit);

        // IMPORTANT: Set viewport to the size of your texture/imgui panel
        gl.Viewport(0, 0, Width, Height);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Unbind() => gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    
}