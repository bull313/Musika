using System;
using System.Collections.Generic;
using System.IO;

namespace compiler
{
    class CompilerTest
    {
        public static int RunUnitTests()
        {
            int tests = 0, testFailures = 0;
            string output = "";
            Console.WriteLine("Compiler Tests:");
            Test[] testList =
            {
                new Token.Test__Token(), new InputBuffer.Test__InputBuffer(), new LexicalAnalyzer.Test__LexicalAnalyzer(),
                new SyntaxError.Test__SyntaxError(), new Parser.Test__Parser(), new NoteSheet.Test__NoteSheet(), new Serializer.Test__Serializer()
            }; /* ADD INSTANCES OF NEW TEST CASES HERE */

            foreach (Test test in testList)
            {
                ++tests;
                output += ("Begin " + test.GetTestName() + " Test:\n").ToUpper();
                test.RunTests();
                output += test.GetResults();
                if (!test.Passes())
                    ++testFailures;
            }

            output += "Overall Test Set:\n".ToUpper() + "Total Tests: " + tests
                    + "\nTotal Failures: " + testFailures + "\nResults: "
                    + ((tests > 0 && testFailures == 0) ? "TEST PASSED" : "TEST FAILED") + "\n";
            System.IO.File.WriteAllText("../../TestResults.txt", output);
            Console.WriteLine(output);

            return testFailures;
        }
    }

    /* ---------------- TESTS ---------------- */

    partial class Serializer
    {
        internal class Test__Serializer : Test
        {
            public Test__Serializer()
            {
                testName = "Serializer";
            }

            private string GetFile(string filename)
            {
                return System.IO.File.ReadAllText("../../../../../test/lang/" + filename); /* Get test file text from the test directory */
            }

            public string GetDirectory()
            {
                return "../../../../../test/lang/";
            }

            public override void RunTests()
            {
                string filename = "note_sheet_test10";
                string filepath = GetDirectory();
                string testProgram = GetFile(filename + ".ka");
                Parser parser = new Parser(testProgram, filepath, filename);

                NoteSheet toSerialize = parser.ParseScore();
                Serializer.Serialize(toSerialize, filepath, filename);

                VerifyEqual(File.Exists(filepath + filename + ".mkc"), true, "Test that the serialized file exists");

                NoteSheet toDeserialize = Serializer.Deserialize(filepath, filename);

                if (toDeserialize != null)
                {
                    VerifyEqual(toSerialize.Title, toDeserialize.Title, "Verify the object has been successfully deserialized and that its data is the same");
                    VerifyEqual(toSerialize.Author, toDeserialize.Author, "Verify the object has been successfully deserialized and that its data is the same");
                    VerifyEqual(toSerialize.key, toDeserialize.key, "Verify the object has been successfully deserialized and that its data is the same");
                    VerifyEqual(toSerialize.time.baseNote, toDeserialize.time.baseNote, "Verify the object has been successfully deserialized and that its data is the same");
                    VerifyEqual(toSerialize.time.beatsPerMeasure, toDeserialize.time.beatsPerMeasure, "Verify the object has been successfully deserialized and that its data is the same");
                    VerifyEqual(toSerialize.tempo, toDeserialize.tempo, "Verify the object has been successfully deserialized and that its data is the same");
                    VerifyEqual(toSerialize.octave, toDeserialize.octave, "Verify the object has been successfully deserialized and that its data is the same");
                }
                else
                {
                    VerifyEqual(true, false, "toDeserialize() returned null");
                }
            }
        }
    }

    partial class NoteSheet
    {
        internal class Test__NoteSheet : Test
        {
            public Test__NoteSheet()
            {
                testName = "Note Sheet";
            }

            private string GetFile(string filename)
            {
                return System.IO.File.ReadAllText("../../../../../test/lang/" + filename); /* Get test file text from the test directory */
            }

            public string GetDirectory()
            {
                return "../../../../../test/lang/";
            }

            private void TestGetFrequency()
            {
                VerifyEqual(new NoteSheet().GetFrequency("A",   4), 440.0f,     "Verify A4 returns correct frequency");
                VerifyEqual(new NoteSheet().GetFrequency("C",   0), 16.35f,     "Verify C0 returns correct frequency");
                VerifyEqual(new NoteSheet().GetFrequency("A$",  2), 110f,       "Verify A2 returns correct frequency");
                VerifyEqual(new NoteSheet().GetFrequency("F#",  3), 185f,       "Verify F#3 returns correct frequency");
                VerifyEqual(new NoteSheet().GetFrequency("Bb",  1), 58.27f,     "Verify Bb1 returns correct frequency");
                VerifyEqual(new NoteSheet().GetFrequency("E*",  6), 1479.98f,   "Verify E*6 returns correct frequency");
                VerifyEqual(new NoteSheet().GetFrequency("Dbb", 5), 523.25f,    "Verify Dbb5 returns correct frequency");
                VerifyEqual(new NoteSheet().GetFrequency("|",   1), 0.0f,       "Verify rest does not produce frequency");
            }

            private void TestFiles()
            {
                bool expectedContextError = false; /* Ensures that a context error is thrown only when expected */

                try
                {
                    /* C Major Scale */
                    string cMajorScaleFile = GetFile("note_sheet_test1.ka");
                    Parser p = new Parser(cMajorScaleFile, GetDirectory(), "note_sheet_test1.ka");
                    NoteSheet n = p.ParseScore();

                    /* Verify Information */
                    VerifyEqual(n.Title, "Test", "Test title was set properly");
                    VerifyEqual(n.Author, "The Tester", "Test author was set properly");
                    VerifyEqual(n.Coauthors, null, "Verify coauthors were not set");
                    VerifyEqual(n.key, 0, "Verify C major scale set as key");
                    VerifyEqual(n.time.baseNote, 4, "Verify quarter note base rhythm");
                    VerifyEqual(n.time.beatsPerMeasure, 4, "Verify bpm set properly");
                    VerifyEqual(n.tempo, 1f, "Verify tempo set properly");
                    VerifyEqual(n.octave, 4, "Verify octave set properly");

                    /* Verify Notes */
                    if (VerifyEqual(n.Sheet.Count, 8, "Verify the number of notes is correct"))
                    {
                        Note[] correctNotes = {
                                                  new Note { note = "C", frequency = 261.63f, length = 4f },
                                                  new Note { note = "D", frequency = 293.66f, length = 3f },
                                                  new Note { note = "E", frequency = 329.63f, length = 2f },
                                                  new Note { note = "F", frequency = 349.23f, length = 1f },
                                                  new Note { note = "G", frequency = 392.00f, length = 2f },
                                                  new Note { note = "A", frequency = 440.00f, length = 3f },
                                                  new Note { note = "B", frequency = 493.88f, length = 4f },
                                                  new Note { note = "C", frequency = 523.25f, length = 5f },
                                              };

                        for (int i = 0; i < correctNotes.Length; ++i)
                        {
                            VerifyEqual(n.Sheet[i][0].note, correctNotes[i].note, "Verify note #" + i + " has the correct note name");
                            VerifyEqual(n.Sheet[i][0].frequency, correctNotes[i].frequency, "Verify note #" + i + " has the correct note frequency");
                            VerifyEqual(n.Sheet[i][0].length, correctNotes[i].length, "Verify note #" + i + " has the correct note length");
                        }
                    }

                    /* More Complex notes and Keys */
                    string test2File = GetFile("note_sheet_test2.ka");
                    p = new Parser(test2File, GetDirectory(), "note_sheet_test2.ka");
                    n = p.ParseScore();

                    string[] expectedCoauthors = { "Jason Ceasarman", "Pablo Pizzoro" };

                    VerifyEqual(n.Title, "More Robust Note Sheet Test", "Test title was set properly");
                    VerifyEqual(n.Author, "Ben", "Test author was set properly");
                    for (int i = 0; i < expectedCoauthors.Length; ++i)
                        VerifyEqual(n.Coauthors[i], expectedCoauthors[i], "Verify coauthor " + expectedCoauthors[i] + " was set properly");
                    VerifyEqual(n.key, -4, "Verify E minor scale set as key");
                    VerifyEqual(n.time.baseNote, 4, "Verify quarter note base rhythm");
                    VerifyEqual(n.time.beatsPerMeasure, 4, "Verify bpm set properly");
                    VerifyEqual(Math.Round(n.tempo, 5), 0.33708, "Verify tempo set properly"); /* Round to 5 decimal places */
                    VerifyEqual(n.octave, 5, "Verify octave set properly");

                    if (VerifyEqual(n.Sheet.Count, 35, "Verify the number of notes is correct"))
                    {
                        Note[] correctNotes = {
                                                  new Note { note = "A",  frequency = 830.61f,  length = 0.33708f / 2f },
                                                  new Note { note = "G",  frequency = 783.99f,  length = 0.33708f / 2f },
                                                  new Note { note = "F",  frequency = 698.46f,  length = 0.33708f / 2f },
                                                  new Note { note = "B",  frequency = 932.33f,  length = 0.33708f      },
                                                  new Note { note = "A",  frequency = 830.61f,  length = 0.33708f / 2f },
                                                  new Note { note = "G",  frequency = 783.99f,  length = 0.33708f / 2f },
                                                  new Note { note = "A",  frequency = 830.61f,  length = 0.33708f      },
                                                  new Note { note = "G",  frequency = 783.99f,  length = 0.33708f / 2f },
                                                  new Note { note = "F",  frequency = 698.46f,  length = 0.33708f / 2f },
                                                  new Note { note = "B",  frequency = 932.33f,  length = 0.33708f      },
                                                  new Note { note = "A",  frequency = 830.61f,  length = 0.33708f / 2f },
                                                  new Note { note = "G",  frequency = 783.99f,  length = 0.33708f      },
                                                  new Note { note = "F",  frequency = 698.46f,  length = 0.33708f / 2f },
                                                  new Note { note = "A",  frequency = 830.61f,  length = 0.33708f / 2f },
                                                  new Note { note = "G",  frequency = 783.99f,  length = 0.33708f / 2f },
                                                  new Note { note = "F",  frequency = 698.46f,  length = 0.33708f / 2f },
                                                  new Note { note = "G",  frequency = 783.99f,  length = 0.33708f / 2f },
                                                  new Note { note = "E$", frequency = 659.25f,  length = 0.33708f / 2f },
                                                  new Note { note = "G",  frequency = 783.99f,  length = 0.33708f / 2f },
                                                  new Note { note = "B",  frequency = 932.33f,  length = 0.33708f / 2f },
                                                  new Note { note = "C",  frequency = 1046.50f, length = 1.34831f      },
                                                  new Note { note = "E",  frequency = 164.81f,  length = 0.14423f      },
                                                  new Note { note = "F",  frequency = 185.00f,  length = 0.14423f      },
                                                  new Note { note = "G",  frequency = 196.00f,  length = 0.14423f      },
                                                  new Note { note = "A",  frequency = 220.00f,  length = 0.14423f * 2f },
                                                  new Note { note = "G",  frequency = 196.00f,  length = 0.14423f      },
                                                  new Note { note = "F",  frequency = 185.00f,  length = 0.14423f      },
                                                  new Note { note = "G",  frequency = 196.00f,  length = 0.14423f      },
                                                  new Note { note = "F",  frequency = 185.00f,  length = 0.14423f      },
                                                  new Note { note = "G",  frequency = 196.00f,  length = 0.14423f      },
                                                  new Note { note = "F",  frequency = 185.00f,  length = 0.14423f      },
                                                  new Note { note = "D",  frequency = 146.83f,  length = 0.14423f      },
                                                  new Note { note = "E",  frequency = 659.25f,  length = 0.14423f * 2f },
                                                  new Note { note = "E",  frequency = 41.20f,   length = 0.14423f * 2f },
                                                  new Note { note = "E",  frequency = 164.81f,  length = 1.15385f      }
                                              };

                        for (int i = 0; i < correctNotes.Length; ++i)
                        {
                            VerifyEqual(n.Sheet[i][0].note, correctNotes[i].note, "Verify note #" + i + " has the correct note name");
                            VerifyEqual(n.Sheet[i][0].frequency, correctNotes[i].frequency, "Verify note #" + i + " has the correct note frequency");
                            VerifyEqual((float) Math.Round(n.Sheet[i][0].length, 5), correctNotes[i].length, "Verify note #" + i + " has the correct note length");
                        }
                    }

                    /* Testing IDs, patterns, and chords */
                    string testFile3 = GetFile("note_sheet_test3.ka");
                    p = new Parser(testFile3, GetDirectory(), "note_sheet_test3.ka");
                    n = p.ParseScore();

                    /* Verify Information */
                    VerifyEqual(n.Title, "New Title", "Test title was set properly");
                    VerifyEqual(n.Author, "Auth", "Test author was set properly");
                    VerifyEqual(n.Coauthors, null, "Verify coauthors were not set");
                    VerifyEqual(n.key, 4, "Verify E major scale set as key");
                    VerifyEqual(n.time.baseNote, 2, "Verify half note base rhythm");
                    VerifyEqual(n.time.beatsPerMeasure, 2, "Verify bpm set properly");
                    VerifyEqual(n.tempo, 2f, "Verify tempo set properly");
                    VerifyEqual(n.octave, 2, "Verify octave set properly");

                    /* Verify Notes */
                    if (VerifyEqual(n.Sheet.Count, 10, "Verify the number of notes is correct"))
                    {
                        Note[] correctNotes = {
                                                  new Note { note = "E", frequency = 82.41f, length =  8f },
                                                  new Note { note = "E", frequency = 82.41f, length =  2f },
                                                  new Note { note = "F", frequency = 92.50f, length =  2f },
                                                  new Note { note = "G", frequency = 103.83f, length = 2f },
                                                  new Note { note = "A", frequency = 110.00f, length = 2f },
                                                  new Note { note = "B", frequency = 123.47f, length = 2f },
                                                  new Note { note = "C", frequency = 138.59f, length = 2f },
                                                  new Note { note = "D", frequency = 155.56f, length = 2f },
                                                  new Note { note = "E", frequency = 164.81f, length = 2f },
                                                  new Note { note = "E", frequency = 82.41f, length =  8f },
                                              };

                        for (int i = 0; i < correctNotes.Length; ++i)
                        {
                            VerifyEqual(n.Sheet[i][0].note, correctNotes[i].note, "Verify note #" + i + " has the correct note name");
                            VerifyEqual(n.Sheet[i][0].frequency, correctNotes[i].frequency, "Verify note #" + i + " has the correct note frequency");
                            VerifyEqual(n.Sheet[i][0].length, correctNotes[i].length, "Verify note #" + i + " has the correct note length");

                            if (n.Sheet[i].Count > 1)
                            {
                                VerifyEqual(n.Sheet[i][1].note, "B", "Verify name of second note in chord");
                                VerifyEqual(n.Sheet[i][1].frequency, 123.47f, "Verify name of second note in chord");
                                VerifyEqual(n.Sheet[i][1].length, 8f, "Verify name of second note in chord");

                                VerifyEqual(n.Sheet[i][2].note, "E", "Verify name of third note in chord");
                                VerifyEqual(n.Sheet[i][2].frequency, 164.81f, "Verify name of third note in chord");
                                VerifyEqual(n.Sheet[i][2].length, 8f, "Verify name of third note in chord");
                            }
                        }
                    }

                    /* Testing repeat function and shorthand */
                    string testFile4 = GetFile("note_sheet_test4.ka");
                    p = new Parser(testFile4, GetDirectory(), "note_sheet_test4.ka");
                    n = p.ParseScore();

                    string[] coauthors = { "Lzzy Hale" };

                    /* Verify Information */
                    VerifyEqual(n.Title, "This Is Test #4", "Test title was set properly");
                    VerifyEqual(n.Author, "Benjamin", "Test author was set properly");
                    for (int i = 0; i < coauthors.Length; ++i)
                        VerifyEqual(n.Coauthors[i], coauthors[i], "Verify coauthor was set");
                    VerifyEqual(n.key, 0, "Verify A minor scale set as key");
                    VerifyEqual(n.time.baseNote, 4, "Verify half note base rhythm");
                    VerifyEqual(n.time.beatsPerMeasure, 4, "Verify bpm set properly");
                    VerifyEqual((float) Math.Round(n.tempo, 5), 0.66667f, "Verify tempo set properly");
                    VerifyEqual(n.octave, 5, "Verify octave set properly");

                    /* Verify Notes */
                    Note[] patternNotes = {
                                            new Note { note = "A", frequency = 880.00f,  length =  0.66667f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.33333f },
                                            new Note { note = "G", frequency = 783.99f,  length =  0.33333f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.66667f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.33333f },
                                            new Note { note = "G", frequency = 783.99f,  length =  0.33333f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.33333f },
                                            new Note { note = "G", frequency = 783.99f,  length =  0.33333f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.33333f },
                                            new Note { note = "G", frequency = 783.99f,  length =  0.33333f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.66667f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.33333f },
                                            new Note { note = "G", frequency = 783.99f,  length =  0.33333f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.33333f },
                                            new Note { note = "G", frequency = 783.99f,  length =  0.33333f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.33333f },
                                            new Note { note = "G", frequency = 783.99f,  length =  0.33333f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.66667f },
                                            new Note { note = "G", frequency = 783.99f,  length =  0.66667f },
                                            new Note { note = "G", frequency = 783.99f,  length =  0.16667f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.33333f },
                                            new Note { note = "G", frequency = 783.99f,  length =  0.33333f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.33333f },
                                            new Note { note = "C", frequency = 1046.50f, length =  0.16667f },
                                            new Note { note = "D", frequency = 1174.66f, length =  0.83333f },
                                            new Note { note = "C", frequency = 1046.50f, length =  0.66667f },
                                            new Note { note = "E", frequency = 1318.51f, length =  0.33333f },
                                            new Note { note = "B", frequency = 987.77f,  length =  0.33333f },
                                            new Note { note = "B", frequency = 987.77f,  length =  0.33333f },
                                            new Note { note = "C", frequency = 1046.50f, length =  0.33333f },
                                            new Note { note = "B", frequency = 987.77f,  length =  0.66667f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.66667f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.33333f },
                                            new Note { note = "G", frequency = 783.99f,  length =  0.33333f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.33333f },
                                            new Note { note = "C", frequency = 1046.50f, length =  0.16667f },
                                            new Note { note = "D", frequency = 1174.66f, length =  0.83333f },
                                            new Note { note = "C", frequency = 1046.50f, length =  0.66667f },
                                            new Note { note = "E", frequency = 1318.51f, length =  0.33333f },
                                            new Note { note = "B", frequency = 987.77f,  length =  0.33333f },
                                            new Note { note = "B", frequency = 987.77f,  length =  0.33333f },
                                            new Note { note = "C", frequency = 1046.50f, length =  0.33333f },
                                            new Note { note = "B", frequency = 987.77f,  length =  0.66667f },
                                            new Note { note = "A", frequency = 880.00f,  length =  0.66667f },
                                            new Note { note = "E", frequency = 1318.51f, length =  0.66667f },
                                            new Note { note = "B", frequency = 987.77f,  length =  0.33333f },
                                            new Note { note = "B", frequency = 987.77f,  length =  0.16667f },
                                            new Note { note = "B", frequency = 987.77f,  length =  0.5f },
                                            new Note { note = "C", frequency = 1046.50f, length =  1f },
                                        };

                    Note chordBase = new Note { note = "A", frequency = 880.00f, length = 2.66667f };

                    Note fSharp = new Note { note = "F#", frequency = 739.99f, length = 2.66667f };

                    NoteSet correctNotesList = new NoteSet();
                    correctNotesList.AddRange(patternNotes);
                    for (int i = 0; i < 4; ++i)
                        correctNotesList.Add(chordBase);
                    for (int i = 0; i < 4; ++i)
                    {
                        correctNotesList.AddRange(patternNotes);
                        correctNotesList.Add(fSharp);
                    }

                    if (VerifyEqual(n.Sheet.Count, correctNotesList.Count, "Verify the number of notes is correct"))
                    {
                        for (int i = 0; i < correctNotesList.Count; ++i)
                        {
                            VerifyEqual(n.Sheet[i][0].note, correctNotesList[i].note, "Verify note #" + i + " has the correct note name");
                            VerifyEqual(n.Sheet[i][0].frequency, correctNotesList[i].frequency, "Verify note #" + i + " has the correct note frequency");
                            VerifyEqual((float) Math.Round(n.Sheet[i][0].length, 5), correctNotesList[i].length, "Verify note #" + i + " has the correct note length");

                            if (n.Sheet[i].Count > 1)
                            {
                                VerifyEqual(n.Sheet[i][1].note, "E", "Verify name of second note in chord");
                                VerifyEqual(n.Sheet[i][1].frequency, 1318.51f, "Verify name of second note in chord");
                                VerifyEqual((float) Math.Round(n.Sheet[i][1].length, 5), 2.66667f, "Verify name of second note in chord");

                                VerifyEqual(n.Sheet[i][2].note, "A", "Verify name of third note in chord");
                                VerifyEqual(n.Sheet[i][2].frequency, 1760.00f, "Verify name of third note in chord");
                                VerifyEqual((float) Math.Round(n.Sheet[i][2].length, 5), 2.66667f, "Verify name of third note in chord");
                            }
                        }
                    }

                    /* Testing layer ability */
                    string testFile5 = GetFile("note_sheet_test5.ka");
                    p = new Parser(testFile5, GetDirectory(), "note_sheet_test5.ka");
                    n = p.ParseScore();

                    /* Verify Information */
                    VerifyEqual(n.Title, "The Layer Test", "Test title was set properly");
                    VerifyEqual(n.Author, "Ben", "Test author was set properly");
                    VerifyEqual(n.Coauthors, null, "Verify coauthors were not set");
                    VerifyEqual(n.key, 2, "Verify D major scale set as key");
                    VerifyEqual(n.time.baseNote, 4, "Verify half note base rhythm");
                    VerifyEqual(n.time.beatsPerMeasure, 4, "Verify bpm set properly");
                    VerifyEqual((float) Math.Round(n.tempo, 5), 1f, "Verify tempo set properly");
                    VerifyEqual(n.octave, 4, "Verify octave set properly");

                    /* Verify Notes */
                    Note[] harmonyNotes = {
                                              new Note { note = "D", frequency = 293.66f, length = 1f },
                                              new Note { note = "E", frequency = 329.63f, length = 1f },
                                              new Note { note = "F", frequency = 369.99f, length = 1f },
                                              new Note { note = "G", frequency = 392.00f, length = 1f },
                                              new Note { note = "A", frequency = 440.00f, length = 1f },
                                              new Note { note = "B", frequency = 493.88f, length = 1f },
                                              new Note { note = "C", frequency = 554.37f, length = 1f },
                                              new Note { note = "D", frequency = 587.33f, length = 1f },
                                              new Note { note = "C", frequency = 554.37f, length = 1f },
                                              new Note { note = "B", frequency = 493.88f, length = 1f },
                                              new Note { note = "A", frequency = 440.00f, length = 1f },
                                              new Note { note = "G", frequency = 392.00f, length = 1f },
                                              new Note { note = "F", frequency = 369.99f, length = 1f },
                                              new Note { note = "E", frequency = 329.63f, length = 1f },
                                              new Note { note = "D", frequency = 293.66f, length = 4f }
                                          };

                    Note[] finishHNotes = {
                                                new Note { note = "F", frequency = 369.99f,  length = 2f },
                                                new Note { note = "F", frequency = 739.99f,  length = 2f },
                                                new Note { note = "F", frequency = 1479.98f, length = 2f },
                                                new Note { note = "F", frequency = 92.50f,   length = 2f },
                                          };

                    Note[] correctNotes5 = {
                                                new Note { note = "D", frequency = 293.66f, length = 1f },
                                                new Note { note = "E", frequency = 329.63f, length = 1f },
                                                new Note { note = "F", frequency = 369.99f, length = 1f },
                                                new Note { note = "G", frequency = 392.00f, length = 1f },
                                                new Note { note = "A", frequency = 440.00f, length = 1f },
                                                new Note { note = "B", frequency = 493.88f, length = 1f },
                                                new Note { note = "C", frequency = 554.37f, length = 1f },
                                                new Note { note = "D", frequency = 587.33f, length = 1f },
                                                new Note { note = "C", frequency = 554.37f, length = 1f },
                                                new Note { note = "B", frequency = 493.88f, length = 1f },
                                                new Note { note = "A", frequency = 440.00f, length = 1f },
                                                new Note { note = "G", frequency = 392.00f, length = 1f },
                                                new Note { note = "F", frequency = 369.99f, length = 1f },
                                                new Note { note = "E", frequency = 329.63f, length = 1f },
                                                new Note { note = "D", frequency = 293.66f, length = 6f },

                                                new Note { note = "D", frequency = 293.66f,  length = 2f },
                                                new Note { note = "D", frequency = 587.33f,  length = 2f },
                                                new Note { note = "D", frequency = 1174.66f, length = 2f },
                                                new Note { note = "D", frequency = 73.42f,   length = 2f },
                                                new Note { note = "|", frequency = 0f,       length = 4f },

                                                new Note { note = "D", frequency = 293.66f,  length = 2f },
                                                new Note { note = "D", frequency = 587.33f,  length = 2f },
                                                new Note { note = "D", frequency = 1174.66f, length = 2f },
                                                new Note { note = "D", frequency = 73.42f,   length = 2f },
                                                new Note { note = "|", frequency = 0f,       length = 4f },

                                                new Note { note = "D", frequency = 293.66f,  length = 2f },
                                                new Note { note = "D", frequency = 587.33f,  length = 2f },
                                                new Note { note = "D", frequency = 1174.66f, length = 2f },
                                                new Note { note = "D", frequency = 73.42f,   length = 2f },
                                                new Note { note = "|", frequency = 0f,       length = 4f },

                                                new Note { note = "D", frequency = 293.66f,  length = 2f },
                                                new Note { note = "D", frequency = 587.33f,  length = 2f },
                                                new Note { note = "D", frequency = 1174.66f, length = 2f },
                                                new Note { note = "D", frequency = 73.42f,   length = 2f },
                                                new Note { note = "|", frequency = 0f,       length = 4f }
                                             };

                    if (VerifyEqual(n.Sheet.Count, correctNotes5.Length, "Verify the number of notes is correct"))
                    {
                        for (int i = 0; i < correctNotes5.Length; ++i)
                        {
                            VerifyEqual(n.Sheet[i][0].note, correctNotes5[i].note, "Verify note #" + i + " has the correct note name");
                            VerifyEqual(n.Sheet[i][0].frequency, correctNotes5[i].frequency, "Verify note #" + i + " has the correct note frequency");
                            VerifyEqual(n.Sheet[i][0].length, correctNotes5[i].length, "Verify note #" + i + " has the correct note length");
                        }
                    }

                    if (VerifyEqual(n.Layers.ContainsKey(2), true, "Verify a layer exists at position 2"))
                    {
                        for (int i = 0; i < harmonyNotes.Length; ++i)
                        {
                            VerifyEqual(n.Layers[2][0][i][0].note, harmonyNotes[i].note, "Verify note #" + i + " has the correct note name");
                            VerifyEqual(n.Layers[2][0][i][0].frequency, harmonyNotes[i].frequency, "Verify note #" + i + " has the correct note frequency");
                            VerifyEqual(n.Layers[2][0][i][0].length, harmonyNotes[i].length, "Verify note #" + i + " has the correct note length");
                        }

                        int[] finishPatternPositions = { 15, 20, 25, 30 };
                        foreach (int position in finishPatternPositions)
                        {
                            if (VerifyEqual(n.Layers.ContainsKey(position), true, "Verify layer exists at position " + position))
                            {
                                for (int i = 0; i < finishHNotes.Length; ++i)
                                {
                                    VerifyEqual(n.Layers[position][0][i][0].note, finishHNotes[i].note, "Verify note #" + i + " has the correct note name");
                                    VerifyEqual(n.Layers[position][0][i][0].frequency, finishHNotes[i].frequency, "Verify note #" + i + " has the correct note frequency");
                                    VerifyEqual(n.Layers[position][0][i][0].length, finishHNotes[i].length, "Verify note #" + i + " has the correct note length");
                                }
                            }
                        }
                    }

                    /* Another layer test */
                    string test6File = GetFile("note_sheet_test6.ka");
                    p = new Parser(test6File, GetDirectory(), "note_sheet_test6.ka");
                    n = p.ParseScore();

                    /* Verify Information */
                    VerifyEqual(n.Title, "Test #6 and #7", "Test title was set properly");
                    VerifyEqual(n.Author, "Ben", "Test author was set properly");
                    VerifyEqual(n.Coauthors, null, "Verify coauthors were not set");
                    VerifyEqual(n.key, -6, "Verify Eb minor scale set as key");
                    VerifyEqual(n.time.baseNote, 4, "Verify quarter note base rhythm");
                    VerifyEqual(n.time.beatsPerMeasure, 4, "Verify bpm set properly");
                    VerifyEqual(n.tempo, 1f, "Verify tempo set properly");
                    VerifyEqual(n.octave, 7, "Verify octave set properly");

                    /* Verify Notes */
                    Note[] majorScale = {
                                            new Note { note = "G", frequency = 369.99f, length = 1f },
                                            new Note { note = "A", frequency = 415.30f, length = 1f },
                                            new Note { note = "B", frequency = 466.16f, length = 1f },
                                            new Note { note = "C", frequency = 493.88f, length = 1f },
                                            new Note { note = "D", frequency = 554.37f, length = 1f },
                                            new Note { note = "E", frequency = 622.25f, length = 1f },
                                            new Note { note = "F", frequency = 698.46f, length = 1f },
                                            new Note { note = "G", frequency = 739.99f, length = 1f },
                                        };

                    Note[] minorScale = {
                                            new Note { note = "E", frequency = 311.13f, length = 1f },
                                            new Note { note = "F", frequency = 349.23f, length = 1f },
                                            new Note { note = "G", frequency = 369.99f, length = 1f },
                                            new Note { note = "A", frequency = 415.30f, length = 1f },
                                            new Note { note = "B", frequency = 466.16f, length = 1f },
                                            new Note { note = "C", frequency = 493.88f, length = 1f },
                                            new Note { note = "D", frequency = 554.37f, length = 1f },
                                            new Note { note = "E", frequency = 622.25f, length = 1f },
                                        };

                    if (VerifyEqual(n.Sheet.Count, minorScale.Length, "Verify the number of notes is correct"))
                    {
                        for (int i = 0; i < minorScale.Length; ++i)
                        {
                            VerifyEqual(n.Sheet[i][0].note, minorScale[i].note, "Verify note #" + i + " has the correct note name");
                            VerifyEqual(n.Sheet[i][0].frequency, minorScale[i].frequency, "Verify note #" + i + " has the correct note frequency");
                            VerifyEqual(n.Sheet[i][0].length, minorScale[i].length, "Verify note #" + i + " has the correct note length");
                        }

                        for (int i = 0; i < majorScale.Length; ++i)
                        {
                            VerifyEqual(n.Layers[0][0][i][0].note, majorScale[i].note, "Verify note #" + i + " has the correct note name");
                            VerifyEqual(n.Layers[0][0][i][0].frequency, majorScale[i].frequency, "Verify note #" + i + " has the correct note frequency");
                            VerifyEqual(n.Layers[0][0][i][0].length, majorScale[i].length, "Verify note #" + i + " has the correct note length");
                        }
                    }

                    /* Test accompaniments */
                    string test7File = GetFile("note_sheet_test7.ka");
                    p = new Parser(test7File, GetDirectory(), "note_sheet_test7.ka");
                    n = p.ParseScore();

                    /* Verify Information */
                    VerifyEqual(n.Title, "Test #6 and #7", "Test title was set properly");
                    VerifyEqual(n.Author, "Ben", "Test author was set properly");
                    VerifyEqual(n.Coauthors, null, "Verify coauthors were not set");
                    VerifyEqual(n.key, 0, "Verify A minor scale set as key");
                    VerifyEqual(n.time.baseNote, 2, "Verify quarter note base rhythm");
                    VerifyEqual(n.time.beatsPerMeasure, 2, "Verify bpm set properly");
                    VerifyEqual(n.tempo, 2f, "Verify tempo set properly");
                    VerifyEqual(n.octave, 4, "Verify octave set properly");

                    /* Verify Notes */
                    NoteSet correctNoteList = new NoteSet();
                    correctNoteList.Add(new Note { note = "E", frequency = 155.56f, length = 4f });
                    correctNoteList.AddRange(majorScale);
                    correctNoteList.AddRange(majorScale);
                    correctNoteList.AddRange(minorScale);
                    correctNoteList.Add(new Note { note = "E", frequency = 155.56f, length = 15f });

                    Note[] wholeNote = { new Note { note = "E", frequency = 155.56f, length = 15f } };

                    if (VerifyEqual(n.Sheet.Count, correctNoteList.Count, "Verify the number of notes is correct"))
                    {
                        for (int i = 0; i < correctNoteList.Count; ++i)
                        {
                            VerifyEqual(n.Sheet[i][0].note, correctNoteList[i].note, "Verify note #" + i + " has the correct note name");
                            VerifyEqual(n.Sheet[i][0].frequency, correctNoteList[i].frequency, "Verify note #" + i + " has the correct note frequency");
                            VerifyEqual(n.Sheet[i][0].length, correctNoteList[i].length, "Verify note #" + i + " has the correct note length");
                        }

                        int[] finishPatternPositions = { 1, 9 };
                        foreach (int position in finishPatternPositions)
                        {
                            if (VerifyEqual(n.Layers.ContainsKey(position), true, "Verify layer exists at position " + position))
                            {
                                for (int i = 0; i < wholeNote.Length; ++i)
                                {
                                    VerifyEqual(n.Layers[position][0][i][0].note, wholeNote[i].note, "Verify note #" + i + " has the correct note name");
                                    VerifyEqual(n.Layers[position][0][i][0].frequency, wholeNote[i].frequency, "Verify note #" + i + " has the correct note frequency");
                                    VerifyEqual(n.Layers[position][0][i][0].length, wholeNote[i].length, "Verify note #" + i + " has the correct note length");
                                }
                            }
                        }
                    }

                    /* Test cross accompaniments 1 */
                    expectedContextError = true;
                    string test8File = GetFile("note_sheet_test8.ka");
                    p = new Parser(test8File, GetDirectory(), "note_sheet_test8.ka");
                    n = p.ParseScore();
                }
                catch (Exception e)
                {
                    VerifyEqual(expectedContextError, true, "Verify that a context error was expected");
                    VerifyEqual(e.Message, "CONTEXT ERROR: " + ContextError.CROSS_REFERENCE_ERROR, "Verify that the correct context error message was thrown");
                }
            }

            public override void RunTests()
            {
                TestGetFrequency();
                TestFiles();
            }
        }
    }

    partial class Parser
    {
        internal class Test__Parser : Test
        {
            private Parser parser;

            public Test__Parser()
            {
                testName = "Parser";
            }

            private void TestReset()
            {
                string program = "ben is awesome!";
                parser = new Parser(program, "", ""); /* filename and filepath are empty because they will be ignored */
                parser.IgnoreContext();
                parser.lexer.GetToken();
                VerifyEqualObj(parser.lexer.PeekToken().Content, "is", "Verify that the program has successfully removed the first token");
                parser.Reset();
                VerifyEqual(parser.program, program, "Verify that the Reset method has restored the program");
            }

            private void TestExpect()
            {
                string program = "accompany =>test comment<= } (";
                parser = new Parser(program, "", ""); /* filename and filepath are empty because they will be ignored */
                parser.IgnoreContext();
                try
                {
                    parser.Expect(TokenType.ACCOMPANY);
                    VerifyEqual(true, true, "Expect function does not throw syntax error when next token was as expected");
                }
                catch (SyntaxError)
                {
                    VerifyEqual(true, false, "Expect function threw syntax error when the token was as expected");
                }

                try
                {
                    parser.Expect(TokenType.RBRACE);
                    VerifyEqual(true, true, "Expect function does not throw syntax error when next token was as expected"
                                            + "(even though comment was present)");
                }
                catch(SyntaxError)
                {
                    VerifyEqual(true, false, "Expect function threw syntax error when the token was as expected");
                }

                try
                {
                    parser.Expect(TokenType.ID);
                    VerifyEqual(true, false, "Expect function did not throw syntax error when token was incorrect");
                }
                catch (SyntaxError)
                {
                    VerifyEqual(true, true, "Expect function threw syntax error when token was incorrect");
                }
            }

            private void TestParse(string name, string program, string errorString)
            {
                parser = new Parser(program, "", ""); /* filename and filepath are empty because they will be ignored */
                parser.IgnoreContext();
                try
                {
                    parser.ParseScore();
                    if (errorString != "")
                        VerifyEqual(true, false, "Testing " + name + ": Parser threw no syntax error when syntax error was expected");
                    else
                        VerifyEqual(true, true, "Testing " + name + ": Parser correctly threw no syntax error");
                }
                catch (SyntaxError s)
                {
                    if (errorString == "")
                        VerifyEqual(true, false, "Testing " + name + ": Parser threw a syntax error when no syntax error was expected:\n\t" + s.Message);
                    else
                        VerifyEqual(errorString, s.Message, "Testing " + name + ": Verify that the correct syntax error was thrown");
                }
            }

            private string GetSampleFile(string filename)
            {
                return System.IO.File.ReadAllText("../../../../../../sample_files/" + filename);
                                                                /* Get test file from the sample file directory */
            }

            private string GetParseFile(string filename)
            {
                return System.IO.File.ReadAllText("../../../../../test/lang/" + filename);
                                                                /* Get test file from the sample file directory */
            }

            public override void RunTests()
            {
                TestReset();
                TestExpect();

                /* Base Test Cases */
                TestParse("Empty program", "", "SYNTAX ERROR: expected: TITLE; received: EOF");
                TestParse("Completely invalid program", "ben is awesome!", "SYNTAX ERROR: expected: TITLE; received: ID");
                TestParse("Example 1",          GetSampleFile("example1.ka"), "");
                TestParse("Example 2",          GetSampleFile("example2.ka"), "");
                TestParse("Example 3",          GetSampleFile("example3.ka"), "");
                TestParse("Chords",             GetSampleFile("chords.ka"), "");
                TestParse("Base Test 5",        GetParseFile("b_parse_test5.ka"), "");
                TestParse("Note Sheet Test 1",  GetParseFile("note_sheet_test1.ka"), "");
                TestParse("Note Sheet Test 2",  GetParseFile("note_sheet_test2.ka"), "");
                TestParse("Note Sheet Test 3",  GetParseFile("note_sheet_test3.ka"), "");
                TestParse("Note Sheet Test 4",  GetParseFile("note_sheet_test4.ka"), "");
                TestParse("Note Sheet Test 5",  GetParseFile("note_sheet_test5.ka"), "");
                TestParse("Note Sheet Test 6", GetParseFile("note_sheet_test6.ka"), "");
                TestParse("Note Sheet Test 7", GetParseFile("note_sheet_test7.ka"), "");
                TestParse("Note Sheet Test 8", GetParseFile("note_sheet_test8.ka"), "");
                TestParse("Note Sheet Test 9", GetParseFile("note_sheet_test9.ka"), "");
                TestParse("Note Sheet Test 10", GetParseFile("note_sheet_test10.ka"), "");

                /* Destructive Testing */
                TestParse("Destructive Test 1", GetParseFile("d_parse_test1.ka"), "");
                TestParse("Destructive Test 2", GetParseFile("d_parse_test2.ka"), "SYNTAX ERROR: expected: BREAK; received: UNKNOWN");
                TestParse("Destructive Test 3", GetParseFile("d_parse_test3.ka"), "SYNTAX ERROR: expected: DOT; received: SEMICOLON");
                TestParse("Destructive Test 4", GetParseFile("d_parse_test4.ka"), "SYNTAX ERROR: expected: AUTHOR or COAUTHORS; received: KEY");
            }
        }
    }

    partial class SyntaxError
    {
        internal class Test__SyntaxError : Test
        {
            public Test__SyntaxError()
            {
                testName = "Syntax Error Exception";
            }

            private void TestCreateErrorString__1(TokenType actual, params TokenType[] expected)
            {
                if (expected.Length != 1)
                    VerifyEqual(true, false, "TEST ERROR: this test must use only one expected token type");
                else
                {
                    string result = CreateErrorString(actual, expected);
                    VerifyEqual("SYNTAX ERROR: expected: " + expected[0].ToString()
                                + "; received: " + actual.ToString(),
                                result, "Verify that syntax error string is correct when there's only one expected token type");
                }
            }

            private void TestCreateErrorString__2(TokenType actual, params TokenType[] expected)
            {
                if (expected.Length != 2)
                    VerifyEqual(true, false, "TEST ERROR: this test must use exactly two expected token types");
                else
                {
                    string result = CreateErrorString(actual, expected);
                    VerifyEqual("SYNTAX ERROR: expected: " + expected[0].ToString() + " or " + expected[1].ToString()
                                + "; received: " + actual.ToString(),
                                result, "Verify that syntax error string is correct when there's only two expected token types");
                }
            }

            private void TestCreateErrorString__3(TokenType actual, params TokenType[] expected)
            {
                if (expected.Length != 3)
                    VerifyEqual(true, false, "TEST ERROR: this test must use exactly three expected token types");
                else
                {
                    string result = CreateErrorString(actual, expected);
                    VerifyEqual("SYNTAX ERROR: expected: " + expected[0].ToString() + ", " + expected[1].ToString()
                                + ", or " + expected[2].ToString() + "; received: " + actual.ToString(),
                                result, "Verify that syntax error string is correct when there's more than two expected token types");
                }
            }

            private void TestCreateErrorString__6(TokenType actual, params TokenType[] expected)
            {
                if (expected.Length != 6)
                    VerifyEqual(true, false, "TEST ERROR: this test must use exactly six expected token types");
                else
                {
                    string result = CreateErrorString(actual, expected);
                    VerifyEqual("SYNTAX ERROR: expected: " + expected[0].ToString() + ", " + expected[1].ToString() + ", "
                                + expected[2].ToString() + ", " + expected[3].ToString() + ", " + expected[4].ToString() + ", or "
                                + expected[5].ToString() + "; received: " + actual.ToString(),
                                result, "Verify that syntax error string is correct when there's more than two expected token types");
                }
            }

            public override void RunTests()
            {
                TestCreateErrorString__1(TokenType.NEWLINE, TokenType.ACCOMPANY);
                TestCreateErrorString__2(TokenType.KEY, TokenType.BANG, TokenType.LPAREN);
                TestCreateErrorString__3(TokenType.RPAREN, TokenType.TIME, TokenType.RBRACE, TokenType.EQUAL);
                TestCreateErrorString__6(TokenType.SEMICOLON, TokenType.DOT, TokenType.OCTAVE, TokenType.COMMA,
                                                                        TokenType.EQUAL, TokenType.GREATER, TokenType.SLASH);
            }
        }
    }

    partial class LexicalAnalyzer
    {
        internal class Test__LexicalAnalyzer : Test
        {
            private LexicalAnalyzer lexer;

            public Test__LexicalAnalyzer()
            {
                testName = "Lexical Analyzer";
            }

            public override void RunTests()
            {
                TestGetToken();
                TestPutToken();
                TestPeekToken();
            }

            private void TestGetToken()
            {
                /* NOTE: This test case does not test returning a token via the tokeBuffer */
                LoadSampleFile("../../../../../test/lang/lexer_test.ka");
                VerifyNextToken("accompany", TokenType.ACCOMPANY);
                VerifyNextToken("[", TokenType.LBRACKET);
                VerifyNextToken("example2", TokenType.ID);
                VerifyNextToken("]", TokenType.RBRACKET);
                VerifyNextToken("name", TokenType.NAME);
                VerifyNextToken("rhythm", TokenType.ID);
                VerifyNextToken("\n---\n", TokenType.BREAK);
                VerifyNextToken("title", TokenType.TITLE);
                VerifyNextToken(":", TokenType.COLON);
                VerifyNextToken("High Noon Chorus Lead", TokenType.STRING);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("author", TokenType.AUTHOR);
                VerifyNextToken(":", TokenType.COLON);
                VerifyNextToken("pattern_lib", TokenType.ID);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("coauthors", TokenType.COAUTHORS);
                VerifyNextToken(":", TokenType.COLON);
                VerifyNextToken("pattern_lib", TokenType.ID);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("key", TokenType.KEY);
                VerifyNextToken(":", TokenType.COLON);
                VerifyNextToken("Amaj", TokenType.SIGN);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("time", TokenType.TIME);
                VerifyNextToken(":", TokenType.COLON);
                VerifyNextToken("6", TokenType.NUMBER);
                VerifyNextToken("/", TokenType.SLASH);
                VerifyNextToken("8", TokenType.NUMBER);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("tempo", TokenType.TEMPO);
                VerifyNextToken(":", TokenType.COLON);
                VerifyNextToken("4", TokenType.NUMBER);
                VerifyNextToken("=", TokenType.EQUAL);
                VerifyNextToken("180", TokenType.NUMBER);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("octave", TokenType.OCTAVE);
                VerifyNextToken(":", TokenType.COLON);
                VerifyNextToken("5", TokenType.NUMBER);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("\n---\n", TokenType.BREAK);
                VerifyNextToken("pattern", TokenType.PATTERN);
                VerifyNextToken("[", TokenType.LBRACKET);
                VerifyNextToken("chorus1", TokenType.ID);
                VerifyNextToken("]", TokenType.RBRACKET);
                VerifyNextToken(":", TokenType.COLON);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("!", TokenType.BANG);
                VerifyNextToken("key", TokenType.KEY);
                VerifyNextToken(":", TokenType.COLON);
                VerifyNextToken("Bbm", TokenType.SIGN);
                VerifyNextToken("!", TokenType.BANG);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("Eb", TokenType.NOTE);
                VerifyNextToken(".", TokenType.DOT);
                VerifyNextToken("A#", TokenType.NOTE);
                VerifyNextToken(".", TokenType.DOT);
                VerifyNextToken("D$", TokenType.NOTE);
                VerifyNextToken(",", TokenType.COMMA);
                VerifyNextToken(".", TokenType.DOT);
                VerifyNextToken(".", TokenType.DOT);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("chord", TokenType.CHORD);
                VerifyNextToken("E5", TokenType.ID);
                VerifyNextToken("is", TokenType.IS);
                VerifyNextToken("E", TokenType.NOTE);
                VerifyNextToken(";", TokenType.SEMICOLON);
                VerifyNextToken("B", TokenType.NOTE);
                VerifyNextToken(";", TokenType.SEMICOLON);
                VerifyNextToken("E", TokenType.NOTE);
                VerifyNextToken("'", TokenType.APOS);
                VerifyNextToken("\n---\n", TokenType.BREAK);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("!", TokenType.BANG);
                VerifyNextToken("time", TokenType.TIME);
                VerifyNextToken(":", TokenType.COLON);
                VerifyNextToken("common", TokenType.COMMON);
                VerifyNextToken("!", TokenType.BANG);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("!", TokenType.BANG);
                VerifyNextToken("time", TokenType.TIME);
                VerifyNextToken(":", TokenType.COLON);
                VerifyNextToken("cut", TokenType.CUT);
                VerifyNextToken("!", TokenType.BANG);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("layer", TokenType.LAYER);
                VerifyNextToken("(", TokenType.LPAREN);
                VerifyNextToken("rhythm", TokenType.ID);
                VerifyNextToken(")", TokenType.RPAREN);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("repeat", TokenType.REPEAT);
                VerifyNextToken("(", TokenType.LPAREN);
                VerifyNextToken("-2", TokenType.NUMBER);
                VerifyNextToken(")", TokenType.RPAREN);
                VerifyNextToken("{", TokenType.LBRACE);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("A", TokenType.NOTE);
                VerifyNextToken("^", TokenType.CARROT);
                VerifyNextToken("4", TokenType.NUMBER);
                VerifyNextToken("_", TokenType.ID);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("}", TokenType.RBRACE);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("%", TokenType.UNKNOWN);
                VerifyNextToken("+", TokenType.PLUS);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("\0", TokenType.EOF);

                /* Error cases that ensure complete code coverage */
                string errorCases = "\n--\n$$$_money_$$$\ninva#lid_id\nthis_*id_is_also_invalid\n\"this string does not close\n<-";
                lexer = new LexicalAnalyzer(errorCases);

                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("-", TokenType.UNKNOWN);
                VerifyNextToken("-", TokenType.UNKNOWN);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("$$$_money_$$$", TokenType.ID);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("inva#lid_id", TokenType.UNKNOWN);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("this_*id_is_also_invalid", TokenType.UNKNOWN);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("this string does not close", TokenType.UNKNOWN);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("<", TokenType.UNKNOWN);
                VerifyNextToken("-", TokenType.UNKNOWN);
            }

            private void TestPutToken()
            {
                /* NOTE: This test case has a dependency on LexicalAnalyzer's GetToken() method functioning properly */
                /* NOTE: This test case also tests GetToken()'s return via the tokenBuffer */
                lexer = new LexicalAnalyzer("");

                Token toPlace = new Token("benisawesome", TokenType.ID);
                lexer.PutToken(toPlace);

                Token toPlace2 = new Token("a b c", TokenType.STRING);
                lexer.PutToken(toPlace2);

                VerifyNextToken("a b c", TokenType.STRING);
                VerifyNextToken("benisawesome", TokenType.ID);

                VerifyNextToken("\0", TokenType.EOF);
            }

            private void TestPeekToken()
            {
                /* NOTE: This test case has a dependency on LexicalAnalyzer's GetToken() and PutToken() methods functioning properly */
                /* NOTE: The dependencies in this case are okay because PeekToken() is solely
                 *          a combination of these methods and should test them together accordingly */

                LoadSampleFile("../../../../../../sample_files/example1.ka");

                Token test = lexer.PeekToken();
                Token test2 = lexer.PeekToken();

                VerifyEqualObj(test, test2, "Verify that the tokens received by PeekToken() are the same "
                                                + "(i.e. PeekToken does not remove the token from the buffer");
            }

            /* Auto-tests the content and type of a token */
            private void VerifyNextToken(string content, TokenType type)
            {
                Token next = lexer.GetToken();
                VerifyEqual(next.Content, content, "Verify that next token has content \"" + content + "\"");
                VerifyEqualObj(next.Type, type, "Verify that this token has a type of " + type.ToString());
            }

            /* Used to load sample files into the lexer instance */
            private void LoadSampleFile(string file)
            {
                string program = System.IO.File.ReadAllText(file);
                lexer = new LexicalAnalyzer(program);
            }
        }
    }

    partial class InputBuffer
    {
        internal class Test__InputBuffer : Test
        {
            private InputBuffer input;

            public Test__InputBuffer()
            {
                testName = "Input Buffer";
            }

            public override void RunTests()
            {
                TestGetChar();
                TestPutChar();
            }

            private void TestGetChar()
            {
                string program = "ac\r\n \r  \n";
                input = new InputBuffer(program);

                char[] characterList = new char[program.Length + 2]; /* get a new char */
                for (int i = 0; i < characterList.Length; ++i) /* for each character in the program string plus a couple more to test eof  */
                    characterList[i] = input.GetChar();

                VerifyEqual(characterList[0], 'a', "Verify that the 1st character was received");
                VerifyEqual(characterList[1], 'c', "Verify that the 2nd character was received");
                VerifyEqual(characterList[2], '\n', "Verify that the 3rd character was a newline");
                VerifyEqual(characterList[3], ' ', "Verify that the 4th character was received");
                VerifyEqual(characterList[4], '\n', "Verify that the 5th character was a newline");
                VerifyEqual(characterList[5], ' ', "Verify that the 6th character was received");
                VerifyEqual(characterList[6], ' ', "Verify that the 7th character was received");
                VerifyEqual(characterList[7], '\n', "Verify that the 8th character was a newline");
                VerifyEqual(characterList[8], '\0', "Verify that the 9th character was a null terminator");
                VerifyEqual(characterList[9], '\0', "Verify that the 10th character was a null terminator");
                VerifyEqual(characterList[10], '\0', "Verify that the 11th character was a null terminator");
            }

            private void TestPutChar()
            {
                string program = "";
                input = new InputBuffer(program);

                input.PutChar('z');
                VerifyEqual(input.GetRemainingText()[0], 'z', "Verify that the new character was put into the program");
                input.PutChar('q');
                VerifyEqual(input.GetRemainingText()[0], 'q', "Verify that the new character was put into the program");
                VerifyEqual(input.GetRemainingText()[1], 'z', "Verify that the previously inserted character was pushed to the 1st index");
            }
        }
    }

    partial class Token
    {
        internal class Test__Token : Test
        {
            public Test__Token()
            {
                testName = "Token";
            }

            public override void RunTests()
            {
                TestConstructor();
            }

            private void TestConstructor()
            {
                Token testToken1 = new Token("coauthors", TokenType.COAUTHORS);
                Token testToken2 = new Token("beniscool", TokenType.ID);
                Token testToken3 = new Token("@", TokenType.UNKNOWN);

                VerifyEqual(testToken1.Content, "coauthors", "Verify that the 1st token content is what was passed");
                VerifyEqualObj(testToken1.Type, TokenType.COAUTHORS, "Verify that the 1st token type is what was passed");

                VerifyEqual(testToken2.Content, "beniscool", "Verify that the 2nd token content is what was passed");
                VerifyEqualObj(testToken2.Type, TokenType.ID, "Verify that the 2nd token type is what was passed");

                VerifyEqual(testToken3.Content, "@", "Verify that the 3rd token content is what was passed");
                VerifyEqualObj(testToken3.Type, TokenType.UNKNOWN, "Verify that the 3rd token type is what was passed");
            }
        }
    }

    /* ---------------- / TESTS -------------- */

    abstract class Test
    {
        private string resultNotes = "";
        protected string testName = "Unnamed";
        protected int comparisons = 0, failures = 0;

        public abstract void RunTests();

        public string GetTestName()
        {
            return testName;
        }

        public bool Passes()
        {
            return comparisons > 0 && failures == 0;
        }

        public string GetResults()
        {
            return resultNotes + "\nOverall Case Results:\n".ToUpper()
                + "Total Comparisons: " + comparisons + "\n"
                + "Total Failures: " + failures + "\n"
                + "Test Result: " + (Passes() ? "PASS" : "FAIL") + "\n\n";
        }

        public bool VerifyEqual(dynamic one, dynamic two, string description)
        {
            ++comparisons;
            resultNotes += description + " => ";
            if (one == two)
            {
                resultNotes += "PASSES\n";
                return true;
            }
            else
            {
                resultNotes += "FAILURE: \"" + one + "\" is not \"" + two + "\"\n";
                ++failures;
                return false;
            }
        }

        public bool VerifyEqualObj(object one, object two, string description)
        {
            ++comparisons;
            resultNotes += description + " => ";
            if (one.Equals(two))
            {
                resultNotes += "PASSES\n";
                return true;
            }
            else
            {
                resultNotes += "FAILURE: \"" + one.ToString() + "\" is not \"" + two.ToString() + "\"\n";
                ++failures;
                return false;
            }
        }
    }
}
