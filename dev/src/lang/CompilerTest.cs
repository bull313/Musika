using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using compiler;

namespace compilerTest
{
    class CompilerTest
    {
        static void Main(string[] args)
        {
            int tests = 0, testFailures = 0;
            Console.WriteLine("Compiler Tests:");
            Test[] testList = { new Test__Token(), new Test__LexicalAnalyzer() }; /* ADD INSTANCES OF NEW TEST CASES HERE */

            foreach (Test test in testList)
            {
                ++tests;
                Console.WriteLine("Begin " + test.GetTestName() + " Test:\n");
                test.RunTests();
                Console.WriteLine(test.GetResults());
                if (!test.Passes())
                    ++testFailures;
            }

            Console.WriteLine("Overall Test Set:\nTotal Tests: {0}\nTotal Failures: {1}\nResults: {2}", tests, testFailures, (tests > 0 && testFailures == 0) ? "TEST PASSED" : "TEST FAILED");
        }
    }

    /* ---------------- TESTS ---------------- */

    class Test__Token : Test
    {
        public Test__Token()
        {
            testName = "Token";
        }

        public void TestConstructor()
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

        public override void RunTests()
        {
            TestConstructor();
        }
    }

    class Test__LexicalAnalyzer : Test
    {
        private Input input;

        public Test__LexicalAnalyzer()
        {
            testName = "Lexical Analyzer";
            string program = "accompany [example2] name rhythm\naccompany [example3] name pattern_lib\n---\ntitle: \"High Noon Chorus Lead\"\nauthor: pattern_lib\ncoauthors: pattern_lib\nkey: pattern_lib\ntime: pattern_lib\ntempo: pattern_lib\noctave: 5\n=>\nAny notes on the piece can be written here in a multi-line comment\n<=\n---\n& this is a single-line comment\npattern [chorus1]:\n! time: 8 !\nE....A..G..F.G.D,..A..G.F. & this is the lead pattern for high noon\n& this is another comment\n---\nlayer(rhythm)\nrepeat(2) {\nchorus1\npattern_lib>chorus2\npattern_lib>chorus_end\n}\n---";
            input = new Input(program);
        }

        public void TestGetChar()
        {
            char[] characterList = new char[4];
            characterList[0] = input.GetChar();
            characterList[1] = input.GetChar();
            characterList[2] = input.GetChar();
            characterList[3] = input.GetChar();

            VerifyEqual(characterList[0], 'a', "Verify that the 1st character was received");
            VerifyEqual(characterList[1], 'c', "Verify that the 2nd character was received");
            VerifyEqual(characterList[2], 'c', "Verify that the 3rd character was received");
            VerifyEqual(characterList[3], 'o', "Verify that the 4th character was received");
        }

        public void TestPutChar()
        {
            input.PutChar('z');
            VerifyEqual(input.GetRemainingText()[0], 'z', "Verify that the new character was put into the program");
            input.PutChar('q');
            VerifyEqual(input.GetRemainingText()[0], 'q', "Verify that the new character was put into the program");
            VerifyEqual(input.GetRemainingText()[1], 'z', "Verify that the previously inserted character was pushed to the 1st index");
        }

        public override void RunTests()
        {
            TestGetChar();
            TestPutChar();
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
            return resultNotes + "\nOverall Results:\n"
                + "Total Comparisons: " + comparisons + "\n"
                + "Total Failures: " + failures + "\n"
                + "Test Result: " + (Passes() ? "PASS" : "FAIL") + "\n";
        }

        public void VerifyEqual(dynamic one, dynamic two, string description)
        {
            ++comparisons;
            resultNotes += description + " => ";
            if (one == two)
            {
                resultNotes += "PASSES\n";
            }
            else
            {
                resultNotes += "FAILURE: \"" + one + "\" is not \"" + two + "\"\n";
                ++failures;
            }
        }

        public void VerifyEqualObj(object one, object two, string description)
        {
            ++comparisons;
            resultNotes += description + " => ";
            if (one.Equals(two))
            {
                resultNotes += "PASSES\n";
            }
            else
            {
                resultNotes += "FAILURE: \"" + one.ToString() + "\" is not \"" + two.ToString() + "\"\n";
                ++failures;
            }
        }
    }
}
