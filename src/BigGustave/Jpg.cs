namespace BigGustave
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Jpgs;

    /// <summary>
    /// A JPEG image.
    /// </summary>
    public class Jpg
    {
        private readonly byte[] rawData;

        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// The JFIF format APP0 section from the image data.
        /// </summary>
        public Jfif Jfif { get; set; }

        /// <summary>
        /// Any comments found in the file.
        /// </summary>
        public IReadOnlyList<Comment> Comments { get; }

        /// <summary>
        /// Create a new <see cref="Jpg"/>.
        /// </summary>
        internal Jpg(int width, int height, byte[] rawData, Jfif jfif, IReadOnlyList<Comment> comments)
        {
            Width = width;
            Height = height;
            Jfif = jfif;
            Comments = comments ?? Array.Empty<Comment>();

            this.rawData = rawData;
        }

        /// <summary>
        /// Get the pixel at the given column and row (x, y).
        /// </summary>
        /// <remarks>
        /// Pixel values are generated on demand from the underlying data to prevent holding many items in memory at once, so consumers
        /// should cache values if they're going to be looped over many times.
        /// </remarks>
        /// <param name="x">The x coordinate (column).</param>
        /// <param name="y">The y coordinate (row).</param>
        /// <returns>The pixel at the coordinate.</returns>
        public Pixel GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"Could not retrieve pixel value at x coordinate: {x}.");
            }

            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"Could not retrieve pixel value at y coordinate: {y}.");
            }

            var flatIndexR = (y * 3) * Width + (x * 3);

            var r = rawData[flatIndexR];
            var g = rawData[flatIndexR + 1];
            var b = rawData[flatIndexR + 2];

            return new Pixel(r, g, b);
        }

        /// <summary>
        /// Open and parse a JPG file from the stream.
        /// </summary>
        public static Jpg Open(Stream stream) => JpgOpener.Open(stream, true);
    }
}
