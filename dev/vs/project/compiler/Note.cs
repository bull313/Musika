using System;

namespace Musika
{
    /* Represents a musical note with its data */
    [Serializable]
    internal struct Note
    {
        internal string note;
        internal float frequency;
        internal float length;
    }
}
