using System.IO;

using Musika.WAV;

namespace Musika
{
    /* Abstract representation of all compiler modules */
    /* This class should be the only one directly referenced by classes outside this namespace */
    public partial class Compiler
    {
        /*
        *  ---------------- CONSTANTS ----------------
        */
        public const string MUSIKA_FILE_EXT = ".ka"; /* Musika file extension */
        /*
        *  ---------------- / CONSTANTS ----------------
        */


        /*
        *  ---------------- PROPERTIES ----------------
        */
        private readonly string filepath;   /* Location to store code/representations           */
        private readonly string filename;   /* Name of code and representation files            */

        private string code;                /* Source code to compile                           */
        /*
        *  ---------------- / PROPERTIES ----------------
        */


        /*
        *  ---------------- CONSTRUCTOR ----------------
        */
        public Compiler(string filepath, string filename, string code = null) /* Path and name are separate */
        {
            this.filepath = filepath;
            this.filename = Path.ChangeExtension(filename, null);

            GetCode(code);
        }
        /*
        *  ---------------- / CONSTRUCTOR ----------------
        */

        /*
        *  ---------------- PRIVATE METHODS ----------------
        */
        private void GetCode(string code) /* If code is not provided, read from the given file path or file name */
        {
            if (code != null)
            {
                this.code = code;
            }
            else
            {
                this.code = File.ReadAllText(Path.Combine(filepath, Path.ChangeExtension( filename, MUSIKA_FILE_EXT )));
            }
        }
        /*
        *  ---------------- / PRIVATE METHODS ----------------
        */

        /*
        *  ---------------- PUBLIC METHODS ----------------
        */
        public void CompileToNoteSheet() /* Compile the code into a note sheet instance serialzed to the filepath and filename */
        {
            Parser parser = new Parser(code, filename, filepath: filepath);
            NoteSheet noteSheet = parser.ParseScore();
            Serializer.Serialize(noteSheet, filepath, filename);
        }

        public void CompileToWAV() /* Construct a WAV file from the given code and store it in the given filepath and filename */
        {
            /* Local Variables */
            string serializedFileAddress;   /* Filepath + name with the serialized file extension   */
            WAVConstructor wavConstructor;  /* Constructs a WAV file from serialized file           */
            /* / Local Variables */

            /* Check if there is a serialized note sheet file. If not, generate it */
            serializedFileAddress = Path.Combine
            (
                filepath, Path.ChangeExtension(filename, Serializer.SERIALIZE_EXT)
            );

            /* Create a note sheet if one does not already exist */
            if (File.Exists(serializedFileAddress) == false)
            {
                CompileToNoteSheet();
            }
            
            /* Use the WAV constructor to write the corresponding WAV file */
            wavConstructor = new WAVConstructor(filepath, filename);
            wavConstructor.ConstructWAV();
        }
        /*
        *  ---------------- / PUBLIC METHODS ----------------
        */
    }
}
