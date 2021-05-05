namespace BigGustave.Jpgs
{
    using System.Collections.Generic;
    using System.IO;

    internal class HuffmanTableSpecification
    {
        public byte DestinationIdentifier { get; }

        public HuffmanClass TableClass { get; }

        public byte[] Lengths { get; }

        public byte[] Elements { get; }

        public HuffmanTableSpecification(byte destinationIdentifier, HuffmanClass tableClass, byte[] lengths, byte[] elements)
        {
            DestinationIdentifier = destinationIdentifier;
            TableClass = tableClass;
            Lengths = lengths;
            Elements = elements;
        }

        public static IReadOnlyList<HuffmanTableSpecification> ReadFromMarker(Stream stream)
        {
            var tableDefinitionLength = stream.ReadShort();

            var results = new List<HuffmanTableSpecification>();

            var remainingLength = tableDefinitionLength;

            while (remainingLength > 2)
            {
                var (tableClass, destinationIdentifier) = stream.ReadNibblePair();

                var lengths = new byte[16];
                var numberOfCodes = 0;
                for (var i = 0; i < 16; i++)
                {
                    var val = stream.ReadByteActual();
                    lengths[i] = val;
                    numberOfCodes += val;
                }

                remainingLength -= 17;

                var huffmanCodeValues = new byte[numberOfCodes];
                for (var i = 0; i < numberOfCodes; i++)
                {
                    huffmanCodeValues[i] = stream.ReadByteActual();
                    remainingLength--;
                }

                results.Add(new HuffmanTableSpecification(
                    destinationIdentifier,
                    (HuffmanClass)tableClass,
                    lengths,
                    huffmanCodeValues));
            }

            return results;
        }

        public enum HuffmanClass : byte
        {
            DcTable = 0,
            AcTable = 1
        }
    }
}
