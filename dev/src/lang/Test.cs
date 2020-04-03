using compiler;
using runtimeenvironment;

namespace test
{
    class UnitTests
    {
        static void Main(string[] args)
        {
            int numFails = 0;
            numFails += CompilerTest.RunUnitTests();
            System.Environment.Exit(numFails);
        }
    }
}