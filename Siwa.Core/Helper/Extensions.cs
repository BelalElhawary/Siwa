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
    
    public static Vector3 ToEuler(this Quaternion q)
    {
        Vector3 angles;

        // Roll (z-axis rotation)
        double sinRCosP = 2 * (q.W * q.Z + q.X * q.Y);
        double cosRCosP = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        angles.Z = (float)Math.Atan2(sinRCosP, cosRCosP);

        // Pitch (x-axis rotation)
        double sinP = 2 * (q.W * q.Y - q.Z * q.X);
        if (Math.Abs(sinP) >= 1)
            angles.Y = (float)Math.CopySign(Math.PI / 2, sinP); // Use 90 degrees if out of range
        else
            angles.Y = (float)Math.Asin(sinP);

        // Yaw (y-axis rotation)
        double sinYCosP = 2 * (q.W * q.X + q.Y * q.Z);
        double cosYCosP = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        angles.X = (float)Math.Atan2(sinYCosP, cosYCosP);

        // Convert Radians to Degrees for ImGui
        return angles * (180f / (float)Math.PI);
    }
}