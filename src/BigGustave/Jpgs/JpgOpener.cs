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

            var comments = new List<CommentSection>();
            var quantizationTables = new Dictionary<int, QuantizationTableSpecification>();
            var dcHuffmanTables = new Dictionary<int, HuffmanTable>();
            var acHuffmanTables = new Dictionary<int, HuffmanTable>();

            var frames = new List<Frame>();

            var marker = stream.ReadSegmentMarker();

            var markerType = (JpgMarkers)marker;

            while (markerType != JpgMarkers.EndOfImage)
            {
                var skipData = true;

                switch (markerType)
                {
                    case JpgMarkers.Comment:
                        skipData = false;
                        var comment = CommentSection.ReadFromMarker(stream);
                        comments.Add(comment);
                        break;
                    case JpgMarkers.DefineQuantizationTable:
                        skipData = false;
                        var specification = QuantizationTableSpecification.ReadFromMarker(stream, strictMode);
                        quantizationTables[specification.TableDestinationIdentifier] = specification;
                        break;
                    case JpgMarkers.DefineHuffmanTable:
                        skipData = false;
                        var huffman = HuffmanTableSpecification.ReadFromMarker(stream);

                        if (huffman.TableClass == HuffmanTableSpecification.HuffmanClass.DcTable)
                        {
                            dcHuffmanTables[huffman.DestinationIdentifier] = HuffmanTable.FromSpecification(huffman);
                        }
                        else
                        {
                            acHuffmanTables[huffman.DestinationIdentifier] = HuffmanTable.FromSpecification(huffman);
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
                            throw new InvalidOperationException($"Scan encountered outside any frame.");
                        }

                        var frameForScan = frames[frames.Count - 1];

                        frameForScan.Scans.Add(scanSingle);

                        ProcessFrame(frameForScan,
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
                    default:
                        break;
                }

                marker = stream.ReadSegmentMarker(skipData, $"Expected next marker after reading section of type: {markerType}.");

                markerType = (JpgMarkers)marker;
            }

            throw new NotImplementedException();
        }

        private static void ProcessFrame(Frame frame, Scan scan,
            IReadOnlyDictionary<int, QuantizationTableSpecification> quantizationTables,
            IReadOnlyDictionary<int, HuffmanTable> dcHuffmanTables,
            IReadOnlyDictionary<int, HuffmanTable> acHuffmanTables)
        {
            var str = new BitStream(scan.Data);

            // Y, Cb, Cr
            var oldDcCoefficients = new int[] { 0, 0, 0 };

            // TODO: 0 special treatment
            var blocksHeight = (frame.NumberOfLines / 8) + (frame.NumberOfLines % 8 > 0 ? 1 : 0);
            var blocksWidth = (frame.NumberOfSamplesPerLine / 8) + (frame.NumberOfSamplesPerLine % 8 > 0 ? 1 : 0);

            for (int i = 0; i < blocksHeight; i++)
            {
                for (int j = 0; j < blocksHeight; j++)
                {
                    var qtY = quantizationTables[frame.FrameComponentSpecifications[0].DestinationQuantizationTableSelector];
                    var qtCb = quantizationTables[frame.FrameComponentSpecifications[1].DestinationQuantizationTableSelector];
                    var qtCr = quantizationTables[frame.FrameComponentSpecifications[2].DestinationQuantizationTableSelector];
                    BuildMatrix(str, 0, qtY, oldDcCoefficients[0], dcHuffmanTables);
                }
            }
        }

        private static void BuildMatrix(BitStream stream,
            int index,
            object quantization,
            int previousDcCoefficient,
            IReadOnlyDictionary<int, HuffmanTable> huffmanTables)
        {
            var table = huffmanTables[index];
            var code = table.GetCode(stream);
        }

        private static (byte, byte, byte) ToRgb(byte y, byte cb, byte cr)
        {
            var crVal = cr - 128;
            var cbVal = cb - 128;

            return (
                    (byte)(y + (1.402 * crVal)),
                    (byte)(y - (0.34414 * cbVal) - (0.71414 * crVal)),
                    (byte)(y + (1.772 * cbVal)));
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

        private static void AddOrUpdate<T>(Dictionary<int, List<T>> dic, int key, T val)
        {
            if (!dic.TryGetValue(key, out var values))
            {
                values = new List<T>();
                dic[key] = values;
            }

            values.Add(val);
        }
    }

    internal class BitStream
    {
        private int bitOffset;
        private readonly IReadOnlyList<byte> data;

        public BitStream(IReadOnlyList<byte> data)
        {
            this.data = data;
        }

        public bool Read()
        {
            var byteIndex = bitOffset / 8;

            var byteVal = data[bitOffset / 8];

            bitOffset++;

            var withinByteIndex = bitOffset - (byteIndex * 8);

            // TODO: LSB?
            return ((1 << withinByteIndex) & byteVal) > 0;
        }
    }

    internal static class InverseDiscreteCosineTransformer
    {
        private static readonly byte[] ZigZagPattern = new byte[]
        {
            0,  1,  8, 16,  9,  2,  3, 10,
            17, 24, 32, 25, 18, 11,  4,  5,
            12, 19, 26, 33, 40, 48, 41, 34,
            27, 20, 13,  6,  7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36,
            29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46,
            53, 60, 61, 54, 47, 55, 62, 63
        };

        public static byte[] Reverse()
        {
            var result = new byte[64];

            return result;
        }
    }
}
