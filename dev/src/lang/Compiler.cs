using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    /* Parses a Musika program to check for correct syntax and synthesize the intermediate representation */
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

        private void AcceptNewlines() /* Consumes NEWLINE*: consumes 0 or more newlines */
        {
            Token next = lexer.GetToken();
            while (next.Type == TokenType.NEWLINE)
                next = lexer.GetToken();
            lexer.PutToken(next);
        }

        public void ParseProgram(bool reset = true) /* This would be considered a score from the grammar */
        {
            /* score -> NEWLINE* accompaniment BREAK NEWLINE* sheet NEWLINE*  | NEWLINE* sheet NEWLINE* */
            if (reset)
                Reset(); /* Restart parsing from the beginning of the file */

            AcceptNewlines();

            Token next = lexer.PeekToken();
            if (next.Type == TokenType.ACCOMPANY) /* Parse an accompaniment section iff there is one */
            {
                ParseAccompaniment();
                Expect(TokenType.BREAK);
                AcceptNewlines();
            }

            ParseSheet();

            AcceptNewlines();
        }

        private void ParseAccompaniment() /* accompaniment -> accompany_statement NEWLINE* accompaniment | accompany_statement */
        {
            ParseAccompanyStatement();
            AcceptNewlines();

            Token next = lexer.PeekToken();

            if (next.Type == TokenType.ACCOMPANY) /* parse the next accompany statement if there is one */
                ParseAccompaniment();
        }

        private void ParseAccompanyStatement() /* accompany_statement -> ACCOMPANY L_BRACKET ID R_BRACKET NAME ID */
        {
            Expect(TokenType.ACCOMPANY);
            Expect(TokenType.LBRACKET);
            Expect(TokenType.ID);
            Expect(TokenType.RBRACKET);
            Expect(TokenType.NAME);
            Expect(TokenType.ID);
        }

        private void ParseSheet() /* sheet -> info NEWLINE* BREAK NEWLINE* patterns NEWLINE* BREAK NEWLINE* music NEWLINE* BREAK NEWLINE* */
        {
            ParseInfo();
            AcceptNewlines();
            Expect(TokenType.BREAK);
            AcceptNewlines();

            ParsePatterns();
            AcceptNewlines();
            Expect(TokenType.BREAK);
            AcceptNewlines();

            ParseMusic();
            AcceptNewlines();
            Expect(TokenType.BREAK);
            AcceptNewlines();
        }

        private void ParseInfo() /* info -> title NEWLINE* author_define NEWLINE* music_info | title NEWLINE* music_info */
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
        }

        private void ParseMusicInfo() /* music_info -> key NEWLINE* time NEWLINE* tempo NEWLINE* octave */
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

        private void ParsePatterns() /* patterns -> pc_definition NEWLINE* patterns | pc_definition NEWLINE* | EPSILON */
        {
            HashSet<TokenType> pcDefFirstSet = new HashSet<TokenType>() { TokenType.PATTERN, TokenType.CHORD };

            Token next = lexer.PeekToken();
            if (pcDefFirstSet.Contains(next.Type))
            {
                ParsePcDefinition();
                AcceptNewlines();

                next = lexer.PeekToken();
                if (pcDefFirstSet.Contains(next.Type))
                    ParsePatterns();
            }
        }

        private void ParseMusic() /* music -> music_element NEWLINE* music | music_element | EPSILON */
        {
            HashSet<TokenType> musicFirstSet = new HashSet<TokenType>() { TokenType.REPEAT, TokenType.LAYER, TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG };

            Token next = lexer.PeekToken();
            if (musicFirstSet.Contains(next.Type))
            {
                ParseMusicElement();
                AcceptNewlines();

                next = lexer.PeekToken();
                if (musicFirstSet.Contains(next.Type))
                    ParseMusic();
            }
        }

        private void ParseTitle() /* title -> TITLE COLON STRING NEWLINE | TITLE COLON ID NEWLINE */
        {
            Expect(TokenType.TITLE);
            Expect(TokenType.COLON);
            Token next = lexer.GetToken();

            if (next.Type == TokenType.STRING || next.Type == TokenType.ID)
                Expect(TokenType.NEWLINE);

            else throw new SyntaxError(next.Type, TokenType.STRING, TokenType.ID);
        }

        private void ParseAuthorDefine() /* author_define -> author NEWLINE* coauthors | coauthors NEWLINE* author | author */
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

        private void ParseAuthor() /* author -> AUTHOR COLON STRING NEWLINE | AUTHOR COLON ID NEWLINE */
        {
            Expect(TokenType.AUTHOR);
            Expect(TokenType.COLON);

            Token next = lexer.GetToken();
            if (next.Type == TokenType.STRING || next.Type == TokenType.ID)
                Expect(TokenType.NEWLINE);

            else throw new SyntaxError(next.Type, TokenType.STRING, TokenType.ID);
        }

        private void ParseCoauthors() /* coauthors -> COAUTHORS COLON STRING NEWLINE | AUTHOR COLON ID NEWLINE */
        {
            Expect(TokenType.COAUTHORS);
            Expect(TokenType.COLON);

            Token next = lexer.GetToken();
            if (next.Type == TokenType.STRING || next.Type == TokenType.ID)
                Expect(TokenType.NEWLINE);

            else throw new SyntaxError(next.Type, TokenType.STRING, TokenType.ID);
        }

        private void ParseKey(bool endWithNewline) /* key -> KEY COLON SIGN NEWLINE | KEY COLON ID NEWLINE */
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

        private void ParseTime(bool endWithNewline) /* time -> TIME COLON (KeyWord2) NEWLINE | TIME COLON ID NEWLINE | TIME COLON NUMBER SLASH NUMBER NEWLINE | TIME COLON NUMBER NEWLINE */
        {
            Expect(TokenType.TIME);
            Expect(TokenType.COLON);

            Token next = lexer.GetToken();
            if (TokenTypeFactory.Tier2Keywords.Contains(next.Type) || next.Type == TokenType.ID)
            {
                if (endWithNewline)
                    Expect(TokenType.NEWLINE);
            }
            else if (next.Type == TokenType.NUMBER)
            {
                next = lexer.GetToken();
                if (next.Type == TokenType.SLASH) /* NUMBER SLASH NUMBER */
                    Expect(TokenType.NUMBER);
                else
                    lexer.PutToken(next); /* NUMBER */

                if (endWithNewline)
                    Expect(TokenType.NEWLINE);
            }

            else throw new SyntaxError(next.Type, TokenType.NUMBER, TokenType.COMMON, TokenType.CUT, TokenType.ID);
        }

        private void ParseTempo(bool endWithNewline) /* tempo -> TEMPO COLON NUMBER EQUAL NUMBER NEWLINE  | TEMPO COLON NUMBER  | TEMPO COLON ID NEWLINE */
        {
            Expect(TokenType.TEMPO);
            Expect(TokenType.COLON);

            Token next = lexer.GetToken();
            if (next.Type == TokenType.NUMBER)
            {
                next = lexer.GetToken();

                if (next.Type == TokenType.EQUAL) /* NUMBER EQUAL NUMBER */
                    Expect(TokenType.NUMBER);
                else
                    lexer.PutToken(next); /* NUMBER */

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

        private void ParseOctave() /* octave -> OCTAVE COLON NUMBER NEWLINE | OCTAVE COLON ID NEWLINE */
        {
            Expect(TokenType.OCTAVE);
            Expect(TokenType.COLON);

            Token next = lexer.GetToken();
            if (next.Type != TokenType.NUMBER && next.Type != TokenType.ID)
                throw new SyntaxError(next.Type, TokenType.NUMBER, TokenType.ID);
        }

        private void ParsePcDefinition() /* pc_definition -> PATTERN L_BRACKET ID R_BRACKET COLON NEWLINE NEWLINE* music | CHORD ID IS chord_type */
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

        private void ParseChordType() /* chord_type -> NOTE | NOTE octave_change | NOTE SEMICOLON chord_type | NOTE octave_change SEMICOLON chord_type */
        {
            Expect(TokenType.NOTE);

            Token next = lexer.GetToken();
            if (next.Type == TokenType.COMMA || next.Type == TokenType.APOS) /* First set of octave_change */
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

        private void ParseMusicElement() /* music_element -> function | riff */
        {
            Token next = lexer.PeekToken();
            if (next.Type == TokenType.REPEAT || next.Type == TokenType.LAYER)
                ParseFunction();

            else if (next.Type == TokenType.NOTE || next.Type == TokenType.ID || next.Type == TokenType.CARROT || next.Type == TokenType.BANG) /* First set of riff */
                ParseRiff();

            else throw new SyntaxError(next.Type, TokenType.REPEAT, TokenType.LAYER, TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG);
        }

        private void ParseFunction() /* function -> repeat | layer */
        {
            Token next = lexer.PeekToken();

            if (next.Type == TokenType.REPEAT)
                ParseRepeat();

            else if (next.Type == TokenType.LAYER)
                ParseLayer();

            else throw new SyntaxError(next.Type, TokenType.REPEAT, TokenType.LAYER);
        }

        private void ParseRepeat() /* repeat -> REPEAT L_PAREN NUMBER R_PAREN L_BRACE music R_BRACE */
        {
            Expect(TokenType.REPEAT);
            Expect(TokenType.LPAREN);
            Expect(TokenType.NUMBER);
            Expect(TokenType.RPAREN);
            Expect(TokenType.LBRACE);

            AcceptNewlines();

            ParseMusic();

            AcceptNewlines();

            Expect(TokenType.RBRACE);
        }

        private void ParseLayer() /* layer -> LAYER L_PAREN callback R_PAREN */
        {
            Expect(TokenType.LAYER);
            Expect(TokenType.LPAREN);
            ParseCallback();
            Expect(TokenType.RPAREN);
        }

        private void ParseRiff() /* riff -> riff_element NEWLINE* riff  | riff_element NEWLINE* */
        {
            HashSet<TokenType> riffElementFirstSet = new HashSet<TokenType>() { TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG };

            ParseRiffElement();
            AcceptNewlines();

            Token next = lexer.PeekToken();
            if (riffElementFirstSet.Contains(next.Type))
                ParseRiff();
        }

        private void ParseRiffElement() /* riff_element -> NOTE dot_set | NOTE octave_change dot_set | callback | callback dot_set
                                           | CARROT NUMBER | BANG key \ NEWLINE BANG | BANG time \ NEWLINE BANG */
        {
            Token next = lexer.GetToken();
            if (next.Type == TokenType.NOTE)
            {
                next = lexer.PeekToken();

                if (next.Type == TokenType.COMMA || next.Type == TokenType.APOS)
                {
                    ParseOctaveChange();
                    ParseDotSet();
                }

                else if (next.Type == TokenType.DOT)
                    ParseDotSet();

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

        private void ParseDotSet() /* dot_set -> DOT dot_set | DOT */
        {
            Expect(TokenType.DOT);
            Token next = lexer.PeekToken();
            if (next.Type == TokenType.DOT)
                ParseDotSet();
        }

        private void ParseOctaveChange() /* octave_change -> ( COMMA | APOS )* */
        {
            Token next = lexer.GetToken();
            if (next.Type == TokenType.COMMA || next.Type == TokenType.APOS)
            {
                while (next.Type == TokenType.COMMA || next.Type == TokenType.APOS)
                    next = lexer.GetToken();
                lexer.PutToken(next);
            }
            else throw new SyntaxError(next.Type, TokenType.COMMA, TokenType.APOS);
        }

        private void ParseCallback() /* callback -> ID | ID GREATER ID */
        {
            Expect(TokenType.ID);
            Token next = lexer.GetToken();
            if (next.Type == TokenType.GREATER)
                Expect(TokenType.ID);
            else
                lexer.PutToken(next);
        }
    }

    /* Exception subclass that is thrown when a syntax error is made in the parser */
    partial class SyntaxError : Exception
    {
        public SyntaxError(TokenType actual, params TokenType[] expected) : base(CreateErrorString(actual, expected)) { }

        private static string CreateErrorString(TokenType actual, TokenType[] expected)
        {
            string errorMsg = "SYNTAX ERROR: expected: ";

            if (expected.Length == 1) /* expected: THIS; received: THAT */
                errorMsg += expected[0];

            else if (expected.Length == 2) /* expected: THIS or THAT; received: OTHER */
                errorMsg += expected[0] + " or " + expected[1];

            else /* expected: THIS, THAT, THESE, or THOSE; received: OTHER */
            {
                for (int i = 0; i < expected.Length - 1; ++i)
                    errorMsg += expected[i] + ", ";
                errorMsg += "or " + expected.Last();
            }

            errorMsg += "; received: " + actual;

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

                /* Skip multi-line comment or return equals sign */
                else if (nextChar == '=') /* Multi-line comment */
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
                    else /* Equals sign */
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
                case '[' : return new Token(char.ToString(nextChar), TokenType.LBRACKET);
                case ']' : return new Token(char.ToString(nextChar), TokenType.RBRACKET);
                case '!' : return new Token(char.ToString(nextChar), TokenType.BANG);
                case '(' : return new Token(char.ToString(nextChar), TokenType.LPAREN);
                case ')' : return new Token(char.ToString(nextChar), TokenType.RPAREN);
                case '{' : return new Token(char.ToString(nextChar), TokenType.LBRACE);
                case '}' : return new Token(char.ToString(nextChar), TokenType.RBRACE);
                case '.' : return new Token(char.ToString(nextChar), TokenType.DOT);
                case '\'': return new Token(char.ToString(nextChar), TokenType.APOS);
                case ',' : return new Token(char.ToString(nextChar), TokenType.COMMA);
                case '>' : return new Token(char.ToString(nextChar), TokenType.GREATER);
                case '|' : return new Token(char.ToString(nextChar), TokenType.NOTE);
                case '/' : return new Token(char.ToString(nextChar), TokenType.SLASH);
                case ':' : return new Token(char.ToString(nextChar), TokenType.COLON);
                case ';' : return new Token(char.ToString(nextChar), TokenType.SEMICOLON);
                case '^' : return new Token(char.ToString(nextChar), TokenType.CARROT);
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
                    TokenType type = TokenType.UNKNOWN;

                    if (nextChar == '\n')
                    {
                        returnTokenString += nextChar;
                        type = TokenType.BREAK;
                    }

                    return new Token(returnTokenString, type);
                }
                else
                {
                    input.PutChar(nextChar);
                    while (returnTokenString.Length > 1)
                    {
                        input.PutChar(returnTokenString.Last());
                        returnTokenString = returnTokenString.Substring(0, returnTokenString.Length - 1); /* Remove last character */
                    }
                    return new Token(returnTokenString, TokenType.NEWLINE);
                }
            }

            /* ID, keyword, SIGN, or NOTE */
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
                    case "A$":          return new Token(returnTokenString, TokenType.NOTE);
                    case "B$":          return new Token(returnTokenString, TokenType.NOTE);
                    case "C$":          return new Token(returnTokenString, TokenType.NOTE);
                    case "D$":          return new Token(returnTokenString, TokenType.NOTE);
                    case "E$":          return new Token(returnTokenString, TokenType.NOTE);
                    case "F$":          return new Token(returnTokenString, TokenType.NOTE);
                    case "G$":          return new Token(returnTokenString, TokenType.NOTE);
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
                    case "_":           return new Token(returnTokenString, TokenType.NOTE); /* Rest note */

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

                bool quoteClosed = true;
                if (nextChar != '\"') /* Ensure string is closed by another quote */
                {
                    input.PutChar(nextChar);
                    quoteClosed = false;
                }

                return new Token(returnTokenString, quoteClosed ? TokenType.STRING : TokenType.UNKNOWN);
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
            /* This replaces remove newline character differences among OSs with one universal \n */
            this.programText = programText.Replace("\r\n", "\n").Replace("\r", "\n");
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
