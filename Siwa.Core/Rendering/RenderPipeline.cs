using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using Siwa.Core.Helper;

namespace Siwa.Core.Rendering;

public sealed class RenderPipeline
{
    private readonly List<RenderCommand> _queue = new();
    public void Submit(RenderCommand command) => _queue.Add(command);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Flush(GL gl)
    {
        var sorted = _queue.OrderBy(c => c.ShaderHandle);
        
        uint activeShader = 0;

        foreach (var cmd in sorted)
        {
            if (cmd.ShaderHandle != activeShader)
            {
                gl.UseProgram(cmd.ShaderHandle);
                activeShader = cmd.ShaderHandle;
            }

            // 1. Bind Material-specific data
            cmd.BindMaterialUniforms(gl);

            // 2. Bind Transform
            var uModel = cmd.WorldMatrix;
            gl.UniformMatrix4(gl.GetUniformLocation(cmd.ShaderHandle, "uModel"), 1, false, uModel.GetMatrixSpan());

            // 3. Draw
            gl.BindVertexArray(cmd.VaoHandle);
            gl.DrawElements(PrimitiveType.Triangles, cmd.IndexCount, DrawElementsType.UnsignedInt, null);
        }
        _queue.Clear();
    }
}