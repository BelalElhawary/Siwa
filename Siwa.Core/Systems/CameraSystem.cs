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
        RenderCameraPerspective();
    }

    public void Start() { }

    public void Update(float dt)
    {
        UpdateCameraMovement(dt);
    }

    private readonly QueryDescription _queryRenderCameraPerspective = new QueryDescription().WithAll<Transform, Camera>();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderCameraPerspective()
    {
        world.Query(in _queryRenderCameraPerspective, (ref Transform transform, ref Camera camera) =>
        {
            camera.Width = (int)viewPort.Width;
            camera.Height = (int)viewPort.Height;
            var view = Matrix4x4.CreateLookAt(transform.Position, transform.Position + camera.Orientation, camera.Up);
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
            if (_keyboard.IsKeyPressed(Key.W))
                transform.Position += cameraMovement.Speed * dt * camera.Orientation;
            if (_keyboard.IsKeyPressed(Key.A))
                transform.Position += -cameraMovement.Speed * dt *
                                      Vector3.Normalize(Vector3.Cross(camera.Orientation, camera.Up));
            if (_keyboard.IsKeyPressed(Key.S))
                transform.Position += cameraMovement.Speed * dt * -camera.Orientation;
            if (_keyboard.IsKeyPressed(Key.D))
                transform.Position += cameraMovement.Speed * dt *
                                      Vector3.Normalize(Vector3.Cross(camera.Orientation, camera.Up));
            if (_keyboard.IsKeyPressed(Key.E))
                transform.Position += cameraMovement.Speed * dt * camera.Up;
            if (_keyboard.IsKeyPressed(Key.Q))
                transform.Position += cameraMovement.Speed * dt * -camera.Up;
            if (_mouse.IsButtonPressed(MouseButton.Right))
            {
                _mouse.Cursor.CursorMode = CursorMode.Hidden;

                float rotX = cameraMovement.Sensitivity * dt * (_mouse.Position.Y - ((float)camera.Height / 2)) /
                             camera.Height;
                float rotY = cameraMovement.Sensitivity * dt * (_mouse.Position.X - ((float)camera.Width / 2)) /
                             camera.Width;

                // 1. Calculate the 'Right' axis (the cross product)
                Vector3 rightAxis = Vector3.Normalize(Vector3.Cross(camera.Orientation, camera.Up));

                // 2. Create the rotation (glm::radians is MathF.PI / 180f)
                float radians = -rotX * (MathF.PI / 180f);
                Quaternion rotation = Quaternion.CreateFromAxisAngle(rightAxis, radians);

                // 3. Update the Orientation vector
                var newOrientation = Vector3.Transform(camera.Orientation, rotation);

                // 1. Define the angle threshold (5 degrees in radians)
                float threshold = 5.0f * (MathF.PI / 180f);

                // 2. Calculate angles using Dot Product and Acos
                // Ensure vectors are normalized for Dot to return a value between -1 and 1
                float angleUp = MathF.Acos(Vector3.Dot(Vector3.Normalize(newOrientation), camera.Up));
                float angleDown = MathF.Acos(Vector3.Dot(Vector3.Normalize(newOrientation), -camera.Up));

                // 3. The if statement
                if (!(angleUp <= threshold || angleDown <= threshold))
                {
                    camera.Orientation = newOrientation;
                }

                // 1. Convert to radians
                float yawRadians = -rotY * (MathF.PI / 180f);

                // 2. Create the rotation (rotating around the global Up axis)
                Quaternion yawRotation = Quaternion.CreateFromAxisAngle(camera.Up, yawRadians);

                // 3. Update the orientation
                camera.Orientation = Vector3.Transform(camera.Orientation, yawRotation);

                camera.Orientation = Vector3.Normalize(camera.Orientation);

                _mouse.Position = new Vector2((float)camera.Width / 2, (float)camera.Height / 2);
            }
            else if (!_mouse.IsButtonPressed(MouseButton.Right))
            {
                _mouse.Cursor.CursorMode = CursorMode.Normal;
            }
        });
    }
}