namespace BigGustave
{
    using System;

    internal static class Decoder
    {
        public static IPixel[,] Decode(byte[] decompressedData, ImageHeader header)
        {
            var bytesPerPixel = BytesPerPixel(header, out var samplesPerPixel);

            var pixels = new IPixel[header.Height, header.Width];
            switch (header.InterlaceMethod)
            {
                case InterlaceMethod.None:
                    {
                        var bytesPerScanline = BytesPerScanline(header, samplesPerPixel);

                        for (var rowIndex = 0; rowIndex < header.Height; rowIndex++)
                        {
                            var filterType = (FilterType)decompressedData[rowIndex * bytesPerScanline + (1 * rowIndex)];

                            var previousRowStartByteAbsolute = (rowIndex) + (bytesPerScanline * (rowIndex - 1));
                            var currentRowStartByteAbsolute = (rowIndex + 1) + (bytesPerScanline * rowIndex);

                            for (var rowByteIndex = 0; rowByteIndex < bytesPerScanline; rowByteIndex++)
                            {
                                var currentByteAbsolute = currentRowStartByteAbsolute + rowByteIndex;

                                ReverseFilter(decompressedData, filterType, previousRowStartByteAbsolute, currentRowStartByteAbsolute, currentByteAbsolute, rowByteIndex, bytesPerPixel);
                            }
                        }

                        for (var row = 0; row < header.Height; row++)
                        {
                            var currentRowStartByteAbsolute = (row + 1) + (bytesPerScanline * row);
                            for (var col = 0; col < header.Width; col++)
                            {
                                var currentByteAbsolute = currentRowStartByteAbsolute + (col * bytesPerPixel);

                                switch (bytesPerPixel)
                                {
                                    case 1:
                                        {
                                            pixels[row, col] = new GrayscalePixel(decompressedData[currentByteAbsolute]);
                                            break;
                                        }
                                    case 4:
                                        {
                                            var r = decompressedData[currentByteAbsolute];
                                            var g = decompressedData[currentByteAbsolute + 1];
                                            var b = decompressedData[currentByteAbsolute + 2];
                                            var a = decompressedData[currentByteAbsolute + 3];
                                            pixels[row, col] = new RgbaPixel(r, g, b, a);
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    }
                case InterlaceMethod.Adam7:
                    {
                        throw new NotImplementedException("Adam7 interlacing not yet implemented.");
                    }
            }

            return pixels;
        }

        private static byte BytesPerPixel(ImageHeader header, out byte samplesPerPixel)
        {
            var bitDepthCorrected = (header.BitDepth + 7) / 8;

            samplesPerPixel = SamplesPerPixel(header);

            return (byte)(samplesPerPixel * bitDepthCorrected);
        }

        private static byte SamplesPerPixel(ImageHeader header)
        {
            switch (header.ColorType)
            {
                case ColorType.None:
                    return 1;
                case ColorType.PaletteUsed:
                    return 1;
                case ColorType.ColorUsed:
                    return 3;
                case ColorType.AlphaChannelUsed:
                    return 2;
                case ColorType.ColorUsed | ColorType.AlphaChannelUsed:
                    return 4;
                default:
                    return 0;
            }
        }

        private static int BytesPerScanline(ImageHeader header, byte samplesPerPixel)
        {
            var width = header.Width;

            switch (header.BitDepth)
            {
                case 1:
                    return (width + 7) / 8;
                case 2:
                    return (width + 3) / 4;
                case 4:
                    return (width + 1) / 2;
                case 8:
                case 16:
                    return width * samplesPerPixel * (header.BitDepth / 8);
                default:
                    return 0;
            }
        }

        private static void ReverseFilter(byte[] data, FilterType type, int previousRowStartByteAbsolute, int rowStartByteAbsolute, int byteAbsolute, int rowByteIndex, int bytesPerPixel)
        {
            byte GetLeftByteValue()
            {
                var leftIndex = rowByteIndex - bytesPerPixel;
                var leftValue = leftIndex >= 0 ? data[rowStartByteAbsolute + leftIndex] : (byte)0;
                return leftValue;
            }

            byte GetAboveByteValue()
            {
                var upIndex = previousRowStartByteAbsolute + rowByteIndex;
                return upIndex >= 0 ? data[upIndex] : (byte)0;
            }

            byte GetAboveLeftByteValue()
            {
                var index = previousRowStartByteAbsolute + rowByteIndex - bytesPerPixel;
                return index < previousRowStartByteAbsolute || previousRowStartByteAbsolute < 0 ? (byte)0 : data[index];
            }

            switch (type)
            {
                case FilterType.None:
                    return;
                case FilterType.Sub:
                    data[byteAbsolute] += GetLeftByteValue();
                    break;
                case FilterType.Up:
                    data[byteAbsolute] += GetAboveByteValue();
                    break;
                case FilterType.Average:
                    data[byteAbsolute] += (byte)((GetLeftByteValue() + GetAboveByteValue()) / 2);
                    break;
                case FilterType.Paeth:
                    var a = GetLeftByteValue();
                    var b = GetAboveByteValue();
                    var c = GetAboveLeftByteValue();
                    data[byteAbsolute] += GetPaethValue(a, b, c);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Computes a simple linear function of the three neighboring pixels (left, above, upper left),
        /// then chooses as predictor the neighboring pixel closest to the computed value.
        /// </summary>
        private static byte GetPaethValue(byte a, byte b, byte c)
        {
            var p = a + b - c;
            var pa = Math.Abs(p - a);
            var pb = Math.Abs(p - b);
            var pc = Math.Abs(p - c);

            if (pa <= pb && pa <= pc)
            {
                return a;
            }

            return pb <= pc ? b : c;
        }
    }

    public enum FilterType
    {
        /// <summary>
        /// The raw byte is unaltered.
        /// </summary>
        None = 0,
        /// <summary>
        /// The byte to the left.
        /// </summary>
        Sub = 1,
        /// <summary>
        /// The byte above.
        /// </summary>
        Up = 2,
        /// <summary>
        /// The mean of bytes left and above, rounded down.
        /// </summary>
        Average = 3,
        /// <summary>
        /// Byte to the left, above or top-left based on Paeth's algorithm.
        /// </summary>
        Paeth = 4
    }
}
