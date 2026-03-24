using System.Numerics;

namespace Siwa.Core.Components;

public struct Camera
{
    public Vector3 Position;
    public Vector3 Orientation;
    public Vector3 Up;
    public Matrix4x4 Matrix;
    
    public int Width;
    public int Height;

    public float Speed;
    public float Sensitivity;
}