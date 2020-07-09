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
            var huffmanTableTracker = new Dictionary<int, List<HuffmanTableSpecification>>();
            var huffmanTables = new Dictionary<int, HuffmanTableSpecification>();

            var frames = new List<BaselineDctFrame>();
            
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
                        var huffman = HuffmanTableSpecification.ReadFromMarker(stream, strictMode);
                        huffmanTables[huffman.DestinationIdentifier] = huffman;
                        AddOrUpdate(huffmanTableTracker, huffman.DestinationIdentifier, huffman);
                        break;
                    case JpgMarkers.DefineRestartInterval:
                        skipData = false;
                        break;
                    case JpgMarkers.StartOfScan:
                        skipData = false;
                        var scanSingle = Scan.ReadFromMarker(stream, strictMode);
                        break;
                    case JpgMarkers.StartOfBaselineDctFrame:
                        skipData = true;
                        var frame = BaselineDctFrame.ReadFromMarker(stream, strictMode);
                        frames.Add(frame);
                        break;
                    case JpgMarkers.StartOfProgressiveDctFrame:
                        skipData = false;
                        break;
                    default:
                        break;
                }

                marker = stream.ReadSegmentMarker(skipData, $"Expected next marker after reading section of type: {markerType}.");

                markerType = (JpgMarkers)marker;
            }

            throw new NotImplementedException();
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
}
