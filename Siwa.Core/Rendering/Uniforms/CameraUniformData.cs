using System.Numerics;
using System.Runtime.InteropServices;

namespace Siwa.Core.Rendering.Uniforms;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct CameraUniformData
{
    public Matrix4x4 CameraMatrix;
    public Vector3 CameraPosition;
    private float _padding;
}