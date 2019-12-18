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
            var green = new Pixel(75, 204, 77);
            var blackish = new Pixel(0, 54, 0);

            var expected = new[]
            {
                green, blackish,
                blackish, green
            };
            
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "jpg", "2by2checkerboard-green.jpg");

            using (var stream = File.OpenRead(path))
            {
                var img = Jpg.Open(stream);

                var i = 0;

                for (var row = 0; row < img.Height; row++)
                {
                    for (var col = 0; col < img.Width; col++)
                    {
                        var pixel = img.GetPixel(col, row);

                        Assert.Equal(expected[i], pixel);

                        i++;
                    }
                }

                Assert.Equal(2, img.Width);
                Assert.Equal(2, img.Height);
            }
        }
    }
}
