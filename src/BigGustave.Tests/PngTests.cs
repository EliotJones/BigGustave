namespace BigGustave.Tests
{
    using Xunit;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class PngTests
    {
        private static readonly Pixel Tr = new Pixel(0, 0, 0, 0, false);
        
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
            var yellow = P(255, 255, 0);
            var green = P(0, 255, 6);
            var pink = P(255, 75, 240);
            var red = P(255, 0, 0);
            var blue = P(31, 57, 255);

            var values = new List<Pixel[]>
            {
                new [] { Tr, Tr, yellow, yellow, yellow, yellow, yellow, yellow, yellow, yellow  },
                new [] { Tr, Tr, yellow, green, green, green, green, pink, yellow, yellow  },
                new [] { Tr, Tr, yellow, yellow, yellow, pink, red, pink, yellow, yellow  },
                new [] { Tr, Tr, yellow, yellow, yellow, pink, yellow, pink, yellow, yellow  },
                new [] { Tr, Tr, yellow, blue, pink, pink, red, pink, yellow, yellow  },
                new [] { Tr, Tr, yellow, blue, green, green, green, green, pink, yellow  },
                new [] { Tr, Tr, yellow, blue, pink, yellow, yellow, yellow, yellow, yellow  },
                new [] { Tr, Tr, yellow, yellow, yellow, yellow, yellow, yellow, yellow, yellow  },
                new [] { Tr, Tr, Tr, Tr, Tr, Tr, Tr, Tr, Tr, Tr  },
                new [] { Tr, Tr, Tr, Tr, Tr, Tr, Tr, Tr, Tr, Tr  }
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

                        Assert.True(pixel.Equals(expectedRow[col]), $"Expected {expectedRow[col]} but got {pixel}.");
                    }
                }
            }
        }

        private static Pixel P(byte r, byte g, byte b)
        {
            return new Pixel(r, g, b, 255, false);
        }
    }
}
