using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using moos.Interfaces;
using SkiaSharp;

namespace moos.Services;

public class ImageEditorService : IImageEditor
{
    private static PixelSize _saveSize = new PixelSize(300, 300);
    private static Vector _saveDpi = new Vector(96, 96);
    private static int _jpegQuality = 85;
    
    public Bitmap ResizeSelectedImage(Bitmap source)
    {
        using var target = new RenderTargetBitmap(_saveSize, _saveDpi);
        using (var ctx = target.CreateDrawingContext(false))
        {
            ctx.DrawImage(
                source, 
                new Rect(0, 0, source.PixelSize.Width, source.PixelSize.Height),
                new Rect(0, 0, _saveSize.Width, _saveSize.Height));
        }
        return target;
    }

    public byte[] EncodeToJpeg(Bitmap bitmap)
    {
        using var skBitmap = ConvertBitmapToSkBitmap(bitmap);
        using var image = SKImage.FromBitmap(skBitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, _jpegQuality);
        return encoded.ToArray();
    }

    public Bitmap CropBitmap(Bitmap bitmap, PixelRect cropArea)
    {
        using var skBitmap = ConvertBitmapToSkBitmap(bitmap);
        var cropped = new SKBitmap(cropArea.Width, cropArea.Height);
        using (var canvas = new SKCanvas(cropped))
        {
            var srcRect = new SKRectI(cropArea.X, cropArea.Y, cropArea.X + cropArea.Width, cropArea.Y + cropArea.Height);
            var destRect = new SKRectI(0, 0, cropArea.Width, cropArea.Height);
            canvas.DrawBitmap(skBitmap, srcRect, destRect);
        }
        using var image = SKImage.FromBitmap(cropped);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return new Bitmap(data.AsStream());
    }

    private SKBitmap ConvertBitmapToSkBitmap(Bitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);
        using var skStream = new SKManagedStream(stream);
        return SKBitmap.Decode(skStream);
    }
}