namespace Musika
{
    /* Object representation of a lexical token in the Musika grammar */
    public partial class Token
    {
        /* PROPERTIES */

        public readonly TokenType Type;
        public readonly int LineNumber;
        public readonly string Content;

        /* / PROPERTIES */

        /* CONSTRUCTOR */

        public Token(string content, TokenType type, int lineNum)
        {
            Content     = content;
            Type        = type;
            LineNumber  = lineNum;
        }

        /* / CONSTRUCTOR */
    }
}
