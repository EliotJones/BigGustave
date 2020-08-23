namespace BigGustave.Tests
{
    using System;
    using System.IO;
    using Xunit;

    public class CompressionTests
    {
        private static string GetImageFile(string fileName) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", fileName);

        [Fact]
        public void CanCompress()
        {
            var file = GetImageFile("pdfpig.png");
            var png = Png.Open(file);

            var builder = PngBuilder.Create(png.Width, png.Height, png.HasAlphaChannel);

            CopyPngToBuilder(png, builder);

            var sizeRaw = builder.Save().Length;

            var compressed = builder.Save(new PngBuilder.SaveOptions
            {
                AttemptCompression = true
            }); ;
        }

        private static void CopyPngToBuilder(Png png, PngBuilder builder)
        {
            for (int y = 0; y < png.Height; y++)
            {
                for (int x = 0; x < png.Width; x++)
                {
                    var pixel = png.GetPixel(x, y);
                    builder.SetPixel(pixel, x, y);
                }
            }
        }
    }
}
