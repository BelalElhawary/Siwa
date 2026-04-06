using Silk.NET.Windowing;
using Siwa.Core.Components;
using SkiaSharp;
using System;

namespace Siwa.Core.Systems;

public class UiSystem : IRenderSystem, IDisposable
{
    private readonly IWindow _window;
    private readonly ViewPort _viewPort;
    private GRContext? _grContext;
    private SKSurface? _surface;
    private uint _lastWidth;
    private uint _lastHeight;

    public UiSystem(IWindow window, ViewPort viewPort)
    {
        _window = window;
        _viewPort = viewPort;
    }

    public void Initialize()
    {
        var glInterface = GRGlInterface.Create(name => _window.GLContext!.TryGetProcAddress(name, out var addr) ? addr : IntPtr.Zero);
        glInterface.Validate();
        _grContext = GRContext.CreateGl(glInterface);
    }

    public void Start()
    {
    }

    public void Update(float dt)
    {
    }

    public void Render()
    {
        if (_viewPort.Width == 0 || _viewPort.Height == 0) return;

        if (_surface == null || _lastWidth != _viewPort.Width || _lastHeight != _viewPort.Height)
        {
            _surface?.Dispose();
            
            // 0x8058 corresponds to GL_RGBA8
            var glInfo = new GRGlFramebufferInfo(_viewPort.FboId, 0x8058); 
            var renderTarget = new GRBackendRenderTarget((int)_viewPort.Width, (int)_viewPort.Height, 0, 8, glInfo);
            
            _surface = SKSurface.Create(_grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
            
            _lastWidth = _viewPort.Width;
            _lastHeight = _viewPort.Height;
        }

        if (_grContext == null || _surface == null) return;

        // Reset the graphics context so Skia doesn't assume its internal state hasn't been changed by Silk.NET
        _grContext.ResetContext();

        var canvas = _surface.Canvas;

        // flush the Skia commands to ensure all drawing is done before Silk.NET tries to render
        // TODO: implement layout and actual UI rendering here instead of just clearing the screen

        // Required to flush the canvas commands to OpenGL
        canvas.Flush();
    }

    public void Dispose()
    {
        _surface?.Dispose();
        _grContext?.Dispose();
    }
}
