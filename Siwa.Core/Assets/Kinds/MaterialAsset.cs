using System.Runtime.InteropServices;
using Siwa.Core.Helper;
using System.Text.Json.Serialization;

namespace Siwa.Core.Assets.Kinds;

public enum MaterialType : byte
{
    Unlit,
    Lit
}

[StructLayout(LayoutKind.Sequential)]
public struct MaterialHandle
{
    [JsonInclude] public RawHandle Handle;
    [JsonInclude] public MaterialType Type;
}