using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using ImGuiNET;
using Siwa.Core.Assets;
using Siwa.Core.Assets.Kinds;
using Siwa.Core.Components;
using Siwa.Core.Data;
using Siwa.Core.Editor;
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

    private bool _includeReadonly = false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Assets()
    {
        ImGui.Begin("Assets");
        ImGui.Checkbox("Show Engine Assets", ref _includeReadonly);

        // 1. Calculate Grid Math
        float thumbnailSize = 64.0f;
        float padding = 16.0f;
        float cellSize = thumbnailSize + padding;
        float panelWidth = ImGui.GetContentRegionAvail().X;
    
        // Ensure we always have at least 1 column to prevent ImGui crashes
        int columnCount = Math.Max(1, (int)(panelWidth / cellSize));

        // 2. Draw the Grid
        if (ImGui.BeginTable("AssetBrowserGrid", columnCount))
        {
            foreach (var asset in _assets.Where(a => _includeReadonly || !a.Readonly))
            {
                ImGui.TableNextColumn();
                ImGui.PushID(HashCode.Combine(asset.GetType(), asset.Handle)); // Ensure unique IDs per asset

                IntPtr icon = GetAssetIcon(asset);
                bool isSelected = _selectedAsset == asset;

                // Draw the visual component
                if (DrawGridItem(asset.Name, icon, thumbnailSize, isSelected))
                {
                    _selectedAsset = asset;
                }

                // Handle the backend drag-and-drop logic
                HandleAssetDragDrop(asset);

                ImGui.PopID();
            }
            ImGui.EndTable();
        }
    
        ImGui.End();
    }
    
    private Entity _lastSelectedObject; // Track whatever your _selected object is
    private Vector3 _inspectorEulerAngles;
    private Quaternion _lastKnownRotation;
    
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
        foreach (var component in components)
        {
            if(component is null) continue;
            switch (component)
            {
                case Transform:
                    if(ImGui.CollapsingHeader("Transform"))
                    {
                        ref Transform reference = ref _selected.Value.Get<Transform>();
                        ImGui.InputVector3("Position", ref reference.Position);
        
                        // 1. Did we select a new object? OR did the game's physics/code change the rotation?
                        // We use a tiny threshold for quaternion comparison to avoid floating point micro-jitters
                        bool rotationChangedExternally = MathF.Abs(Quaternion.Dot(reference.Rotation, _lastKnownRotation)) < 0.9999f;

                        if (_selected.Value != _lastSelectedObject || rotationChangedExternally)
                        {
                            // Only extract from the Quaternion if we HAVE to
                            _inspectorEulerAngles = reference.Rotation.ToEuler();
                            _lastSelectedObject = _selected.Value;
                            _lastKnownRotation = reference.Rotation;
                        }

                        // 2. Drive the UI with our CACHED Euler angles
                        // "%.2f°"
                        if (ImGui.InputVector3("Rotation", ref _inspectorEulerAngles))
                        {
                            // 3. User dragged the slider! Calculate the new Quaternion
                            float radX = _inspectorEulerAngles.X * (MathF.PI / 180f);
                            float radY = _inspectorEulerAngles.Y * (MathF.PI / 180f);
                            float radZ = _inspectorEulerAngles.Z * (MathF.PI / 180f);

                            reference.Rotation = Quaternion.Normalize(Quaternion.CreateFromYawPitchRoll(radY, radX, radZ));
            
                            // 4. IMMEDIATELY update our known rotation so the external change detector (Step 1) 
                            // doesn't trigger on the next frame and ruin our nice Euler values
                            _lastKnownRotation = reference.Rotation;
                        }

                        ImGui.InputVector3("Scale", ref reference.Scale, 1f);
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
                        ImGui.InputHandle(nameof(Renderable.Model), ref reference.Model);
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
                ImGui.InputHandle("Shader", ref material.Shader);
            } break;
            case LitMaterialAsset lit:
            {
                ImGui.InputText("Name", ref lit.Name, 256);
                ref var material = ref AssetPool<LitMaterial>.Registry.Get(lit.Handle.ToHandle<LitMaterial>());
                ImGui.InputHandle("Shader", ref material.Shader);
                ImGui.InputHandle("Albedo", ref material.AlbedoTexture);
                ImGui.InputHandle("Specular", ref material.SpecularTexture);
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
                    ImGui.InputMaterialHandle($"Mesh {i} Material", ref materialHandle);
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

    // Helper 1: Resolves the icon cleanly
    private IntPtr GetAssetIcon(Asset asset)
    {
        return asset switch
        {
            ModelAsset => Images.ModelIcon,
            ShaderAsset => Images.ShaderIcon,
            LitMaterialAsset => Images.MaterialIcon,
            UnlitMaterialAsset => Images.MaterialIcon,
            TextureAsset tex => (IntPtr)AssetPool<Texture>.Registry.Get(tex.Handle.ToHandle<Texture>()).Handle,
            _ => IntPtr.Zero
        };
    }

    // Helper 2: Draws a beautiful, selectable grid item with centered text
    private bool DrawGridItem(string name, IntPtr icon, float size, bool isSelected)
    {
        bool clicked = false;
        ImGui.BeginGroup();

        // 1. Selection Highlight (Tint the button background if selected)
        Vector4 bgColor = isSelected ? new Vector4(0.2f, 0.4f, 0.8f, 1.0f) : new Vector4(0, 0, 0, 0);
        ImGui.PushStyleColor(ImGuiCol.Button, bgColor);
        
        // Transparent background for normal state, highlighted if selected
        if (ImGui.ImageButton("##icon", icon, new Vector2(size, size)))
        {
            clicked = true;
        }
        ImGui.PopStyleColor();

        // 2. Text Formatting (Centered below icon)
        // Truncate long names to prevent breaking the grid layout
        string displayName = name.Length > 12 ? name.Substring(0, 10) + "..." : name;
        
        float textWidth = ImGui.CalcTextSize(displayName).X;
        float textOffset = (size - textWidth) * 0.5f; // Center math
        
        if (textOffset > 0) 
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + textOffset);
        }
        
        ImGui.Text(displayName);

        // Optional: Add a tooltip so users can read the full name on hover
        if (ImGui.IsItemHovered() && name.Length > 12)
        {
            ImGui.SetTooltip(name);
        }

        ImGui.EndGroup();
        return clicked;
    }

    // Helper 3: Centralized Drag & Drop Data Packaging
    private unsafe void HandleAssetDragDrop(Asset asset)
    {
        if (!ImGui.BeginDragDropSource()) return;

        if (asset is UnlitMaterialAsset or LitMaterialAsset)
        {
            var handleValue = new MaterialHandle
            {
                Handle = asset.Handle,
                Type = asset is LitMaterialAsset ? MaterialType.Lit : MaterialType.Unlit
            };
            ImGui.SetDragDropPayload(nameof(MaterialHandle), (IntPtr)(&handleValue), (uint)sizeof(MaterialHandle));
        }
        else if (asset is ModelAsset) SetStandardPayload<Model>(asset);
        else if (asset is ShaderAsset) SetStandardPayload<Shader>(asset);
        else if (asset is TextureAsset) SetStandardPayload<Texture>(asset);

        // This renders the text next to the mouse cursor WHILE dragging
        ImGui.Text($"Move {asset.Name}"); 
        ImGui.EndDragDropSource();
    }

    private unsafe void SetStandardPayload<T>(Asset asset) where T : struct
    {
        long handleValue = RawHandle.ToLong(asset.Handle);
        ImGui.SetDragDropPayload(typeof(T).Name, (IntPtr)(&handleValue), sizeof(long));
    }
}