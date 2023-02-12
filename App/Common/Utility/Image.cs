using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Saber.Common.Utility
{
    public struct ImageInfo
    {
        public string path;
        public string filename;
        public int width;
        public int height;
        public Image<Rgba32> bitmap;
    }
        
    public static class Image
    {
        public static ImageInfo Load(string path, string filename)
        {
            using (var fs = File.OpenRead(App.MapPath(path + filename)))
            {
                return Load(fs, path, filename);
            }
        }

        public static ImageInfo Load(Stream stream, string path = "", string filename = "")
        {
            ImageInfo newImg = new ImageInfo();
            var image = SixLabors.ImageSharp.Image.Load(stream);
            newImg.bitmap = (Image<Rgba32>)image;
            newImg.filename = filename;
            newImg.path = path;
            newImg.width = image.Width;
            newImg.height = image.Height;
            return newImg;
        }
        
        public static void Shrink(string filename, string outfile, int width)
        {
            using (var fs = File.OpenRead(App.MapPath(filename)))
            {
                var image = SixLabors.ImageSharp.Image.Load(fs);

                if (image.Width > width)
                {
                    image.Mutate(img => img.Resize(new ResizeOptions()
                    {
                        Size = new Size(width, 0)
                    }));
                }
                image.Save(App.MapPath(outfile));
                fs.Dispose();
            }
        }

        public static void ConvertPngToJpg(string filename, string outfile, int quality = 100)
        {
            using (var fs = File.OpenRead(App.MapPath(filename)))
            {
                var image = SixLabors.ImageSharp.Image.Load(fs);

                image.Save(App.MapPath(outfile), new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder()
                {
                    Quality = quality
                });
                fs.Dispose();
            }
        }
    }
}
