using System.Collections.Generic;

namespace Musika
{
    /* Collection of TokenType category sets */
    class TokenTypeFactory
    {
        public static readonly HashSet<TokenType> BasicLexicons = new HashSet<TokenType>()
        {
          TokenType.NEWLINE,   TokenType.LBRACKET, TokenType.RBRACKET, TokenType.BANG,  TokenType.LPAREN,
          TokenType.RPAREN,    TokenType.LBRACE,   TokenType.RBRACE,   TokenType.DOT,   TokenType.APOS,
          TokenType.COMMA,     TokenType.EQUAL,    TokenType.GREATER,  TokenType.SLASH, TokenType.COLON,
          TokenType.SEMICOLON, TokenType.CARROT
        };

        public static readonly HashSet<TokenType> CompoundLexicons = new HashSet<TokenType>()
        {
           TokenType.BREAK, TokenType.ID, TokenType.SIGN, TokenType.NOTE, TokenType.STRING, TokenType.NUMBER
        };

        public static readonly HashSet<TokenType> Tier1Keywords = new HashSet<TokenType>()
        {
           TokenType.ACCOMPANY, TokenType.NAME,  TokenType.AUTHOR, TokenType.COAUTHORS, TokenType.TITLE, TokenType.KEY,
           TokenType.TIME,      TokenType.TEMPO, TokenType.OCTAVE, TokenType.PATTERN,   TokenType.CHORD, TokenType.IS
        };

        public static readonly HashSet<TokenType> Tier2Keywords = new HashSet<TokenType>()
        {
           TokenType.COMMON, TokenType.CUT
        };

        public static readonly HashSet<TokenType> Tier3Keywords = new HashSet<TokenType>()
        {
           TokenType.REPEAT, TokenType.LAYER
        };
    }
}
