using Siwa.Core.Assets;
using Siwa.Core.Assets.Kinds;
using Siwa.Core.Components;

namespace Siwa.Core.Data;

public static class Images
{
    public static IntPtr ModelIcon = IntPtr.Zero;
    public static IntPtr TextureIcon = IntPtr.Zero;
    public static IntPtr ShaderIcon = IntPtr.Zero;
    public static IntPtr MaterialIcon = IntPtr.Zero;

    public static void Load()
    {
        MaterialIcon = (IntPtr)AssetPool<Texture>.Registry.Get(AssetLoader.Instance.GetAsset<TextureAsset>("MaterialIcon")!
            .Handle.ToHandle<Texture>()).Handle;
        TextureIcon = (IntPtr)AssetPool<Texture>.Registry.Get(AssetLoader.Instance.GetAsset<TextureAsset>("TextureIcon")!
            .Handle.ToHandle<Texture>()).Handle;
        ShaderIcon = (IntPtr)AssetPool<Texture>.Registry.Get(AssetLoader.Instance.GetAsset<TextureAsset>("ShaderIcon")!
            .Handle.ToHandle<Texture>()).Handle;
        ModelIcon = (IntPtr)AssetPool<Texture>.Registry.Get(AssetLoader.Instance.GetAsset<TextureAsset>("ModelIcon")!
            .Handle.ToHandle<Texture>()).Handle;
    }
}