using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using Buffer = Silk.NET.OpenGL.Buffer;

namespace Siwa.Core.Assets.Kinds;

public struct Model
{
    public Mesh[] Meshes;
}

public struct Mesh
{
    public VertexArray Vao;
    public Buffer Vbo;
    public Buffer Ebo;
    public MaterialHandle Material;
    public uint IndicesCount;
}

public class ModelAsset : Asset
{
    [JsonInclude] public string Model;
    [JsonInclude] public MaterialHandle[] MaterialAssets;
    [JsonIgnore] public List<Mesh> MeshAssets = new();
    // Material investigate this

    public override void OnRestore()
    {
        AssetPool<Model>.Registry.Restore(Handle.ToHandle<Model>());
    }

    public override void OnLoad()
    {
        LoadModel();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void LoadMesh(AssimpMesh* mesh)
    {
        var gl = AssetLoader.Instance.Gl;
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

            // AssetPool<Mesh>.Registry.Get(MeshAssets[_meshCount]);
        }

        

        // Indices
        uint[] indices = new uint[mesh->MNumFaces * 3];
        for (int i = 0; i < mesh->MNumFaces; i++)
        {
            indices[i * 3 + 0] = mesh->MFaces[i].MIndices[0];
            indices[i * 3 + 1] = mesh->MFaces[i].MIndices[1];
            indices[i * 3 + 2] = mesh->MFaces[i].MIndices[2];
        }
        
        var vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        // 2. Upload the loaded data
        var vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        fixed(float* buffer = data)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(float)), buffer, BufferUsageARB.StaticDraw);
        
        
        var ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        fixed(uint* buffer = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buffer, BufferUsageARB.StaticDraw);
    
        // 3. Link attributes (Standard 3-3-2 layout)
        // Layout 0: Position (vec3)
        LinkVboToVao(gl, vbo, 0, 3, VertexAttribPointerType.Float, 8 * sizeof(float), (void*)0);
        // Layout 1: Normal or Color (vec3) - OBJ files have Normals, not Colors
        LinkVboToVao(gl, vbo, 1, 3, VertexAttribPointerType.Float, 8 * sizeof(float), (void*)(3 * sizeof(float)));
        // Layout 2: TexCoords (vec2)
        LinkVboToVao(gl, vbo, 2, 2, VertexAttribPointerType.Float, 8 * sizeof(float), (void*)(6 * sizeof(float)));

        // Do NOT unbind EBO before VAO here!
        gl.BindVertexArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        var meshRegistry = new Mesh
        {
            Material = MaterialAssets[mesh->MMaterialIndex],
            IndicesCount = (uint)indices.Length,
            Vao = new VertexArray { Handle = vao },
            Vbo = new Buffer { Handle = vbo },
            Ebo = new Buffer { Handle = ebo }
        };
        MeshAssets.Add(meshRegistry);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void LinkVboToVao(GL gl, uint buffer, uint layout, int size, VertexAttribPointerType type, uint stride, void* offset)
    {
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer);
        gl.VertexAttribPointer(layout, size, type, false, stride, offset);
        gl.EnableVertexAttribArray(layout);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private unsafe void LoadModel()
    {
        var scene = AssetLoader.Instance.Assimp.ImportFile(Model, (uint)PostProcessSteps.Triangulate);
        
        if (scene == null || scene->MFlags == Assimp.SceneFlagsIncomplete || scene->MRootNode == null)
        {
            throw new Exception($"Assimp error: Could not load file {Model}.");
        }
        
        Console.WriteLine($"Loaded {Model} with {scene->MNumMeshes} mesh.");

        ProcessNode(scene->MRootNode, scene, Matrix4x4.Identity);

        AssetLoader.Instance.Assimp.FreeScene(scene);

        ref var model = ref AssetPool<Model>.Registry.Get(Handle.ToHandle<Model>());
        model.Meshes = new Mesh[MeshAssets.Count];
        Array.Copy(MeshAssets.ToArray(),model.Meshes, MeshAssets.Count);
    }
    
    private unsafe void ProcessNode(Node* node, Scene* scene, Matrix4x4 parentTransform)
    {
        // 1. Get this node's local transform and combine with parent
        Matrix4x4 localTransform = node->MTransformation;
        Matrix4x4 globalTransform = parentTransform * localTransform;

        // 2. Process all meshes attached to THIS node
        for (int i = 0; i < node->MNumMeshes; i++)
        {
            uint meshIndex = node->MMeshes[i];
            Silk.NET.Assimp.Mesh* mesh = scene->MMeshes[meshIndex];
            LoadMesh(mesh);
            // You'll need to store the 'globalTransform' with the mesh 
            // so you can use it as the 'Model' matrix during render!
        }

        // 3. Recurse for all children
        for (int i = 0; i < node->MNumChildren; i++)
        {
            ProcessNode(node->MChildren[i], scene, globalTransform);
        }
    }

    protected override void OnUnload()
    {
        var gl = AssetLoader.Instance.Gl;
        ref var model = ref AssetPool<Model>.Registry.Get(Handle.ToHandle<Model>());
        foreach (var mesh in model.Meshes)
        {
            gl.DeleteVertexArray(mesh.Vao.Handle);
            gl.DeleteBuffer(mesh.Vbo.Handle);
            gl.DeleteBuffer(mesh.Ebo.Handle);
        }
    }
}