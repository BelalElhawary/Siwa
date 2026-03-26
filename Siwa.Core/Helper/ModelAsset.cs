using System.Numerics;
using System.Text.Json.Serialization;
using Silk.NET.Assimp;
using Siwa.Core.Assets;

namespace Siwa.Core.Helper;

public class ModelAsset : Asset
{
    [JsonInclude]
    public string ModelPath = null!;
    [JsonInclude]
    public Guid[] Materials = [];
    
    [JsonIgnore]
    public readonly List<Mesh> Meshes = [];
    [JsonIgnore]
    private Handle<MaterialAsset>[] _tempMaterialAssets = [];
    

    public override void OnLoad()
    {
        _tempMaterialAssets = Materials.Select(m => 
            AssetLoader.Instance.AssetGuidLookupDictionary[m].ToHandle<MaterialAsset>()).ToArray();
        
        LoadModel();

        _tempMaterialAssets = [];
    }

    private unsafe void LoadModel()
    {
        var scene = AssetLoader.Instance.Assimp.ImportFile(ModelPath, (uint)PostProcessSteps.Triangulate);
        
        if (scene == null || scene->MFlags == Assimp.SceneFlagsIncomplete || scene->MRootNode == null)
        {
            throw new Exception("Assimp error: Could not load file.");
        }
        
        Console.WriteLine("Loaded model with {0} meshes.", scene->MNumMeshes);

        ProcessNode(scene->MRootNode, scene, Matrix4x4.Identity);

        AssetLoader.Instance.Assimp.FreeScene(scene);
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

            // You'll need to store the 'globalTransform' with the mesh 
            // so you can use it as the 'Model' matrix during render!
            var renderable = new Mesh(mesh)
            {
                Transform = globalTransform
            };

            renderable.Material = _tempMaterialAssets[renderable.MaterialIndex];
            
            renderable.OnLoad(AssetLoader.Instance.Gl);

            Meshes.Add(renderable);
        }

        // 3. Recurse for all children
        for (int i = 0; i < node->MNumChildren; i++)
        {
            ProcessNode(node->MChildren[i], scene, globalTransform);
        }
    }

    protected override void OnUnload()
    {
        foreach (var mesh in Meshes)
        {
            mesh.Dispose(AssetLoader.Instance.Gl);
        }
    }
}