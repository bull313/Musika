using System.Collections.Generic;
using System.Linq;

namespace Musika
{
    /* Gets and manages tokens from the input buffer */
    public partial class LexicalAnalyzer
    {
        /*
        *  ---------------- CONSTANTS ----------------
        */

        private const int BREAK_DASH_COUNT = 3;

        /*
        *  ---------------- / CONSTANTS ----------------
        */

        /*
        *  ---------------- PROPERTIES ----------------
        */

        private readonly InputBuffer input;
        private readonly Stack<Token> tokenBuffer;

        public Dictionary<int, string> SingleLineCommentStartPositionMap { get; private set; }
        public Dictionary<int, string> MultiLineCommentStartPositionMap { get; private set; }

        /*
        *  ---------------- / PROPERTIES ----------------
        */

        /*
        *  ---------------- CONSTRUCTOR ----------------
        */

        public LexicalAnalyzer(string programText)
        {
            input = new InputBuffer(programText);
            tokenBuffer = new Stack<Token>();
            SingleLineCommentStartPositionMap = new Dictionary<int, string>();
            MultiLineCommentStartPositionMap = new Dictionary<int, string>();
        }

        /*
        *  ---------------- / CONSTRUCTOR ----------------
        */

        /*
        *  ---------------- PUBLIC METHODS ----------------
        */
        public Token GetToken() /* Read and remove the next token from the token buffer or extract the next token from the input buffer */
        {
            /* Local Variables */

            TokenType   type;               /* Buffer for the current token's type                              */
            bool        quoteClosed;        /* Checks if an opening quotation mark was closed by another mark   */
            char        nextChar;           /* Current character to analyze                                     */
            int         dashCount;          /* Used to count the number of dashes for a BREAK token             */
            int         commentPosition;    /* Position of found comment                                        */
            string      foundComment;       /* Identified comment when finding token                            */

            /* / Local Variables */

            if (tokenBuffer.Count > 0)
                return tokenBuffer.Pop();

            nextChar = ' ';

                /* Check for whitespace (other than newline)            check for comment characters  */
            while ((nextChar != '\n' && nextChar != '\r' && char.IsWhiteSpace(nextChar)) || nextChar == '&' || nextChar == '=')
            {
                /* Skip single-line comment */
                if (nextChar == '&')
                {
                    commentPosition = input.Position;
                    foundComment = nextChar + "";

                    while (nextChar != '\n' && nextChar != '\r' && nextChar != '\0') /* End of file detecting to prevent infiite loop */
                    {
                        nextChar = input.GetChar();
                        foundComment += nextChar;
                    }

                    input.PutChar(nextChar);
                    foundComment = foundComment.Substring(0, foundComment.Length - 1);

                    /* Save comment */
                    SingleLineCommentStartPositionMap.Add(commentPosition, foundComment);
                }

                /* Skip multi-line comment or return equals sign */
                else if (nextChar == '=')
                {
                    foundComment = nextChar + "";
                    nextChar = input.GetChar();

                    /* Multi-line comment */
                    if (nextChar == '>')
                    {
                        commentPosition = input.Position - 1; /* Offset for first multi line comment start character */
                        foundComment += nextChar;

                        /* Stop at the end of file to prevent infinite loop */
                        while (nextChar != '\0')
                        {
                            nextChar = input.GetChar();
                            foundComment += nextChar;

                            /* Check for closing multiline comment */
                            if (nextChar == '<')
                            {
                                nextChar = input.GetChar();
                                foundComment += nextChar;

                                if (nextChar == '=')
                                {
                                    nextChar = input.GetChar();
                                    break;
                                }
                            }
                        }

                        MultiLineCommentStartPositionMap.Add(commentPosition, foundComment);
                    }

                    /* Equals sign */
                    else
                    {
                        input.PutChar(nextChar); /* Put back the next char */
                        return new Token("=", TokenType.EQUAL);
                    }
                }

                /* Skip whitespace */
                else
                {
                    while (nextChar != '\n' && nextChar != '\r' && char.IsWhiteSpace(nextChar))
                        nextChar = input.GetChar();
                }
            }

            switch (nextChar)
            {
                /* Single-character tokens */
                case '[' :  return new Token(char.ToString(nextChar), TokenType.LBRACKET);
                case ']' :  return new Token(char.ToString(nextChar), TokenType.RBRACKET);
                case '!' :  return new Token(char.ToString(nextChar), TokenType.BANG);
                case '(' :  return new Token(char.ToString(nextChar), TokenType.LPAREN);
                case ')' :  return new Token(char.ToString(nextChar), TokenType.RPAREN);
                case '{' :  return new Token(char.ToString(nextChar), TokenType.LBRACE);
                case '}' :  return new Token(char.ToString(nextChar), TokenType.RBRACE);
                case '.' :  return new Token(char.ToString(nextChar), TokenType.DOT);
                case '\'':  return new Token(char.ToString(nextChar), TokenType.APOS);
                case ',' :  return new Token(char.ToString(nextChar), TokenType.COMMA);
                case '>' :  return new Token(char.ToString(nextChar), TokenType.GREATER);
                case '|' :  return new Token(char.ToString(nextChar), TokenType.NOTE);
                case '/' :  return new Token(char.ToString(nextChar), TokenType.SLASH);
                case ':' :  return new Token(char.ToString(nextChar), TokenType.COLON);
                case ';' :  return new Token(char.ToString(nextChar), TokenType.SEMICOLON);
                case '^' :  return new Token(char.ToString(nextChar), TokenType.CARROT);
                case '+':   return new Token(char.ToString(nextChar), TokenType.PLUS);
                case '\0':  return new Token(char.ToString(nextChar), TokenType.EOF);
            }

            string returnTokenString = "";

            /* Complex tokens */

            /* BREAK */
            if (nextChar == '\n' || nextChar == '\r')
            {
                /* Store the newline in the return string (either \r\n or \n) */
                if (nextChar == '\r')
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();

                    if (nextChar == '\n')
                    {
                        returnTokenString += nextChar; /* Store \r\n as newline */
                    }
                    else
                    {
                        input.PutChar(nextChar);
                        returnTokenString = returnTokenString.Substring(0, returnTokenString.Length - 1);
                        returnTokenString += '\n'; /* Change \r to \n to save \n */
                    }
                }
                else
                {
                    returnTokenString += nextChar; /* Character was \n */
                }

                nextChar = input.GetChar();

                /* Count the number of dashes present (max 3) */
                dashCount = 0;

                while (dashCount < BREAK_DASH_COUNT && nextChar == '-')
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                    ++dashCount;
                }

                /* If the dash count hit the maximum, ensure there is a newline following to return a BREAK token */
                if (dashCount == BREAK_DASH_COUNT)
                {
                    type = TokenType.UNKNOWN;

                    /* Check for a closing newline (ignoring comments if they are present) */

                    /* Ignore all other characters until a newline or EOF is reached */
                    while (nextChar != '\n' && nextChar != '\r' && nextChar != '\0')
                    {
                        /* Collect single-line comment */
                        if (nextChar == '&')
                        {
                            commentPosition = input.Position;
                            foundComment = nextChar + "";

                            while (nextChar != '\n' && nextChar != '\r' && nextChar != '\0')
                            {
                                nextChar = input.GetChar();
                                foundComment += nextChar;
                            }

                            SingleLineCommentStartPositionMap.Add(commentPosition, foundComment);
                        }
                        else if (nextChar == '=')
                        {
                            foundComment = nextChar + "";
                            nextChar = input.GetChar();

                            if (nextChar == '>')
                            {
                                commentPosition = input.Position - 1;
                                foundComment += nextChar;

                                /* Handle multi-line comment */
                                while (nextChar != '\0')
                                {
                                    nextChar = input.GetChar();
                                    foundComment += nextChar;

                                    if (nextChar == '<')
                                    {
                                        nextChar = input.GetChar();
                                        foundComment += nextChar;

                                        if (nextChar == '=')
                                        {
                                            break;
                                        }
                                    }
                                }

                                MultiLineCommentStartPositionMap.Add(commentPosition, foundComment);
                            }
                        }
                        else if (!char.IsWhiteSpace(nextChar))
                        {
                            /* Other character (non-whitespace) found. Therefore, BREAK token is not recognized */
                            returnTokenString += nextChar;
                            return new Token(returnTokenString, type);
                        }

                        nextChar = input.GetChar();
                    }

                    /* If a newline was reached, the BREAK is \n---\n, ignoring all other characters in between */
                    if (nextChar == '\n' || nextChar == '\r')
                    {
                        /* Store the newline in the return string */
                        if (nextChar == '\r')
                        {
                            returnTokenString += nextChar;
                            nextChar = input.GetChar();

                            if (nextChar == '\n')
                            {
                                returnTokenString += nextChar;
                            }
                            else
                            {
                                input.PutChar(nextChar);
                                returnTokenString = returnTokenString.Substring(0, returnTokenString.Length - 1);
                                returnTokenString += '\n';
                            }
                        }
                        else
                        {
                            returnTokenString += nextChar;
                        }
                        type = TokenType.BREAK;
                    }

                    return new Token(returnTokenString, type);
                }

                /* Since there were too few dashes for a BREAK token, put the dashes away (if there were any) and return the initial newline as a NEWLINE token */
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
                /* Store the initial character in the  return string */
                returnTokenString += nextChar;
                nextChar = input.GetChar();

                /* Keep adding characters to to return string until an invalid character arrives */
                while (nextChar == '#' || nextChar == '*' || nextChar == '_' || nextChar == '$' || char.IsLetterOrDigit(nextChar))
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                }

                input.PutChar(nextChar);

                /* Check if the return string is any special word */
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
                    case "Cmaj":        return new Token(returnTokenString, TokenType.SIGN);
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

                    /* If none of these, then it's an ID */
                    default:
                        type = TokenType.ID;
                        if (returnTokenString.Contains("#") || returnTokenString.Contains("*"))
                            type = TokenType.UNKNOWN; /* If this is an ID, make sure there's no # or * in it */
                        return new Token(returnTokenString, type);
                }
            }

            /* String */
            else if (nextChar == '\"')
            {
                /* Keep adding content to the return string until the closing quote or newline is reached */
                nextChar = input.GetChar();

                while (nextChar != '\"' && nextChar != '\n' && nextChar != '\r' && nextChar != '\0')
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                }

                /* If the final character was a close quote, it's a valid string; if it was a newline, it is an unknown token (string was never closed in the same line) */
                quoteClosed = true;

                if (nextChar != '\"')
                {
                    input.PutChar(nextChar);
                    quoteClosed = false;
                }

                return new Token(returnTokenString, quoteClosed ? TokenType.STRING : TokenType.UNKNOWN);
            }

            /* Number */
            else if (nextChar == '-' || char.IsNumber(nextChar))
            {
                /* Include a dash if there is one (to include negative numbers) */
                if (nextChar == '-')
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                }

                /* Keep adding numbers only to the return token string */
                while (char.IsNumber(nextChar))
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                }

                input.PutChar(nextChar);

                /* Return a number token iff the returns string is parseable as a number */
                return new Token(returnTokenString, (int.TryParse( returnTokenString, out _ )) ? TokenType.NUMBER : TokenType.UNKNOWN);
            }

            /* Else unknown token */
            else
            {
                returnTokenString += nextChar;
                return new Token(returnTokenString, TokenType.UNKNOWN);
            }
        }

        public void PutToken(Token t) /* Place the given token into the token buffer */
        {
            tokenBuffer.Push(t);
        }

        public Token PeekToken() /* Read the next token and then return it to the token buffer */
        {
            Token returnToken = GetToken();
            PutToken(returnToken);
            return returnToken;
        }

        public int GetTextPosition() /* Get the current position in the program text */
        {
            return input.Position;
        }

        /*
        *  ---------------- / PUBLIC METHODS ----------------
        */
    }
}
