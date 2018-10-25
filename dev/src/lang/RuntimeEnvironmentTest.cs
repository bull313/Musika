using System;
using compiler;

namespace runtimeenvironment
{
    class RuntimeEnvironmentTest
    {
        public static int RunUnitTests()
        {
            int tests = 0, testFailures = 0;
            string output = "";
            Console.WriteLine("Runtime Environment Tests:");
            Test[] testList =
            {
                new RuntimeEnvironment.Test__RuntimeEnvironment()
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

    /* ---------------- TESTS -------------- */

    partial class RuntimeEnvironment
    {
        internal class Test__RuntimeEnvironment : Test
        {
            public Test__RuntimeEnvironment()
            {
                testName = "Runtime Environment";
            }

            private void TestConstructor()
            {
                string filepath = "../../../../../test/lang/";
                string filename = "re_test1.ka";

                Parser p = new Parser(System.IO.File.ReadAllText(filepath + filename), filepath, filename);
                Serializer.Serialize(p.ParseScore(), filepath, filename);

                RuntimeEnvironment r = new RuntimeEnvironment(filepath, filename);
                if (r.NoteSheetReceived)
                {
                    NoteSheet n = r.compiledSheet;

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
                    else
                    {
                        VerifyEqual(true, false, "Note sheet failed to be sent to the RE");
                    }
                }
            }

            public override void RunTests()
            {
                TestConstructor();
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