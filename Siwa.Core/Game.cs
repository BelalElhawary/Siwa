using System.Numerics;
using System.Text.Json;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Persistence;
using Arch.System;
using ImGuiNET;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL.Extensions.ImGui;
using Siwa.Core.Components;
using Siwa.Core.Helper;
using Siwa.Core.Systems;
using Camera = Siwa.Core.Components.Camera;
using File = System.IO.File;
using Light = Siwa.Core.Helper.Light;
using Shader = Siwa.Core.Helper.Shader;

namespace Siwa.Core
{
    public sealed unsafe class Game : IDisposable
    {
        private readonly IWindow _window;
        private GL _gl = null!;
        private World _world = null!;
        private Assimp _assimp = null!;
        
        // Input and UI
        private ImGuiController _imGuiController = null!;
        private IInputContext _inputContext = null!;

        //protected IKeyboard? PrimaryKeyboard;
        //protected IMouse? PrimaryMouse;

        private Shader _shader;
        private Shader _lightShader;
        // private ObjModel _renderable = null!;
        private Light _light;
        private Entity _camera;

        private CameraSystem _cameraSystem;
        private RenderSystem _renderSystem;
        private BaseSystem<World, float>[] _systems = [];

        private float a = 3.0f, b = 0.7f;

        public Game()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "Siwa Game";
            options.VSync = true;
            options.ShouldSwapAutomatically = true;
            options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Default, new APIVersion(3, 3));
            _window = Window.Create(options);
            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Resize += OnResize;
        }

        private void OnResize(Vector2D<int> size)
        {
            _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
            _world.Query(new QueryDescription().WithAny<Camera>(), (Entity entity, ref Camera camera) =>
            {
                camera.Width = size.X;
                camera.Height = size.Y;
            });
        }

        public void Run()
        {
            _window.Run();
        }

        private void OnStart()
        {
            var modelHandle = AssetLoader.Instance.GetAssetHandle<ModelAsset>("2b3802e4-9d51-476b-99f4-d66e2ed9871a");
            var tableEntity = _world.Create(new Model { ModelHandle = modelHandle }, new Transform() { Position = new Vector3() });
            var secondTableEntity = _world.Create(new Model { ModelHandle = modelHandle }, new Transform() { Position = new Vector3() });
        }
        
        private void OnRender(double dt)
        {
            var size = _window.FramebufferSize;
            _gl.ClearColor(0.529f, 0.811f, 0.921f, 1.0f);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _cameraSystem.UpdateMatrixQuery(_world);
            
            // render code
            _shader.Activate(_gl);
            ref var cameraComponent = ref _camera.TryGetRef<Camera>(out bool exists);
            if(exists)
                _gl.Uniform3(_gl.GetUniformLocation(_shader.ShaderProgram, "camPos"), cameraComponent.Position);
            _gl.Uniform1(_gl.GetUniformLocation(_shader.ShaderProgram, "a"), a);
            _gl.Uniform1(_gl.GetUniformLocation(_shader.ShaderProgram, "b"), b);
            
            _cameraSystem.MatrixQuery(_world, _shader);
            _light.SupplyColorUniforms(_gl, _shader);
            _renderSystem.Render((float)dt);
            // _renderable.OnRender(_gl, _shader);
            
            _lightShader.Activate(_gl);
            _cameraSystem.MatrixQuery(_world, _lightShader);
            _light.OnRender(_gl, _lightShader);
            
            foreach(var system in _systems)
                system.AfterUpdate((float)dt);

            OnImGuiRender((float)dt);
            _imGuiController.Render();
        }
        
        private float[] _fpsHistory = new float[100];
        private double _historyUpdateTimer;
        private float _fps;
        
        private void OnImGuiRender(float delta)
        {
            ref var camera = ref _camera.TryGetRef<Camera>(out var exists);
            if(!exists) return;
            ImGui.Begin("Camera");
            ImGui.InputFloat3("Position", ref camera.Position);
            ImGui.InputFloat("Speed", ref camera.Speed);
            ImGui.InputFloat("Sensitivity", ref camera.Sensitivity);
            ImGui.End();
            
            _renderSystem.RenderImGui();
            
            ImGui.Begin("Light");
            ImGui.InputFloat3("Position", ref _light.Position);
            ImGui.ColorEdit4("Color", ref _light.Color);
            ImGui.DragFloat("A", ref a, 0.01f);
            ImGui.DragFloat("B", ref b, 0.01f);
            ImGui.End();
            
            // ImGui.Begin("Obj");
            // for(int i = 0; i < _renderable.Meshes.Count; i++)
            // {
            //     ImGui.InputFloat3(i + " Mesh Position", ref _renderable.Meshes[i].Position);
            // }
            // ImGui.End();

            
            ImGui.Begin("World");
            if(ImGui.Button("Load"))
                LoadWorld();
            if(ImGui.Button("Save"))
                SaveWorld();
            ImGui.End();
            
            
            // 4. Create the FPS Debug Window
            ImGui.Begin("Debug Metrics");
            ImGui.Text($"FPS: {_fps:F1}"); // F1 for 1 decimal place
            ImGui.Text($"Frame Time: {delta * 1000:F2} ms");
            
    
            // Inside your Update or Render loop
            _historyUpdateTimer += delta;

            if (_historyUpdateTimer >= 0.1) // Update 10 times per second
            {
                // 1. Shift all elements to the left
                for (int i = 1; i < _fpsHistory.Length - 1; i++)
                {
                    _fpsHistory[i] = _fpsHistory[i + 1];
                }

                _fps = (float)(1.0 / delta);
                
                // 2. Add the new FPS value to the very end
                _fpsHistory[_fpsHistory.Length - 1] = _fps;
    
                _historyUpdateTimer = 0;
            }
            
            // Optional: Simple FPS Graph
            // (Note: You'd need to store a history array for a moving graph)
            ImGui.PlotLines("Performance", ref _fpsHistory[0], _fpsHistory.Length);
    
            ImGui.End();
        }

        private void OnUpdate(double dt) 
        {
            _imGuiController.Update((float)dt);
            _cameraSystem.Update((float)dt);
            foreach(var system in _systems)
                system.Update((float)dt);
        }

        private void OnLoad()
        {
            _gl = _window.CreateOpenGL();
            _gl.Enable(EnableCap.DepthTest);
            // _gl.Disable(EnableCap.CullFace);
            
            _assimp = Assimp.GetApi();
            AssetLoader.Initialize(_gl, _assimp);
            
            _shader = new Shader(_gl, "Shaders/shader.frag", "Shaders/shader.vert");
            _shader.Activate(_gl);
            
            _lightShader = new Shader(_gl, "Shaders/light.frag", "Shaders/light.vert");
            _lightShader.Activate(_gl);
            _light = new Light();
            _light.OnLoad(_gl);

            if (_gl is null) throw new Exception("Failed to initialize OpenGL context.");

            _inputContext = _window.CreateInput();
            _imGuiController = new ImGuiController(_gl, _window, _inputContext);

            AssetLoader.Instance.LoadAssetFiles();
            
            
            LoadWorld();
            

            foreach (var system in _systems)
                system.Initialize();
            
            
            OnStart();
        }

        private const string DefaultWorld = "Assets/default.world";
        private void LoadWorld()
        {
            if(_world is not null) _world.Dispose();
            
            if (File.Exists(DefaultWorld))
            {
                var serializer = new ArchJsonSerializer();
                _world = serializer.Deserialize(File.ReadAllBytes(DefaultWorld));
            }
            else
            {
                _world = World.Create();   
            }
            
            _cameraSystem = new CameraSystem(_gl, _world, _inputContext);
            _renderSystem = new RenderSystem(_world, _gl, _shader);
            _renderSystem.Initialize();
            
            
            var camera = Ecs.CreateCamera(_window.Size.X, _window.Size.Y, new Vector3(0, 0, 3f));
            _camera = _world.Create(camera);
        }

        private void SaveWorld()
        {
            var serializer = new ArchJsonSerializer();
            var buffer = serializer.Serialize(_world);
            File.WriteAllBytes(DefaultWorld, buffer);
        }
        
        public void Dispose()
        {
            _shader.Delete(_gl);
            _imGuiController.Dispose();
            _inputContext.Dispose();
            _window.Dispose();
            _world.Dispose();
        }
    }
}
