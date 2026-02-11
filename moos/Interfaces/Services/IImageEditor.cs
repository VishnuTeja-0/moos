using Avalonia.Media.Imaging;

namespace moos.Interfaces.Services;

public interface IImageEditor
{
    Bitmap ResizeSelectedImage(Bitmap bitmap, int? width, int? height);
    byte[] EncodeToJpeg(Bitmap bitmap);
    Bitmap CropBitmap(Bitmap bitmap, int x, int y, int side);
}