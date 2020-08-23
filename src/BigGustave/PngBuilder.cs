namespace BigGustave
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Used to construct PNG images. Call <see cref="Create"/> to make a new builder.
    /// </summary>
    public class PngBuilder
    {
        private const byte Deflate32KbWindow = 120;
        private const byte ChecksumBits = 1;

        private readonly byte[] rawData;
        private readonly bool hasAlphaChannel;
        private readonly int width;
        private readonly int height;
        private readonly int bytesPerPixel;

        /// <summary>
        /// Create a builder for a PNG with the given width and size.
        /// </summary>
        public static PngBuilder Create(int width, int height, bool hasAlphaChannel)
        {
            var bpp = hasAlphaChannel ? 4 : 3;

            var length = (height * width * bpp) + height;

            return new PngBuilder(new byte[length], hasAlphaChannel, width, height, bpp);
        }

        /// <summary>
        /// Create a builder from a <see cref="Png"/>.
        /// </summary>
        public static PngBuilder FromPng(Png png)
        {
            var result = Create(png.Width, png.Height, png.HasAlphaChannel);

            for (int y = 0; y < png.Height; y++)
            {
                for (int x = 0; x < png.Width; x++)
                {
                    result.SetPixel(png.GetPixel(x, y), x, y);
                }
            }

            return result;
        }

        /// <summary>
        /// Create a builder from the bytes of the specified PNG image.
        /// </summary>
        public static PngBuilder FromPngBytes(byte[] png)
        {
            var pngActual = Png.Open(png);
            return FromPng(pngActual);
        }

        private PngBuilder(byte[] rawData, bool hasAlphaChannel, int width, int height, int bytesPerPixel)
        {
            this.rawData = rawData;
            this.hasAlphaChannel = hasAlphaChannel;
            this.width = width;
            this.height = height;
            this.bytesPerPixel = bytesPerPixel;
        }

        /// <summary>
        /// Sets the RGB pixel value for the given column (x) and row (y).
        /// </summary>
        public PngBuilder SetPixel(byte r, byte g, byte b, int x, int y) => SetPixel(new Pixel(r, g, b), x, y);

        /// <summary>
        /// Set the pixel value for the given column (x) and row (y).
        /// </summary>
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
        
        /// <summary>
        /// Get the bytes of the PNG file for this builder.
        /// </summary>
        public byte[] Save(SaveOptions options = null)
        {
            using (var memoryStream = new MemoryStream())
            {
                Save(memoryStream, options);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Write the PNG file bytes to the provided stream.
        /// </summary>
        public void Save(Stream outputStream, SaveOptions options = null)
        {
            AttemptCompressionOfRawData(rawData, options ?? new SaveOptions());

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
            const int headerLength = 2;
            const int checksumLength = 4;
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionLevel.Fastest, true))
            {
                compressor.Write(data, 0, data.Length);
                compressor.Close();

                compressStream.Seek(0, SeekOrigin.Begin);

                var result = new byte[headerLength + compressStream.Length + checksumLength];

                // Write the ZLib header.
                result[0] = Deflate32KbWindow;
                result[1] = ChecksumBits;

                // Write the compressed data.
                int streamValue;
                var i = 0;
                while ((streamValue = compressStream.ReadByte()) != -1)
                {
                    result[headerLength + i] = (byte) streamValue;
                    i++;
                }

                // Write Checksum of raw data.
                var checksum = Adler32Checksum.Calculate(data);

                var offset = headerLength + compressStream.Length;

                result[offset++] = (byte)(checksum >> 24);
                result[offset++] = (byte)(checksum >> 16);
                result[offset++] = (byte)(checksum >> 8);
                result[offset] = (byte)(checksum >> 0);

                return result;
            }
        }

        /// <summary>
        /// Attempt to improve compressability of the raw data by using adaptive filtering.
        /// </summary>
        private void AttemptCompressionOfRawData(byte[] rawData, SaveOptions options)
        {
            if (!options.AttemptCompression)
            {
                return;
            }

            var bytesPerScanline = 1 + (bytesPerPixel * width);
            var scanlineCount = rawData.Length / bytesPerScanline;

            var scanData = new byte[bytesPerScanline - 1];

            for (var scanlineRowIndex = 0; scanlineRowIndex < scanlineCount; scanlineRowIndex++)
            {
                var sourceIndex = (scanlineRowIndex * bytesPerScanline) + 1;

                Array.Copy(rawData, sourceIndex, scanData, 0, bytesPerScanline - 1);

                var noneFilterSum = 0;
                for (int i = 0; i < scanData.Length; i++)
                {
                    noneFilterSum += scanData[i];
                }

                var leftFilterSum = 0;
                for (int i = 0; i < scanData.Length; i++)
                {

                }
                /* 
                 * A heuristic approach is to use adaptive filtering as follows: 
                 *    independently for each row, apply all five filters and select the filter that produces the smallest sum of absolute values per row. 
                 */
            }
        }

        /// <summary>
        /// Options for configuring generation of PNGs from a builder.
        /// </summary>
        public class SaveOptions
        {
            /// <summary>
            /// Whether the library should try to compress the resulting image size by brute-force searching filters for the data.
            /// This is a lossless process but can increase the save time.
            /// </summary>
            public bool AttemptCompression { get; set; }

            /// <summary>
            /// The number of parallel tasks allowed during compression.
            /// </summary>
            public int MaxDegreeOfParallelism { get; set; } = 1;
        }
    }
}
