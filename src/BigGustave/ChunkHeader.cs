namespace BigGustave
{
    using System;

    public readonly struct ChunkHeader
    {
        /// <summary>
        /// The position/start of the chunk header within the stream.
        /// </summary>
        public long Position { get; }

        /// <summary>
        /// The length of the chunk in bytes.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// The name of the chunk, uppercase first letter means the chunk is critical (vs. ancillary).
        /// </summary>
        public string Name { get; }

        public bool IsCritical => char.IsUpper(Name[0]);
        public bool IsPublic => char.IsUpper(Name[1]);
        public bool IsSafeToCopy => char.IsUpper(Name[3]);

        public ChunkHeader(long position, int length, string name)
        {
            if (length < 0)
            {
                throw new ArgumentException($"Length less than zero ({length}) encountered when reading chunk at position {position}.");
            }

            Position = position;
            Length = length;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name} at {Position} (length: {Length}).";
        }
    }
}