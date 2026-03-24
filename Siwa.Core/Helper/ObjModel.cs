using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using AssimpMesh = Silk.NET.Assimp.Mesh;

namespace Siwa.Core.Helper;

public unsafe class ObjModel : IRenderable
{
    public readonly List<Mesh> Meshes = new();
    private readonly Components.Material[] _materials;
    

    public ObjModel(Assimp assimp, string modelPath)
    {
        // importing the model using assimp
        var scene = assimp.ImportFile(modelPath, (uint)PostProcessSteps.Triangulate);

        // making sure everything went ok
        if (scene == null || scene->MFlags == Assimp.SceneFlagsIncomplete || scene->MRootNode == null)
        {
            throw new Exception("Assimp error: Could not load file.");
        }

        _materials = new Components.Material[scene->MNumMaterials];
        
        for(int i = 0; i < scene->MNumMaterials; i++)
        {
            var mat = scene->MMaterials[i];
            for(int j = 0; j < mat->MNumProperties; j++)
            {
                var prop = mat->MProperties[j];
                Console.WriteLine("Key: {0}, Type: {1} Value: {2}", prop->MKey.AsString, prop->MType, Value(prop));
            }
        }

        // LOG: loaded meshes count within the model
        Console.WriteLine("Loaded model with {0} meshes.", scene->MNumMeshes);

        // converting from assimp scene to engine friendly mesh list
        ProcessNode(scene->MRootNode, scene, Matrix4x4.Identity, assimp);

        assimp.FreeScene(scene);
    }

    public object Value(MaterialProperty* property)
    {
        byte[] managedArray = new byte[property->MDataLength];
        Marshal.Copy((IntPtr)property->MData, managedArray, 0, managedArray.Length);
        switch (property->MType)
        {
            case PropertyTypeInfo.String:
                return Encoding.ASCII.GetString(managedArray);
            case PropertyTypeInfo.Float:
                return BitConverter.ToSingle(managedArray);
            case PropertyTypeInfo.Integer:
                return BitConverter.ToInt32(managedArray);
            default: return 0;
        }
    }

    // HINT: AI generated
    private void ProcessNode(Node* node, Scene* scene, Matrix4x4 parentTransform, Assimp assimp)
    {
        // 1. Get this node's local transform and combine with parent
        Matrix4x4 localTransform = node->MTransformation;
        Matrix4x4 globalTransform = parentTransform * localTransform;

        // 2. Process all meshes attached to THIS node
        for (int i = 0; i < node->MNumMeshes; i++)
        {
            uint meshIndex = node->MMeshes[i];
            AssimpMesh* mesh = scene->MMeshes[meshIndex];

            // You'll need to store the 'globalTransform' with the mesh 
            // so you can use it as the 'Model' matrix during render!
            var renderable = new Mesh(mesh)
            {
                Transform = globalTransform
            };

            Meshes.Add(renderable);
        }

        // 3. Recurse for all children
        for (int i = 0; i < node->MNumChildren; i++)
        {
            ProcessNode(node->MChildren[i], scene, globalTransform, assimp);
        }
    }

    public void OnLoad(GL gl)
    {
        _materials[0] = new Components.Material
        {
            Albedo = gl.NewTexture("Assets/SpecularTest/basetexture.jpg"),
            Specular = gl.NewTexture("Assets/SpecularTest/speculartestjpg.jpg", 1, PixelFormat.Red)
        };
        
        _materials[1] = new Components.Material
        {
            Albedo = gl.NewTexture("Assets/SpecularTest/basetexture.jpg"),
            Specular = gl.NewTexture("Assets/SpecularTest/speculartestjpg.jpg", 1, PixelFormat.Red)
        };
        
        // _materials[0] = new Components.Material
        // {
        //     Albedo = gl.NewTexture("Assets/objs/Textures/KARPENTER_GRASSHOPPER_cloth_d1.jpg"),
        //     Specular = gl.NewTexture("Assets/objs/Textures/KARPENTER_GRASSHOPPER_cloth_b.jpg", 1, PixelFormat.Red)
        // };
        //
        // _materials[1] = new Components.Material
        // {
        //     Albedo = gl.NewTexture("Assets/objs/Textures/KARPENTER_GRASSHOPPER_cloth_d2.jpg"),
        //     Specular = gl.NewTexture("Assets/objs/Textures/KARPENTER_GRASSHOPPER_cloth_b.jpg", 1, PixelFormat.Red)
        // };
        //
        // _materials[2] = new Components.Material
        // {
        //     Albedo = gl.NewTexture("Assets/objs/Textures/KARPENTER_GRASSHOPPER_wood_d.jpg"),
        //     Specular = gl.NewTexture("Assets/objs/Textures/KARPENTER_GRASSHOPPER_wood_b.jpg", 1, PixelFormat.Red)
        // };

        foreach (var mesh in Meshes)
        {
            mesh.Material = _materials[mesh.MaterialIndex];
            mesh.OnLoad(gl);
        }
    }
    
    public void OnRender(GL gl, Shader shader)
    {
        // update the model uniform foreach mesh inside the model, render each mesh afterwards
        // TODO: replace with an ecs friendly way by making each mesh its own separate entity ?
        var location = gl.GetUniformLocation(shader.ShaderProgram, "model");
        foreach (var mesh in Meshes)
        {
            var matrix = Matrix4x4.CreateTranslation(mesh.Position);
            gl.UniformMatrix4(location, 1, false, matrix.GetMatrixSpan());
            mesh.OnRender(gl, shader);
        }
    }
    
    public void Dispose(GL gl)
    {
        foreach (var mesh in Meshes)
            mesh.Dispose(gl);
    }
}