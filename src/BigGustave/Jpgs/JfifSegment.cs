namespace BigGustave.Jpgs
{
    using System.IO;

    internal class JfifSegment
    {
        public byte MajorVersion { get; }
        
        public byte MinorVersion { get; }

        public PixelUnitDensity PixelUnitDensity { get; }

        public short HorizontalPixelDensity { get; }

        public short VerticalPixelDensity { get; }

        public byte[] Thumbnail { get; }

        public JfifSegment(
            byte majorVersion,
            byte minorVersion,
            PixelUnitDensity pixelUnitDensity,
            short horizontalPixelDensity,
            short verticalPixelDensity,
            byte[] thumbnail)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            PixelUnitDensity = pixelUnitDensity;
            HorizontalPixelDensity = horizontalPixelDensity;
            VerticalPixelDensity = verticalPixelDensity;
            Thumbnail = thumbnail;
        }

        public static JfifSegment ReadFromApp0(Stream stream)
        {
            var pos = stream.Position;
            var length = stream.ReadShort();
            var b1 = stream.ReadByteActual();
            if (b1 != 'J')
            {
                stream.Seek(pos, SeekOrigin.Begin);
                return null;
            }

            var b2 = stream.ReadByteActual();
            if (b2 != 'F')
            {
                stream.Seek(pos, SeekOrigin.Begin);
                return null;
            }

            var b3 = stream.ReadByteActual();
            if (b3 != 'I')
            {
                stream.Seek(pos, SeekOrigin.Begin);
                return null;
            }

            var b4 = stream.ReadByteActual();
            if (b4 != 'F')
            {
                stream.Seek(pos, SeekOrigin.Begin);
                return null;
            }

            var b5 = stream.ReadByteActual();
            if (b5 != 0)
            {
                stream.Seek(pos, SeekOrigin.Begin);
                return null;
            }

            var major = stream.ReadByteActual();
            var minor = stream.ReadByteActual();

            var pixelDensity = stream.ReadByteActual();

            var horizontalPixelDensity = stream.ReadShort();
            var verticalPixelDensity = stream.ReadShort();

            var horizontalThumbnailPixelCount = stream.ReadByteActual();
            var verticalThumbnailPixelCount = stream.ReadByteActual();

            var thumbnailLength = 3 * horizontalThumbnailPixelCount * verticalThumbnailPixelCount;
            var thumbnailRgb = new byte[thumbnailLength];
                stream.Read(thumbnailRgb, 0, thumbnailRgb.Length);

            return new JfifSegment(major, minor, (Jpgs.PixelUnitDensity)pixelDensity,
                horizontalPixelDensity,
                verticalPixelDensity,
                thumbnailRgb);
        }
    }

    internal enum PixelUnitDensity : byte
    {
        None = 0,
        PixelsPerInch = 1,
        PixelsPerCm = 2
    }
}
