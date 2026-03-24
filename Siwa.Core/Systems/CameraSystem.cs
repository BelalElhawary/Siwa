using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Siwa.Core.Components;
using Siwa.Core.Helper;
using Shader = Siwa.Core.Helper.Shader;

namespace Siwa.Core.Systems;

public partial class CameraSystem(GL gl, World world, IInputContext inputContext)
    : BaseSystem<World, float>(world)
{
    private readonly IMouse _mouse = inputContext.Mice[0];
    private readonly IKeyboard _keyboard = inputContext.Keyboards[0];
    private const string CameraUniform = "camMatrix";
    
    public override void Update(in float dt)
    {
        CameraMovementQuery(World, dt);
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateMatrix(ref Camera camera)
    {
        var view = Matrix4x4.CreateLookAt(camera.Position, camera.Position + camera.Orientation, camera.Up);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(Scalar.DegreesToRadians(45f),
            (float)camera.Width / camera.Height, 0.01f, 1000f);
        camera.Matrix = view * projection;
    }
    
    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Matrix(ref Camera camera, [Data] in Shader shader)
    {
        gl.UniformMatrix4(gl.GetUniformLocation(shader.ShaderProgram, CameraUniform), 1, false, camera.Matrix.GetMatrixSpan());
    }
    

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CameraMovement([Data] in float dt, ref Camera camera)
    {
        if (_keyboard.IsKeyPressed(Key.W))
            camera.Position += camera.Speed * dt * camera.Orientation;
        if (_keyboard.IsKeyPressed(Key.A))
            camera.Position += -camera.Speed * dt *
                               Vector3.Normalize(Vector3.Cross(camera.Orientation, camera.Up));
        if (_keyboard.IsKeyPressed(Key.S))
            camera.Position += camera.Speed * dt * -camera.Orientation;
        if (_keyboard.IsKeyPressed(Key.D))
            camera.Position += camera.Speed * dt *
                               Vector3.Normalize(Vector3.Cross(camera.Orientation, camera.Up));
        if (_keyboard.IsKeyPressed(Key.E))
            camera.Position += camera.Speed * dt * camera.Up;
        if (_keyboard.IsKeyPressed(Key.Q))
            camera.Position += camera.Speed * dt * -camera.Up;
        if (_mouse.IsButtonPressed(MouseButton.Right))
        {
            _mouse.Cursor.CursorMode = CursorMode.Hidden;

            float rotX = camera.Sensitivity * dt * (_mouse.Position.Y - ((float)camera.Height / 2)) /
                         camera.Height;
            float rotY = camera.Sensitivity * dt * (_mouse.Position.X - ((float)camera.Width / 2)) /
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
    }
}