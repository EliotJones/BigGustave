namespace BigGustave.Tests
{
    using System;
    using System.IO;
    using Xunit;

    public class JpgTests
    {
        [Fact]
        public void TwoByTwoCheckerboardGreen()
        {
            var green = new Pixel(77, 203, 77);
            var blackish = new Pixel(0, 53, 0);

            var expected = new[]
            {
                blackish, green,
                green, blackish
            };
            
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "jpg", "8by8.jpg");

            using (var stream = File.OpenRead(path))// @"C:\git\python\micro-jpeg-visualizer\images\gritty.jpg"))
            {
                var img = Jpg.Open(stream);

                var i = 0;

                var png = PngBuilder.Create(img.Width, img.Height, false);

                for (var row = 0; row < img.Height; row++)
                {
                    for (var col = 0; col < img.Width; col++)
                    {
                        var pixel = img.GetPixel(col, row);

                        png.SetPixel(pixel, col, row);

                        Assert.Equal(expected[i], pixel);

                        i++;
                    }
                }

                // File.WriteAllBytes(@"C:\temp\gritty.jpg", png.Save());

                Assert.Equal(2, img.Width);
                Assert.Equal(2, img.Height);
            }
        }
    }
}
