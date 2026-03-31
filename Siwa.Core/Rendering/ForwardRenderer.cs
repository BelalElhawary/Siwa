using System.Runtime.CompilerServices;
using Arch.Core;
using Silk.NET.OpenGL;

namespace Siwa.Core.Rendering;

public class ForwardRenderer
{
    private readonly List<IRendererExtension> _extensions = new();
    public readonly RenderPipeline RenderPipeline = new();

    public void AddExtension(IRendererExtension extension) => _extensions.Add(extension);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnRender(GL gl, World world)
    {
        // Global settings (Depth test, Clear color, etc.)
        gl.Enable(EnableCap.DepthTest);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        for (int i = 0; i < _extensions.Count; i++)
        {
            _extensions[i].CollectCommands(world);
        }
        
        RenderPipeline.Flush(gl);
    }
}