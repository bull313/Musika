using System.IO;
using System.Media;

namespace Musika
{
    /* Stores and runs (plays) compiled music */
    public partial class SongPlayer
    {
        /* PROPERTIES */
        private readonly SoundPlayer soundPlayer;   /* WAV media player             */
        private readonly string filepath;           /* File path of the WAV file    */
        private readonly string filename;           /* File name of the WAV file    */
        /* / PROPERTIES */


        /* CONSTRUCTOR */
        public SongPlayer(string filepath, string filename)
        {
            this.filepath = filepath;
            this.filename = Path.ChangeExtension(filename, WAVFile.WAV_FILE_EXT);

            soundPlayer = new SoundPlayer
            {
                SoundLocation = Path.Combine(this.filepath, this.filename)
            };
        }
        /* / CONSTRUCTOR */


        /* PUBLIC METHODS */
        public void PlayWAVFile() /* Play the WAV file as audible sound */
        {
            soundPlayer.Play(); /* Play the sound asynchronous with the thread of execution */
        }

        public void StopWAVFile() /* Stop a WAV file that is already playing */
        {
            soundPlayer.Stop();
        }
        /* / PUBLIC METHODS */
    }
}
