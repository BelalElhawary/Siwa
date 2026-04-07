using System.Numerics;

namespace Siwa.Core.Components;

public struct Transform2D()
{
    public Vector2 Position = Vector2.Zero;
    public float Rotation;
    public Vector2 Scale = Vector2.One;
}