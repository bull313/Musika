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
            int passCount = 0;
            Console.WriteLine("Compiler Tests:");
            Test[] testList = { new Test__Token() };

            foreach (Test test in testList)
            {
                Console.WriteLine("Begin " + test.GetTestName() + " Test:\n");
                test.RunTests();
                Console.WriteLine(test.GetResults());
                if (test.Passes())
                    ++passCount;
            }
        }
    }

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

        public void VerifyEqual(dynamic one, dynamic two)
        {
            ++comparisons;
            if (one == two)
            {
                resultNotes += "\"" + one + "\"" + " = " + "\"" + two + "\"" + ": Comparison #" + comparisons + " passes\n";
            }
            else
            {
                resultNotes += "Failure at comparison #" + comparisons + ": " + one + " == " + two + " is false!\n";
                ++failures;
            }
        }

        public void VerifyEqualObj(object one, object two)
        {
            ++comparisons;
            if (one.Equals(two))
            {
                resultNotes += one + " = " + two + ": Comparison #" + comparisons + " passes\n";
            }
            else
            {
                resultNotes += "Failure at comparison #" + comparisons + ": " + one + " == " + two + " is false!\n";
                ++failures;
            }
        }
    }

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

            VerifyEqual(testToken1.Content, "coauthors");
            VerifyEqualObj(testToken1.Type, TokenType.COAUTHORS);

            VerifyEqual(testToken2.Content, "beniscool");
            VerifyEqualObj(testToken2.Type, TokenType.ID);

            VerifyEqual(testToken3.Content, "@");
            VerifyEqualObj(testToken3.Type, TokenType.UNKNOWN);
        }

        public override void RunTests()
        {
            TestConstructor();
        }
    }
}
