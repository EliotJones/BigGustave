namespace BigGustave.Jpgs
{
    using System.IO;

    internal class Scan
    {
        public static Scan ReadFromMarker(Stream stream, bool strictMode)
        {
            var length = stream.ReadShort();

            var numberOfScanImageComponents = stream.ReadByteActual();

            var componentSpecificationParameters = new ComponentSpecificationParameters[numberOfScanImageComponents];


                for (var i = 0; i <  numberOfScanImageComponents; i++)
                {
                    var csj = stream.ReadByteActual();
                    var (tdj, taj) = stream.ReadNibblePair();

                componentSpecificationParameters[i] = new ComponentSpecificationParameters(csj, tdj, taj);
            }

            var startOfSpectralOrPredictorSelection = stream.ReadByteActual();

            var endOfSpectralSelection = stream.ReadByteActual();

            var (successiveApproximationBitHigh, succesiveApproximationBitLow) = stream.ReadNibblePair();

            // Read entropy-coded segment

            return  new Scan();
        }

        private struct ComponentSpecificationParameters
        {
            public byte ScanComponentSelector;

            public byte DcEntropyCodingTableDestinationSelector;

            public byte AcEntropyCodingTableDestinationSelector;

            public ComponentSpecificationParameters(byte scanComponentSelector, byte dcEntropyCodingTableDestinationSelector, byte acEntropyCodingTableDestinationSelector)
            {
                ScanComponentSelector = scanComponentSelector;
                DcEntropyCodingTableDestinationSelector = dcEntropyCodingTableDestinationSelector;
                AcEntropyCodingTableDestinationSelector = acEntropyCodingTableDestinationSelector;
            }
        }
    }
}
