namespace Musika
{
    /* Object representation of a lexical token in the Musika grammar */
    public partial class Token
    {
        /* PROPERTIES */

        public readonly string Content;
        public readonly TokenType Type;

        /* / PROPERTIES */

        /* CONSTRUCTOR */

        public Token(string content, TokenType type)
        {
            this.Content = content;
            this.Type = type;
        }

        /* / CONSTRUCTOR */
    }
}
