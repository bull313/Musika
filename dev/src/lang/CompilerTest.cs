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
            Test[] testList = { new Test__Token(), new Test__InputBuffer(), new Test__LexicalAnalyzer() }; /* ADD INSTANCES OF NEW TEST CASES HERE */

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

    class Test__LexicalAnalyzer : Test
    {
        private LexicalAnalyzer lexer;

        public Test__LexicalAnalyzer()
        {
            testName = "Lexical Analyzer";
        }

        private void TestGetToken()
        {
            string program = "accompany [example3]";
            lexer = new LexicalAnalyzer(program);

            Token t1 = lexer.GetToken();
            VerifyEqual(t1.Content, "accompany", "Verify that the accompany string was put into the first token");
            VerifyEqualObj(t1.Type, TokenType.ACCOMPANY, "Verify that the first token was recognized as the accompany keyword");

            Token t2 = lexer.GetToken();
            VerifyEqual(t2.Content, "[", "Verify that the left bracket string was put into the second token");
            VerifyEqualObj(t2.Type, TokenType.LBRACKET, "Verify that the first token was recognized as the left bracket");

            Token t3 = lexer.GetToken();
            VerifyEqual(t3.Content, "example3", "Verify that the example3 id string was put into the third token");
            VerifyEqualObj(t3.Type, TokenType.ID, "Verify that the first token was recognized as an id");

            Token t4 = lexer.GetToken();
            VerifyEqual(t4.Content, "]", "Verify that the right bracket string was put into the fourth token");
            VerifyEqualObj(t4.Type, TokenType.RBRACKET, "Verify that the first token was recognized as the right bracket");

            Token t5 = lexer.GetToken();
            VerifyEqual(t5.Content, "\0", "Verify that the end of file has been reached");
            VerifyEqualObj(t5.Type, TokenType.EOF, "Verify that the fifth token was recognized as the end of file");
        }

        private void TestPutToken()
        {
            /* NOTE: This test has a dependency on TestGetToken(), which must pass in order for this test to work */

            string program = "Bbm";
            lexer = new LexicalAnalyzer(program);

            Token t = new Token("A", TokenType.NOTE);
            lexer.PutToken(t);

            t = lexer.GetToken();
            VerifyEqual(t.Content, "A", "Verify that the A note was successfully put");
            VerifyEqualObj(t.Type, TokenType.NOTE, "Verify that put token is of the correct type");

            t = lexer.GetToken();
            VerifyEqual(t.Content, "Bbm", "Verify that the next token is the one in the program string");
            VerifyEqualObj(t.Type, TokenType.SIGN, "Verify that the next token (from the program string) is the correct type");
        }

        public override void RunTests()
        {
            TestGetToken();
            TestPutToken();
        }
    }

    class Test__InputBuffer : Test
    {
        private InputBuffer input;

        public Test__InputBuffer()
        {
            testName = "Input Buffer";
        }

        private void TestGetChar()
        {
            string program = "acco";
            input = new InputBuffer(program);

            char[] characterList = new char[6];
            characterList[0] = input.GetChar();
            characterList[1] = input.GetChar();
            characterList[2] = input.GetChar();
            characterList[3] = input.GetChar();
            characterList[4] = input.GetChar();
            characterList[5] = input.GetChar();

            VerifyEqual(characterList[0], 'a', "Verify that the 1st character was received");
            VerifyEqual(characterList[1], 'c', "Verify that the 2nd character was received");
            VerifyEqual(characterList[2], 'c', "Verify that the 3rd character was received");
            VerifyEqual(characterList[3], 'o', "Verify that the 4th character was received");
            VerifyEqual(characterList[4], '\0', "Verify that the 5th character was null terminator");
            VerifyEqual(characterList[5], '\0', "Verify that the 6th character was null terminator");
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

        public override void RunTests()
        {
            TestGetChar();
            TestPutChar();
        }
    }

    class Test__Token : Test
    {
        public Test__Token()
        {
            testName = "Token";
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

        public override void RunTests()
        {
            TestConstructor();
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
