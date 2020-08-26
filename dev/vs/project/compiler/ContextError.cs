using System;

namespace Musika
{
    /* Indicates that contextual error has been made */
    public partial class ContextError : Exception
    {
        /*
        *  ---------------- CONSTANTS ----------------
        */
        private const string BASE_STRING                = "CONTEXT ERROR: ";

        public const string CROSS_REFERENCE_ERROR       = "cross-referencing files is not allowed.";
        public const string DUPLICATE_NAME_ERROR        = "cannot have duplicate names for patterns, chords, or accompaniment names!";
        public const string INVALID_ACC_REFERENCE_ERROR = "invalid accompaniment reference!";
        public const string INVALID_FILENAME_ERROR      = "invalid Musika file name in accompaniment!";
        public const string INVALID_PA_REF_ERROR        = "invalid pattern or accompaniment reference name!";
        public const string INVALID_REPEAT_NUM_ERROR    = "you cannot repeat a negative number of times!";
        public const string KEY_ERROR                   = "Invalid key signature specified.";
        public const string LAYER_NO_PATTERN_ERROR      = "a pattern or accompaniment name must be specified in a layer call!";
        public const string NULL_PATTERN_ERROR          = "A callback type case was not covered because the layer pattern was set to null";
        public const string OCTAVE_ERROR                = "Octave cannot be negative!";
        public const string PATTERN_DOT_ERROR           = "pattern cannot be followed by dots!";
        public const string PC_REFERENCE_ERROR          = "Invalid pattern or chord reference!";
        public const string REPEAT_NOTHING_ERROR        = "cannot use shorthand repeat on nothing";
        public const string SELF_REFERENCE_ERROR        = "cannot reference the current file!";
        public const string TIER_2_DICT_ERROR           = "Tier 2 keyword read but content invalid. Check lexical analyzer";
        public const string TIME_ERROR                  = "Make sure to set the beats per measure and the base beat!";
        public const string ZERO_TIME_ERROR             = "time base note and beats per measure cannot be set to zero!";
        /*
        *  ---------------- / CONSTANTS ----------------
        */

        /*
        *  ---------------- CONSTRUCTOR ----------------
        */
        public ContextError(string text) : base(BASE_STRING + text) { }
        /*
        *  ---------------- / CONSTRUCTOR ----------------
        */
    }
}
