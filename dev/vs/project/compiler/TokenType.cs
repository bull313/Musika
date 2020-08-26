namespace Musika
{
    /* All the possible token types */
    public enum TokenType
    {
        /* Basic Lexicons */
        NEWLINE, LBRACKET, RBRACKET, BANG, LPAREN, RPAREN, LBRACE, RBRACE, DOT, APOS, COMMA, EQUAL, GREATER, SLASH, COLON, SEMICOLON, CARROT, PLUS,

        /* Compound Lexicons */
        BREAK, ID, SIGN, NOTE, STRING, NUMBER,

        /* Tier 1 Keywords */
        ACCOMPANY, NAME, AUTHOR, COAUTHORS, TITLE, KEY, TIME, TEMPO, OCTAVE, PATTERN, CHORD, IS,

        /* Tier 2 Keywords */
        COMMON, CUT,

        /* Tier 3 Keywords */
        REPEAT, LAYER,

        /* End of file */
        EOF,

        /* Unknown Token Type (that means there's a syntax error) */
        UNKNOWN
    }
}
