namespace BigGustave
{
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    public class PngBuilder
    {
        private static readonly byte[] FakeDeflateHeader = { 120, 156 };

        private readonly byte[] rawData;
        private readonly bool hasAlphaChannel;
        private readonly int width;
        private readonly int height;
        private readonly int bytesPerPixel;

        public static PngBuilder Create(int width, int height, bool hasAlphaChannel)
        {
            var bpp = hasAlphaChannel ? 4 : 3;

            var length = (height * width * bpp) + height;

            return new PngBuilder(new byte[length], hasAlphaChannel, width, height, bpp);
        }

        private PngBuilder(byte[] rawData, bool hasAlphaChannel, int width, int height, int bytesPerPixel)
        {
            this.rawData = rawData;
            this.hasAlphaChannel = hasAlphaChannel;
            this.width = width;
            this.height = height;
            this.bytesPerPixel = bytesPerPixel;
        }

        public PngBuilder SetPixel(Pixel pixel, int x, int y)
        {
            var start = (y * ((width * bytesPerPixel) + 1)) + 1 + (x * bytesPerPixel);

            rawData[start++] = pixel.R;
            rawData[start++] = pixel.G;
            rawData[start++] = pixel.B;

            if (hasAlphaChannel)
            {
                rawData[start] = pixel.A;
            }

            return this;
        }

        public void Save(Stream outputStream)
        {
            outputStream.Write(HeaderValidationResult.ExpectedHeader, 0, HeaderValidationResult.ExpectedHeader.Length);

            var stream = new PngStreamWriteHelper(outputStream);

            stream.WriteChunkLength(13);
            stream.WriteChunkHeader(ImageHeader.HeaderBytes);

            StreamHelper.WriteBigEndianInt32(stream, width);
            StreamHelper.WriteBigEndianInt32(stream, height);
            stream.WriteByte(8);

            var colorType = ColorType.ColorUsed;
            if (hasAlphaChannel)
            {
                colorType |= ColorType.AlphaChannelUsed;
            }

            stream.WriteByte((byte)colorType);
            stream.WriteByte((byte)CompressionMethod.DeflateWithSlidingWindow);
            stream.WriteByte((byte)FilterMethod.AdaptiveFiltering);
            stream.WriteByte((byte)InterlaceMethod.None);

            stream.WriteCrc();

            var imageData = Compress(rawData);
            stream.WriteChunkLength(imageData.Length);
            stream.WriteChunkHeader(Encoding.ASCII.GetBytes("IDAT"));
            stream.Write(imageData, 0, imageData.Length);
            stream.WriteCrc();

            stream.WriteChunkLength(0);
            stream.WriteChunkHeader(Encoding.ASCII.GetBytes("IEND"));
            stream.WriteCrc();
        }
        
        private static byte[] Compress(byte[] data)
        {
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionLevel.Fastest))
            {
                compressStream.Write(FakeDeflateHeader, 0, FakeDeflateHeader.Length);
                compressor.Write(data, 0, data.Length);
                compressor.Close();
                return compressStream.ToArray();
            }
        }
    }
}
