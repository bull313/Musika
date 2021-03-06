﻿using System;

namespace Musika
{
    namespace WAV
    {
        /* Represents issues relating to the construction of a WAV file from a note sheet */
        public partial class WAVConstructionError : Exception
        {
            private const string BASE_STRING            = "WAV CONSTRUCTION ERROR: ";

            public const string MEMORY_OVERFLOW_ERROR   = "Unfortunatley, Musika does not have enough memory to process your song. Please condense your song and then try again.";
            public const string NO_FREQUENCIES_ERROR    = "No music is playing, so no audio can be generated";

            public WAVConstructionError(string text) : base($"{BASE_STRING} {text}") { }
        }
    }
}
