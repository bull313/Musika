namespace Musika
{
    /* Holds the program as a raw string and allows other classes to access it char by char */
    public partial class InputBuffer
    {
        /*
        *  ---------------- CONSTANTS ----------------
        */

        public const char CARRIAGE_RETURN   = '\r';
        public const char NEWLINE           = '\n';
        public const char NULL_TERMINATOR   = '\0';

        /*
        *  ---------------- / CONTSTANTS ----------------
        */

        /*
        *  ---------------- PROPERTIES ----------------
        */

        public string ProgramText { get; private set; } /* Program code as a string of characters */
        public int Position { get; private set; }
        public int LineNumber { get; private set; }

        /*
        *  ---------------- / PROPERTIES ----------------
        */

        /*
        *  ---------------- CONSTRUCTOR ----------------
        */

        public InputBuffer(string programText)
        {
            /* This replaces remove newline character differences among OSs with one universal \n */
            ProgramText = programText + NULL_TERMINATOR;
            Position    = -1;
            LineNumber  = 1;
        }

        /*
        *  ---------------- / CONSTRUCTOR ----------------
        */

        /*
        *  ---------------- PUBLIC METHODS ----------------
        */

        public char GetChar() /* Get the frontmost input character from the string if there is one */
        {
            /* Local Variables */
            char retVal;                /* Character to send out                    */
            /* / Local Variables */

            /* Return a null terminator to allow other objects to know there is no more input */
            if (ProgramText.Length == 0)
                return NULL_TERMINATOR;

            /* Return the first character in the input buffer and remove it from the buffer */
            retVal = ProgramText[0];

            ProgramText = ProgramText.Substring(1, ProgramText.Length - 1);
            ++Position;

            /* Update the line number IF: the current character is a newline*/

            if (retVal == NEWLINE)
            {
                ++LineNumber;
            }

            return retVal;
        }

        public void PutChar(char c) /* Place the given character at the beginning of the program string */
        {
            ProgramText = c + ProgramText;
            --Position;

            /* Decrease the line number if the character is a newline */
            if (c == NEWLINE)
            {
                --LineNumber;
            }
        }

        /*
        *  ---------------- / PUBLIC METHODS ----------------
        */
    }
}
