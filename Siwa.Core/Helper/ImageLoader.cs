using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Siwa.Core.Helper;

public static class ImageLoader
{
    public static byte[] LoadRgba32(string path, out float width, out float height)
    {
        using var image = Image.Load<Rgba32>(path); 
        var pixelData = new byte[image.Width * image.Height * 4];
        image.Mutate(x => x.Flip(FlipMode.Vertical));
        image.CopyPixelDataTo(pixelData);
        width = image.Width;
        height = image.Height;
        return pixelData;
    }
    
    public static byte[] LoadL8(string path, out float width, out float height)
    {
        using var image = Image.Load<L8>(path); 
        var pixelData = new byte[image.Width * image.Height];
        image.Mutate(x => x.Flip(FlipMode.Vertical));
        image.CopyPixelDataTo(pixelData);
        width = image.Width;
        height = image.Height;
        return pixelData;
    }
}