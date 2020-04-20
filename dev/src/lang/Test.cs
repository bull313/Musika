using Musika;

namespace MusikaTest
{
    class UnitTests
    {
        static void Main(string[] args)
        {
            int numFails = 0;
            numFails += CompilerTest.RunUnitTests();
            numFails += RuntimeEnvironmentTest.RunUnitTests();
            System.Environment.Exit(numFails);
        }
    }

    public abstract class Test
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