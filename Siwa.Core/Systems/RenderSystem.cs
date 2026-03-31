using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using ImGuiNET;
using Silk.NET.OpenGL;
using Siwa.Core.Assets;
using Siwa.Core.Assets.Kinds;
using Siwa.Core.Components;
using Siwa.Core.Rendering;

namespace Siwa.Core.Systems;

public class RenderSystem(World world, GL gl, ForwardRenderer renderer) : IRenderSystem
{
    public void Initialize()
    {
        
    }

    public void Start()
    {
        
    }

    public void Update(float dt)
    {
        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Render()
    {
        renderer.OnRender(gl, world);
    }
}