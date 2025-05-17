using Avalonia.Media.Imaging;

namespace moos.Interfaces;

public interface IImageEditor
{
    Bitmap ResizeSelectedImage(Bitmap bitmap);
    byte[] EncodeToJpeg(Bitmap bitmap);
}