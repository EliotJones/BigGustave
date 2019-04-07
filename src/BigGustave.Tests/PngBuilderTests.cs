namespace BigGustave.Tests
{
    using System.IO;
    using Xunit;

    public class PngBuilderTests
    {
        [Fact]
        public void SimpleCheckerboard()
        {
            var builder = PngBuilder.Create(2, 2, false);

            var red = new Pixel(255, 0, 12, 255, false);
            var black = new Pixel(0, 0, 0, 255, false);

            builder.SetPixel(new Pixel(255, 0, 12, 255, false), 0, 0);
            builder.SetPixel(new Pixel(255, 0, 12, 255, false), 1, 1);

            using (var memory = new MemoryStream())
            {
                builder.Save(memory);

                memory.Seek(0, SeekOrigin.Begin);

                var read = Png.Open(memory);

                var left = read.GetPixel(0, 0);
                Assert.Equal(red, left);
                var right = read.GetPixel(1, 0);
                Assert.Equal(black, right);
                var bottomLeft = read.GetPixel(0, 1);
                Assert.Equal(black, bottomLeft);
                var bottomRight = read.GetPixel(1, 1);
                Assert.Equal(red, bottomRight);
            }
        }
    }
}
