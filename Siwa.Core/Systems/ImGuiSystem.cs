using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using ImGuiNET;
using Siwa.Core.Assets;
using Siwa.Core.Assets.Kinds;
using Siwa.Core.Components;
using Siwa.Core.Data;
using Siwa.Core.Helper;

namespace Siwa.Core.Systems;

public class ImGuiSystem(World world, ViewPort viewPort)
{
    private Entity? _selected;
    private Asset? _selectedAsset;
    private float _fps;
    private float _frameTime;
    private double _historyUpdateTimer;
    private readonly float[] _fpsHistory = new float[100];
    private readonly List<Asset> _assets = AssetLoader.Instance.GetAssets();
    public static ImFontPtr Font;
    
    public void Update(float delta)
    {
        // Inside your Update or Render loop
        _historyUpdateTimer += delta;

        if (!(_historyUpdateTimer >= 0.1)) return; // Update 10 times per second
            
        // 1. Shift all elements to the left
        for (int i = 1; i < _fpsHistory.Length - 1; i++)
        {
            _fpsHistory[i] = _fpsHistory[i + 1];
        }

        _fps = (float)(1.0 / delta);
                
        // 2. Add the new FPS value to the very end
        _fpsHistory[^1] = _fps;
    
        _historyUpdateTimer = 0;

        _frameTime = delta * 1000;
    }

    public void Render()
    {
        ImGui.PushFont(Font);
        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking;
        windowFlags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize |
                       ImGuiWindowFlags.NoMove;
        windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        ImGui.Begin("MainDockSpace", windowFlags);
        ImGui.PopStyleVar(3);

        // 2. Create the DockSpace ID
        var dockSpaceId = ImGui.GetID("MyDockSpace");
        ImGui.DockSpace(dockSpaceId, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        MenuBar();
        SceneViewport();
        Hierarchy();
        Inspector();
        Assets();
        AssetEditor();

        ImGui.End();
        ImGui.PopFont();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SceneViewport()
    {
        ImGui.Begin("Scene");
        {
            viewPort.IsFocused = ImGui.IsWindowFocused();
            // Get the size of the ImGui window content area
            Vector2 size = ImGui.GetContentRegionAvail();

            // Update your viewportWidth/Height here for the next frame to handle resizing
            if ((uint)size.X != viewPort.Width || (uint)size.Y != viewPort.Height)
            {
                viewPort.Rescale((uint)size.X, (uint)size.Y);
            }

            // Silk.NET uses (IntPtr) for the texture handle in ImGui.Image
            // NOTE: OpenGL textures are upside down in ImGui by default, 
            // so we flip the UVs (0,1) to (1,0)
            ImGui.Image((IntPtr)viewPort.TextureColorBuffer, size, new Vector2(0, 1), new Vector2(1, 0));
        }
        ImGui.End();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MenuBar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Save World"))
                {
                    AssetLoader.Instance.SaveWorld("default", world);
                }

                if (ImGui.MenuItem("Exit"))
                {
                    Environment.Exit(0);
                }

                ImGui.EndMenu();
            }

            ImGui.Spacing();
            ImGui.Text($"FPS: {_fps:F1}"); // F1 for 1 decimal place
            ImGui.Text($"Frame Time: {_frameTime:F2} ms");
            ImGui.PlotLines("Performance", ref _fpsHistory[0], _fpsHistory.Length);
            ImGui.EndMenuBar();
        }
    }

    private readonly QueryDescription _queryHierarchy = new QueryDescription().WithAll<Tag>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Hierarchy()
    {
        ImGui.Begin("Hierarchy");
        if (ImGui.TreeNode("Root"))
        {
            world.Query(_queryHierarchy, (Entity entity, ref Tag tag) =>
            {
                ImGui.TreeNodeEx(tag.Name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.SpanAvailWidth);
                if (ImGui.IsItemClicked())
                {
                    _selected = entity;
                }
                ImGui.TreePop();
            });
            ImGui.TreePop();
        }
        ImGui.End();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Assets()
    {
        ImGui.Begin("Assets");
        foreach (var asset in _assets)
        {
            ImGui.BeginGroup();
            switch (asset)
            {
                case ModelAsset model:
                    ImGuiDraggableAsset<Model>(model, Images.ModelIcon);
                    break;
                case ShaderAsset shader:
                    ImGuiDraggableAsset<Shader>(shader, Images.ShaderIcon);
                    break;
                case UnlitMaterialAsset unlit:
                    ImGuiDraggableMaterialAsset(unlit, Images.MaterialIcon, MaterialType.Unlit);
                    break;
                case LitMaterialAsset lit:
                    ImGuiDraggableMaterialAsset(lit, Images.MaterialIcon, MaterialType.Lit);
                    break;
                case TextureAsset texture:
                    ref var handle = ref AssetPool<Texture>.Registry.Get(texture.Handle.ToHandle<Texture>());
                    ImGuiDraggableAsset<Texture>(texture, (IntPtr)handle.Handle);
                    break;
            }
            ImGui.SameLine();
            ImGui.Text(asset.Name);
            ImGui.EndGroup();
        }
        ImGui.End();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Inspector()
    {
        if (!_selected.HasValue)
        {
            ImGui.Begin("Inspector");
            ImGui.Text("No entity selected");
            ImGui.End();
            return;
        }
        
        var components = _selected.Value.GetAllComponents();
        ImGui.Begin("Inspector");
        ImGui.Text("Entity ID: " + _selected.Value.Id);
        ref var tag = ref _selected.Value.TryGetRef<Tag>(out var exists);
        if(exists) ImGui.InputText(nameof(Tag.Name), ref tag.Name, 255);
        for (int i = 0; i < components.Length; i++)
        {
            if(components[i] is null) continue;
            switch (components[i])
            {
                case Transform: 
                    if(ImGui.CollapsingHeader(nameof(Transform)))
                    {
                        ref Transform reference = ref _selected.Value.Get<Transform>();
                        ImGui.InputFloat3(nameof(Transform.Position), ref reference.Position);
                        Vector3 eulerDegrees = reference.Rotation.ToEuler();
                        // 2. Use InputFloat3 for a better user experience
                        if (ImGui.InputFloat3("Rotation", ref eulerDegrees))
                        {
                            // 3. Convert Degrees back to Radians
                            float radX = eulerDegrees.X * ((float)Math.PI / 180f);
                            float radY = eulerDegrees.Y * ((float)Math.PI / 180f);
                            float radZ = eulerDegrees.Z * ((float)Math.PI / 180f);

                            // 4. Create the new Quaternion (Order: Yaw, Pitch, Roll)
                            reference.Rotation = Quaternion.CreateFromYawPitchRoll(radY, radX, radZ);
                        }
                        ImGui.InputFloat3(nameof(Transform.Scale), ref reference.Scale);
                    } 
                    break;
                case Camera:
                    if(ImGui.CollapsingHeader(nameof(Camera)))
                    { }
                    break;
                case CameraMovement:
                    if(ImGui.CollapsingHeader(nameof(CameraMovement)))
                    {
                        ref CameraMovement reference = ref _selected.Value.Get<CameraMovement>();
                        ImGui.InputFloat(nameof(CameraMovement.Speed), ref reference.Speed);
                        ImGui.InputFloat(nameof(CameraMovement.Sensitivity), ref reference.Sensitivity);
                    }
                    break;
                case Renderable:
                    if(ImGui.CollapsingHeader(nameof(Renderable)))
                    {
                        ref Renderable reference = ref _selected.Value.Get<Renderable>();
                        ImGuiHandle(nameof(Renderable.Model), ref reference.Model);
                    } 
                    break;
            }
        }
        ImGui.End();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssetEditor()
    {
        if (!ImGui.Begin("Asset Editor")) return;

        if (_selectedAsset == null)
        {
            ImGui.Text("No asset selected");
            goto AssetEditor_End;
        }

        switch (_selectedAsset)
        {
            case UnlitMaterialAsset unlit:
            {
                ImGui.InputText("Name", ref unlit.Name, 256);
                ref var material = ref AssetPool<UnlitMaterial>.Registry.Get(unlit.Handle.ToHandle<UnlitMaterial>());
                ImGui.ColorEdit4("Color", ref material.Color);
                ImGuiHandle("Shader", ref material.Shader);
            } break;
            case LitMaterialAsset lit:
            {
                ImGui.InputText("Name", ref lit.Name, 256);
                ref var material = ref AssetPool<LitMaterial>.Registry.Get(lit.Handle.ToHandle<LitMaterial>());
                ImGuiHandle("Shader", ref material.Shader);
                ImGuiHandle("Albedo", ref material.AlbedoTexture);
                ImGuiHandle("Specular", ref material.SpecularTexture);
                ImGui.ColorEdit4("Color", ref material.Color);
                ImGui.InputFloat3("Light Position",  ref material.LightPosition);
                ImGui.InputFloat("Light Range",  ref material.LightRange);
                ImGui.InputFloat("Light Falloff", ref material.LightFalloff);
            } break;
            case ModelAsset model:
            {
                ImGui.InputText("Name", ref model.Name, 256);
                ref var reference = ref AssetPool<Model>.Registry.Get(model.Handle.ToHandle<Model>());
                for (int i = 0; i < reference.Meshes.Length; i++)
                {
                    ref MaterialHandle materialHandle = ref reference.Meshes[i].Material;
                    ImGuiMaterialHandle($"Mesh {i} Material", ref materialHandle);
                }
                
            } break;
            case TextureAsset texture:
            {
                ref var reference = ref AssetPool<Texture>.Registry.Get(texture.Handle.ToHandle<Texture>());
                ImGui.Image((IntPtr)reference.Handle, new Vector2(150, 150));
                ImGui.InputText("Name", ref texture.Name, 256);
            } break;
            case ShaderAsset shader:
            {
                ImGui.Text($"{nameof(ShaderAsset)}: {shader.Name}");
            } break;
        }

        AssetEditor_End:
        ImGui.End();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ImGuiDraggableMaterialAsset(Asset asset, IntPtr icon, MaterialType materialType)
    {
        var clicked = ImGui.ImageButton(asset.Name, icon, new Vector2(50, 50));
        
        if (ImGui.BeginDragDropSource())
        {
            var handleValue = new MaterialHandle
            {
                Handle = asset.Handle,
                Type = materialType
            };
            ImGui.SetDragDropPayload(nameof(MaterialHandle), (IntPtr)(&handleValue), (uint)sizeof(MaterialHandle));
            ImGui.Text(asset.Name);
            ImGui.EndDragDropSource();
        }

        if (!clicked) return;

        _selectedAsset = asset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ImGuiHandle<T>(string label, ref Handle<T> handle) where T : struct
    {
        ImGui.Text(label);
        ImGui.SameLine();
    
        // Create a read-only selectable or a button to act as the "Slot"
        ImGui.Button(typeof(T).Name + Handle<T>.ToLong(handle), new Vector2(ImGui.GetContentRegionAvail().X, 0));

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload(typeof(T).Name);
            unsafe
            {
                if (payload.NativePtr != null)
                {
                    var uid = *(long*)payload.Data;
                    handle = Handle<T>.FromLong(uid);
                }
            }
            ImGui.EndDragDropTarget();
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ImGuiDraggableAsset<T>(Asset asset, IntPtr icon) where T : struct
    {
        var clicked = ImGui.ImageButton(asset.Name, icon, new Vector2(50, 50));
        
        if (ImGui.BeginDragDropSource())
        {
            long handleValue = RawHandle.ToLong(asset.Handle);
            ImGui.SetDragDropPayload(typeof(T).Name, (IntPtr)(&handleValue), sizeof(long));
            ImGui.Text(asset.Name);
            ImGui.EndDragDropSource();
        }

        if (!clicked) return;

        _selectedAsset = asset;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ImGuiMaterialHandle(string label, ref MaterialHandle handle)
    {
        ImGui.Text(label);
        ImGui.SameLine();
    
        ImGui.Button(RawHandle.ToLong(handle.Handle).ToString(), new Vector2(ImGui.GetContentRegionAvail().X, 0));

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload(nameof(MaterialHandle));
            unsafe
            {
                if (payload.NativePtr != null)
                {
                    var material = *(MaterialHandle*)payload.Data;
                    handle.Handle = new RawHandle(material.Handle.Index, material.Handle.Generation);
                    handle.Type =  material.Type;
                }
            }
            ImGui.EndDragDropTarget();
        }
    }
}