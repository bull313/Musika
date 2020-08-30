using System.Collections.Generic;
using System.IO;
using System.Linq;

using Musika.TypeNames;

namespace Musika
{
    namespace WAV
    {
        /* Constructs a WAV file from a compiled note sheet */
        class WAVConstructor
        {
            /* PROPERTIES */

            private readonly NoteSheet noteSheet;    /* The compiled note sheet to be converted to WAV   */
            private readonly string filename;        /* The name of the constructed WAV file             */
            private readonly string filepath;        /* Location where the WAV file should be saved      */

            /* / PROPERTIES */


            /* CONSTRUCTOR */

            public WAVConstructor(string filepath, string filename)
            {
                /* Store the file information */
                this.filepath = filepath;
                this.filename = filename;

                /* Deserialize the compiled NoteSheet and store it */
                noteSheet = Serializer.Deserialize(filepath, filename);
            }

            /* / CONSTRUCTOR */


            /* PRIVATE METHODS */

            private void ConstructFreqAndDurationTables(Sheet sheet, out double[][] frequencyTable, out double[] durationTable) /* Takes a Sheet of music and converts it into 2 tables:                    */
            {                                                                                                                   /* A table of chords laid out by frequencies and a table of note durations  */
                /* Local Variables */
                List<List<double>>  frequencyTableList;  /* Table to dynamically add frequenices to a table */
                List<double>        durationTableList;   /* Table to dynamically add durations to a table   */
                /* / Local Variables */

                /* Initialize dynamic tables */
                frequencyTableList  = new List<List<double>>();
                durationTableList   = new List<double>();

                /* Get the list of list of frequencies and maximum durations from the note sheet */
                foreach (NoteSet noteSet in sheet)
                {
                    frequencyTableList.Add(noteSet.Select(note => (double)note.frequency).ToList());
                    durationTableList.Add(noteSet.Select(note => note.length).Max());
                }

                /* Convert dynamic tables to arrays */
                frequencyTable  = frequencyTableList.Select(chord => chord.ToArray()).ToArray();
                durationTable   = durationTableList.ToArray();
            }

            /* / PRIVATE METHODS */


            /* PUBLIC METHODS */

            public void ConstructWAV()  /* Package the music sheet and store it as a WAV file */
            {
                /* Local Variables */
                OffsetFrequencyDurationTableListMap positionFrequencyDurationTable; /* Maps duration offsets (in seconds) to a list of musical sheets that should begin playing at that offset  */
                WAVFile wavFile;                                                    /* WAV file object to convert frequency and duration data to a WAV binary                                   */
                double offset;                                                      /* Buffer for the current offset (in seconds)                                                               */
                int i;                                                              /* Increment variable                                                                                       */
                int numLayers;                                                      /* Number of layers in the music including the sheet                                                        */
                /* Local Variables */

                /* Create frequency and duration tables from the main sheet */
                ConstructFreqAndDurationTables(noteSheet.Sheet, out double[][] frequencyTable, out double[] durationTable);

                /* Create a list for the first offset (where the main sheet will be placed) and add that buffer to the main map */
                positionFrequencyDurationTable = new OffsetFrequencyDurationTableListMap
                {
                    {
                        0.0,
                        new FrequencyDurationTableList
                        {
                            new FrequencyDurationTable(frequencyTable, durationTable)
                        }
                    }
                };

                foreach (KeyValuePair<int, SheetSet> positionSheetSetPair in noteSheet.Layers)
                {
                    /* Convert a note position to an offset by summing the note lengths before it */
                    offset = 0.0;

                    for (i = 0; i < positionSheetSetPair.Key; ++i)
                    {
                        offset += noteSheet.Sheet[i][0].length;
                    }

                    foreach (Sheet layerSheet in positionSheetSetPair.Value)
                    {
                        /* Create frequency and duration tables from this layer */
                        ConstructFreqAndDurationTables(layerSheet, out double[][] layerFrequencyTable, out double[] layerDurationTable);

                        /* If there is already a table at this duration, add this table to the map */
                        if (positionFrequencyDurationTable.ContainsKey(offset))
                        {
                            positionFrequencyDurationTable[offset].Add(new FrequencyDurationTable(layerFrequencyTable, layerDurationTable));
                        }

                        /* If there is no table at this duration, create a new table list and add that to the map */
                        else
                        {
                            positionFrequencyDurationTable.Add
                            (
                                offset,
                                new FrequencyDurationTableList
                                {
                                    new FrequencyDurationTable(layerFrequencyTable, layerDurationTable)
                                }
                            );
                        }
                    }
                }

                /* Compute the number of layers in the song */
                numLayers = 1; /* Count the main sheet first */

                foreach (KeyValuePair<int, SheetSet> posSheetSetPair in noteSheet.Layers)
                {
                    numLayers += posSheetSetPair.Value.Count;
                }

                /* Write WAV file */
                wavFile = new WAVFile(numLayers, positionFrequencyDurationTable, filepath, Path.ChangeExtension(filename, WAVFile.WAV_FILE_EXT));
                wavFile.WriteWAVFile();
            }

            public bool NoteSheetReceived() /* Check if the notesheet was successfully stored from the serialized Musika file */
            {
                return noteSheet != null;
            }

            /* / PUBLIC METHODS */
        }
    }
}
