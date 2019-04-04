namespace BigGustave
{
    using System;

    /// <summary>
    /// Provides convenience methods for indexing into a raw byte array to extract pixel values.
    /// </summary>
    internal class RawPngData
    {
        private readonly byte[] data;
        private readonly int bytesPerPixel;
        private readonly int width;
        private readonly Palette palette;
        private readonly int rowOffset;

        /// <summary>
        /// Create a new <see cref="RawPngData"/>.
        /// </summary>
        /// <param name="data">The decoded pixel data as bytes.</param>
        /// <param name="bytesPerPixel">The number of bytes in each pixel.</param>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="interlaceMethod">The interlace method used.</param>
        /// <param name="palette"></param>
        public RawPngData(byte[] data, int bytesPerPixel, int width, InterlaceMethod interlaceMethod, Palette palette)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException($"Width must be greater than or equal to 0, got {width}.");
            }

            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.bytesPerPixel = bytesPerPixel;
            this.width = width;
            this.palette = palette;
            rowOffset = interlaceMethod == InterlaceMethod.Adam7 ? 0 : 1;
        }

        public Pixel GetPixel(int x, int y)
        {
            var rowStartPixel = (rowOffset + (rowOffset * y)) + (bytesPerPixel * width * y);

            var pixelStartIndex = rowStartPixel + (bytesPerPixel * x);

            var first = data[pixelStartIndex];

            if (palette != null)
            {
                return palette.GetPixel(first);
            }

            switch (bytesPerPixel)
            {
                case 1:
                    return new Pixel(first, first, first, 255, true);
                case 2:
                    return new Pixel(first, first, first, data[pixelStartIndex + 1], true);
                case 3:
                    return new Pixel(first, data[pixelStartIndex + 1], data[pixelStartIndex + 2], 255, false);
                case 4:
                    return new Pixel(first, data[pixelStartIndex + 1], data[pixelStartIndex + 2], data[pixelStartIndex + 3], false);
                default:
                    throw new InvalidOperationException($"Unreconized number of bytes per pixel: {bytesPerPixel}.");
            }
        }
    }
}