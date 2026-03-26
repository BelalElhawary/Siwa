using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using ImGuiNET;
using Silk.NET.OpenGL;
using Siwa.Core.Assets;
using Siwa.Core.Components;
using Siwa.Core.Helper;
using Shader = Siwa.Core.Helper.Shader;

namespace Siwa.Core.Systems;

public partial class RenderSystem(World world, GL gl, Shader shader) : BaseSystem<World, float>(world)
{
    private Entity? _selected;
    private ModelAsset[] _allModels = [];
    private string[] _allModelNames = [];
    private readonly World _world = world;

    public override void Initialize()
    {
        _allModels = AssetLoader.Instance.Assets.OfType<ModelAsset>().ToArray();
        _allModelNames = _allModels.Select(a => a.Name).ToArray();
    }

    public void Render(in float dt)
    {
        RenderMeshQuery(_world);
    }

    public void RenderImGui()
    {
        ImGui.Begin("Hierarchy");
        if (ImGui.TreeNode("Root"))
        {
            RenderImGuiMenuQuery(_world);
            ImGui.TreePop();
        }
        ImGui.End();
        
        if (!_selected.HasValue) return;
        ImGui.Begin("Inspector");
        ref var model = ref _selected.Value.Get<Model>();
        ref var transform = ref _selected.Value.Get<Transform>();
        // ImGui.InputText("Name", ref model.)
        for (int i = 0; i < _allModels.Length; i++)
        {
            if(ImGui.Button(_allModelNames[i]))
            {
                model.ModelHandle =  _allModels[i].Handle.ToHandle<ModelAsset>();
            }
        }
        ImGui.InputFloat3("Position", ref transform.Position);
        ImGui.End();
    }
    
    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderMesh(ref Model model, ref Transform transform)
    {
        var modelAsset = AssetPool<ModelAsset>.Registry.Get(model.ModelHandle);
        if (modelAsset == null) return;
        var location = gl.GetUniformLocation(shader.ShaderProgram, "model");
        foreach (var mesh in modelAsset.Meshes)
        {
            var matrix = Matrix4x4.CreateTranslation(transform.Position);
            gl.UniformMatrix4(location, 1, false, matrix.GetMatrixSpan());
            mesh.OnRender(gl, shader);
        }
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderImGuiMenu(Entity entity, ref Model model)
    {
        var modelAsset = AssetPool<ModelAsset>.Registry.Get(model.ModelHandle);
        if (modelAsset == null) return;
        ImGui.TreeNodeEx(modelAsset.Name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.SpanAvailWidth);
        if (ImGui.IsItemClicked())
        {
            _selected = entity;
        } 
        ImGui.TreePop();
    }
}