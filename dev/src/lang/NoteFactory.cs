using System;
using System.Collections.Generic;

namespace Musika
{
    /* Converts a note to an appropriate string */
    class NoteFactory
    {
        private static readonly Dictionary<char, string> SharpToXML = new Dictionary<char, string>()
        {
            { 'A', "As" },
            { 'B', "C"  },
            { 'C', "Cs" },
            { 'D', "Ds" },
            { 'E', "F"  },
            { 'F', "Fs" },
            { 'G', "Gs" }
        };

        private static readonly Dictionary<char, string> FlatToXML = new Dictionary<char, string>()
        {
            { 'A', "Gs" },
            { 'B', "As" },
            { 'C', "B"  },
            { 'D', "Cs" },
            { 'E', "Ds" },
            { 'F', "E"  },
            { 'G', "Fs" }
        };

        private static readonly Dictionary<char, string> DoubleSharpToXML = new Dictionary<char, string>()
        {
            { 'A', "B"  },
            { 'B', "Cs" },
            { 'C', "D"  },
            { 'D', "E"  },
            { 'E', "Fs" },
            { 'F', "G"  },
            { 'G', "A"  }
        };

        private static readonly Dictionary<char, string> DoubleFlatToXML = new Dictionary<char, string>()
        {
            { 'A', "G"  },
            { 'B', "A"  },
            { 'C', "Bb" },
            { 'D', "C"  },
            { 'E', "D"  },
            { 'F', "Eb" },
            { 'G', "F"  }
        };

        private static readonly char[] sharpOrder = { 'F', 'C', 'G', 'D', 'A', 'E', 'B' };
        private static readonly char[] flatOrder = { 'B', 'E', 'A', 'D', 'G', 'C', 'F' };

        /* Adjust note by key and format it as a valid element name in the XML file    */
        /* XML element names consist of the note name followed by the octave number    */
        /* The accidental symbols are replased with an "s" for sharp or nothing at all */
        /* For example, a 5th octave B in D minor will be converted to "As5"           */
        public static string GetFormattedNote(string name, int numSharpsOrFlats, int octave)
        {
            /* Local Variables */
            char[] order;
            char finalChar;
            char nameCharCopy;
            string noteName;
            int i;
            bool flatKey;
            /* / Local Variables */

            finalChar = name[name.Length - 1];

            switch (finalChar)
            {
                /* Just remove the $ */
                case '$':
                    noteName = name[0].ToString();
                    break;

                /* Just replace # with s */
                case '#':
                    noteName = SharpToXML[name[0]];
                    break;

                case 'b':
                    /* If the note is a double flat, convert using the double flat table */
                    if (name[name.Length - 2] == 'b')
                        noteName = DoubleFlatToXML[name[0]];

                    /* Otherwise convert using the single flat table */
                    else
                        noteName = FlatToXML[name[0]];

                    break;

                /* Convert using the double sharp table */
                case '*':
                    noteName = DoubleSharpToXML[name[0]];
                    break;

                /* No accidental, so adjust to key signature */
                default:
                    /* We only need the first character in the name string */
                    noteName = name[0].ToString();
                    nameCharCopy = noteName[0]; /* Used for readability */

                    /* A flat key in this compiler is represented by a negative number of sharps */
                    flatKey = numSharpsOrFlats < 0;

                    /* Figure out the order of flats/sharps to use */
                    order = (flatKey) ? flatOrder : sharpOrder;
                    numSharpsOrFlats = Math.Abs(numSharpsOrFlats);

                    /* Iterate until the number of sharps/flats has been reached                                                                                */
                    /* (Example: Bb Major (a 2-flat key) would only check the first 2 values (Bb and Eb) and leave the rest unchecked since they are natural)   */
                    for (i = 0; i < numSharpsOrFlats; ++i)
                    {
                        /* If there is a match, it means that the note needs to be adjusted, so use the appropriate table to adjust it */
                        if (nameCharCopy == order[i])
                        {
                            noteName = (flatKey) ? FlatToXML[nameCharCopy] : SharpToXML[nameCharCopy];
                            break;
                        }
                    }

                    break;
            }

            /* Return the name concatenated with the octave for valid XML frequency table element name */
            return noteName + octave;
        }
    }
}
