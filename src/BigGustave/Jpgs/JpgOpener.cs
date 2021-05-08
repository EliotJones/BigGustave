namespace BigGustave.Jpgs
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal static class JpgOpener
    {
        private const byte MarkerStart = 255;
        private const byte StartOfImage = 216;

        public static Jpg Open(Stream stream, bool strictMode)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException($"The provided stream of type {stream.GetType().FullName} was not readable.");
            }

            if (!HasJpgHeader(stream) && strictMode)
            {
                throw new ArgumentException("The provided stream did not start with the JPEG header.");
            }

            var jfif = default(Jfif);
            var comments = new List<Comment>();
            var quantizationTables = new Dictionary<int, QuantizationTableSpecification>();
            var dcHuffmanTables = new Dictionary<int, HuffmanTable>();
            var acHuffmanTables = new Dictionary<int, HuffmanTable>();

            var rgbData = new byte[0];
            var frames = new List<Frame>();

            var marker = stream.ReadSegmentMarker();

            var markerType = (JpgMarkers)marker;

            while (markerType != JpgMarkers.EndOfImage)
            {
                var skipData = true;

                switch (markerType)
                {
                    case JpgMarkers.ApplicationSpecific0:
                        jfif = Jfif.ReadFromApp0(stream);
                        break;
                    case JpgMarkers.Comment:
                        skipData = false;
                        var comment = Comment.ReadFromMarker(stream);
                        comments.Add(comment);
                        break;
                    case JpgMarkers.DefineQuantizationTable:
                        skipData = false;
                        var specifications = QuantizationTableSpecification.ReadFromMarker(stream, strictMode);
                        foreach (var specification in specifications)
                        {
                            quantizationTables[specification.TableDestinationIdentifier] = specification;
                        }
                        break;
                    case JpgMarkers.DefineHuffmanTable:
                        skipData = false;
                        var huffmanTableSpecifications = HuffmanTableSpecification.ReadFromMarker(stream);

                        foreach (var specification in huffmanTableSpecifications)
                        {
                            if (specification.TableClass == HuffmanTableSpecification.HuffmanClass.DcTable)
                            {
                                dcHuffmanTables[specification.DestinationIdentifier] = HuffmanTable.FromSpecification(specification);
                            }
                            else
                            {
                                acHuffmanTables[specification.DestinationIdentifier] = HuffmanTable.FromSpecification(specification);
                            }
                        }
                        break;
                    case JpgMarkers.DefineArithmeticCodingConditioning:
                        throw new NotSupportedException("No support for arithmetic coding conditioning table yet.");
                    case JpgMarkers.DefineRestartInterval:
                        skipData = false;
                        // Specifies the length of this segment.
                        var restartIntervalSegmentLength = stream.ReadShort();
                        // Specifies the number of MCU in the restart interval.
                        var restartInterval = stream.ReadShort();
                        break;
                    case JpgMarkers.StartOfScan:
                        skipData = false;
                        var scanSingle = Scan.ReadFromMarker(stream, strictMode);
                        if (frames.Count == 0)
                        {
                            throw new InvalidOperationException("Scan encountered outside any frame.");
                        }

                        var frameForScan = frames[frames.Count - 1];

                        rgbData = new byte[3 * frameForScan.ImageHeight * frameForScan.ImageWidth];

                        frameForScan.Scans.Add(scanSingle);

                        if (frameForScan.FrameType != FrameType.BaselineHuffman)
                        {
                            throw new NotSupportedException($"No support for frame type: {frameForScan.FrameType}.");
                        }

                        ProcessScan(
                            rgbData,
                            frameForScan,
                            scanSingle,
                            quantizationTables,
                            dcHuffmanTables,
                            acHuffmanTables);

                        break;
                    case JpgMarkers.StartOfBaselineDctHuffmanFrame:
                    case JpgMarkers.StartOfExtendedSequentialDctHuffmanFrame:
                    case JpgMarkers.StartOfProgressiveDctHuffmanFrame:
                    case JpgMarkers.StartOfLosslessHuffmanFrame:
                    case JpgMarkers.StartOfDifferentialSequentialDctHuffmanFrame:
                    case JpgMarkers.StartOfDifferentialProgressiveDctHuffmanFrame:
                    case JpgMarkers.StartOfDifferentialLosslessHuffmanFrame:
                    case JpgMarkers.StartOfExtendedSequentialDctArithmeticFrame:
                    case JpgMarkers.StartOfProgressiveDctArithmeticFrame:
                    case JpgMarkers.StartOfLosslessArithmeticFrame:
                    case JpgMarkers.StartOfDifferentialSequentialDctArithmeticFrame:
                    case JpgMarkers.StartOfDifferentialProgressiveDctArithmeticFrame:
                    case JpgMarkers.StartOfDifferentialLosslessArithmeticFrame:
                        skipData = false;
                        var frame = Frame.ReadFromMarker(stream, strictMode, marker);
                        frames.Add(frame);

                        break;
                }

                marker = stream.ReadSegmentMarker(skipData, $"Expected next marker after reading section of type: {markerType}.");

                markerType = (JpgMarkers)marker;
            }

            if (frames.Count == 0 || frames[frames.Count - 1].Scans.Count == 0)
            {
                throw new InvalidOperationException($"No image data found in the provided JPG.");
            }

            var result = frames[frames.Count - 1];

            return new Jpg(result.ImageWidth, result.ImageHeight, rgbData, jfif, comments);
        }

        private static void ProcessScan(
            byte[] resultHolder,
            Frame frame,
            Scan scan,
            IReadOnlyDictionary<int, QuantizationTableSpecification> quantizationTables,
            IReadOnlyDictionary<int, HuffmanTable> dcHuffmanTables,
            IReadOnlyDictionary<int, HuffmanTable> acHuffmanTables)
        {
            var str = new BitStream(scan.Data);

            // Y, Cb, Cr
            var oldDcCoefficients = new int[frame.NumberOfComponents];

            var samplesByComponent = new List<List<double[]>>();

            var indexForResult = 0;
            for (var row = 0; row < frame.McusPerY; row++)
            {
                for (var col = 0; col < frame.McusPerX; col++)
                {
                    samplesByComponent.Clear();

                    for (var componentIndex = 0; componentIndex < frame.Components.Length; componentIndex++)
                    {
                        var componentSamples = new List<double[]>();
                        var component = frame.Components[componentIndex];

                        var qt = quantizationTables[component.DestinationQuantizationTableSelector];
                        var index = componentIndex > 0 ? 1 : 0;

                        for (var y = 0; y < component.HorizontalSamplingFactor; y++)
                        {
                            for (var x = 0; x < component.VerticalSamplingFactor; x++)
                            {
                                var (newDcCoeff, dataUnit) = DecodeDataUnit(
                                    str,
                                    index,
                                    qt,
                                    oldDcCoefficients[componentIndex],
                                    dcHuffmanTables,
                                    acHuffmanTables);

                                oldDcCoefficients[componentIndex] = newDcCoeff;

                                componentSamples.Add(dataUnit);
                            }
                        }

                        samplesByComponent.Add(componentSamples);
                    }

                    for (int y = 0; y < 8; y++)
                    {
                        if (y >= frame.ImageHeight)
                        {
                            break;
                        }

                        for (int x = 0; x < 8; x++)
                        {
                            if (x >= frame.ImageWidth)
                            {
                                break;
                            }

                            var flatIndex = (y * 8) + x;
                            var (r, g, b) = ToRgb(samplesByComponent[0][0][flatIndex],
                                samplesByComponent[1][0][flatIndex],
                                samplesByComponent[2][0][flatIndex]);
                            resultHolder[indexForResult++] = r;
                            resultHolder[indexForResult++] = g;
                            resultHolder[indexForResult++] = b;
                        }
                    }
                }
            }
        }

        private static (int dcCoefficient, double[] data) DecodeDataUnit(BitStream stream,
            int index,
            QuantizationTableSpecification quantization,
            int previousDcCoefficient,
            IReadOnlyDictionary<int, HuffmanTable> dcHuffmanTables,
            IReadOnlyDictionary<int, HuffmanTable> acHuffmanTables)
        {
            // Each Minimum Coded Unit (MCU / 8*8 block) has 64 values, 1 DC and 63 AC coefficients.

            // First up we get the DC coefficient, this is encoded as a difference from the DC coefficient in the previous MCU.
            var table = dcHuffmanTables[index];

            var category = table.Read(stream);

            if (!category.HasValue)
            {
                throw new InvalidOperationException();
            }

            var value = stream.ReadNBits(category.Value);

            var difference = JpgDecodeUtil.GetDcDifferenceOrAcCoefficient(category.Value, value);

            var newDcCoefficient = previousDcCoefficient + difference;

            var data = new double[64];

            data[0] = newDcCoefficient * quantization.QuantizationTableElements[0];

            var acHuffmanTable = acHuffmanTables[index];

            // Now we decode the AC coefficients.
            for (var i = 1; i < 64; i++)
            {
                /*
                 * AC coefficients are run-length encoded (RLE). The RLE data is then saved
                 * as the number of preceding zeros (RRRR) and the actual value (SSSS).
                 */
                var acCategoryRead = acHuffmanTable.Read(stream);

                if (!acCategoryRead.HasValue)
                {
                    throw new InvalidOperationException();
                }

                var acCategory = acCategoryRead.Value;

                // The end-of-block (EOB) special marker, all remaining values are 0.
                if (acCategory == 0b0000_0000)
                {
                    break;
                }

                // The high 4 bits are the number of preceding values
                if (acCategory > 0b0000_1111)
                {
                    i += (acCategory >> 4);
                    acCategory = (byte)(acCategory & 0b0000_1111);
                }

                if (i > 63)
                {
                    break;
                }

                var bits = stream.ReadNBits(acCategory);

                var acCoefficient = JpgDecodeUtil.GetDcDifferenceOrAcCoefficient(acCategory, bits);

                var acValue = acCoefficient * quantization.QuantizationTableElements[i];

                var indexForValue = JpgDecodeUtil.ZigZagPattern[i];

                data[indexForValue] = acValue;
            }

            var normalizingZeroFactor = 1 / Math.Sqrt(2);

            var fullResult = new double[64];

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    // TODO: better IDCT https://github.com/libjpeg-turbo/libjpeg-turbo/blob/master/jddctmgr.c
                    // Or we could just cache most of this
                    var sum = 0d;

                    for (int u = 0; u < 8; u++)
                    {
                        var cu = u == 0 ? normalizingZeroFactor : 1;
                        for (int v = 0; v < 8; v++)
                        {
                            var cv = v == 0 ? normalizingZeroFactor : 1;

                            var valInner = cu * cv * data[(u * 8) + v]
                                           * Math.Cos(((2 * x + 1) * u * Math.PI) / 16)
                                           * Math.Cos(((2 * y + 1) * v * Math.PI) / 16);

                            sum += valInner;
                        }
                    }

                    // Transpose X and Y here
                    var resultIndex = (y) + (x * 8);
                    var pixelValue = Math.Round((0.25 * sum) + 128);

                    fullResult[resultIndex] = pixelValue;
                }
            }

            return (newDcCoefficient, fullResult);
        }

        private static (byte, byte, byte) ToRgb(double y, double cb, double cr)
        {
            var crVal = cr - 128;
            var cbVal = cb - 128;

            var r = Math.Floor(y + (1.402 * crVal));
            var g = Math.Floor(y - (0.34414 * cbVal) - (0.71414 * crVal));
            var b = Math.Floor(y + (1.772 * cbVal));

            r = r < 0 ? 0 : r > 255 ? 255 : r;
            g = g < 0 ? 0 : g > 255 ? 255 : g;
            b = b < 0 ? 0 : b > 255 ? 255 : b;

            return ((byte)r, (byte)g, (byte)b);
        }

        public static bool HasJpgHeader(Stream stream)
        {
            var bytes = new byte[2];

            var read = stream.Read(bytes, 0, 2);

            if (read != 2)
            {
                return false;
            }

            return bytes[0] == MarkerStart
                   && bytes[1] == StartOfImage;
        }
    }
}
