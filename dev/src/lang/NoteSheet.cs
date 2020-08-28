using System;
using System.Collections.Generic;
using System.Xml;

using Musika.TypeNames;

namespace Musika
{
    /* Intermediate representation of a Musika file. Contains notes laid out in order */
    [Serializable]
    partial class NoteSheet
    {
        /*
        *  ---------------- CONSTANTS ----------------
        */

        private static readonly string NOTE_FREQUENCY_FILE = "../../../../../src/lang/data/note_frequency.xml";    /* Note frequency XML chart address */

        public static readonly Dictionary<TokenType, TimeSignature> TimeSignatureDict = new Dictionary<TokenType, TimeSignature>() /* Common time signatures (with keywords associated */
        {
              { TokenType.COMMON, new TimeSignature { baseNote = 4, beatsPerMeasure = 4 } },
              { TokenType.CUT,    new TimeSignature { baseNote = 2, beatsPerMeasure = 2 } }
        };

        public static readonly Dictionary<string, int> KeyConversion = new Dictionary<string, int>() /* Converts a key signature into the number of sharps or flats it has. */
                                                                                                     /* (positive number: sharps, negative number: flats)                   */
        {
            /* Major Keys */
            { "Cmaj",    0 }, { "Gmaj",    1 }, { "Dmaj",    2 }, { "Amaj",    3 },
            { "Emaj",    4 }, { "Bmaj",    5 }, { "F#maj",   6 }, { "C#maj",   7 },
            { "Fmaj",   -1 }, { "Bbmaj",  -2 }, { "Ebmaj",  -3 }, { "Abmaj",  -4 },

            /* Minor Keys */
            { "Am",      0 }, { "Em",      1 }, { "Bm",      2 }, { "F#m",     3 },
            { "C#m",     4 }, { "Dm",     -1 }, { "Gm",     -2 }, { "Cm",     -3 },
            { "Fm",     -4 }, { "Bbm",    -5 }, { "Ebm",    -6 }, { "Abm",    -7 }
        };

        /*
        *  ---------------- / CONSTANTS ----------------
        */

        /*
        *  ---------------- PROPERTIES ----------------
        */

        public Dictionary<string, PositionSheetMap> RelativeLayerPositions { get; set; }    /* Used to track the relative positions of layer calls in patterns  */

        private string title;                                                               /* Song title */
        public string Title                                                                 /* Can only be set once */
        {
            set { if (title == null) title = value; }
            get { return title; }
        }

        private string author;                                                              /* Composer of the song */
        public string Author                                                                /* Can only be set once */
        {
            set { if (author == null) author = value; }
            get { return author; }
        }

        private string[] coauthors;                                                         /* Any other co-composers if there are any */
        public string[] Coauthors                                                           /* Can only be set once */
        {
            set { if (coauthors == null) coauthors = value; }
            get { return coauthors; }
        }

        public int Key { get; set; }                                                        /* Song's key signature                                               */
        public TimeSignature Time;                                                          /* Song's time signature                                              */
        public float Tempo { get; set; }                                                    /* Song's tempo (seconds per base beat)                               */
        public int Octave { get; set; }                                                     /* Song's main octave                                                 */

        public Sheet Sheet { get; set; }                                                    /* Main music sheet                                                   */
        public Dictionary<string, NoteSheet> Accompaniments { get; set; }                   /* Collection of accompaniment sheets                                 */
        public Dictionary<string, Sheet> Patterns { get; set; }                             /* Collection of local pattern sheets                                 */
        public Dictionary<string, NoteSet> Chords { get; set; }                             /* Collection of local chord sheets                                   */
        public Dictionary<int, SheetSet> Layers { get; set; }                               /* Collection of layered music at specified positions                 */

        /*
        *  ---------------- / PROPERTIES ----------------
        */

        /*
        *  ---------------- CONSTRUCTOR ----------------
        */

        public NoteSheet()
        {
            /* Initialize all instance objects */
            Sheet                   = new Sheet();
            Patterns                = new Dictionary<string, Sheet>();
            Chords                  = new Dictionary<string, NoteSet>();
            Accompaniments          = new Dictionary<string, NoteSheet>();
            Layers                  = new Dictionary<int, SheetSet>();
            RelativeLayerPositions  = new Dictionary<string, PositionSheetMap>();
        }

        /*
        *  ---------------- / CONSTRUCTOR ----------------
        */

        /*
        *  ---------------- PUBLIC METHODS ----------------
        */

        public bool KeySignuatureExists(string key) /* Verifies if a given key signature is defined */
        {
            return KeyConversion.ContainsKey(key);
        }

        public float GetFrequency(string noteName, int octave) /* Takes a note and its octave and returns the frequency of that sound */
        {
            /* Local Variables */
            XmlDocument noteFrequencyChart; /* XML freq chart reference         */
            XmlNodeList chartSearchResults; /* XML freq chart query results     */
            float returnValue;        /* Note frequency (return value)    */
            string name;               /* Formatted note name              */
            /* / Local Variables */

            /* Return 0 frequency for "rest" note */
            if (noteName == "|") returnValue = 0;

            else
            {
                /* Adjust note by key and format it as a valid element name in the XML frequency lookup file */
                name = NoteFactory.GetFormattedNote(noteName, Key, octave);

                /* Load XML frequency lookup file */
                noteFrequencyChart = new XmlDocument();
                noteFrequencyChart.Load(NOTE_FREQUENCY_FILE);

                /* Get XML result */
                chartSearchResults = noteFrequencyChart.GetElementsByTagName(name);

                /* Return an "inaudible" frequency if the octave is too high or low or the returned frequency value otherwise */
                if (chartSearchResults.Count == 0)
                    returnValue = 0;
                else
                    returnValue = float.Parse(chartSearchResults[0].InnerText);
            }

            /* Return frequency */
            return returnValue;
        }

        /*
        *  ---------------- / PUBLIC METHODS ----------------
        */
    }
}
