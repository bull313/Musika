using System;

namespace Musika
{
    /* Represents a time signature (a number for beats per measure and number for base beat rhythm) */
    [Serializable]
    internal struct TimeSignature
    {
        internal int baseNote;
        internal float beatsPerMeasure;
    }
}
