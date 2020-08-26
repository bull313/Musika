namespace Musika
{
    /* Holds the program as a raw string and allows other classes to access it char by char */
    public partial class InputBuffer
    {
        /*
        *  ---------------- PROPERTIES ----------------
        */

        public string ProgramText { get; private set; } /* Program code as a string of characters */
        public int Position { get; private set; }

        /*
        *  ---------------- / PROPERTIES ----------------
        */

        /*
        *  ---------------- CONSTRUCTOR ----------------
        */

        public InputBuffer(string programText)
        {
            /* This replaces remove newline character differences among OSs with one universal \n */
            ProgramText = programText + '\0';
            Position = -1;
        }

        /*
        *  ---------------- / CONSTRUCTOR ----------------
        */

        /*
        *  ---------------- PUBLIC METHODS ----------------
        */

        public char GetChar() /* Get the frontmost input character from the string if there is one */
        {
            /* Return a null terminator to allow other objects to know there is no more input */
            if (ProgramText.Length == 0)
                return '\0';

            /* Return the first character in the input buffer and remove it from the buffer */
            char retVal = ProgramText[0];

            ProgramText = ProgramText.Substring(1, ProgramText.Length - 1);
            ++Position;

            return retVal;
        }

        public void PutChar(char c) /* Place the given character at the beginning of the program string */
        {
            ProgramText = c + ProgramText;
            --Position;
        }

        /*
        *  ---------------- / PUBLIC METHODS ----------------
        */
    }
}
