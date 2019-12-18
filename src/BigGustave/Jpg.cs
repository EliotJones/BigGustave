namespace BigGustave
{
    using System;
    using System.IO;
    using Jpgs;

    /// <summary>
    /// A JPEG image.
    /// </summary>
    public class Jpg
    {
        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public int Height { get; }
        
        /// <summary>
        /// Get the pixel at the given column and row (x, y).
        /// </summary>
        /// <remarks>
        /// Pixel values are generated on demand from the underlying data to prevent holding many items in memory at once, so consumers
        /// should cache values if they're going to be looped over many time.
        /// </remarks>
        /// <param name="x">The x coordinate (column).</param>
        /// <param name="y">The y coordinate (row).</param>
        /// <returns>The pixel at the coordinate.</returns>
        public Pixel GetPixel(int x, int y) => throw new NotImplementedException("Not implemented for jpegs.");

        public static Jpg Open(Stream stream) => JpgOpener.Open(stream, true);
    }
}
