using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Arch.Core;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Siwa.Core.Components;
using Siwa.Core.Rendering.Uniforms;

namespace Siwa.Core.Systems;

public class CameraSystem2D(GL gl, World world, IInputContext inputContext, ViewPort viewPort) : IRenderSystem
{
    private readonly uint _ubo = gl.GenBuffer();
    private readonly IMouse _mouse = inputContext.Mice[0];
    private readonly IKeyboard _keyboard = inputContext.Keyboards[0];
    private readonly nuint _cameraUniformDataSize = (nuint)Marshal.SizeOf<CameraUniformData>();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Initialize()
    {
        gl.BindBuffer(BufferTargetARB.UniformBuffer, _ubo);
        
        // Allocate memory (144 bytes based on the struct above)
        gl.BufferData(BufferTargetARB.UniformBuffer, _cameraUniformDataSize, null, BufferUsageARB.DynamicDraw);
    
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

        // Bind this buffer to "Binding Point 0"
        gl.BindBufferBase(BufferTargetARB.UniformBuffer, 0, _ubo);
    }
    
    public void Render()
    {
        RenderCamera2D();
    }

    public void Start() { }

    public void Update(float dt)
    {
        UpdateCamera2DMovement(dt);
    }

    private readonly QueryDescription _queryRenderCameraPerspective = new QueryDescription().WithAll<Transform2D, Camera2D>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderCamera2D()
    {
        world.Query(in _queryRenderCameraPerspective, (ref Transform2D transform, ref Camera2D camera) =>
        {
            camera.Width = (int)viewPort.Width;
            camera.Height = (int)viewPort.Height;

            // 1. Projection: Mapping screen space (0,0 to Width,Height)
            // Adjusting bottom/top swaps between Y-up or Y-down (Standard 2D is Y-down)
            var projection = Matrix4x4.CreateOrthographicOffCenter(
                0,                 // Left
                camera.Width,      // Right
                camera.Height,     // Bottom (Y-down)
                0,                 // Top
                -1.0f,             // Near plane
                1.0f               // Far plane
            );

            // 2. View: The "Camera" transform
            // In 2D, we use Position.XY and Rotation.Z (Roll)
            // We negate Position because moving the camera right = moving the world left
            var translation = Matrix4x4.CreateTranslation(-transform.Position.X, -transform.Position.Y, 0);
            var rotation = Matrix4x4.CreateRotationZ(-transform.Rotation); // Using Z for 2D rotation
            var scale = Matrix4x4.CreateScale(transform.Scale.X, transform.Scale.Y, 1.0f); // Use Scale for Zoom

            // 3. Center the camera: If you want (0,0) to be the camera's focus point
            var centerOffset = Matrix4x4.CreateTranslation(camera.Width / 2f, camera.Height / 2f, 0);

            // Final Matrix: Translate -> Rotate -> Scale -> Offset to Center
            var view = translation * rotation * scale * centerOffset;

            var uniform = new CameraUniformData
            {
                CameraMatrix = view * projection,
                CameraPosition = transform.Position.AsVector3()
            };

            gl.BindBuffer(BufferTargetARB.UniformBuffer, _ubo);
            gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, _cameraUniformDataSize, in uniform);
            gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
        });
    }

    private readonly QueryDescription _queryUpdateCameraMovement = new QueryDescription().WithAll<Transform2D, Camera2D, CameraMovement>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateCamera2DMovement(float dt)
    {
        world.Query(_queryUpdateCameraMovement, (ref Transform2D transform, ref Camera2D camera, ref CameraMovement cameraMovement) =>
        {
            // Simple WASD for 2D panning
            Vector2 moveDir = Vector2.Zero;
            if (_keyboard.IsKeyPressed(Key.W)) moveDir.Y -= 1;
            if (_keyboard.IsKeyPressed(Key.S)) moveDir.Y += 1;
            if (_keyboard.IsKeyPressed(Key.A)) moveDir.X -= 1;
            if (_keyboard.IsKeyPressed(Key.D)) moveDir.X += 1;

            if (moveDir != Vector2.Zero)
            {
                transform.Position += Vector2.Normalize(moveDir) * cameraMovement.Speed * dt;
            }

            Vector2 currentMousePos = _mouse.Position;

            if (_mouse.IsButtonPressed(MouseButton.Right))
            {
                // 1. Calculate the delta since the last frame
                // We use the difference in screen pixels
                Vector2 delta = currentMousePos - cameraMovement.LastMousePosition;

                // 2. Adjust for Zoom (Scale)
                // If we are zoomed in (Scale > 1), we move the camera less.
                // If we are zoomed out (Scale < 1), we move the camera more.
                float zoomX = transform.Scale.X != 0 ? transform.Scale.X : 1.0f;
                float zoomY = transform.Scale.Y != 0 ? transform.Scale.Y : 1.0f;

                // 3. Apply to Camera Position
                // We subtract the delta because dragging the mouse "right" 
                // should move the camera "left" to make the world follow the cursor.
                transform.Position.X -= delta.X / zoomX;
                transform.Position.Y -= delta.Y / zoomY;
            }

            // 4. Update the tracker for the next frame
            cameraMovement.LastMousePosition = currentMousePos;

            // --- Optional: Zoom with Mouse Wheel ---
            float scroll = _mouse.ScrollWheels[0].Y;
            if (scroll != 0)
            {
                float zoomSpeed = 0.1f;
                transform.Scale.X += scroll * zoomSpeed;
                transform.Scale.Y += scroll * zoomSpeed;

                // Clamp zoom to prevent flipping or invisible world
                transform.Scale.X = Math.Clamp(transform.Scale.X, 0.01f, 10f);
                transform.Scale.Y = Math.Clamp(transform.Scale.Y, 0.01f, 10f);
            }
        });
    }
}