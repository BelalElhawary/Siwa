using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Siwa.Core.Components;

namespace Siwa.Core.Helper;

public static class Extensions
{
    public static ReadOnlySpan<float> GetMatrixSpan(this ref Matrix4x4 matrix)
    {
        return MemoryMarshal.CreateReadOnlySpan(ref matrix.M11, 16);
    }

    public static Vector4 ToVector4(this Color color)
    {
        return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }
}