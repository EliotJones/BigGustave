namespace BigGustave.Jpgs
{
    using System.IO;

    internal class HuffmanTableSpecification
    {
        private const short NonValueLength = 2 + 1 + 16;

        public byte DestinationIdentifier { get; set; }

        public byte TableClass { get; set; }

        public byte[] CodesPerLength { get; set; }

        public byte[] CodeValues { get; set; }

        public static HuffmanTableSpecification ReadFromMarker(Stream stream, bool strictMode)
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

            return new HuffmanTableSpecification
            {
                TableClass = tableClass,
                DestinationIdentifier = destinationIdentifier,
                CodeValues = huffmanCodeValues,
                CodesPerLength = lengths
            };
        }
    }
}
