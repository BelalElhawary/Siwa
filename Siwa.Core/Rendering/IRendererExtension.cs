using Arch.Core;
using Silk.NET.OpenGL;

namespace Siwa.Core.Rendering;

public interface IRendererExtension
{
    void CollectCommands(World world);
}