using compiler;

namespace runtimeenvironment
{
    /* Stores and runs (plays) a compiled Musika Note Sheet */
    partial class RuntimeEnvironment
    {
        private string      filepath, filename;                                             /* File path and name for file to run   */
        private NoteSheet   compiledSheet;                                                  /* NoteSheet instance to play           */

        private bool        noteSheetReceived;                                              /* Was a NoteSheet instance received? */
        public  bool        NoteSheetReceived { get { return noteSheetReceived; } set { } } /* Getters and setters for noteSheetReceived */

        /* Construct a notesheet using the specified file name and path */
        public RuntimeEnvironment(string filepath, string filename)
        {
            /* Store the file information */
            this.filepath = filepath;
            this.filename = filename;

            /* Deserialize the compiled NoteSheet and store it */
            compiledSheet = Serializer.Deserialize(filepath, filename);
            noteSheetReceived = compiledSheet != null;
        }
    }
}