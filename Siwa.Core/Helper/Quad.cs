using System.Numerics;
using Silk.NET.OpenGL;
using Siwa.Core.Components;
using Siwa.Core.Data;
using Texture = Siwa.Core.Components.Texture;

namespace Siwa.Core.Helper;

// used for debug texture (manual usage for now)
public unsafe class Quad : IRenderable
{
    private Vao _vao;
    private Vbo _vbo;
    private Ebo _ebo;
    private Texture _texture;

    private uint _indicesCount;
    // HINT: no public use for now (might be in future)
    public Vector3 Position;
    
    public void OnLoad(GL gl)
    {
        _vao = gl.NewVao();
        gl.BindVertexArray(_vao.Id);
        
        _vbo = gl.NewVbo(QuadModel.Vertices);
        _ebo = gl.NewEbo(QuadModel.Indices);
        _indicesCount = (uint)QuadModel.Indices.Length;
        
        gl.LinkVboToVao(_vbo, 0, 3, VertexAttribPointerType.Float, 8 * sizeof(float), (void*)0);
        gl.LinkVboToVao(_vbo, 1, 3, VertexAttribPointerType.Float, 8 * sizeof(float), (void*)(3 * sizeof(float)));
        gl.LinkVboToVao(_vbo, 2, 2, VertexAttribPointerType.Float, 8 * sizeof(float), (void*)(6 * sizeof(float)));
        
        
        gl.BindVertexArray(0); // unbind VAO FIRST so the EBO reference is preserved inside it
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        // _texture = new Texture(gl, "Assets/objs/Textures/colormap.png");
        _texture = gl.NewTexture("Assets/pop-cat.jpg");
    }
    
    public void OnRender(GL gl, Shader shader)
    {
        var matrix = Matrix4x4.CreateTranslation(Position);
        gl.UniformMatrix4(gl.GetUniformLocation(shader.ShaderProgram, "model"), 1, false, matrix.GetMatrixSpan());
        gl.BindTexture(_texture);
        gl.BindVertexArray(_vao.Id);
        gl.DrawElements(PrimitiveType.Triangles, _indicesCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    public void Dispose(GL gl)
    {
        gl.DeleteVertexArray(_vao.Id);
        gl.DeleteBuffer(_vbo.Id);
        gl.DeleteBuffer(_ebo.Id);
        gl.DeleteTexture(_texture.Id);
    }
}