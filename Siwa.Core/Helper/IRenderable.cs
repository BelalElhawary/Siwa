using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;

namespace Siwa.Core.Helper;

public interface IRenderable
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    void OnLoad(GL gl);
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    void OnRender(GL gl, Shader shader);
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    void Dispose(GL gl);
}