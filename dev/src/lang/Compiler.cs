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
        NEWLINE, LBRACKET, RBRACKET, UNDERSCORE, DOLLARSIGN, BANG, POUND, LPAREN, RPAREN, LBRACE, RBRACE, AMPERSAND, ASTERISK, SEMICOLON, DOT, APOS, COMMA, EQUAL, GREATER, LESS, SLASH, COLON,
        /* Compound Lexicons */
        BREAK, ID, SIGN, NOTE, STRING, LARROW, RARROW, LARROWLARGE, RARROWLARGE,
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

            /* Remove whitespace (except newline) characters */
            char nextChar = ' ';
            while (nextChar != '\n' && char.IsWhiteSpace(nextChar))
                nextChar = input.GetChar();
            /* / Remove whitespace (except newline) characters */

            switch (nextChar)
            {
                /* Single-character tokens */
                case '[': return new Token(char.ToString(nextChar), TokenType.LBRACKET);
                case ']': return new Token(char.ToString(nextChar), TokenType.RBRACKET);
                case '_': return new Token(char.ToString(nextChar), TokenType.UNDERSCORE);
                case '$': return new Token(char.ToString(nextChar), TokenType.DOLLARSIGN);
                case '!': return new Token(char.ToString(nextChar), TokenType.BANG);
                case '#': return new Token(char.ToString(nextChar), TokenType.POUND);
                case '(': return new Token(char.ToString(nextChar), TokenType.LPAREN);
                case ')': return new Token(char.ToString(nextChar), TokenType.RPAREN);
                case '{': return new Token(char.ToString(nextChar), TokenType.LBRACE);
                case '}': return new Token(char.ToString(nextChar), TokenType.RBRACE);
                case '&': return new Token(char.ToString(nextChar), TokenType.AMPERSAND);
                case '*': return new Token(char.ToString(nextChar), TokenType.ASTERISK);
                case ';': return new Token(char.ToString(nextChar), TokenType.SEMICOLON);
                case '.': return new Token(char.ToString(nextChar), TokenType.DOT);
                case '\'': return new Token(char.ToString(nextChar), TokenType.APOS);
                case ',': return new Token(char.ToString(nextChar), TokenType.COMMA);
                case '>': return new Token(char.ToString(nextChar), TokenType.GREATER);
                case '<': return new Token(char.ToString(nextChar), TokenType.LESS);
                case '|': return new Token(char.ToString(nextChar), TokenType.NOTE);
                case '/': return new Token(char.ToString(nextChar), TokenType.SLASH);
                case ':': return new Token(char.ToString(nextChar), TokenType.COLON);
                case '\0': return new Token(char.ToString(nextChar), TokenType.EOF);
            }

            string returnTokenString = "";

            /* Complex tokens */

            /* BREAK */
            if (nextChar == '\n')
            {
                returnTokenString += nextChar;
                nextChar = input.GetChar();

                int dashCount = 0;
                while (dashCount <= 3 && nextChar == '-')
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                    ++dashCount;
                }

                if (dashCount == 3)
                {
                    if (nextChar == '\n')
                    {
                        returnTokenString += nextChar;
                        return new Token(returnTokenString, TokenType.BREAK);
                    }
                    else
                    {
                        return new Token(returnTokenString, TokenType.UNKNOWN);
                    }
                }
                else
                {
                    while (returnTokenString.Length > 1)
                    {
                        input.PutChar(returnTokenString[returnTokenString.Length - 1]);
                        returnTokenString = returnTokenString.Substring(0, returnTokenString.Length - 1);
                    }
                    return new Token(returnTokenString, TokenType.NEWLINE);
                }
            }

            /* ID, keyword, sign, or note */
            else if (char.IsLetter(nextChar))
            {
                returnTokenString += nextChar;
                nextChar = input.GetChar();
                while (nextChar == '#' || nextChar == '*' || char.IsLetterOrDigit(nextChar))
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                }

                input.PutChar(nextChar);

                switch (returnTokenString)
                {
                    /* Keywords */
                    case "accompany":   return new Token(returnTokenString, TokenType.ACCOMPANY);
                    case "name":        return new Token(returnTokenString, TokenType.NAME);
                    case "author":      return new Token(returnTokenString, TokenType.AUTHOR);
                    case "coauthors":   return new Token(returnTokenString, TokenType.COAUTHORS);
                    case "title":       return new Token(returnTokenString, TokenType.TITLE);
                    case "key":         return new Token(returnTokenString, TokenType.KEY);
                    case "time":        return new Token(returnTokenString, TokenType.TIME);
                    case "tempo":       return new Token(returnTokenString, TokenType.TEMPO);
                    case "octave":      return new Token(returnTokenString, TokenType.OCTAVE);
                    case "pattern":     return new Token(returnTokenString, TokenType.PATTERN);
                    case "chord":       return new Token(returnTokenString, TokenType.CHORD);
                    case "is":          return new Token(returnTokenString, TokenType.IS);
                    case "common":      return new Token(returnTokenString, TokenType.COMMON);
                    case "cut":         return new Token(returnTokenString, TokenType.CUT);
                    case "repeat":      return new Token(returnTokenString, TokenType.REPEAT);
                    case "layer":       return new Token(returnTokenString, TokenType.LAYER);
                    /* Signs */
                    case "Gmaj":        return new Token(returnTokenString, TokenType.SIGN);
                    case "Dmaj":        return new Token(returnTokenString, TokenType.SIGN);
                    case "Amaj":        return new Token(returnTokenString, TokenType.SIGN);
                    case "Emaj":        return new Token(returnTokenString, TokenType.SIGN);
                    case "Bmaj":        return new Token(returnTokenString, TokenType.SIGN);
                    case "F#maj":       return new Token(returnTokenString, TokenType.SIGN);
                    case "C#maj":       return new Token(returnTokenString, TokenType.SIGN);
                    case "Fmaj":        return new Token(returnTokenString, TokenType.SIGN);
                    case "Bbmaj":       return new Token(returnTokenString, TokenType.SIGN);
                    case "Ebmaj":       return new Token(returnTokenString, TokenType.SIGN);
                    case "Abmaj":       return new Token(returnTokenString, TokenType.SIGN);
                    case "Cm":          return new Token(returnTokenString, TokenType.SIGN);
                    case "Gm":          return new Token(returnTokenString, TokenType.SIGN);
                    case "Dm":          return new Token(returnTokenString, TokenType.SIGN);
                    case "Am":          return new Token(returnTokenString, TokenType.SIGN);
                    case "Em":          return new Token(returnTokenString, TokenType.SIGN);
                    case "Bm":          return new Token(returnTokenString, TokenType.SIGN);
                    case "F#m":         return new Token(returnTokenString, TokenType.SIGN);
                    case "C#m":         return new Token(returnTokenString, TokenType.SIGN);
                    case "Fm":          return new Token(returnTokenString, TokenType.SIGN);
                    case "Bbm":         return new Token(returnTokenString, TokenType.SIGN);
                    case "Ebm":         return new Token(returnTokenString, TokenType.SIGN);
                    case "Abm":         return new Token(returnTokenString, TokenType.SIGN);
                    /* Notes */
                    case "A":           return new Token(returnTokenString, TokenType.NOTE);
                    case "B":           return new Token(returnTokenString, TokenType.NOTE);
                    case "C":           return new Token(returnTokenString, TokenType.NOTE);
                    case "D":           return new Token(returnTokenString, TokenType.NOTE);
                    case "E":           return new Token(returnTokenString, TokenType.NOTE);
                    case "F":           return new Token(returnTokenString, TokenType.NOTE);
                    case "G":           return new Token(returnTokenString, TokenType.NOTE);
                    case "A#":          return new Token(returnTokenString, TokenType.NOTE);
                    case "B#":          return new Token(returnTokenString, TokenType.NOTE);
                    case "C#":          return new Token(returnTokenString, TokenType.NOTE);
                    case "D#":          return new Token(returnTokenString, TokenType.NOTE);
                    case "E#":          return new Token(returnTokenString, TokenType.NOTE);
                    case "F#":          return new Token(returnTokenString, TokenType.NOTE);
                    case "G#":          return new Token(returnTokenString, TokenType.NOTE);
                    case "Ab":          return new Token(returnTokenString, TokenType.NOTE);
                    case "Bb":          return new Token(returnTokenString, TokenType.NOTE);
                    case "Cb":          return new Token(returnTokenString, TokenType.NOTE);
                    case "Db":          return new Token(returnTokenString, TokenType.NOTE);
                    case "Eb":          return new Token(returnTokenString, TokenType.NOTE);
                    case "Fb":          return new Token(returnTokenString, TokenType.NOTE);
                    case "Gb":          return new Token(returnTokenString, TokenType.NOTE);
                    case "A*":          return new Token(returnTokenString, TokenType.NOTE);
                    case "B*":          return new Token(returnTokenString, TokenType.NOTE);
                    case "C*":          return new Token(returnTokenString, TokenType.NOTE);
                    case "D*":          return new Token(returnTokenString, TokenType.NOTE);
                    case "E*":          return new Token(returnTokenString, TokenType.NOTE);
                    case "F*":          return new Token(returnTokenString, TokenType.NOTE);
                    case "G*":          return new Token(returnTokenString, TokenType.NOTE);
                    case "Abb":         return new Token(returnTokenString, TokenType.NOTE);
                    case "Bbb":         return new Token(returnTokenString, TokenType.NOTE);
                    case "Cbb":         return new Token(returnTokenString, TokenType.NOTE);
                    case "Dbb":         return new Token(returnTokenString, TokenType.NOTE);
                    case "Ebb":         return new Token(returnTokenString, TokenType.NOTE);
                    case "Fbb":         return new Token(returnTokenString, TokenType.NOTE);
                    case "Gbb":         return new Token(returnTokenString, TokenType.NOTE);
                    /* If none of these, then it's an id */
                    default:
                        TokenType returnType = TokenType.ID;
                        if (returnTokenString.Contains("#") || returnTokenString.Contains("*"))
                            returnType = TokenType.UNKNOWN; /* If this is an ID, make sure there's no # or * in it */
                        return new Token(returnTokenString, returnType);
                }
            }

            /* String */
            else if (nextChar == '\"')
            {
                nextChar = input.GetChar();
                while (nextChar != '\"' && nextChar != '\n')
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                }

                if (nextChar == '\"') /* Ensure string is closed by another quote */
                    return new Token(returnTokenString, TokenType.STRING);
                else
                {
                    input.PutChar(nextChar);
                    return new Token(returnTokenString, TokenType.UNKNOWN);
                }
            }

            /* Arrow */
            /* Left arrow (large or small) */
            else if (nextChar == '<')
            {
                returnTokenString += nextChar;
                nextChar = input.GetChar();
                if (nextChar == '-')
                {
                    returnTokenString += nextChar;
                    return new Token(returnTokenString, TokenType.LARROW);
                }
                else if (nextChar == '=')
                {
                    returnTokenString += nextChar;
                    return new Token(returnTokenString, TokenType.LARROWLARGE);
                }
                else
                {
                    input.PutChar(nextChar);
                    return new Token(returnTokenString, TokenType.UNKNOWN);
                }
            }

            /* Right arrow small */
            else if (nextChar == '-')
            {
                returnTokenString += nextChar;
                nextChar = input.GetChar();
                if (nextChar == '>')
                {
                    returnTokenString += nextChar;
                    return new Token(returnTokenString, TokenType.RARROW);
                }
                else
                {
                    input.PutChar(nextChar);
                    return new Token(returnTokenString, TokenType.UNKNOWN);
                }
            }

            /* Right arrow large */
            else if (nextChar == '=')
            {
                returnTokenString += nextChar;
                nextChar = input.GetChar();
                if (nextChar == '>')
                {
                    returnTokenString += nextChar;
                    return new Token(returnTokenString, TokenType.RARROWLARGE);
                }
                else
                {
                    input.PutChar(nextChar);
                    return new Token(returnTokenString, TokenType.UNKNOWN);
                }
            }

            /* Else unknown token */
            else
            {
                returnTokenString += nextChar;
                return new Token(returnTokenString, TokenType.UNKNOWN);
            }
        }

        public void PutToken(Token t)
        {
            tokenBuffer.Push(t);
        }
    }

    class SyntaxError : Exception
    {
        public SyntaxError(TokenType actual, params TokenType[] expected) : base(CreateErrorString(actual, expected)) {}

        private static string CreateErrorString(TokenType actual, TokenType[] expected)
        {
            string errorMsg = "SYNTAX ERROR: expected: ";
            if (expected.Length == 1)
                errorMsg += expected[0].ToString();
            else
            {
                for (int i = 0; i < expected.Length - 1; ++i)
                    errorMsg += expected[i].ToString() + ", ";
                errorMsg += " or " + expected[expected.Length - 1].ToString();
            }
            errorMsg += "; received: " + actual.ToString() + "\n";

            return errorMsg;
        }
    }

    class Parser
    {
        private string program, errorMsg;
        private LexicalAnalyzer lexer;

        public Parser(string program)
        {
            this.program = program;
            Reset();
        }

        public void Reset()
        {
            lexer = new LexicalAnalyzer(program);
        }

        public void Expect(TokenType etype)
        {
            Token next = lexer.GetToken();
            if (next.Type != etype && errorMsg == "")
                throw new SyntaxError(next.Type, etype);
        }

        private Token PeekToken()
        {
            Token returnToken = lexer.GetToken();
            lexer.PutToken(returnToken);
            return returnToken;
        }

        public void ParseProgram() /* This would be considered a score */
        {
            Reset();
            Token next = PeekToken();
            if (next.Type == TokenType.ACCOMPANY)
            {
                ParseAccompaniment();
                Expect(TokenType.BREAK);
                ParseSheet();
            }
            else if (next.Type == TokenType.TITLE)
            {
                ParseSheet();
            }
            else
            {
                throw new SyntaxError(next.Type, TokenType.ACCOMPANY, TokenType.TITLE);
            }
        }

        private void ParseSheet()
        {
            ParseInfo();
            Expect(TokenType.BREAK);
            ParsePatterns();
            Expect(TokenType.BREAK);
            ParseMusic();
            Expect(TokenType.BREAK);
        }

        private void ParseAccompaniment()
        {
            Expect(TokenType.ACCOMPANY);
            Expect(TokenType.LBRACKET);
            Expect(TokenType.ID);
            Expect(TokenType.RBRACKET);
            Expect(TokenType.NAME);
            Expect(TokenType.ID);
        }

        private void ParseInfo()
        {
            ParseTitle();
            Token next = lexer.GetToken();
            lexer.PutToken(next); /* This is effectively a peek at the token without taking it out */
            if (next.Type == TokenType.AUTHOR)
                ParseAuthorDefine();
            ParseMusicInfo();
        }

        private void ParseMusicInfo()
        {
            ParseKey();
            ParseTime();
            ParseTempo();
            ParseOctave();
        }

        private void ParsePatterns()
        {
            ParsePcDefinition();
            Token next = PeekToken();
            if (next.Type == TokenType.PATTERN)
                ParsePatterns();
        }

        private void ParseMusic()
        {
        }

        private void ParseTitle()
        {
        }

        private void ParseAuthorDefine()
        { }

        private void ParseAuthor()
        {
        }

        private void ParseCoauthors()
        { }

        private void ParseKey()
        { }

        private void ParseTime()
        { }

        private void ParseTempo()
        { }

        private void ParseOctave()
        { }

        private void ParsePcDefinition()
        { }

        private void ParseChordType()
        { }

        private void MusicElement()
        { }

        private void ParseFunction()
        { }

        private void ParseRepeat()
        { }

        private void ParseLayer()
        { }

        private void ParseRiff()
        { }

        private void ParseRiffElement()
        { }

        private void ParseDotSet()
        { }

        private void ParseOctaveChange()
        { }

        private void ParseCallback()
        { }
    }
}
