namespace Musika
{
    namespace WAV
    {
        /* Contains constants for the WAV binary header chunk */
        static class WAVHeader
        {
            public static readonly char[] S_GROUP_ID = "RIFF".ToCharArray();
            public static readonly char[] S_RIFF_TYPE = "WAVE".ToCharArray();
        }
    }
}
