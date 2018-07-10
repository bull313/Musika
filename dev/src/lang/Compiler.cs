using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    /* All the possible token types */
    enum TokenType
    {
        /* Basic Lexicons */
        NEWLINE, LBRACKET, RBRACKET, LETTER, DIGIT, UNDERSCORE, DOLLARSIGN, BANG, POUND, QUOTE, LPAREN, RPAREN, LBRACE, RBRACE, AMPERSAND, ASTERISK, SEMICOLON, DOT, APOS, COMMA, EQUAL, GREATER, LESS, PIPE, SLASH,
        /* Compound Lexicons */
        BREAK, ID, ALNUM, SIGN, NOTE, STRING, LARROW, RARROW, LARRORLARGE, RARROWLARGE, NUMBER,
        /* Tier 1 Keywords */
        ACCOMPANY, NAME, AUTHOR, COAUTHORS, TITLE, KEY, TIME, TEMPO, OCTAVE, PATTERN, CHORD, IS,
        /* Tier 2 Keywords */
        COMMON, CUT,
        /* Tier 3 Keywords */
        REPEAT, LAYER,
        /* Unknown Token Type (that means there's a syntax error) */
        UNKNOWN
    };

    /* Object representation of a lexical token in the Musika grammar */
    class Token
    {
        public readonly string Content;
        public readonly TokenType Type;

        public Token(string content, TokenType type)
        {
            this.Content = content;
            this.Type = type;
        }
    }

    /* Holds the program as a raw string and allows other classes to access it char by char */
    class InputBuffer
    {
        private string programText;

        public InputBuffer(string programText)
        {
            this.programText = programText;
        }

        public char GetChar()
        {
            if (programText.Length == 0)
                return '\0';
            char retVal = programText[0];
            programText = programText.Substring(1, programText.Length - 1);
            return retVal;
        }

        public void PutChar(char c)
        {
            programText = c + programText;
        }

        public string GetRemainingText()
        {
            return programText;
        }
    }

    /* Gets and manages tokens from the input buffer */
    class LexicalAnalyzer
    {
        private InputBuffer input;
        private Stack<Token> tokenBuffer;

        public LexicalAnalyzer(string programText)
        {
            input = new InputBuffer(programText);
            tokenBuffer = new Stack<Token>();
        }

        public Token GetToken()
        {
            if (tokenBuffer.Count > 0)
                return tokenBuffer.Pop();

            char nextChar = input.GetChar();
            // THIS IS WHERE WE FIGURE OUT WHICH TOKEN THIS BELONGS TO
        }

        public void PutToken(Token t)
        {
            tokenBuffer.Push(t);
        }
    }
}
