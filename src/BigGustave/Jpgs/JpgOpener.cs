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
                        break;
                    case JpgMarkers.DefineRestartInterval:
                        skipData = false;
                        break;
                    case JpgMarkers.StartOfScan:
                        skipData = true;
                        break;
                    case JpgMarkers.StartOfBaselineDctFrame:
                        var frame = BaselineDctFrame.ReadFromMarker(stream, strictMode);
                        skipData = true;
                        break;
                    case JpgMarkers.StartOfProgressiveDctFrame:
                        skipData = false;
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
    }
}
