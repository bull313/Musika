using System;
using System.IO;

using MusikaTest;

namespace Musika
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
                new SongPlayer.Test__RuntimeEnvironment()
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

    partial class SongPlayer
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

                Musika.Compiler c = new Musika.Compiler(filepath, filename);
                c.CompileToNoteSheet();
                c.CompileToWAV();

                string wavFileAddress = Path.Combine(filepath, Path.ChangeExtension(filename, WAVFile.WAV_FILE_EXT));

                VerifyEqual(File.Exists(wavFileAddress), true, "Verify that the WAV file was generated");

                SongPlayer re = new SongPlayer(filepath, filename);
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
}
