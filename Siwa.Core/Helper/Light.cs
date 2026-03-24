using System.Numerics;
using Silk.NET.OpenGL;
using Siwa.Core.Components;
using Siwa.Core.Data;

namespace Siwa.Core.Helper;

public unsafe class Light : IRenderable
{
    private Vao _vao;
    private Vbo _vbo;
    private Ebo _ebo;
    private uint _indicesCount;

    public Vector3 Position;
    public Vector4 Color = new (1, 1, 1, 1);
    private Matrix4x4 _model = Matrix4x4.Identity;

    public void OnLoad(GL gl)
    {
        _vao = gl.NewVao();
        gl.BindVertexArray(_vao.Id);
        
        _vbo = gl.NewVbo(QuadModel.CoordinatesOnly);
        _ebo = gl.NewEbo(QuadModel.Indices);
        _indicesCount = (uint)QuadModel.Indices.Length;
        
        gl.LinkVboToVao(_vbo, 0, 3, VertexAttribPointerType.Float, 3 * sizeof(float), (void*)0);
        
        gl.BindVertexArray(0);
        // unbind VAO FIRST so the EBO reference is preserved inside it
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        
    }
    
    public void OnRender(GL gl, Shader shader)
    {
        _model = Matrix4x4.CreateTranslation(Position);
        gl.UniformMatrix4(gl.GetUniformLocation(shader.ShaderProgram, "model"), 1, false, _model.GetMatrixSpan());
        gl.Uniform4(gl.GetUniformLocation(shader.ShaderProgram, "lightColor"), ref Color);
        gl.BindVertexArray(_vao.Id);
        gl.DrawElements(PrimitiveType.Triangles, _indicesCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    public void SupplyColorUniforms(GL gl, Shader shader)
    {
        gl.Uniform4(gl.GetUniformLocation(shader.ShaderProgram, "lightColor"), ref Color);
        gl.Uniform3(gl.GetUniformLocation(shader.ShaderProgram, "lightPos"), ref Position);
    }
    
    public void Dispose(GL gl)
    {
        gl.DeleteVertexArray(_vao.Id);
        gl.DeleteBuffer(_vbo.Id);
        gl.DeleteBuffer(_ebo.Id);
    }
}