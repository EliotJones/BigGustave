namespace BigGustave
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    internal class RawPngData
    {
        private readonly byte[] data;
        private readonly int bytesPerPixel;
        private readonly int width;

        public RawPngData(byte[] data, int bytesPerPixel, int width)
        {
            this.data = data;
            this.bytesPerPixel = bytesPerPixel;
            this.width = width;
        }

        public Pixel GetPixel(int x, int y)
        {
            var rowStartPixel = (1 + y) + (bytesPerPixel * width * y);

            var pixelStartIndex = rowStartPixel + (bytesPerPixel * x);

            var first = data[pixelStartIndex];

            switch (bytesPerPixel)
            {
                case 1:
                    return new Pixel(first, first, first, 0, true);
                case 2:
                    return new Pixel(first, first, first, data[pixelStartIndex + 1], true);
                case 3:
                    return new Pixel(first, data[pixelStartIndex + 1], data[pixelStartIndex + 2], 0, false);
                case 4:
                    return new Pixel(first, data[pixelStartIndex + 1], data[pixelStartIndex + 2], data[pixelStartIndex + 3], false);
                default:
                    throw new InvalidOperationException($"Unreconized number of bytes per pixel: {bytesPerPixel}.");
            }
        }
    }

    public class Png
    {
        private readonly RawPngData data;

        public ImageHeader Header { get; }

        public int Width => Header.Width;

        public int Height => Header.Height;

        public bool HasAlphaChannel => (Header.ColorType & ColorType.AlphaChannelUsed) != 0;

        internal Png(ImageHeader header, RawPngData data)
        {
            Header = header;
            this.data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public Pixel GetPixel(int x, int y) => data.GetPixel(x, y);

        public static Png Open(Stream stream, IChunkVisitor chunkVisitor = null)
            => PngOpener.Open(stream, chunkVisitor);
    }


    public readonly struct Pixel
    {
        public byte R { get; }

        public byte G { get; }

        public byte B { get; }

        public byte A { get; }

        public bool IsGrayscale { get; }

        public Pixel(byte r, byte g, byte b, byte a, bool isGrayscale)
        {
            R = r;
            G = g;
            B = b;
            A = a;
            IsGrayscale = isGrayscale;
        }

        public override bool Equals(object obj)
        {
            if (obj is Pixel pixel)
            {
                return IsGrayscale == pixel.IsGrayscale
                       && A == pixel.A
                       && R == pixel.R
                       && G == pixel.G
                       && B == pixel.B;
            }

            return false;
        }

        public bool Equals(Pixel other)
        {
            return R == other.R && G == other.G && B == other.B && A == other.A && IsGrayscale == other.IsGrayscale;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                hashCode = (hashCode * 397) ^ A.GetHashCode();
                hashCode = (hashCode * 397) ^ IsGrayscale.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"({R}, {G}, {B}, {A})";
        }
    }

    internal static class PngOpener
    {
        public static Png Open(Stream stream, IChunkVisitor chunkVisitor = null)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException($"The provided stream of type {stream.GetType().FullName} was not readable.");
            }

            var validHeader = HasValidHeader(stream);

            if (!validHeader.IsValid)
            {
                throw new ArgumentException($"The provided stream did not start with the PNG header. Got {validHeader}.");
            }

            var crc = new byte[4];
            var imageHeader = ReadImageHeader(stream, crc);

            var hasEncounteredImageEnd = false;

            using (var output = new MemoryStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    while (TryReadChunkHeader(stream, out var header))
                    {
                        if (hasEncounteredImageEnd)
                        {
                            throw new InvalidOperationException($"Found another chunk {header} after already reading the IEND chunk.");
                        }

                        var bytes = new byte[header.Length];
                        var read = stream.Read(bytes, 0, bytes.Length);
                        if (read != bytes.Length)
                        {
                            throw new InvalidOperationException($"Did not read {header.Length} bytes for the {header} header, only found: {read}.");
                        }

                        if (header.IsCritical)
                        {
                            switch (header.Name)
                            {
                                case "PLTE":
                                    // TODO: read palette
                                    break;
                                case "IDAT":
                                    memoryStream.Write(bytes, 0, bytes.Length);
                                    break;
                                case "IEND":
                                    hasEncounteredImageEnd = true;
                                    break;
                                default:
                                    throw new NotSupportedException($"Encountered critical header {header} which was not recognised.");
                            }
                        }

                        read = stream.Read(crc, 0, crc.Length);
                        if (read != 4)
                        {
                            throw new InvalidOperationException($"Did not read 4 bytes for the CRC, only found: {read}.");
                        }

                        chunkVisitor?.Visit(stream, imageHeader, header, bytes, crc);
                    }

                    memoryStream.Flush();
                    memoryStream.Seek(2, SeekOrigin.Begin);

                    using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(output);
                        deflateStream.Close();
                    }
                }

                var bytesOut = output.ToArray();

                var bytesPerPixel = Decoder.Decode(bytesOut, imageHeader);

                return new Png(imageHeader, new RawPngData(bytesOut, bytesPerPixel, imageHeader.Width));
            }
        }

        private static HeaderValidationResult HasValidHeader(Stream stream)
        {
            return new HeaderValidationResult(stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte(),
                stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte());
        }

        private static bool TryReadChunkHeader(Stream stream, out ChunkHeader chunkHeader)
        {
            chunkHeader = default;

            var position = stream.Position;
            if (!StreamHelper.TryReadHeaderBytes(stream, out var headerBytes))
            {
                return false;
            }

            var length = StreamHelper.ReadBigEndianInt32(headerBytes, 0);

            var name = Encoding.ASCII.GetString(headerBytes, 4, 4);

            chunkHeader = new ChunkHeader(position, length, name);

            return true;
        }

        private static ImageHeader ReadImageHeader(Stream stream, byte[] crc)
        {
            if (!TryReadChunkHeader(stream, out var header))
            {
                throw new ArgumentException("The provided stream did not contain a single chunk.");
            }

            if (header.Name != "IHDR")
            {
                throw new ArgumentException($"The first chunk was not the IHDR chunk: {header}.");
            }

            if (header.Length != 13)
            {
                throw new ArgumentException($"The first chunk did not have a length of 13 bytes: {header}.");
            }

            var ihdrBytes = new byte[13];
            var read = stream.Read(ihdrBytes, 0, ihdrBytes.Length);

            if (read != 13)
            {
                throw new InvalidOperationException($"Did not read 13 bytes for the IHDR, only found: {read}.");
            }

            read = stream.Read(crc, 0, crc.Length);
            if (read != 4)
            {
                throw new InvalidOperationException($"Did not read 4 bytes for the CRC, only found: {read}.");
            }

            var width = StreamHelper.ReadBigEndianInt32(ihdrBytes, 0);
            var height = StreamHelper.ReadBigEndianInt32(ihdrBytes, 4);
            var bitDepth = ihdrBytes[8];
            var colorType = ihdrBytes[9];
            var compressionMethod = ihdrBytes[10];
            var filterMethod = ihdrBytes[11];
            var interlaceMethod = ihdrBytes[12];

            return new ImageHeader(width, height, bitDepth, (ColorType)colorType, (CompressionMethod)compressionMethod, (FilterMethod)filterMethod,
                (InterlaceMethod)interlaceMethod);
        }
    }
}
