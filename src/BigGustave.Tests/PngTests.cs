using Xunit;

namespace BigGustave.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class PngTests
    {
        [Fact]
        public void Test()
        {
            var file = @"C:\Users\eliot\OneDrive\Pictures\g5319.png";

            if (!File.Exists(file))
            {
                return;
            }

            using (var stream = File.OpenRead(file))
            {
                Png.Open(stream);
            }
        }

        [Fact]
        public void FourByFourGrayscale()
        {
            var values = new List<byte[]>
            {
                new byte[] {176, 255, 255, 176},
                new byte[] {255, 0, 0, 255},
                new byte[] {133, 255, 255, 133},
                new byte[] {255, 176, 176, 255}
            };

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "4by4grayscale.png");

            using (var stream = File.OpenRead(path))
            {
                var img = Png.Open(stream);

                for (var row = 0; row < values.Count; row++)
                {
                    var expectedRow = values[row];

                    for (var col = 0; col < expectedRow.Length; col++)
                    {
                        var pixel = img.GetPixel(col, row);

                        Assert.Equal(values[row][col], pixel.R);
                        Assert.True(pixel.IsGrayscale);
                    }
                }

                Assert.Equal(4, img.Width);
                Assert.Equal(4 ,img.Height);
                Assert.False(img.HasAlphaChannel);
            }
        }

        [Fact]
        public void TenByTenRgbAWithAdam7()
        {
            var values = new List<byte[]>
            {
                new byte[] {176, 255, 255, 176},
                new byte[] {255, 0, 0, 255},
                new byte[] {133, 255, 255, 133},
                new byte[] {255, 176, 176, 255}
            };

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "10by10rgbaadam7.png");

            using (var stream = File.OpenRead(path))
            {
                var img = Png.Open(stream);

                Assert.Equal(10, img.Width);
                Assert.Equal(10, img.Height);

                Assert.True(img.HasAlphaChannel);

                for (var row = 0; row < values.Count; row++)
                {
                    var expectedRow = values[row];

                    for (var col = 0; col < expectedRow.Length; col++)
                    {
                        var pixel = img.GetPixel(col, row);
                    }
                }
            }
        }
    }
}
