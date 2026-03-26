using System.Numerics;
using Silk.NET.OpenGL;
using Siwa.Core.Assets;
using Siwa.Core.Components;
using AssimpMesh = Silk.NET.Assimp.Mesh;

namespace Siwa.Core.Helper;

public unsafe class Mesh : IRenderable
{
    private Vao _vao;
    private Vbo _vbo;
    private Ebo _ebo;
    public Handle<MaterialAsset> Material;
    public readonly uint MaterialIndex;
    // private Texture _texture;
    // private Texture _specularTexture;
    private readonly float[] _vertices;
    private readonly uint[] _indices;
    public Matrix4x4 Transform = Matrix4x4.Identity;
    public Vector3 Position = Vector3.Zero;

    public Mesh(AssimpMesh* mesh)
    {
        // 8 floats per vertex: Pos(3), Normal(3), UV(2)
        float[] data = new float[mesh->MNumVertices * 8];
    
        for (int i = 0; i < mesh->MNumVertices; i++)
        {
            int baseIndex = i * 8;

            // 1. Position (Layout 0)
            data[baseIndex + 0] = mesh->MVertices[i].X;
            data[baseIndex + 1] = mesh->MVertices[i].Y;
            data[baseIndex + 2] = mesh->MVertices[i].Z;

            // 2. Normals (Layout 1 - used as 'color' in your shader)
            if (mesh->MNormals != null)
            {
                data[baseIndex + 3] = mesh->MNormals[i].X;
                data[baseIndex + 4] = mesh->MNormals[i].Y;
                data[baseIndex + 5] = mesh->MNormals[i].Z;
            }

            // 3. UVs (Layout 2)
            if (mesh->MTextureCoords[0] != null)
            {
                data[baseIndex + 6] = mesh->MTextureCoords[0][i].X;
                data[baseIndex + 7] = mesh->MTextureCoords[0][i].Y;
            }

            MaterialIndex = mesh->MMaterialIndex;
        }

        // Indices
        uint[] indices = new uint[mesh->MNumFaces * 3];
        for (int i = 0; i < mesh->MNumFaces; i++)
        {
            indices[i * 3 + 0] = mesh->MFaces[i].MIndices[0];
            indices[i * 3 + 1] = mesh->MFaces[i].MIndices[1];
            indices[i * 3 + 2] = mesh->MFaces[i].MIndices[2];
        }

        _vertices = data;
        _indices = indices;
    }

    public void OnLoad(GL gl)
    {
        _vao = gl.NewVao();
        gl.BindVertexArray(_vao.Id);
    
        // 2. Upload the loaded data
        _vbo = gl.NewVbo(_vertices); // Ensure your Vbo class accepts float[]
        _ebo = gl.NewEbo(_indices);  // Ensure your Ebo class accepts uint[]
    
        // 3. Link attributes (Standard 3-3-2 layout)
        // Layout 0: Position (vec3)
        gl.LinkVboToVao(_vbo, 0, 3, VertexAttribPointerType.Float, 8 * sizeof(float), (void*)0);
        // Layout 1: Normal or Color (vec3) - OBJ files have Normals, not Colors
        gl.LinkVboToVao(_vbo, 1, 3, VertexAttribPointerType.Float, 8 * sizeof(float), (void*)(3 * sizeof(float)));
        // Layout 2: TexCoords (vec2)
        gl.LinkVboToVao(_vbo, 2, 2, VertexAttribPointerType.Float, 8 * sizeof(float), (void*)(6 * sizeof(float)));

        // Do NOT unbind EBO before VAO here!
        gl.BindVertexArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    private readonly AssetPool<MaterialAsset> _materialRegistry = AssetPool<MaterialAsset>.Registry;
    public void OnRender(GL gl, Shader shader)
    {
        var material = _materialRegistry.Get(Material);
        if (material != null)
        {
            gl.BindTexture(material.Albedo);
            gl.BindTexture(material.Specular);
        }
        gl.BindVertexArray(_vao.Id);
        gl.DrawElements(PrimitiveType.Triangles, (uint)_indices.Length, DrawElementsType.UnsignedInt, (void*)0);
    }

    public void Dispose(GL gl)
    {
        gl.DeleteVertexArray(_vao.Id);
        gl.DeleteBuffer(_vbo.Id);
        gl.DeleteBuffer(_ebo.Id);
    }
}