using System.Numerics;
using Arch.Core;
using ImGuiNET;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL.Extensions.ImGui;
using Siwa.Core.Assets;
using Siwa.Core.Assets.Kinds;
using Siwa.Core.Components;
using Siwa.Core.Data;
using Siwa.Core.Rendering;
using Siwa.Core.Serialization;
using Siwa.Core.Systems;
using Camera = Siwa.Core.Components.Camera;

namespace Siwa.Core
{
    public sealed class Game : IDisposable
    {
        private readonly IWindow _window;
        private GL? _gl;
        private World? _world;
        private Assimp? _assimp;
        
        private ImGuiController _imGuiController = null!;
        private IInputContext _inputContext = null!;
        private ImGuiSystem _guiSystem = null!;

        private ViewPort _viewPort = null!;

        private ForwardRenderer _renderer = null!;

        private readonly SystemCollection<IRenderSystem> _renderCollection = new();
        private IRenderSystem[] _renderSystems = [];

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
        }

        public void Run()
        {
            _window.Run();
        }

        private void OnStart()
        {
            foreach (var system in _renderSystems)
                system.Start();
        }
        
        private void OnRender(double dt)
        {
            _viewPort.OnRender();
            
            foreach (var system in _renderSystems)
                system.Render();
            
            _viewPort.Unbind();
            
            _imGuiController.Update((float)dt);
            var size = _window.FramebufferSize;
            _gl!.Viewport(0, 0, (uint)size.X, (uint)size.Y);
            _gl.ClearColor(0.22f, 0.22f, 0.22f, 1.0f);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            _guiSystem.Render();
            _imGuiController.Render();
        }
        

        private void OnUpdate(double dt)
        {
            _guiSystem.Update((float)dt);
            if (!_viewPort.IsFocused) return;
            foreach(var system in _renderSystems)
                system.Update((float)dt);
        }

        private void OnLoad()
        {
            _gl = _window.CreateOpenGL();
            if (_gl is null) throw new Exception("Failed to initialize OpenGL context.");
            
            _assimp = Assimp.GetApi();
            if(_assimp is null) throw new Exception("Failed to initialize Assimp context.");
            
            _inputContext = _window.CreateInput();
            
            _renderer = new ForwardRenderer();
            _renderer.AddExtension(new MaterialProcessor(_renderer.RenderPipeline));
            
            SerializationManager.Initialize();
            
            AssetLoader.Initialize(_gl, _assimp, "D:\\SiwaProject");
            AssetLoader.Instance.LoadAssetFiles();
            
            _viewPort = new ViewPort(_gl);
            _viewPort.OnLoad();

            _imGuiController = new ImGuiController(_gl, _window, _inputContext, () =>
            {
                var io = ImGui.GetIO();
                var font = io.Fonts.AddFontFromFileTTF("D:\\SiwaProject\\Engine\\JetBrainsMono-Regular.ttf", 16f);
                io.Fonts.Build();
                ImGuiSystem.Font = font;
                io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
                io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
                ImGuiTheme.Nord();
            });
            
            LoadWorld(_gl);

            _renderSystems = _renderCollection.ToArray();

            foreach (var system in _renderSystems)
                system.Initialize();
            
            OnStart();
        }

        private void LoadWorld(GL gl)
        {
            if (_world is not null) _world.Dispose();
            
            _world = AssetLoader.Instance.LoadWorld("default");
            
            _guiSystem = new ImGuiSystem(_world, _viewPort);

            _renderCollection.Add(0, new CameraSystem(gl, _world, _inputContext, _viewPort));
            _renderCollection.Add(1, new RenderSystem(_world, gl, _renderer));
        }
        
        public void Dispose()
        {
            _imGuiController.Dispose();
            _inputContext.Dispose();
            _window.Dispose();
            _world?.Dispose();
        }
    }
}
