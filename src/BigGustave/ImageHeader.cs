namespace BigGustave
{
    using System;
    using System.Collections.Generic;

    public readonly struct ImageHeader
    {
        internal static readonly byte[] HeaderBytes = {
            73, 72, 68, 82
        };

        private static readonly IReadOnlyDictionary<ColorType, HashSet<byte>> PermittedBitDepths = new Dictionary<ColorType, HashSet<byte>>
        {
            {ColorType.None, new HashSet<byte> {1, 2, 4, 8, 16}},
            {ColorType.ColorUsed, new HashSet<byte> {8, 16}},
            {ColorType.PaletteUsed | ColorType.ColorUsed, new HashSet<byte> {1, 2, 4, 8}},
            {ColorType.AlphaChannelUsed, new HashSet<byte> {8, 16}},
            {ColorType.AlphaChannelUsed | ColorType.ColorUsed, new HashSet<byte> {8, 16}},
        };

        public int Width { get; }

        public int Height { get; }

        public byte BitDepth { get; }

        public ColorType ColorType { get; }

        public CompressionMethod CompressionMethod { get; }

        public FilterMethod FilterMethod { get; }

        public InterlaceMethod InterlaceMethod { get; }

        public ImageHeader(int width, int height, byte bitDepth, ColorType colorType, CompressionMethod compressionMethod, FilterMethod filterMethod, InterlaceMethod interlaceMethod)
        {
            if (width == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Invalid width (0) for image.");
            }

            if (height == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Invalid height (0) for image.");
            }

            if (!PermittedBitDepths.TryGetValue(colorType, out var permitted)
                || !permitted.Contains(bitDepth))
            {
                throw new ArgumentException($"The bit depth {bitDepth} is not permitted for color type {colorType}.");
            }

            Width = width;
            Height = height;
            BitDepth = bitDepth;
            ColorType = colorType;
            CompressionMethod = compressionMethod;
            FilterMethod = filterMethod;
            InterlaceMethod = interlaceMethod;
        }

        public override string ToString()
        {
            return $"w: {Width}, h: {Height}, bitDepth: {BitDepth}, colorType: {ColorType}, " +
                   $"compression: {CompressionMethod}, filter: {FilterMethod}, interlace: {InterlaceMethod}.";
        }
    }
}