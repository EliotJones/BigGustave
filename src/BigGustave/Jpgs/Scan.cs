using System.Collections.Generic;

namespace BigGustave.Jpgs
{
    using System.IO;

    internal class Scan
    {
        private readonly List<int> restartIndices;
        
        public IReadOnlyList<byte> Data { get; }

        public ComponentSpecificationParameters[] Components { get; }

        public (byte start, byte end) SpectralPredictionSelection { get; }

        public (byte high, byte low) SuccessiveApproximationBits { get; }

        public Scan(ComponentSpecificationParameters[] components, 
            (byte start, byte end) spectralPredictionSelection, 
            (byte high, byte low) successiveApproximationBits, 
            List<byte> data, 
            List<int> restartIndices)
        {
            Components = components;
            SpectralPredictionSelection = spectralPredictionSelection;
            SuccessiveApproximationBits = successiveApproximationBits;
            Data = data;
            this.restartIndices = restartIndices;
        }

        public static Scan ReadFromMarker(Stream stream, bool strictMode)
        {
            // ReSharper disable once UnusedVariable
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

            var approximationBits = stream.ReadNibblePair();

            // Read entropy-coded segment
            // In new C# we could potentially use Spans here, depending on lifetime rules...
            var data = new List<byte>();

            var restartIndices = new List<int>();

            byte lastByte = 0x00;
            while (true)
            {
                var bi = stream.ReadByte();

                if (bi < 0)
                {
                    break;
                }

                var b = (byte) bi;

                if (lastByte == 0xFF)
                {
                    if (b == 0x00)
                    {
                        // Represents an 0xFF byte
                        data.Add(lastByte);
                        lastByte = 0;
                        continue;
                    }
                    
                    if (b >= 0xD0 && b <= 0xD7)
                    {
                        // Restart markers
                        lastByte = 0x00;
                        restartIndices.Add(data.Count);
                        continue;
                    }
                    
                    if (b == 0xFF)
                    {
                        // Fill bytes
                        lastByte = b;
                        continue;
                    }

                    stream.Seek(-2, SeekOrigin.Current);
                    break;
                }

                if (b == 0xFF)
                {
                    lastByte = b;
                    continue;
                }

                data.Add(b);

                lastByte = b;
            }

            return new Scan(componentSpecificationParameters, 
                (startOfSpectralOrPredictorSelection, endOfSpectralSelection), 
                approximationBits,
                data, restartIndices);
        }

        public readonly struct ComponentSpecificationParameters
        {
            /// <summary>
            /// Scan component selector - selects which of the image components specified in
            /// the frame parameters shall be this component in the scan.
            /// </summary>
            public readonly byte ScanComponentSelector;

            /// <summary>
            /// Specifies 1 of 4 possible DC entropy coding table destinations
            /// from which the entropy table needed for decoding the DC coefficients
            /// of this component is retrieved.
            /// </summary>
            public readonly byte DcEntropyCodingTableDestinationSelector;

            /// <summary>
            /// Specifies 1 of 4 possible AC entropy coding table destinations
            /// from which the entropy table needed for decoding the AC coefficients
            /// of this component is retrieved.
            /// </summary>
            public readonly byte AcEntropyCodingTableDestinationSelector;

            public ComponentSpecificationParameters(byte scanComponentSelector, byte dcEntropyCodingTableDestinationSelector, byte acEntropyCodingTableDestinationSelector)
            {
                ScanComponentSelector = scanComponentSelector;
                DcEntropyCodingTableDestinationSelector = dcEntropyCodingTableDestinationSelector;
                AcEntropyCodingTableDestinationSelector = acEntropyCodingTableDestinationSelector;
            }
        }
    }
}
