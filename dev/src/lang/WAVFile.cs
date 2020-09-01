using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Musika.TypeNames;

namespace Musika
{
    namespace WAV
    {
        class WAVFile
        {
            /* CONSTANTS */

            public const double ANGULAR_FREQUENCY_COEFFICIENT = (2 * Math.PI) / WAVDataChunk.SAMPLES_PER_SEC_PER_CHANNEL;   /* Angular frequency of the sine wave function adjusted for the samples per second per channel                      */
            public const double NOTE_VALUE_CUTOFF_PERCENTAGE = 0.9995;                                                      /* Indicates the percentage of each note to keep; the remaining time will be cut off to allow space between notes   */
            public const double AMPLITUDE_PROPORTION = 0.95;                                                                /* Indicates percentage of amplitude to threshold each of the data samples                                          */
            public const int BITS_IGNORED = 8;                                                                              /* Number of bits ignored when calculating WAV file size                                                            */
            public const string WAV_FILE_EXT = ".wav";                                                                      /* File extension WAV files                                                                                         */

            /* / CONSTANTS */


            /* PROPERTIES */

            private readonly OffsetFrequencyDurationTableListMap positionFrequencyDurationTable;    /* Map containing a list of frequency and duration tables at each offset of the song    */
            private readonly WAVDataChunk data;                                                     /* WAV data chunk contains sample data and some metadata                                */
            private readonly string wavFilename;                                                    /* Name of the WAV file                                                                 */
            private readonly string wavFilePath;                                                    /* Path where the WAV file is located                                                   */

            /* / PROPERTIES */


            /* CONSTRUCTOR */

            public WAVFile(OffsetFrequencyDurationTableListMap positionFrequencyDurationTable, string wavFilePath, string wavFilename)
            {
                data = new WAVDataChunk();

                this.positionFrequencyDurationTable = positionFrequencyDurationTable;
                this.wavFilePath                    = wavFilePath;
                this.wavFilename                    = wavFilename;
            }

            /* / CONSTRUCTOR */


            /* PRIVATE METHODS */

            private double ConvertSecondsToSamples(double duration) /* Number of samples is the duratin in seconds times the sample rate per channel */
            {
                return WAVDataChunk.SAMPLES_PER_SEC_PER_CHANNEL * duration;
            }

            private int GetNumberOfSamples(double[] durationTable) /* Number of samples is the sum total duration times the sample rate per channel */
            {
                return (int)durationTable.Select(duration => ConvertSecondsToSamples(duration)).Sum();
            }

            private int[] GenerateSampleData(double[][] frequencyTable, double[] durationTable) /* Compute sample data values from a given frequency and duration table */
            {
                try
                {
                    /* Local Variables */
                    double amplitude;                   /* Sine wave function amplitude                                 */
                    double elapsedTime;                 /* Time (in seconds) of already processed sample data           */
                    double cutoffTime;                  /* Indicates amount of time for chord to be cut off             */
                    int chordIndex;                     /* Increment variable for chords (collection of frequencies)    */
                    int freqIndex;                      /* Increment variable for the current frequency in a chord      */
                    int timeIndex;                      /* Increment variable for the current time/sample               */
                    int[] returnData;                   /* Generated sample data from the frequency and duration tabels */
                    /* / Local Variables */

                    /* Initiailize the return data */
                    returnData = new int[GetNumberOfSamples(durationTable)];

                    /* Compute max amplitude based on the number of bits and number of notes in largest chord */
                    amplitude = WAVDataChunk.MAX_AMPLITUDE;

                    /* Generate and store samples */
                    elapsedTime = 0.0;

                    for (chordIndex = 0; chordIndex < durationTable.Length; ++chordIndex)
                    {
                        cutoffTime = ((int)ConvertSecondsToSamples(elapsedTime + durationTable[chordIndex])) * NOTE_VALUE_CUTOFF_PERCENTAGE;

                        for (
                            timeIndex = (int)ConvertSecondsToSamples(elapsedTime);
                            timeIndex < (int)ConvertSecondsToSamples(elapsedTime + durationTable[chordIndex]);
                            ++timeIndex
                        )
                        {
                            for (freqIndex = 0; freqIndex < frequencyTable[chordIndex].Length; ++freqIndex)
                            {
                                if (timeIndex <= cutoffTime)
                                {
                                    returnData[timeIndex] += (int)
                                    (
                                        amplitude * Math.Sin(ANGULAR_FREQUENCY_COEFFICIENT * frequencyTable[chordIndex][freqIndex] * timeIndex)
                                    );
                                }
                                else
                                {
                                    /* Cutoff time reached - fill remaining data with space */
                                    returnData[timeIndex] += 0;
                                }
                            }
                        }

                        elapsedTime += durationTable[chordIndex];
                    }

                    /* Return result */
                    return returnData;
                }
                catch (OutOfMemoryException)
                {
                    throw new WAVConstructionError(WAVConstructionError.MEMORY_OVERFLOW_ERROR);
                }
            }

            private int[] CombineSampleDataArrays(List<int[]> sampleDatasetList) /* Combine a list of sample data arrays into one big data array */
            {
                /* Local Variables */
                int i;                          /* Increment variable  */
                int[] combinedSampleData;     /* Combined data array */
                /* / Local Variables */

                /* Create a combined array with a size equal to the longest length sub-data list    */
                /* Add each sample data value to the combined array                                 */
                combinedSampleData = new int[sampleDatasetList.Select(sampleDataSet => sampleDataSet.Length).ToList().Max()];

                foreach (int[] sampleDataset in sampleDatasetList)
                {
                    for (i = 0; i < sampleDataset.Length; ++i)
                    {
                        combinedSampleData[i] += sampleDataset[i];
                    }
                }

                return combinedSampleData;
            }

            private bool CheckValuesExist() /* Check if there are any frequencies/durations present */
            {
                /* Local Variables */
                bool valueExists; /* Return value - true if there exists at least one frequency and duration that is non-zero */
                /* / Local Variables */

                valueExists = false;

                /* Check all frequency duration tables for a given offset position */
                foreach (KeyValuePair<double, FrequencyDurationTableList> positionFreqDurTablePair in positionFrequencyDurationTable)
                {
                    /* Check each frequency and duration table that are present at this offset position  */
                    foreach (FrequencyDurationTable freqDurTable in positionFreqDurTablePair.Value)
                    {
                        /* Check for nonzero length in both tables */
                        if (freqDurTable.FrequencyTable.Length > 0 && freqDurTable.DurationTable.Length > 0)
                        {
                            valueExists = true;
                            break;
                        }
                    }
                }

                /* Return Result */
                return valueExists;
            }

            private void CreateSampleData() /* Create a data array from the position frequency duration table map */
            {
                /* Local Variables */
                Dictionary<double, int> offsetDict;                 /* Maps a time in seconds to the number of samples that makes it up                     */
                OffsetSampleDatasetListMap offsetSampleDataListMap; /* Maps start times in seconds to lists of sample data sets that start at those offsets */
                double amplitudeReducer;                            /* Coefficient post-processing to lower the amplitude down to the maximum               */
                int finalSampleDataLength;                          /* Size of the final sample data array                                                  */
                int timeIndex;                                      /* Increment variable for the current sample in a sample data array                     */
                int offsetIndex;                                    /* Starting sample index equivalent to waiting an offset in seconds                     */
                int sampleDataLength;                               /* Length of sample data of a sub-sampled data array                                    */
                int[] sampleData;                                   /* Buffer for sample data generated by given frequency and duration table               */
                /* / Local Variables */

                /* Cancel Construction if there are no frequencies to construct */
                if (CheckValuesExist())
                {
                    /* Convert frequency and duration tables into data tables */
                    offsetSampleDataListMap = new OffsetSampleDatasetListMap();

                    foreach (KeyValuePair<double, FrequencyDurationTableList> positionFreqDurTablePair in positionFrequencyDurationTable)
                    {
                        foreach (FrequencyDurationTable freqDurTable in positionFreqDurTablePair.Value)
                        {
                            sampleData = GenerateSampleData(freqDurTable.FrequencyTable, freqDurTable.DurationTable);

                            if (offsetSampleDataListMap.ContainsKey(positionFreqDurTablePair.Key))
                            {
                                offsetSampleDataListMap[positionFreqDurTablePair.Key].Add(sampleData);
                            }
                            else
                            {
                                offsetSampleDataListMap.Add(positionFreqDurTablePair.Key, new List<int[]>() { sampleData });
                            }
                        }
                    }

                    /* Combine data tabes that are at the same offest */
                    OffsetSampleDatasetMap positionDataTable = new OffsetSampleDatasetMap();

                    foreach (KeyValuePair<double, List<int[]>> positionDataListPair in offsetSampleDataListMap)
                    {
                        positionDataTable.Add(positionDataListPair.Key, CombineSampleDataArrays(positionDataListPair.Value));
                    }

                    /* Compute the length of the final sample data array */
                    finalSampleDataLength = 0;
                    offsetDict = new Dictionary<double, int>();

                    foreach (KeyValuePair<double, int[]> positionDataPair in positionDataTable)
                    {
                        offsetIndex = (int)ConvertSecondsToSamples(positionDataPair.Key);
                        offsetDict.Add(positionDataPair.Key, offsetIndex);

                        sampleDataLength = offsetIndex + positionDataPair.Value.Length;

                        if (finalSampleDataLength < sampleDataLength)
                        {
                            finalSampleDataLength = sampleDataLength;
                        }
                    }

                    /* Build the final sample data array */
                    sampleData = new int[finalSampleDataLength];
                    data.sampleData = new short[finalSampleDataLength];
                    data.dwChunkSize = (uint)finalSampleDataLength * (WAVFormatChunk.W_BITS_PER_SAMPLE / 8);

                    foreach (KeyValuePair<double, int[]> positionDataPair in positionDataTable)
                    {
                        offsetIndex = offsetDict[positionDataPair.Key];

                        for (timeIndex = 0; timeIndex < positionDataPair.Value.Length; ++timeIndex)
                        {
                            sampleData[timeIndex + offsetIndex] += positionDataPair.Value[timeIndex];
                        }
                    }

                    /* Reduce sample data values to ensure the maximum is the max amplitude */
                    amplitudeReducer = ( WAVDataChunk.MAX_AMPLITUDE * AMPLITUDE_PROPORTION ) / ((double)sampleData.Max());

                    for (timeIndex = 0; timeIndex < sampleData.Length; ++timeIndex)
                    {
                        sampleData[timeIndex] = (int)(sampleData[timeIndex] * amplitudeReducer);
                    }

                    /* Populate the actual sample data array */
                    for (timeIndex = 0; timeIndex < data.sampleData.Length; ++timeIndex)
                    {
                        data.sampleData[timeIndex] = (short)sampleData[timeIndex];
                    }
                }

                /* No frequencies were found in the music => cannot construct WAV file */
                else
                {
                    throw new WAVConstructionError(WAVConstructionError.NO_FREQUENCIES_ERROR);
                }
            }

            /* / PRIVATE METHODS */

            /* PUBLIC METHODS */

            public void WriteWAVFile() /* Construct WAV binary file from WAV data */
            {
                /* Local Variables */
                BinaryWriter binaryWriter;  /* Writes the binary data to a file stream          */
                FileStream fileStream;      /* Stream to store the final WAV file               */
                uint dwFileLength;          /* Size of the binary file minus the header data    */
                /* / Local Variables */

                /* Generate audio samples from the Notesheet */
                CreateSampleData();

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
                dwFileLength = (uint)binaryWriter.BaseStream.Length - BITS_IGNORED;
                binaryWriter.Write(dwFileLength);

                /* Close streams */
                binaryWriter.Close();
                fileStream.Close();
            }

            /* / PUBLIC METHODS */
        }
    }
}
