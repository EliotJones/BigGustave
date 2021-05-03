namespace BigGustave.Jpgs
{
    using System.IO;

    internal class HuffmanTableSpecification
    {
        private const short NonValueLength = 2 + 1 + 16;

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

        public static HuffmanTableSpecification ReadFromMarker(Stream stream)
        {
            var tableDefinitionLength = stream.ReadShort();

            var (tableClass, destinationIdentifier) = stream.ReadNibblePair();

            var lengths = new byte[16];
            for (var i = 0; i < 16; i++)
            {
                lengths[i] = stream.ReadByteActual();
            }

            var variablesLength = tableDefinitionLength - NonValueLength;

            var huffmanCodeValues = new byte[variablesLength];
            for (var i = 0; i < variablesLength; i++)
            {
                huffmanCodeValues[i] = stream.ReadByteActual();
            }

            return new HuffmanTableSpecification(
                destinationIdentifier,
                (HuffmanClass) tableClass,
                lengths,
                huffmanCodeValues);
        }

        public enum HuffmanClass : byte
        {
            DcTable = 0,
            AcTable = 1
        }
    }
}
