using System;
using System.IO;
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

            public string GetDirectory()
            {
                return "../../../../../test/lang/";
            }

            private void TestPlayWAVFile()
            {
                string filepath = GetDirectory();
                string filename = "re_test4.ka";

                Compiler c = new Compiler(filepath, filename);
                c.CompileToNoteSheet();
                c.CompileToWAV();

                string wavFileAddress = Path.Combine(filepath, Path.ChangeExtension(filename, WAVFile.WAV_FILE_EXT));

                VerifyEqual(File.Exists(wavFileAddress), true, "Verify that the WAV file was generated");

                RuntimeEnvironment re = new RuntimeEnvironment(filepath, filename);
                re.PlayWAVFile();

                VerifyEqual(true, true, "WILL NEED TO VERIFY SOUND ON YOUR OWN: MAKE SURE THE SONG SOUDNDS RIGHT!");
            }

            public override void RunTests()
            {
                TestPlayWAVFile();
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
