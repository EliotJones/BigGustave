﻿namespace BigGustave.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class PngBuilderTests
    {
        [Fact]
        public void SimpleCheckerboard()
        {
            var builder = PngBuilder.Create(2, 2, false);

            var red = new Pixel(255, 0, 12);
            var black = new Pixel(0, 0, 0, 255, false);

            builder.SetPixel(new Pixel(255, 0, 12), 0, 0);
            builder.SetPixel(255, 0, 12, 1, 1);

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

        [Fact]
        public void SimpleCheckerboardWithArbitraryTextData()
        {
            var builder = PngBuilder.Create(2, 2, false);

            builder.SetPixel(new Pixel(255, 0, 12), 0, 0);
            builder.SetPixel(255, 0, 12, 1, 1);

            builder.StoreText("Title", "Checkerboard");
            builder.StoreText("another-data", "bərd that's good and other\r\nstuff");

            using (var memory = new MemoryStream())
            {
                builder.Save(memory);

                memory.Seek(0, SeekOrigin.Begin);

                var visitor = new MyChunkVisitor();

                var read = Png.Open(memory, visitor);

                Assert.NotNull(read);

                var textChunks = visitor.Visited.Where(x => x.header.Name == "iTXt").ToList();

                Assert.Equal(2, textChunks.Count);
            }
        }

        [Fact]
        public void BiggerImage()
        {
            var builder = PngBuilder.Create(10, 10, false);

            var green = new Pixel(0, 255, 25, 255, false);
            var color1 = new Pixel(60, 201, 32, 255, false);
            var color2 = new Pixel(100, 5, 250, 255, false);

            builder.SetPixel(green, 1, 1).SetPixel(green, 2, 1).SetPixel(green, 3, 1).SetPixel(green, 4, 1).SetPixel(green, 5, 1);

            builder.SetPixel(color1, 5, 7).SetPixel(color1, 5, 8)
                .SetPixel(color1, 6, 7).SetPixel(color1, 6, 8)
                .SetPixel(color1, 7, 7).SetPixel(color1, 7, 8);

            builder.SetPixel(color2, 9, 9).SetPixel(color2, 8, 8);

            using (var memoryStream = new MemoryStream())
            {
                builder.Save(memoryStream);
            }
        }

        private class MyChunkVisitor : IChunkVisitor
        {
            private readonly List<(ChunkHeader header, byte[] data)> visited = new List<(ChunkHeader header, byte[] data)>();
            public IReadOnlyList<(ChunkHeader header, byte[] data)> Visited => visited;

            public void Visit(Stream stream, ImageHeader header, ChunkHeader chunkHeader, byte[] data, byte[] crc)
            {
                visited.Add((chunkHeader, data));
            }
        }
    }
}
