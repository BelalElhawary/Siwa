using System.Numerics;
using Silk.NET.OpenGL;

namespace Siwa.Core.Rendering;

public struct RenderCommand
{
    public uint ShaderHandle;
    public uint VaoHandle;
    public uint IndexCount;
    public Matrix4x4 WorldMatrix;
    // A callback or a pointer to the code that knows how to bind the uniforms
    public Action<GL> BindMaterialUniforms; 
}