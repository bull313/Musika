namespace Musika
{
    namespace WAV
    {
        /* Contains variables and constants for the WAV binary data chunk */
        class WAVDataChunk
        {
            public const uint MAX_AMPLITUDE = (1 << 15) - 8;
            public const uint SAMPLES_PER_SEC_PER_CHANNEL = WAVFormatChunk.DW_SAMPLES_PER_SEC * WAVFormatChunk.W_CHANNELS;

            public static readonly char[] S_GROUP_ID = "data".ToCharArray();

            public uint dwChunkSize;
            public short[] sampleData;
        }
    }
}
