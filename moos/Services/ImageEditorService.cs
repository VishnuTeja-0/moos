using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using moos.Interfaces;
using SkiaSharp;

namespace moos.Services;

public class ImageEditorService : IImageEditor
{
    private static PixelSize _saveSize = new PixelSize(300, 300);
    private static PixelSize _frameSize = new PixelSize(200, 200);
    private static Vector _saveDpi = new Vector(96, 96);
    private static int _jpegQuality = 85;
    
    public Bitmap ResizeSelectedImage(Bitmap source, int? width, int? height)
    {
        var targetWidth = width ?? _saveSize.Width;
        var targetHeight = height ?? _saveSize.Height;
        var targetSize = new PixelSize(targetWidth, targetHeight);
        var writeableBitmap = new WriteableBitmap(targetSize, _saveDpi);
        using var fb = writeableBitmap.Lock();
        using var skSurface = SKSurface.Create(
            new SKImageInfo(targetSize.Width, targetSize.Height, SKColorType.Bgra8888, SKAlphaType.Premul),
            fb.Address,
            fb.RowBytes
        );
        using var skBitmap = ConvertBitmapToSkBitmap(source);

        var destRect = new SKRect(0, 0, targetSize.Width, targetSize.Height);
        skSurface.Canvas.Clear(SKColors.Transparent);
        skSurface.Canvas.DrawBitmap(skBitmap, destRect);
        skSurface.Canvas.Flush();

        return writeableBitmap;
    }

    public byte[] EncodeToJpeg(Bitmap bitmap)
    {
        using var skBitmap = ConvertBitmapToSkBitmap(bitmap);
        using var image = SKImage.FromBitmap(skBitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, _jpegQuality);
        return encoded.ToArray();
    }

    public Bitmap CropBitmap(Bitmap bitmap, int x, int y, int side)
    {
        var resized = ResizeSelectedImage(bitmap, _frameSize.Width, _frameSize.Height);
        using var skBitmap = ConvertBitmapToSkBitmap(resized);
        var srcRect = new SKRectI(x, y, x + side, y + side);
        var cropped = new SKBitmap(side, side, skBitmap.ColorType, skBitmap.AlphaType);
        if (skBitmap.ExtractSubset(cropped, srcRect))
        {

            using var image = SKImage.FromBitmap(cropped);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var memoryStream = new MemoryStream();
            data.AsStream().CopyTo(memoryStream);
            memoryStream.Position = 0;
            var result = new Bitmap(memoryStream);

            return result;
        }
        else
        {
            return bitmap;
        }
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