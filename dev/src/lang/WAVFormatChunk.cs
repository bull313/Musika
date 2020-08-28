namespace Musika
{
    namespace WAV
    {
        /* Contains contants for the WAV binary format chunk */
        static class WAVFormatChunk
        {
            public const uint DW_AVG_BYTES_PER_SEC      = DW_SAMPLES_PER_SEC * W_BLOCK_ALIGN;
            public const uint DW_CHUNK_SIZE             = 16;
            public const uint DW_SAMPLES_PER_SEC        = 44100;

            public const ushort W_BITS_PER_SAMPLE       = 16;
            public const ushort W_BLOCK_ALIGN           = (W_BITS_PER_SAMPLE / 8) * W_CHANNELS;
            public const ushort W_FORMAT_TAG            = 1;
            public const ushort W_CHANNELS              = 1;

            public static readonly char[] S_GROUP_ID    = "fmt ".ToCharArray();
        }
    }
}
