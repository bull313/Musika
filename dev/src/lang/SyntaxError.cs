using System;
using System.Linq;

namespace Musika
{
    /* Exception subclass that is thrown when a syntax error is made in the parser */
    public partial class SyntaxError : Exception
    {
        public SyntaxError(Token actual, params TokenType[] expected) : base(CreateErrorString(actual, expected)) { }

        private static string CreateErrorString(Token actual, TokenType[] expected)
        {
            /* Local Variables */
            string errorMsg;    /* Buffer for syntax error message */
            int i;              /* Increment variable */
            /* / Local Variables */

            errorMsg = $"SYNTAX ERROR at line number {actual.LineNumber}: expected: ";

            if (expected.Length == 1) /* expected: THIS; received: THAT */
                errorMsg += expected[0];

            else if (expected.Length == 2) /* expected: THIS or THAT; received: OTHER */
                errorMsg += expected[0] + " or " + expected[1];

            else /* expected: THIS, THAT, THESE, or THOSE; received: OTHER */
            {
                for (i = 0; i < expected.Length - 1; ++i)
                    errorMsg += expected[i] + ", ";
                errorMsg += "or " + expected.Last();
            }

            errorMsg += "; received: " + actual.Type + " of value \"" + actual.Content + "\"";

            return errorMsg;
        }
    }
}
