using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Siwa.Core.Importers;

public static class ImageImporter
{
    public static byte[] Rgba32(string path, bool flipY, out float width, out float height)
    {
        using var image = Image.Load<Rgba32>(path); 
        var pixelData = new byte[image.Width * image.Height * 4];
        if(flipY)
            image.Mutate(x => x.Flip(FlipMode.Vertical));
        image.CopyPixelDataTo(pixelData);
        width = image.Width;
        height = image.Height;
        return pixelData;
    }
    
    public static byte[] L8(string path, bool flipY, out float width, out float height)
    {
        using var image = Image.Load<L8>(path); 
        var pixelData = new byte[image.Width * image.Height];
        if(flipY)
            image.Mutate(x => x.Flip(FlipMode.Vertical));
        image.CopyPixelDataTo(pixelData);
        width = image.Width;
        height = image.Height;
        return pixelData;
    }
}