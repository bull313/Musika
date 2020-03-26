using compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public void ConstructWAV()
        {
            /* Construct frequency and duration tables from note sheet */
            Dictionary<double, List<KeyValuePair<double[][], double[]>>> positionFrequencyDurationTable = new Dictionary<double, List<KeyValuePair<double[][], double[]>>>();

            ConstructFreqAndDurationTables(compiledSheet.Sheet, out double[][] frequencyTable, out double[] durationTable);

            List<KeyValuePair<double[][], double[]>> position0FrequencyDurationTable = new List<KeyValuePair<double[][], double[]>>();

            position0FrequencyDurationTable.Add(new KeyValuePair<double[][], double[]>(frequencyTable, durationTable));

            positionFrequencyDurationTable.Add(0.0, position0FrequencyDurationTable);

            foreach (KeyValuePair<int, SheetSet> positionSheetSetPair in compiledSheet.Layers)
            {
                /* Convert a note position to an offset */
                double offset = 0.0;

                for (int i = 0; i < positionSheetSetPair.Key; ++i)
                {
                    offset += compiledSheet.Sheet[i][0].length;
                }

                /* Create Frequency and Duration tables for the layer and add them to the dictionary */
                foreach (Sheet layerSheet in positionSheetSetPair.Value)
                {
                    ConstructFreqAndDurationTables(layerSheet, out double[][] layerFrequencyTable, out double[] layerDurationTable);

                    if (positionFrequencyDurationTable.ContainsKey(offset))
                    {
                        positionFrequencyDurationTable[offset].Add(new KeyValuePair<double[][], double[]>(layerFrequencyTable, layerDurationTable));
                    }
                    else
                    {
                        List<KeyValuePair<double[][], double[]>> positionXFrequencyDurationTable = new List<KeyValuePair<double[][], double[]>>();
                        positionXFrequencyDurationTable.Add(new KeyValuePair<double[][], double[]>(layerFrequencyTable, layerDurationTable));
                        positionFrequencyDurationTable.Add(offset, positionXFrequencyDurationTable);
                    }
                }
            }

            /* Write WAV file */
            WAVFile wavFile = new WAVFile(compiledSheet.Layers.Count + 1, positionFrequencyDurationTable, filepath, Path.ChangeExtension(filename, ".wav"));
            wavFile.WriteWAVFile();
        }

        private void ConstructFreqAndDurationTables(Sheet sheet, out double[][] frequencyTable, out double[] durationTable)
        {
            List<List<double>> frequencyTableList = new List<List<double>>();
            List<double> durationTableList = new List<double>();

            foreach (NoteSet noteSet in sheet)
            {
                List<double> frequencyList = new List<double>();
                double maxDuration = 0.0;

                foreach (Note note in noteSet)
                {
                    frequencyList.Add(note.frequency);

                    if (note.length > maxDuration)
                    {
                        maxDuration = note.length;
                    }
                }

                frequencyTableList.Add(frequencyList);
                durationTableList.Add(maxDuration);
            }

            frequencyTable = frequencyTableList.Select(chord => chord.ToArray()).ToArray();
            durationTable = durationTableList.ToArray();
        }
    }

    class WAVFile
    {
        private int numLayers;
        private string wavFilePath, wavFilename;
        private WAVDataChunk data;
        Dictionary<double, List<KeyValuePair<double[][], double[]>>> positionFrequencyDurationTable;

        public WAVFile(int numLayers, Dictionary<double, List<KeyValuePair<double[][], double[]>>> positionFrequencyDurationTable, string wavFilePath, string wavFilename)
        {
            this.numLayers = numLayers;
            this.positionFrequencyDurationTable = positionFrequencyDurationTable;
            this.wavFilePath = wavFilePath;
            this.wavFilename = wavFilename;
            data = new WAVDataChunk();
        }

        private int GetNumberOfSamples(double[] durationTable)
        {
            double audioDuration = 0.0;

            foreach (double duration in durationTable)
            {
                audioDuration += duration;
            }

            return (int) (WAVDataChunk.SAMPLES_PER_SEC_PER_CHANNEL * audioDuration);
        }

        public void WriteWAVFile()
        {
            /* Local Variables */
            uint dwFileLength;
            BinaryWriter binaryWriter;
            FileStream fileStream;
            /* / Local Variables */

            /* Generate audio samples from the Notesheet */
            GenerateSamples();

            /* Create a file and binary writer to write to it */
            fileStream = new FileStream(Path.Combine(wavFilePath, wavFilename), FileMode.Create);
            binaryWriter = new BinaryWriter(fileStream);

            /* Write WAV Header */
            binaryWriter.Write(WAVHeader.S_GROUP_ID);
            binaryWriter.Write(0); /* Temporarily set to 0: will be updated when everything else is written */
            binaryWriter.Write(WAVHeader.S_RIFF_TYPE);

            /* Write WAV Format Chunk */
            binaryWriter.Write(WAVFormatChunk.S_GROUP_ID);
            binaryWriter.Write(WAVFormatChunk.DW_CHUNK_SIZE);
            binaryWriter.Write(WAVFormatChunk.W_FORMAT_TAG);
            binaryWriter.Write(WAVFormatChunk.W_CHANNELS);
            binaryWriter.Write(WAVFormatChunk.DW_SAMPLES_PER_SEC);
            binaryWriter.Write(WAVFormatChunk.DW_AVG_BYTES_PER_SEC);
            binaryWriter.Write(WAVFormatChunk.W_BLOCK_ALIGN);
            binaryWriter.Write(WAVFormatChunk.W_BITS_PER_SAMPLE);

            /* Write WAV Data Chunk */
            binaryWriter.Write(WAVDataChunk.S_GROUP_ID);
            binaryWriter.Write(data.dwChunkSize);

            foreach (short dataPoint in data.sampleData)
            {
                binaryWriter.Write(dataPoint);
            }

            /* Update the file size section of the WAV header */
            binaryWriter.Seek(WAVHeader.S_GROUP_ID.Length, SeekOrigin.Begin);
            dwFileLength = (uint)binaryWriter.BaseStream.Length - 8;
            binaryWriter.Write(dwFileLength);

            /* Close streams */
            binaryWriter.Close();
            fileStream.Close();
        }

        private void GenerateSamples()
        {
            /* Find the maximum number of notes that can possibly play simultaneously at the same time */
            int maxChordLength = 0;

            foreach (KeyValuePair<double, List<KeyValuePair<double[][], double[]>>> positionFreqDurTablePair in positionFrequencyDurationTable)
            {
                foreach (KeyValuePair<double[][], double[]> frequencyDurationTable in positionFreqDurTablePair.Value)
                {
                    double[][] frequencyTable = frequencyDurationTable.Key;

                    foreach (double[] chord in frequencyTable)
                    {
                        if (chord.Length > maxChordLength)
                        {
                            maxChordLength = chord.Length;
                        }
                    }
                }
            }

            int maxNumFrequencies = maxChordLength * numLayers;

            /* Convert frequency and duration tables into data tables */
            Dictionary<double, List<short[]>> positionDataListTable = new Dictionary<double, List<short[]>>();

            foreach (KeyValuePair<double, List<KeyValuePair<double[][], double[]>>> positionFreqDurTablePair in positionFrequencyDurationTable)
            {
                foreach (KeyValuePair<double[][], double[]> frequencyDurationTable in positionFreqDurTablePair.Value)
                {
                    double[][] frequencyTable = frequencyDurationTable.Key;
                    double[] durationTable = frequencyDurationTable.Value;
                    short[] sampleData = new short[GetNumberOfSamples(durationTable)];

                    GetSampleData(maxNumFrequencies, frequencyTable, durationTable, sampleData);

                    if (positionDataListTable.ContainsKey(positionFreqDurTablePair.Key))
                    {
                        positionDataListTable[positionFreqDurTablePair.Key].Add(sampleData);
                    }
                    else
                    {
                        List<short[]> sampleDataList = new List<short[]>();
                        sampleDataList.Add(sampleData);
                        positionDataListTable.Add(positionFreqDurTablePair.Key, sampleDataList);
                    }
                }
            }

            /* Combine data tabes that are at the same offest */
            Dictionary<double, short[]> positionDataTable = new Dictionary<double, short[]>();

            foreach (KeyValuePair<double, List<short[]>> positionDataListPair in positionDataListTable)
            {
                short[] combinedSampleData = CombineSampleDataArrays(positionDataListPair.Value);
                positionDataTable.Add(positionDataListPair.Key, combinedSampleData);
            }

            /* Compute the length of the final sample data array */
            int finalSampleDataLength = 0;

            foreach (KeyValuePair<double, short[]> positionDataPair in positionDataTable)
            {
                int sampleDataLength = GetNumberOfSamples(new double[1] { positionDataPair.Key }) + positionDataPair.Value.Length;

                if (finalSampleDataLength < sampleDataLength)
                {
                    finalSampleDataLength = sampleDataLength;
                }
            }

            /* Build the final sample data array */
            data.sampleData = new short[finalSampleDataLength];
            data.dwChunkSize = (uint) finalSampleDataLength * ( WAVFormatChunk.W_BITS_PER_SAMPLE / 8 );

            foreach (KeyValuePair<double, short[]> positionDataPair in positionDataTable)
            {
                int offsetIndex = GetNumberOfSamples(new double[1] { positionDataPair.Key });

                for (int i = 0; i < positionDataPair.Value.Length; ++i)
                {
                    data.sampleData[i + offsetIndex] += positionDataPair.Value[i];
                }
            }
        }

        private void GetSampleData(int maxChordLength, double[][] frequencyTable, double[] durationTable, short[] sampleData)
        {
            /* Compute angular frequencies from given frequencies */
            double angularFrequencyCoefficient = (2 * Math.PI) / WAVDataChunk.SAMPLES_PER_SEC_PER_CHANNEL;
            double[,] angularFrequencyTable = new double[frequencyTable.Length, maxChordLength];

            for (int i = 0; i < frequencyTable.Length; ++i)
            {
                for (int j = 0; j < frequencyTable[i].Length; ++j)
                {
                    angularFrequencyTable[i, j] = angularFrequencyCoefficient * frequencyTable[i][j];
                }
            }

            /* Compute max amplitude based on the number of bits and number of notes in largest chord */
            double amplitude = WAVDataChunk.MAX_AMPLITUDE / maxChordLength;

            /* Generate and store samples */
            double elapsedTime = 0.0;

            for (int chordIndex = 0; chordIndex < durationTable.Length; ++chordIndex)
            {
                for (
                    int timeIndex = (int) ( elapsedTime * WAVDataChunk.SAMPLES_PER_SEC_PER_CHANNEL ); 
                    timeIndex < (int) ( (elapsedTime + durationTable[chordIndex]) * WAVDataChunk.SAMPLES_PER_SEC_PER_CHANNEL );
                    ++timeIndex
                )
                {
                    for (int angFreqIdx = 0; angFreqIdx < angularFrequencyTable.GetLength(1); ++angFreqIdx)
                    {
                        sampleData[timeIndex] += (short)
                        (
                            amplitude * Math.Sin(angularFrequencyTable[chordIndex, angFreqIdx] * timeIndex)
                        );
                    }
                }

                elapsedTime += durationTable[chordIndex];
            }
        }

        private short[] CombineSampleDataArrays(List<short[]> sampleDataList)
        {
            int maxNumSamples = 0;

            /* Compute the length of the new list by finding the list with maximum length */
            foreach (short[] sampleData in sampleDataList)
            {
                if (maxNumSamples < sampleData.Length)
                {
                    maxNumSamples = sampleData.Length;
                }
            }

            short[] combinedSampleData = new short[maxNumSamples];

            /* Add each sample data value to the combined array */
            foreach (short[] sampleData in sampleDataList)
            {
                for (int i = 0; i < sampleData.Length; ++i)
                {
                    combinedSampleData[i] += sampleData[i];
                }
            }

            return combinedSampleData;
        }
    }

    static class WAVHeader
    {
        public static readonly char[] S_GROUP_ID = "RIFF".ToCharArray();
        public static readonly char[] S_RIFF_TYPE = "WAVE".ToCharArray();
    }

    static class WAVFormatChunk
    {
        public const uint DW_AVG_BYTES_PER_SEC = DW_SAMPLES_PER_SEC * W_BLOCK_ALIGN;
        public const uint DW_CHUNK_SIZE = 16;
        public const uint DW_SAMPLES_PER_SEC = 44100;

        public const ushort W_BITS_PER_SAMPLE = 16;
        public const ushort W_BLOCK_ALIGN = (W_BITS_PER_SAMPLE / 8) * W_CHANNELS;
        public const ushort W_FORMAT_TAG = 1;
        public const ushort W_CHANNELS = 1;

        public static readonly char[] S_GROUP_ID = "fmt ".ToCharArray();
    }

    class WAVDataChunk
    {
        public const uint MAX_AMPLITUDE = (1 << 15) - 8;
        public const uint SAMPLES_PER_SEC_PER_CHANNEL = WAVFormatChunk.DW_SAMPLES_PER_SEC * WAVFormatChunk.W_CHANNELS;

        public static readonly char[] S_GROUP_ID = "data".ToCharArray();

        public uint dwChunkSize;
        public short[] sampleData;
    }
}