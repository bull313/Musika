using System;
using System.Collections.Generic;

namespace Musika
{
    namespace TypeNames
    {
        /* Custom types and substitute type names */
        [Serializable]
        class PositionSheetPair /* Replacement for KeyValuePair<int, Sheet> */
        {
            public int Key;
            public Sheet Value;

            public PositionSheetPair(int pos, Sheet s)
            {
                Key = pos;
                Value = s;
            }
        }

        class FrequencyDurationTable /* Replacement for KeyValuePair<double[][], double[]> */
        {
            public double[][] FrequencyTable;
            public double[] DurationTable;

            public FrequencyDurationTable(double[][] freqTable, double[] durTable)
            {
                FrequencyTable = freqTable;
                DurationTable = durTable;
            }
        }

        [Serializable]
        class NoteSet : List<Note> { }
        [Serializable]
        class Sheet : List<NoteSet> { }
        [Serializable]
        class SheetSet : List<Sheet> { }
        [Serializable]
        class PositionSheetMap : List<PositionSheetPair> { }
        class FrequencyDurationTableList : List<FrequencyDurationTable> { }
        class OffsetFrequencyDurationTableListMap : Dictionary<double, FrequencyDurationTableList> { }
        class OffsetSampleDatasetListMap : Dictionary<double, List<int[]>> { }
        class OffsetSampleDatasetMap : Dictionary<double, int[]> { }
    }
}
