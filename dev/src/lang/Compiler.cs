using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    partial class Parser
    {
        private string program;
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

        private void Expect(TokenType etype)
        {
            Token next = lexer.GetToken();
            if (next.Type != etype)
                throw new SyntaxError(next.Type, etype);
        }

        private void AcceptNewlines()
        {
            Token next = lexer.GetToken();
            while (next.Type == TokenType.NEWLINE)
                next = lexer.GetToken();
            lexer.PutToken(next);
        }

        public void ParseProgram(bool reset = true) /* This would be considered a score from the grammar */
        {
            if (reset)
                Reset(); /* Restart parsing from the beginning of the file */

            Token next = lexer.PeekToken();
            if (next.Type == TokenType.ACCOMPANY) /* Parse an accompaniment section iff there is one */
            {
                ParseAccompaniment();
                Expect(TokenType.BREAK);
            }

            AcceptNewlines();

            ParseSheet();
        }

        private void ParseSheet()
        {
            ParseInfo();
            Expect(TokenType.BREAK);
            AcceptNewlines();

            ParsePatterns();
            Expect(TokenType.BREAK);
            AcceptNewlines();

            ParseMusic();
            Expect(TokenType.BREAK);
            AcceptNewlines();
        }

        private void ParseAccompaniment()
        {
            ParseAccompanyStatement();
            AcceptNewlines();
            Token next = lexer.PeekToken();

            if (next.Type == TokenType.ACCOMPANY) /* parse the next accompany statement if there is one */
                ParseAccompaniment();
        }

        private void ParseAccompanyStatement()
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
            AcceptNewlines();

            Token next = lexer.PeekToken();
            if (next.Type == TokenType.AUTHOR || next.Type == TokenType.COAUTHORS) /* parse author info iff there is info */
            {
                ParseAuthorDefine();
                AcceptNewlines();
            }

            ParseMusicInfo();
            AcceptNewlines();
        }

        private void ParseMusicInfo()
        {
            ParseKey(true);
            AcceptNewlines();

            ParseTime(true);
            AcceptNewlines();

            ParseTempo(true);
            AcceptNewlines();

            ParseOctave();
            AcceptNewlines();
        }

        private void ParsePatterns()
        {
            TokenType[] acceptedTypes = { TokenType.PATTERN, TokenType.CHORD };

            Token next = lexer.PeekToken();
            if (acceptedTypes.Contains((TokenType)next.Type))
            {
                ParsePcDefinition();
                AcceptNewlines();
            }

            next = lexer.PeekToken();
            if (acceptedTypes.Contains((TokenType) next.Type))
                ParsePatterns();
        }

        private void ParseMusic()
        {
            TokenType[] acceptedTypes = { TokenType.REPEAT, TokenType.LAYER, TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG };

            Token next = lexer.PeekToken();
            if (acceptedTypes.Contains((TokenType) next.Type))
            {
                ParseMusicElement();
                AcceptNewlines();
            }

            next = lexer.PeekToken();
            if (acceptedTypes.Contains((TokenType) next.Type))
                ParseMusic();
        }

        private void ParseTitle()
        {
            Expect(TokenType.TITLE);
            Expect(TokenType.COLON);
            Token next = lexer.GetToken();

            if (next.Type == TokenType.STRING || next.Type == TokenType.ID)
                Expect(TokenType.NEWLINE);
            else throw new SyntaxError(next.Type, TokenType.STRING, TokenType.ID);
        }

        private void ParseAuthorDefine()
        {
            Token next = lexer.PeekToken();
            if (next.Type == TokenType.AUTHOR)
            {
                ParseAuthor();
                AcceptNewlines();
                next = lexer.PeekToken();
                if (next.Type == TokenType.COAUTHORS)
                    ParseCoauthors();
            }
            else if (next.Type == TokenType.COAUTHORS)
            {
                ParseCoauthors();
                AcceptNewlines();
                ParseAuthor();
            }
        }

        private void ParseAuthor()
        {
            Expect(TokenType.AUTHOR);
            Expect(TokenType.COLON);

            Token next = lexer.GetToken();
            if (next.Type == TokenType.STRING || next.Type == TokenType.ID)
                Expect(TokenType.NEWLINE);

            else throw new SyntaxError(next.Type, TokenType.STRING, TokenType.ID);
        }

        private void ParseCoauthors()
        {
            Expect(TokenType.COAUTHORS);
            Expect(TokenType.COLON);

            Token next = lexer.GetToken();
            if (next.Type == TokenType.STRING || next.Type == TokenType.ID)
                Expect(TokenType.NEWLINE);

            else throw new SyntaxError(next.Type, TokenType.STRING, TokenType.ID);
        }

        private void ParseKey(bool endWithNewline)
        {
            Expect(TokenType.KEY);
            Expect(TokenType.COLON);

            Token next = lexer.GetToken();
            if (next.Type == TokenType.SIGN || next.Type == TokenType.ID)
            {
                if (endWithNewline)
                    Expect(TokenType.NEWLINE);
            }

            else throw new SyntaxError(next.Type, TokenType.SIGN, TokenType.ID);
        }

        private void ParseTime(bool endWithNewline)
        {
            Expect(TokenType.TIME);
            Expect(TokenType.COLON);

            Token next = lexer.GetToken();
            if (next.Type == TokenType.COMMON || next.Type == TokenType.CUT || next.Type == TokenType.ID)
            {
                if (endWithNewline)
                    Expect(TokenType.NEWLINE);
            }
            else if (next.Type == TokenType.NUMBER)
            {
                next = lexer.GetToken();
                if (next.Type == TokenType.SLASH)
                    Expect(TokenType.NUMBER);
                else
                    lexer.PutToken(next);
                if (endWithNewline)
                    Expect(TokenType.NEWLINE);
            }

            else throw new SyntaxError(next.Type, TokenType.NUMBER, TokenType.COMMON, TokenType.CUT, TokenType.ID);
        }

        private void ParseTempo(bool endWithNewline)
        {
            Expect(TokenType.TEMPO);
            Expect(TokenType.COLON);

            Token next = lexer.GetToken();
            if (next.Type == TokenType.NUMBER)
            {
                next = lexer.GetToken();
                if (next.Type == TokenType.EQUAL)
                    Expect(TokenType.NUMBER);
                else
                    lexer.PutToken(next);
                if (endWithNewline)
                    Expect(TokenType.NEWLINE);
            }
            else if (next.Type == TokenType.ID)
            {
                if (endWithNewline)
                    Expect(TokenType.NEWLINE);
            }

            else throw new SyntaxError(next.Type, TokenType.NUMBER, TokenType.ID);
        }

        private void ParseOctave()
        {
            Expect(TokenType.OCTAVE);
            Expect(TokenType.COLON);

            Token next = lexer.GetToken();
            if (next.Type != TokenType.NUMBER && next.Type != TokenType.ID)
                throw new SyntaxError(next.Type, TokenType.NUMBER, TokenType.ID);
        }

        private void ParsePcDefinition()
        {
            Token next = lexer.GetToken();

            if (next.Type == TokenType.PATTERN)
            {
                Expect(TokenType.LBRACKET);
                Expect(TokenType.ID);
                Expect(TokenType.RBRACKET);
                Expect(TokenType.COLON);
                Expect(TokenType.NEWLINE);
                AcceptNewlines();
                ParseMusic();
            }
            else if (next.Type == TokenType.CHORD)
            {
                Expect(TokenType.ID);
                Expect(TokenType.IS);
                ParseChordType();
            }

            else throw new SyntaxError(next.Type, TokenType.PATTERN, TokenType.CHORD);
        }

        private void ParseChordType()
        {
            Expect(TokenType.NOTE);

            Token next = lexer.GetToken();
            if (next.Type == TokenType.COMMA || next.Type == TokenType.APOS)
            {
                lexer.PutToken(next);
                ParseOctaveChange();
                next = lexer.GetToken();
                if (next.Type == TokenType.SEMICOLON)
                    ParseChordType();
                else
                    lexer.PutToken(next);
            }
            else if (next.Type == TokenType.SEMICOLON)
                ParseChordType();
            else
                lexer.PutToken(next);
        }

        private void ParseMusicElement()
        {
            Token next = lexer.PeekToken();
            if (next.Type == TokenType.REPEAT || next.Type == TokenType.LAYER)
                ParseFunction();
            else if (next.Type == TokenType.NOTE || next.Type == TokenType.ID || next.Type == TokenType.CARROT || next.Type == TokenType.BANG)
                ParseRiff();
            else throw new SyntaxError(next.Type, TokenType.REPEAT, TokenType.LAYER, TokenType.NOTE);
        }

        private void ParseFunction()
        {
            Token next = lexer.PeekToken();

            if (next.Type == TokenType.REPEAT)
            {
                ParseRepeat();
                AcceptNewlines();
            }

            else if (next.Type == TokenType.LAYER)
            {
                ParseLayer();
                AcceptNewlines();
            }

            else throw new SyntaxError(next.Type, TokenType.REPEAT, TokenType.LAYER);
        }

        private void ParseRepeat()
        {
            Expect(TokenType.REPEAT);
            Expect(TokenType.LPAREN);
            Expect(TokenType.NUMBER);
            Expect(TokenType.RPAREN);
            Expect(TokenType.LBRACE);

            AcceptNewlines();

            ParseMusic();

            Expect(TokenType.RBRACE);
        }

        private void ParseLayer()
        {
            Expect(TokenType.LAYER);
            Expect(TokenType.LPAREN);
            ParseCallback();
            Expect(TokenType.RPAREN);
        }

        private void ParseRiff()
        {
            TokenType[] acceptedTypes = { TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG };

            ParseRiffElement();
            AcceptNewlines();

            Token next = lexer.PeekToken();
            if (acceptedTypes.Contains((TokenType)next.Type)) /* Check for first set of riff */
                ParseRiff();
        }

        private void ParseRiffElement()
        {
            Token next = lexer.GetToken();
            if (next.Type == TokenType.NOTE)
            {
                next = lexer.PeekToken();

                if (next.Type == TokenType.DOT)
                    ParseDotSet();

                else if (next.Type == TokenType.COMMA || next.Type == TokenType.APOS)
                {
                    ParseOctaveChange();
                    ParseDotSet();
                }

                else throw new SyntaxError(next.Type, TokenType.DOT, TokenType.COMMA, TokenType.APOS);
            }

            else if (next.Type == TokenType.ID)
            {
                lexer.PutToken(next);
                ParseCallback();
                next = lexer.PeekToken();
                if (next.Type == TokenType.DOT)
                    ParseDotSet();
            }

            else if (next.Type == TokenType.CARROT)
                Expect(TokenType.NUMBER);

            else if (next.Type == TokenType.BANG)
            {
                next = lexer.PeekToken();

                if (next.Type == TokenType.KEY)
                {
                    ParseKey(false);
                    Expect(TokenType.BANG);
                }
                else if (next.Type == TokenType.TIME)
                {
                    ParseTime(false);
                    Expect(TokenType.BANG);
                }
                else if (next.Type == TokenType.TEMPO)
                {
                    ParseTempo(false);
                    Expect(TokenType.BANG);
                }
                else if (next.Type == TokenType.OCTAVE)
                {
                    ParseOctave();
                    Expect(TokenType.BANG);
                }
                else throw new SyntaxError(next.Type, TokenType.KEY, TokenType.TIME, TokenType.TEMPO, TokenType.OCTAVE);
            }
            else throw new SyntaxError(next.Type, TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG);
        }

        private void ParseDotSet()
        {
            Expect(TokenType.DOT);
            Token next = lexer.PeekToken();
            if (next.Type == TokenType.DOT)
                ParseDotSet();
        }

        private void ParseOctaveChange()
        {
            //A'=>comment<='',,
            Token next = lexer.GetToken();
            if (next.Type == TokenType.COMMA || next.Type == TokenType.APOS)
            {
                while (next.Type == TokenType.COMMA || next.Type == TokenType.APOS)
                    next = lexer.GetToken();
                lexer.PutToken(next);
            }
            else throw new SyntaxError(next.Type, TokenType.COMMA, TokenType.APOS);
        }

        private void ParseCallback()
        {
            Expect(TokenType.ID);
            Token next = lexer.GetToken();
            if (next.Type == TokenType.GREATER)
                Expect(TokenType.ID);
            else
                lexer.PutToken(next);
        }
    }

    partial class SyntaxError : Exception
    {
        public SyntaxError(TokenType actual, params TokenType[] expected) : base(CreateErrorString(actual, expected)) { }

        private static string CreateErrorString(TokenType actual, TokenType[] expected)
        {
            string errorMsg = "SYNTAX ERROR: expected: ";
            if (expected.Length == 1)
                errorMsg += expected[0].ToString();
            else if (expected.Length == 2)
                errorMsg += expected[0].ToString() + " or " + expected[1].ToString();
            else
            {
                for (int i = 0; i < expected.Length - 1; ++i)
                    errorMsg += expected[i].ToString() + ", ";
                errorMsg += "or " + expected[expected.Length - 1].ToString();
            }
            errorMsg += "; received: " + actual.ToString();

            return errorMsg;
        }
    }

    /* Gets and manages tokens from the input buffer */
    partial class LexicalAnalyzer
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

            char nextChar = ' ';
                /* Check for whitespace (other than newline)            check for comment characters  */
            while ((nextChar != '\n' && char.IsWhiteSpace(nextChar)) || nextChar == '&' || nextChar == '=')
            {
                /* Skip single-line comment */
                if (nextChar == '&')
                {
                    while (nextChar != '\n' && nextChar != '\0') /* End of file detecting to prevent infiite loop */
                        nextChar = input.GetChar();
                    input.PutChar(nextChar);
                }

                /* Skip multi-line comment */
                else if (nextChar == '=')
                {
                    nextChar = input.GetChar();
                    if (nextChar == '>') /* Open multiline comment; skip all characters until closed */
                    {
                        while (nextChar != '\0') /* Stop at the end of file to prevent infinite loop */
                        {
                            nextChar = input.GetChar();
                            if (nextChar == '<') /* Check for closing multiline comment */
                            {
                                nextChar = input.GetChar();
                                if (nextChar == '=')
                                {
                                    nextChar = input.GetChar();
                                    break;
                                }
                            }
                        }
                    }
                    else /* This is actually just an equals sign */
                    {
                        input.PutChar(nextChar); /* Put back the next char */
                        return new Token("=", TokenType.EQUAL);
                    }
                }

                /* Skip whitespace */
                else
                {
                    while (nextChar != '\n' && char.IsWhiteSpace(nextChar))
                        nextChar = input.GetChar();
                }
            }

            switch (nextChar)
            {
                /* Single-character tokens */
                case '[': return new Token(char.ToString(nextChar), TokenType.LBRACKET);
                case ']': return new Token(char.ToString(nextChar), TokenType.RBRACKET);
                case '!': return new Token(char.ToString(nextChar), TokenType.BANG);
                case '(': return new Token(char.ToString(nextChar), TokenType.LPAREN);
                case ')': return new Token(char.ToString(nextChar), TokenType.RPAREN);
                case '{': return new Token(char.ToString(nextChar), TokenType.LBRACE);
                case '}': return new Token(char.ToString(nextChar), TokenType.RBRACE);
                case '.': return new Token(char.ToString(nextChar), TokenType.DOT);
                case '\'': return new Token(char.ToString(nextChar), TokenType.APOS);
                case ',': return new Token(char.ToString(nextChar), TokenType.COMMA);
                case '>': return new Token(char.ToString(nextChar), TokenType.GREATER);
                case '|': return new Token(char.ToString(nextChar), TokenType.NOTE);
                case '/': return new Token(char.ToString(nextChar), TokenType.SLASH);
                case ':': return new Token(char.ToString(nextChar), TokenType.COLON);
                case ';': return new Token(char.ToString(nextChar), TokenType.SEMICOLON);
                case '^': return new Token(char.ToString(nextChar), TokenType.CARROT);
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
                while (dashCount < 3 && nextChar == '-')
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
                    input.PutChar(nextChar);
                    while (returnTokenString.Length > 1)
                    {
                        input.PutChar(returnTokenString[returnTokenString.Length - 1]);
                        returnTokenString = returnTokenString.Substring(0, returnTokenString.Length - 1);
                    }
                    return new Token(returnTokenString, TokenType.NEWLINE);
                }
            }

            /* ID, keyword, sign, or note */
            else if (char.IsLetter(nextChar) || nextChar == '_' || nextChar == '$')
            {
                returnTokenString += nextChar;
                nextChar = input.GetChar();
                while (nextChar == '#' || nextChar == '*' || nextChar == '_' || nextChar == '$' || char.IsLetterOrDigit(nextChar))
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                }

                input.PutChar(nextChar);

                switch (returnTokenString)
                {
                    /* Keywords */
                    case "accompany": return new Token(returnTokenString, TokenType.ACCOMPANY);
                    case "name": return new Token(returnTokenString, TokenType.NAME);
                    case "author": return new Token(returnTokenString, TokenType.AUTHOR);
                    case "coauthors": return new Token(returnTokenString, TokenType.COAUTHORS);
                    case "title": return new Token(returnTokenString, TokenType.TITLE);
                    case "key": return new Token(returnTokenString, TokenType.KEY);
                    case "time": return new Token(returnTokenString, TokenType.TIME);
                    case "tempo": return new Token(returnTokenString, TokenType.TEMPO);
                    case "octave": return new Token(returnTokenString, TokenType.OCTAVE);
                    case "pattern": return new Token(returnTokenString, TokenType.PATTERN);
                    case "chord": return new Token(returnTokenString, TokenType.CHORD);
                    case "is": return new Token(returnTokenString, TokenType.IS);
                    case "common": return new Token(returnTokenString, TokenType.COMMON);
                    case "cut": return new Token(returnTokenString, TokenType.CUT);
                    case "repeat": return new Token(returnTokenString, TokenType.REPEAT);
                    case "layer": return new Token(returnTokenString, TokenType.LAYER);
                    /* Signs */
                    case "Gmaj": return new Token(returnTokenString, TokenType.SIGN);
                    case "Dmaj": return new Token(returnTokenString, TokenType.SIGN);
                    case "Amaj": return new Token(returnTokenString, TokenType.SIGN);
                    case "Emaj": return new Token(returnTokenString, TokenType.SIGN);
                    case "Bmaj": return new Token(returnTokenString, TokenType.SIGN);
                    case "F#maj": return new Token(returnTokenString, TokenType.SIGN);
                    case "C#maj": return new Token(returnTokenString, TokenType.SIGN);
                    case "Fmaj": return new Token(returnTokenString, TokenType.SIGN);
                    case "Bbmaj": return new Token(returnTokenString, TokenType.SIGN);
                    case "Ebmaj": return new Token(returnTokenString, TokenType.SIGN);
                    case "Abmaj": return new Token(returnTokenString, TokenType.SIGN);
                    case "Cm": return new Token(returnTokenString, TokenType.SIGN);
                    case "Gm": return new Token(returnTokenString, TokenType.SIGN);
                    case "Dm": return new Token(returnTokenString, TokenType.SIGN);
                    case "Am": return new Token(returnTokenString, TokenType.SIGN);
                    case "Em": return new Token(returnTokenString, TokenType.SIGN);
                    case "Bm": return new Token(returnTokenString, TokenType.SIGN);
                    case "F#m": return new Token(returnTokenString, TokenType.SIGN);
                    case "C#m": return new Token(returnTokenString, TokenType.SIGN);
                    case "Fm": return new Token(returnTokenString, TokenType.SIGN);
                    case "Bbm": return new Token(returnTokenString, TokenType.SIGN);
                    case "Ebm": return new Token(returnTokenString, TokenType.SIGN);
                    case "Abm": return new Token(returnTokenString, TokenType.SIGN);
                    /* Notes */
                    case "A": return new Token(returnTokenString, TokenType.NOTE);
                    case "B": return new Token(returnTokenString, TokenType.NOTE);
                    case "C": return new Token(returnTokenString, TokenType.NOTE);
                    case "D": return new Token(returnTokenString, TokenType.NOTE);
                    case "E": return new Token(returnTokenString, TokenType.NOTE);
                    case "F": return new Token(returnTokenString, TokenType.NOTE);
                    case "G": return new Token(returnTokenString, TokenType.NOTE);
                    case "A$": return new Token(returnTokenString, TokenType.NOTE);
                    case "B$": return new Token(returnTokenString, TokenType.NOTE);
                    case "C$": return new Token(returnTokenString, TokenType.NOTE);
                    case "D$": return new Token(returnTokenString, TokenType.NOTE);
                    case "E$": return new Token(returnTokenString, TokenType.NOTE);
                    case "F$": return new Token(returnTokenString, TokenType.NOTE);
                    case "G$": return new Token(returnTokenString, TokenType.NOTE);
                    case "A#": return new Token(returnTokenString, TokenType.NOTE);
                    case "B#": return new Token(returnTokenString, TokenType.NOTE);
                    case "C#": return new Token(returnTokenString, TokenType.NOTE);
                    case "D#": return new Token(returnTokenString, TokenType.NOTE);
                    case "E#": return new Token(returnTokenString, TokenType.NOTE);
                    case "F#": return new Token(returnTokenString, TokenType.NOTE);
                    case "G#": return new Token(returnTokenString, TokenType.NOTE);
                    case "Ab": return new Token(returnTokenString, TokenType.NOTE);
                    case "Bb": return new Token(returnTokenString, TokenType.NOTE);
                    case "Cb": return new Token(returnTokenString, TokenType.NOTE);
                    case "Db": return new Token(returnTokenString, TokenType.NOTE);
                    case "Eb": return new Token(returnTokenString, TokenType.NOTE);
                    case "Fb": return new Token(returnTokenString, TokenType.NOTE);
                    case "Gb": return new Token(returnTokenString, TokenType.NOTE);
                    case "A*": return new Token(returnTokenString, TokenType.NOTE);
                    case "B*": return new Token(returnTokenString, TokenType.NOTE);
                    case "C*": return new Token(returnTokenString, TokenType.NOTE);
                    case "D*": return new Token(returnTokenString, TokenType.NOTE);
                    case "E*": return new Token(returnTokenString, TokenType.NOTE);
                    case "F*": return new Token(returnTokenString, TokenType.NOTE);
                    case "G*": return new Token(returnTokenString, TokenType.NOTE);
                    case "Abb": return new Token(returnTokenString, TokenType.NOTE);
                    case "Bbb": return new Token(returnTokenString, TokenType.NOTE);
                    case "Cbb": return new Token(returnTokenString, TokenType.NOTE);
                    case "Dbb": return new Token(returnTokenString, TokenType.NOTE);
                    case "Ebb": return new Token(returnTokenString, TokenType.NOTE);
                    case "Fbb": return new Token(returnTokenString, TokenType.NOTE);
                    case "Gbb": return new Token(returnTokenString, TokenType.NOTE);
                    case "_": return new Token(returnTokenString, TokenType.NOTE); /* Rest note */
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

            /* Number */
            else if (nextChar == '-' || char.IsNumber(nextChar))
            {
                if (nextChar == '-')
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                }

                if (char.IsNumber(nextChar))
                {
                    while (char.IsNumber(nextChar))
                    {
                        returnTokenString += nextChar;
                        nextChar = input.GetChar();
                    }

                    input.PutChar(nextChar);

                    return new Token(returnTokenString, TokenType.NUMBER);
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

        public Token PeekToken()
        {
            Token returnToken = GetToken();
            PutToken(returnToken);
            return returnToken;
        }
    }

    /* Holds the program as a raw string and allows other classes to access it char by char */
    partial class InputBuffer
    {
        private string programText;

        public InputBuffer(string programText)
        {
            this.programText = programText.Replace("\r\n", "\n").Replace("\r", "\n");
                                                                      /* This replaces remove newline character differences among OSs with one universal \n */
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

    /* Object representation of a lexical token in the Musika grammar */
    partial class Token
    {
        public readonly string Content;
        public readonly TokenType Type;

        public Token(string content, TokenType type)
        {
            this.Content = content;
            this.Type = type;
        }
    }

    /* All the possible token types */
    enum TokenType
    {
        /* Basic Lexicons */
        NEWLINE, LBRACKET, RBRACKET, BANG, LPAREN, RPAREN, LBRACE, RBRACE, DOT, APOS, COMMA, EQUAL, GREATER, SLASH, COLON, SEMICOLON, CARROT,
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
