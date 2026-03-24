using System.Numerics;
using System.Runtime.InteropServices;
using Arch.Core;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Siwa.Core.Components;

namespace Siwa.Core.Helper;

public static class Extensions
{
    public static ReadOnlySpan<float> GetMatrixSpan(this ref Matrix4x4 matrix)
    {
        return MemoryMarshal.CreateReadOnlySpan(ref matrix.M11, 16);
    }
}

public static class Ecs
{
    public static Camera CreateCamera(int width, int height, Vector3 position)
    {
        return new Camera
        {
            Position = position,
            Width = width,
            Height = height,
            Orientation = new(0, 0, -1f),
            Up = new(0, 1f, 0f),
            Matrix = Matrix4x4.Identity,
            Speed = 5f,
            Sensitivity = 2500f
        };
    }
}