using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Musika
{
    /* Serializes and deserializes note sheets to allow storage and access in persistent memory */
    static partial class Serializer
    {
        public static readonly string SERIALIZE_EXT = ".mkc"; /* File extension of serialized NoteSheet objects */

        public static void Serialize(NoteSheet instance, string filepath, string filename) /* Takes a NoteSheet instance, file name, and file path and uses them to */
                                                                                           /* serialize the instance and store it at the path with the name         */
        {
            /* Set up the formatter and stream object s */
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(Path.Combine( filepath, Path.ChangeExtension(filename, SERIALIZE_EXT) ), FileMode.Create, FileAccess.Write, FileShare.None);

            /* Serialize the given obejct */
            formatter.Serialize(stream, instance);
            stream.Close();
        }

        public static NoteSheet Deserialize(string filepath, string filename)   /* Takes a file name and file path, deserializes the file at that path, and returns the */
                                                                                /* NoteSheet instance                                                                   */
        {
            try
            {
                /* Set up the formatter and stream object */
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(Path.Combine(filepath, Path.ChangeExtension(filename, SERIALIZE_EXT)), FileMode.Open, FileAccess.Read, FileShare.Read);

                /* Deserialize the object and return it */
                NoteSheet instance = (NoteSheet)formatter.Deserialize(stream);
                return instance;
            }

            /* Return null if the given file was not found */
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }
}
