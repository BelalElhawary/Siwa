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

public class CameraSystem(GL gl, World world, IInputContext inputContext, ViewPort viewPort) : IRenderSystem
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
        RenderCamera3D();
    }

    public void Start() { }

    public void Update(float dt)
    {
        UpdateCameraMovement(dt);
    }

    private readonly QueryDescription _queryRenderCameraPerspective = new QueryDescription().WithAll<Transform, Camera>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderCamera3D()
    {
        world.Query(in _queryRenderCameraPerspective, (ref Transform transform, ref Camera camera) =>
        {
            camera.Width = (int)viewPort.Width;
            camera.Height = (int)viewPort.Height;
            var view = Matrix4x4.CreateLookAt(transform.Position, transform.Position + Vector3.Transform(-Vector3.UnitZ, transform.Rotation), Game.Up);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(Scalar.DegreesToRadians(45f),
                (float)camera.Width / camera.Height, 0.01f, 1000f);
            var uniform = new CameraUniformData { CameraMatrix = view * projection, CameraPosition = transform.Position };
        
            gl.BindBuffer(BufferTargetARB.UniformBuffer, _ubo); 
            gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, _cameraUniformDataSize, in uniform);
            gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
        });
    }

    private readonly QueryDescription _queryUpdateCameraMovement = new QueryDescription().WithAll<Transform, Camera, CameraMovement>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateCameraMovement(float dt)
    {
        world.Query(_queryUpdateCameraMovement, (ref Transform transform, ref Camera camera, ref CameraMovement cameraMovement) =>
        {
            // Calculate the current forward and right vectors from the Quaternion
            Vector3 forward = Vector3.Normalize(Vector3.Transform(-Vector3.UnitZ, transform.Rotation));
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Game.Up));
            
            if (_keyboard.IsKeyPressed(Key.W))
                transform.Position += cameraMovement.Speed * dt * forward;
            if (_keyboard.IsKeyPressed(Key.A))
                transform.Position += -cameraMovement.Speed * dt * right;
            if (_keyboard.IsKeyPressed(Key.S))
                transform.Position += -cameraMovement.Speed * dt * forward;
            if (_keyboard.IsKeyPressed(Key.D))
                transform.Position += cameraMovement.Speed * dt * right;
            if (_keyboard.IsKeyPressed(Key.E))
                transform.Position += cameraMovement.Speed * dt * Game.Up;
            if (_keyboard.IsKeyPressed(Key.Q))
                transform.Position += cameraMovement.Speed * dt * -Game.Up;
            // 3. Update rotational movement (Mouse Look)
            if (_mouse.IsButtonPressed(MouseButton.Right))
            {
                _mouse.Cursor.CursorMode = CursorMode.Hidden;

                // 1. Calculate an EXACT integer pixel for the center
                float centerX = (int)(camera.Width / 2f);
                float centerY = (int)(camera.Height / 2f);

                if (cameraMovement.FirstClick)
                {
                    // Snap the mouse to the exact integer center
                    _mouse.Position = new Vector2(centerX, centerY);
                    cameraMovement.FirstClick = false;
                    return; 
                }

                // 2. Calculate the raw pixel deltas
                float deltaX = _mouse.Position.X - centerX;
                float deltaY = _mouse.Position.Y - centerY;

                // 3. Apply a 1-pixel deadzone (kills OS rounding jitters)
                if (MathF.Abs(deltaX) < 1.0f) deltaX = 0;
                if (MathF.Abs(deltaY) < 1.0f) deltaY = 0;

                // 4. Calculate rotation based on deltas
                float rotX = cameraMovement.Sensitivity * dt * (deltaY / camera.Height);
                float rotY = cameraMovement.Sensitivity * dt * (deltaX / camera.Width);

                // --- Pitch (Up / Down) ---
                float pitchRadians = -rotX * (MathF.PI / 180f);
                Quaternion pitchRotation = Quaternion.CreateFromAxisAngle(right, pitchRadians);
                Vector3 newForward = Vector3.Transform(forward, pitchRotation);

                float threshold = 5.0f * (MathF.PI / 180f);
                float angleUp = MathF.Acos(Vector3.Dot(Vector3.Normalize(newForward), Game.Up));
                float angleDown = MathF.Acos(Vector3.Dot(Vector3.Normalize(newForward), -Game.Up));

                if (!(angleUp <= threshold || angleDown <= threshold))
                {
                    transform.Rotation = Quaternion.Normalize(pitchRotation * transform.Rotation);
                }

                // --- Yaw (Left / Right) ---
                float yawRadians = -rotY * (MathF.PI / 180f);
                Quaternion yawRotation = Quaternion.CreateFromAxisAngle(Game.Up, yawRadians);
                
                transform.Rotation = Quaternion.Normalize(yawRotation * transform.Rotation);

                // 5. Reset mouse position back to the EXACT integer center
                _mouse.Position = new Vector2(centerX, centerY);
            }
            else
            {
                _mouse.Cursor.CursorMode = CursorMode.Normal;
                cameraMovement.FirstClick = true;
            }
        });
    }
}