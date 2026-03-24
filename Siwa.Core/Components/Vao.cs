using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using Siwa.Core.Helper;

namespace Siwa.Core.Components;

public struct Vao
{
    public uint Id;
    
    // [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    // public void Bind(GL gl) => gl.BindVertexArray(Id);
    //
    // [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    // public void Unbind(GL gl) => gl.BindVertexArray(0);
    //
    // [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    // public void Delete(GL gl) => gl.DeleteVertexArray(Id);
}