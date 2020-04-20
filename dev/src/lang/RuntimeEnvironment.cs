using compiler;
using System.IO;
using System.Media;

namespace runtimeenvironment
{
    /* Stores and runs (plays) compiled music */
    partial class RuntimeEnvironment
    {
        /* PROPERTIES */
        private readonly string filepath;
        private readonly string filename;
        /* / PROPERTIES */


        /* CONSTRUCTOR */
        public RuntimeEnvironment(string filepath, string filename)
        {
            this.filepath = filepath;
            this.filename = Path.ChangeExtension(filename, WAVFile.WAV_FILE_EXT);
        }
        /* / CONSTRUCTOR */


        /* PUBLIC METHODS */
        public void PlayWAVFile() /* Play the WAV file as audible sound */
        {
            /* Local Variables */
            SoundPlayer soundPlayer; /* Media player */
            /* / Local Variables */

            soundPlayer = new SoundPlayer
            {
                SoundLocation = Path.Combine(filepath, filename)
            };

            soundPlayer.PlaySync(); /* Play the sound synched with the thread of execution */
        }
        /* / PUBLIC METHODS */
    }
}
