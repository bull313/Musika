using System;
using System.IO;

namespace compiler
{
    class CompilerTest
    {
        static void Main(string[] args)
        {
            int tests = 0, testFailures = 0;
            string output = "";
            Console.WriteLine("Compiler Tests:");
            Test[] testList =
            {
                new Token.Test__Token(), new InputBuffer.Test__InputBuffer(), new LexicalAnalyzer.Test__LexicalAnalyzer(),
                new SyntaxError.Test__SyntaxError(), new Parser.Test__Parser()
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

            System.Environment.Exit(testFailures);
        }
    }

    /* ---------------- TESTS ---------------- */

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
                parser = new Parser(program);
                parser.lexer.GetToken();
                VerifyEqualObj(parser.lexer.PeekToken().Content, "is", "Verify that the program has successfully removed the first token");
                parser.Reset();
                VerifyEqual(parser.program, program, "Verify that the Reset method has restored the program");
            }

            private void TestExpect()
            {
                string program = "accompany =>test comment<= } (";
                parser = new Parser(program);
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
                parser = new Parser(program);
                try
                {
                    parser.ParseProgram();
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

            private string GetTestFile(string filename)
            {
                return System.IO.File.ReadAllText("../../../../../test/lang/parser/" + filename);
                                                                /* Get test file text from the test directory */
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
                TestParse("Example 1", GetSampleFile("example1.ka"), "");
                TestParse("Example 2", GetSampleFile("example2.ka"), "");
                TestParse("Example 3", GetSampleFile("example3.ka"), "");
                TestParse("Chords", GetSampleFile("chords.ka"), "");

                /* Destructive Testing */
                TestParse("Destructive Test 1", GetParseFile("parser_test1.ka"), "");
                TestParse("Destructive Test 2", GetParseFile("parser_test2.ka"), "SYNTAX ERROR: expected: BREAK; received: UNKNOWN");
                TestParse("Destructive Test 3", GetParseFile("parser_test3.ka"), "SYNTAX ERROR: expected: DOT, COMMA, or APOS; received: SEMICOLON");
                TestParse("Destructive Test 4", GetParseFile("parser_test4.ka"), "SYNTAX ERROR: expected: KEY, TIME, TEMPO, or OCTAVE; received: AUTHOR");
            }
        }
    }

    partial class SyntaxError : Exception
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
                VerifyNextToken("_", TokenType.NOTE);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("}", TokenType.RBRACE);
                VerifyNextToken("\n", TokenType.NEWLINE);
                VerifyNextToken("%", TokenType.UNKNOWN);
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
