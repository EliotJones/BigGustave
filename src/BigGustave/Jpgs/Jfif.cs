namespace BigGustave.Jpgs
{
    using System.IO;

    /// <summary>
    /// The JFIF format information in the APP0 section.
    /// </summary>
    public class Jfif
    {
        /// <summary>
        /// The major version.
        /// </summary>
        public byte MajorVersion { get; }
        
        /// <summary>
        /// The minor version.
        /// </summary>
        public byte MinorVersion { get; }

        /// <summary>
        /// The units for horizontal/vertical pixel densities.
        /// </summary>
        public PixelDensityUnit PixelDensityUnit { get; }

        /// <summary>
        /// The horizontal pixel density in <see cref="PixelDensityUnit"/>s.
        /// </summary>
        public short HorizontalPixelDensity { get; }

        /// <summary>
        /// The vertical pixel density in <see cref="PixelDensityUnit"/>s.
        /// </summary>
        public short VerticalPixelDensity { get; }

        /// <summary>
        /// The raw bytes of the thumbnail if present (R, G, B).
        /// </summary>
        public byte[] Thumbnail { get; }

        internal Jfif(
            byte majorVersion,
            byte minorVersion,
            PixelDensityUnit pixelDensityUnit,
            short horizontalPixelDensity,
            short verticalPixelDensity,
            byte[] thumbnail)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            PixelDensityUnit = pixelDensityUnit;
            HorizontalPixelDensity = horizontalPixelDensity;
            VerticalPixelDensity = verticalPixelDensity;
            Thumbnail = thumbnail;
        }

        internal static Jfif ReadFromApp0(Stream stream)
        {
            var pos = stream.Position;
            stream.ReadShort();
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

            return new Jfif(major, minor, (PixelDensityUnit)pixelDensity,
                horizontalPixelDensity,
                verticalPixelDensity,
                thumbnailRgb);
        }
    }
}
