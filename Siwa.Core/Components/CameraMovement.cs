using System.Numerics;

namespace Siwa.Core.Components;

public struct CameraMovement
{
    public float Speed;
    public float Sensitivity;
    public bool FirstClick;
    public Vector2 LastMousePosition;
}