using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Musika.TypeNames;

namespace Musika
{
    /* Parses a Musika program to check for correct syntax and synthesize the intermediate representation */
    partial class Parser
    {
        /* CONSTANTS */

        public static readonly string STDLIB_FILEPATH = "../../../../../src/lang/stdlib";

        public static readonly HashSet<TokenType> musicFirstSet = new HashSet<TokenType>()                      /* First set of "music" grammar rule */
        {
            TokenType.REPEAT, TokenType.LAYER, TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG
        };

        public static readonly HashSet<TokenType> pcDefFirstSet = new HashSet<TokenType>()                      /* First set of "pattern-chord definition" grammar rule */
        {
            TokenType.PATTERN, TokenType.CHORD
        };

        public static readonly HashSet<TokenType> riffFirstSet = new HashSet<TokenType>()                       /* First set of "riff" grammar rule */
        {
            TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG
        };

        public static readonly HashSet<TokenType> riffElementFirstSet = new HashSet<TokenType>()                /* First set of "riff element" grammar rule */
        {
            TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG
        };

        public static readonly HashSet<TokenType> octaveChangeFirstSet = new HashSet<TokenType>()               /* First set of "octave change" grammar rule */
        {
            TokenType.COMMA, TokenType.APOS
        };

        /* / CONSTANTS */

        /* PROPERTIES */

        private readonly HashSet<string> doNotCompileSet;                       /* Do not use any of these files in an accompaniment                */
        private readonly string filename;                                       /* File name of the current Musika file                             */
        private readonly string filepath;                                       /* File path of the current Musika file                             */
        private readonly string program;                                        /* Musika program text                                              */

        private LexicalAnalyzer lexer;                                          /* Token manager                                                    */
        private List<int>       noteCountTracker;                               /* Tracks the current note count for layering position purposes     */
        private NoteSheet       noteSheet;                                      /* Intermediate representation of the compiled Musika file          */
        private bool            ignoreContext;                                  /* Flag marked true if only syntax is checked                       */

        /* / PROPERTIES */

        /* CONSTRUCTOR */

        public Parser(string program, string filepath, string filename, HashSet<string> ignoreSet = null)
        {
            /* If filepath not set, set it to the default (current) directory (default directory: dev/vs/project/compiler/bin) */
            if (filepath == null)
                filepath = Directory.GetCurrentDirectory();
            else
                this.filepath = filepath;

            /* Add a forward slash to the file directory if it is not already present */
            if (filepath != "" && filepath[filepath.Length - 1] != '/')
                filepath += '/';

            /* If the do-not-compile set is null, initialize */
            if (doNotCompileSet == null)
                doNotCompileSet = new HashSet<string>();

            /* Add all strings in the passed do-not-compile set (iff it was specified) */
            if (ignoreSet != null)
                foreach (string file in ignoreSet)
                    doNotCompileSet.Add(file);

            /* Add the Musika file extension to the filename if not already present */
            if (!filename.EndsWith(Compiler.MUSIKA_FILE_EXT))
                filename += Compiler.MUSIKA_FILE_EXT;
            this.filename = filename;

            /* Add the current file to the do-not-compile set */
            doNotCompileSet.Add(filepath + filename);

            /* Initialize other instance variables */
            this.program = program;
            Reset();
        }

        /* / CONSTRUCTOR */

        /* HELPER METHODS */

        public void IgnoreContext() /* Sets parser to only check for syntax: this will NOT generate a NoteSheet instance */
        {
            ignoreContext = true;
            noteSheet     = null;
        }

        public void Reset() /* Restore the program and refresh/restart the lexical analyzer and notesheet */
        {
            lexer                   = new LexicalAnalyzer(program);
            noteSheet               = new NoteSheet();
            noteCountTracker        = new List<int>();
        }

        private Token Expect(TokenType etype) /* The passed TokenType must be read from the lexer or a syntax error is thrown (we EXPECT this token type next) */
        {
            Token next = lexer.GetToken();

            if (next.Type != etype)
                throw new SyntaxError(next.Type, etype);

            return next;
        }

        private void ConsumeNewlines() /* Consumes NEWLINE*: 0 or more newlines */
        {
            Token next = lexer.GetToken();

            while (next.Type == TokenType.NEWLINE)
                next = lexer.GetToken();

            lexer.PutToken(next);
        }

        private void ClearNoteCountTracker() /* Set the note count tracker to 0 notes at layer 0 (default) */
        {
            noteCountTracker.Clear();
            noteCountTracker.Add(0);
        }

        private void AddNoteCountTrackerLayer() /* Add a new layer to the note */
        {
            noteCountTracker.Add(0);
        }

        private void RemoveNoteCountTrackerLayer() /* Remove the topmost layer of the note tracker */
        {
            noteCountTracker.RemoveAt(noteCountTracker.Count - 1);
        }

        private void IncrementNoteCountTracker() /* Incremement the value of the topmost layer of the note tracker */
        {
            ++noteCountTracker[noteCountTracker.Count - 1];
        }

        private int GetNoteCountTrackerNoteCount() /* Get the total number of notes from the note tracker */
        {
            return noteCountTracker.Sum();
        }

        /* / HELPER METHODS */

        /* GRAMMAR PARSER METHODS */

        public NoteSheet ParseScore(bool reset = true) /* score -> NEWLINE* accompaniment BREAK NEWLINE* sheet NEWLINE* | NEWLINE* sheet NEWLINE* */
                                                       /* This is the start symbol                                                                */
        {
            /* Unless explicitly told to not do this, restart parsing from the beginning of the file */
            if (reset)
                Reset();

            /* Consume any newlines at the start of the program */
            ConsumeNewlines();

            /* Parse an accompaniment section iff there is one */
            Token next = lexer.PeekToken();
            if (next.Type == TokenType.ACCOMPANY)
            {
                ParseAccompaniment();
                Expect(TokenType.BREAK);
                ConsumeNewlines();
            }

            /* Parse the next rule */
            ParseSheet();

            /* Consume any ending newline characters */
            ConsumeNewlines();

            /* Return the immediate representation */
            return noteSheet;
        }

        private void ParseAccompaniment() /* accompaniment -> accompany_statement NEWLINE* accompaniment | accompany_statement */
        {
            /* Parse the first rule and consume any newlines afterward */
            ParseAccompanyStatement();
            ConsumeNewlines();

            /* Parse the next accompany statement if there is one */
            Token next = lexer.PeekToken();
            if (next.Type == TokenType.ACCOMPANY)
                ParseAccompaniment();
        }

        private void ParseAccompanyStatement() /* accompany_statement -> ACCOMPANY L_BRACKET ID R_BRACKET NAME ID */
        {
            /* Constants */
            const string STDLIB_REFERENCE_INDICATOR = "__";
            /* / Cosntants */

            /* Local Variables */
            Token     fileNameToken;        /* Accompaniment file name token                                                    */
            Token     nameToken;            /* Reference name of the accompaniment token                                        */
            Parser    accParser;            /* Parses accompanied Musika file                                                   */
            NoteSheet accSheet;             /* Note sheet from accompanied file                                                 */
            string    accProgram;           /* Accompanied file program text                                                    */
            string    file;                 /* Accompanied file path + accompanied file name                                    */
            string    filename;             /* Accompanied file path                                                            */
            string    stdlibReferenceCheck; /* Captures first and last character in file name to check for std lib reference    */
            /* / Local Variables */

            /* Parse The Accompany Statement */
            Expect(TokenType.ACCOMPANY);
            Expect(TokenType.LBRACKET);

            fileNameToken = Expect(TokenType.ID);

            Expect(TokenType.RBRACKET);
            Expect(TokenType.NAME);

            nameToken = Expect(TokenType.ID);

            /* Check for standard library reference */
            stdlibReferenceCheck = null;

            if (fileNameToken.Content.Length > 1)
            {
                stdlibReferenceCheck = char.ToString(fileNameToken.Content[0]) + char.ToString(fileNameToken.Content[fileNameToken.Content.Length - 1]);
            }

            /* Construct the literal file (filepath + filename) */
            if (stdlibReferenceCheck == STDLIB_REFERENCE_INDICATOR)
            {
                filename = Path.ChangeExtension(fileNameToken.Content.Substring(1, fileNameToken.Content.Length - 2), Serializer.SERIALIZE_EXT); /* Remove stdlib indicators and look for binary file */
                file = Path.Combine(STDLIB_FILEPATH, filename);

                if (!ignoreContext)
                {
                    /* Load standard library binary */
                    if (File.Exists(file))
                    {
                        accSheet = Serializer.Deserialize(STDLIB_FILEPATH, filename);
                        noteSheet.Accompaniments.Add(nameToken.Content, accSheet);
                    }
                    else if (!ignoreContext)
                        throw new ContextError(ContextError.INVALID_FILENAME_ERROR);
                }
            }
            else
            {
                filename = Path.ChangeExtension(fileNameToken.Content, Compiler.MUSIKA_FILE_EXT);
                file = Path.Combine(filepath, filename);

                if (!ignoreContext)
                {
                    /* File is not in the do-not-compile list */
                    if (!doNotCompileSet.Contains(file))
                    {
                        /* Compile the referenced file and add its notesheet to the accompaniments dictionary */
                        if (File.Exists(file))
                        {
                            accProgram = File.ReadAllText(file);
                            accParser = new Parser(accProgram, filepath, filename, doNotCompileSet);
                            accSheet = accParser.ParseScore();

                            noteSheet.Accompaniments.Add(nameToken.Content, accSheet);
                        }
                        else if (!ignoreContext)
                            throw new ContextError(ContextError.INVALID_FILENAME_ERROR);
                    }

                    /* File is in the do-not-compile list */
                    else
                    {
                        /* Check for self-reference and throw self-reference error if there is one */
                        if (filename == this.filename)
                            throw new ContextError(ContextError.SELF_REFERENCE_ERROR);

                        /* Throw a cross-reference error */
                        else
                            throw new ContextError(ContextError.CROSS_REFERENCE_ERROR);
                    }
                }
            }
        }

        private void ParseSheet() /* sheet -> info NEWLINE* BREAK NEWLINE* patterns NEWLINE* BREAK NEWLINE* music NEWLINE* BREAK NEWLINE* */
        {
            /* Parse the info section (title, author, coauthor(s), key, time, tempo, and octave) */
            ParseInfo();
            ConsumeNewlines();
            Expect(TokenType.BREAK);
            ConsumeNewlines();

            /* Parse the pattern and chord definitions and store them in the NoteSheet's pattern and chord dictionaries */
            ParsePatterns();
            ConsumeNewlines();
            Expect(TokenType.BREAK);
            ConsumeNewlines();

            /* Changes to key, time, tempo, and octave will be temporary; so save the original values in buffers */
            int key                      = noteSheet.Key;
            TimeSignature time           = noteSheet.Time;
            float tempo                  = noteSheet.Tempo;
            int octave                   = noteSheet.Octave;
            Sheet noteList;

            /* Reset note count tracker */
            ClearNoteCountTracker();

            /* Generate a note sheet from the main music */
            noteList = ParseMusic(out _);

            /* The Parse Music function returns the main music representation */
            noteSheet.Sheet = noteList;

            /* Restore key, time, tempo, and octave info */
            noteSheet.Key    = key;
            noteSheet.Time   = time;
            noteSheet.Tempo  = tempo;
            noteSheet.Octave = octave;

            /* Parse the rest of the sheet */
            ConsumeNewlines();
            Expect(TokenType.BREAK);
            ConsumeNewlines();
        }

        private void ParseInfo() /* info -> title NEWLINE* author_define NEWLINE* music_info | title NEWLINE* music_info */
        {
            /* Parse the title section and consume any newlines afterward */
            ParseTitle();
            ConsumeNewlines();

            /* Parse author info iff present, then consume any newlines */
            Token next = lexer.PeekToken();
            if (next.Type == TokenType.AUTHOR || next.Type == TokenType.COAUTHORS)
            {
                ParseAuthorDefine();
                ConsumeNewlines();
            }
            else
                throw new SyntaxError(next.Type, TokenType.AUTHOR, TokenType.COAUTHORS);

            /* Parse the key, time, tempo, and octave */
            ParseMusicInfo();
        }

        private void ParseMusicInfo() /* music_info -> key NEWLINE* time NEWLINE* tempo NEWLINE* octave */
        {
            /* Parse the key signature definition and consume newlines */
            ParseKey(true);
            ConsumeNewlines();

            /* Parse the time signature definition and consume newlines */
            ParseTime(true);
            ConsumeNewlines();

            /* Parse the tempo definition and consume newlines */
            ParseTempo(true);
            ConsumeNewlines();

            /* Parse the octave definition and consume newlines */
            ParseOctave();
            ConsumeNewlines();
        }

        private void ParsePatterns() /* patterns -> pc_definition NEWLINE* patterns | pc_definition NEWLINE* | EPSILON */
        {
            /* Recursively parse 0 or more pattern or chord definitions */
            Token next = lexer.PeekToken();
            if (pcDefFirstSet.Contains(next.Type))
            {
                ParsePcDefinition();
                ConsumeNewlines();

                next = lexer.PeekToken();
                if (pcDefFirstSet.Contains(next.Type))
                    ParsePatterns();
            }
        }

        private Sheet ParseMusic(out PositionSheetMap layerDict, string patternName = null) /* music -> music_element NEWLINE* music | music_element | EPSILON */
        {
            /* Local Variables */
            List<PositionSheetPair> layerPositionSheetPairs = new List<PositionSheetPair>();    /* Position sheet pairs extracted out of a music element for use later  */
            Sheet returnValue                               = new Sheet();                      /* Main music return value                                              */
            Token next;                                                                         /* Next token                                                           */
            /* / Local Variables */

            /* Initialize the dictionary of layer sheets */
            layerDict = new PositionSheetMap();

            /* Parse each music element */
            next = lexer.PeekToken();
            while (musicFirstSet.Contains(next.Type))
            {
                /* Clear layer buffer list for new musical element */
                layerPositionSheetPairs.Clear();

                /* Parse the music element and store the Sheet instance and layer data it returns */
                Sheet elementList = ParseMusicElement(layerPositionSheetPairs, patternName: patternName);

                /* If any layers were present, add them to the layer dicitonary */
                layerDict.AddRange(layerPositionSheetPairs);

                /* If a list of notes was returned from parsing the music element, add each note to returned note sheet  */
                if (elementList != null)
                {
                    foreach (NoteSet element in elementList)
                    {
                        returnValue.Add(element);
                        IncrementNoteCountTracker();
                    }
                }

                /* Continue parsing */
                ConsumeNewlines();
                next = lexer.PeekToken();
            }

            /* Return list of notes */
            return returnValue;
        }

        private void ParseTitle() /* title -> TITLE COLON STRING NEWLINE | TITLE COLON ID NEWLINE */
        {
            /* Local Variables */
            NoteSheet referenceSheet;   /* Referenced accompaniment note sheet */
            Token     next;             /* Next token                          */
            string    idName;           /* Name of accompaniment               */
            /* / Local Variables */

            /* Parse keyword tokens */
            Expect(TokenType.TITLE);
            Expect(TokenType.COLON);

            /* Parse title value */
            next = lexer.GetToken();
            if (next.Type == TokenType.STRING || next.Type == TokenType.ID)
            {
                /* If the vlaue is a literal, set the title to the literal string */
                if (next.Type == TokenType.STRING)
                    noteSheet.Title = next.Content;

                /* If the value is a refrence, search for the referred value */
                else
                {
                    if (!ignoreContext)
                    {
                        /* Make sure the reference is a valid accompaniment name */
                        idName = next.Content;
                        if (!noteSheet.Accompaniments.ContainsKey(idName))
                            throw new ContextError(ContextError.INVALID_ACC_REFERENCE_ERROR);

                        /* Look up the referred value and set the title to it */
                        referenceSheet = noteSheet.Accompaniments[idName];
                        noteSheet.Title = referenceSheet.Title;
                    }
                }

                /* Finish parsing */
                Expect(TokenType.NEWLINE);
            }

            /* Neither a literal nor a reference was specified */
            else throw new SyntaxError(next.Type, TokenType.STRING, TokenType.ID);
        }

        private void ParseAuthorDefine() /* author_define -> author NEWLINE* coauthors | coauthors NEWLINE* author | author */
        {
            /* Parse the required author definition followed by the optional coauthor definition */
            Token next = lexer.PeekToken();
            if (next.Type == TokenType.AUTHOR)
            {
                ParseAuthor();
                ConsumeNewlines();

                next = lexer.PeekToken();
                if (next.Type == TokenType.COAUTHORS)
                    ParseCoauthors();
            }

            /* In this case, coauthors were defined first, so parse the required author definition afterwards */
            else if (next.Type == TokenType.COAUTHORS)
            {
                ParseCoauthors();
                ConsumeNewlines();
                ParseAuthor();
            }
        }

        private void ParseAuthor() /* author -> AUTHOR COLON STRING NEWLINE | AUTHOR COLON ID NEWLINE */
        {
            /* Local Variables */
            NoteSheet referenceSheet;   /* Referenced accompaniment note sheet */
            Token     next;             /* Next token                          */
            string    idName;           /* Name of accompaniment               */
            /* / Local Variables */

            /* Parse keyword tokens */
            Expect(TokenType.AUTHOR);
            Expect(TokenType.COLON);

            /* Parse author value */
            next = lexer.GetToken();
            if (next.Type == TokenType.STRING || next.Type == TokenType.ID)
            {
                /* If the vlaue is a literal, set the author to the literal string */
                if (next.Type == TokenType.STRING)
                    noteSheet.Author = next.Content;

                /* If the value is a refrence, search for the referred value */
                else
                {
                    if (!ignoreContext)
                    {
                        /* Make sure the reference is a valid accompaniment name */
                        idName = next.Content;
                        if (!noteSheet.Accompaniments.ContainsKey(idName))
                            throw new ContextError(ContextError.INVALID_ACC_REFERENCE_ERROR);

                        /* Look up the referred value and set the author to it */
                        referenceSheet = noteSheet.Accompaniments[idName];
                        noteSheet.Author = referenceSheet.Author;
                    }
                }

                /* Finish parsing */
                Expect(TokenType.NEWLINE);
            }

            /* Neither a literal nor a reference was specified */
            else throw new SyntaxError(next.Type, TokenType.STRING, TokenType.ID);
        }

        private void ParseCoauthors() /* coauthors -> COAUTHORS COLON STRING NEWLINE | AUTHOR COLON ID NEWLINE */
        {
            /* Local Variables */
            NoteSheet referenceSheet;   /* Referenced accompaniment note sheet */
            Token     next;             /* Next token                          */
            string    idName;           /* Name of accompaniment               */
            /* / Local Variables */

            /* Parse keyword tokens */
            Expect(TokenType.COAUTHORS);
            Expect(TokenType.COLON);

            /* Parse coauthor(s) value */
            next = lexer.GetToken();
            if (next.Type == TokenType.STRING || next.Type == TokenType.ID)
            {
                /* If the vlaue is a literal, set the coauthors to the literal string, each new coauthor separated by comma-space */
                if (next.Type == TokenType.STRING)
                    noteSheet.Coauthors = next.Content.Split(new string[] { ", " }, StringSplitOptions.None);

                /* If the value is a refrence, search for the referred value */
                else
                {
                    if (!ignoreContext)
                    {
                        /* Make sure the reference is a valid accompaniment name */
                        idName = next.Content;
                        if (!noteSheet.Accompaniments.ContainsKey(idName))
                            throw new ContextError(ContextError.INVALID_ACC_REFERENCE_ERROR);

                        /* Look up the referred value and set the coauthors to it */
                        referenceSheet = noteSheet.Accompaniments[idName];
                        noteSheet.Coauthors = referenceSheet.Coauthors;
                    }
                }

                /* Finish parsing */
                Expect(TokenType.NEWLINE);
            }

            /* Neither a literal nor a reference was specified */
            else throw new SyntaxError(next.Type, TokenType.STRING, TokenType.ID);
        }

        private void ParseKey(bool endWithNewline) /* key -> KEY COLON SIGN NEWLINE | KEY COLON ID NEWLINE */
        {
            /* Local Variables */
            NoteSheet referenceSheet;   /* Referenced accompaniment note sheet */
            Token     next;             /* Next token                          */
            string    idName;           /* Name of accompaniment               */
            /* / Local Variables */

            /* Parse keyword tokens */
            Expect(TokenType.KEY);
            Expect(TokenType.COLON);

            /* Parse key value */
            next = lexer.GetToken();
            if (next.Type == TokenType.SIGN || next.Type == TokenType.ID)
            {
                /* If the vlaue is a literal, set the key to the literal value */
                if (next.Type == TokenType.SIGN && !ignoreContext)
                    if (noteSheet.KeySignuatureExists(next.Content))
                        noteSheet.Key = NoteSheet.KeyConversion[next.Content];
                    else
                        throw new ContextError(ContextError.KEY_ERROR);

                /* If the value is a refrence, search for the referred value */
                else
                {
                    if (!ignoreContext)
                    {
                        /* Make sure the reference is a valid accompaniment name */
                        idName = next.Content;
                        if (!noteSheet.Accompaniments.ContainsKey(idName))
                            throw new ContextError(ContextError.INVALID_ACC_REFERENCE_ERROR);

                        /* Look up the referred value and set the key to it */
                        referenceSheet = noteSheet.Accompaniments[idName];
                        noteSheet.Key = referenceSheet.Key;
                    }
                }

                /* Finish parsing */
                if (endWithNewline)
                    Expect(TokenType.NEWLINE);
            }

            /* Neither a literal nor a reference was specified */
            else throw new SyntaxError(next.Type, TokenType.SIGN, TokenType.ID);
        }

        private void ParseTime(bool endWithNewline) /* time -> TIME COLON (KeyWord2) NEWLINE | TIME COLON ID NEWLINE | TIME COLON NUMBER SLASH NUMBER NEWLINE | TIME COLON NUMBER NEWLINE */
        {
            /* Local Variables */
            NoteSheet referenceSheet;       /* Referenced accompaniment note sheet  */
            Token     next;                 /* Next token                           */
            int       firstNumber;          /* First number in command              */
            int       originalBaseNote;     /* Current TS base note                 */
            int       secondNumber;         /* Second number in command             */
            float     baseNoteRatio;        /* Magnitude change in base note        */
            string    idName;               /* Name of accompaniment reference      */
            /* / Local Variables */

            /* Parse keyword tokens */
            Expect(TokenType.TIME);
            Expect(TokenType.COLON);

            /* Store the original time signature in a temp variable to use later */
            originalBaseNote = noteSheet.Time.baseNote;

            /* Value is numeric */
            next = lexer.GetToken();
            if (next.Type == TokenType.NUMBER)
            {
                firstNumber = int.Parse(next.Content);

                next = lexer.GetToken();

                /* If the value is a fraction, set the base note to the numerator and the beats per measure to the denominator */
                if (next.Type == TokenType.SLASH) /* NUMBER SLASH NUMBER */
                {
                    next = Expect(TokenType.NUMBER);
                    secondNumber = int.Parse(next.Content);

                    noteSheet.Time = new TimeSignature { baseNote = secondNumber, beatsPerMeasure = firstNumber };
                }

                /* If the value is a single integer, set the base note to the specified value and adjust the beats per measure accordingly */
                else
                {
                    /* Check that the time signature was initialized (there must be an initial value before you can modify it) */
                    if (noteSheet.Time.baseNote == 0 && noteSheet.Time.beatsPerMeasure == 0 && !ignoreContext)
                        throw new ContextError(ContextError.TIME_ERROR);
                    else
                    {
                        /* Check that the time was not set to 0 (because that would not make much sense) */
                        if (firstNumber == 0)
                            throw new ContextError(ContextError.ZERO_TIME_ERROR);

                        /* Increase/Decrease the beats per measure by the same order of magnitude (so that the true time signature remains the same */
                        /* Example: if the original time signature is 4 / 4, and the base note is set to 8, the new time signature is 8 / 8         */
                        baseNoteRatio = (float)firstNumber / (float)noteSheet.Time.baseNote;
                        noteSheet.Time.baseNote = firstNumber;
                        noteSheet.Time.beatsPerMeasure *= baseNoteRatio;
                    }

                    lexer.PutToken(next); /* NUMBER */
                }
            }

            /* If the value specified is a keyword (a standard time signature), find the time signature values from the keyword and store the new time */
            else if (TokenTypeFactory.Tier2Keywords.Contains(next.Type))
            {
                if (NoteSheet.TimeSignatureDict.ContainsKey(next.Type))
                    noteSheet.Time = NoteSheet.TimeSignatureDict[next.Type];

                /* This should never happend because the token is already checked to be a Tier 2 keyword and all Tier 2 keywords should be keys in the */
                /* time signature dictionary (NoteSheet class)                                                                                         */
                else
                    throw new ContextError(ContextError.TIER_2_DICT_ERROR); /* This should never happen */
            }

            /* If the value is a refrence, search for the referred value */
            else if (next.Type == TokenType.ID)
            {
                if (!ignoreContext)
                {
                    /* Make sure the reference is a valid accompaniment name */
                    idName = next.Content;
                    if (!noteSheet.Accompaniments.ContainsKey(idName))
                        throw new ContextError(ContextError.INVALID_ACC_REFERENCE_ERROR);

                    /* Look up the referred value and set the time to it */
                    referenceSheet = noteSheet.Accompaniments[idName];
                    noteSheet.Time = referenceSheet.Time;
                }
            }

            /* Invalid token */
            else throw new SyntaxError(next.Type, TokenType.NUMBER, TokenType.COMMON, TokenType.CUT, TokenType.ID);

            /* Continue parsing */
            if (endWithNewline)
                Expect(TokenType.NEWLINE);

            /* Update the tempo if the base note changed. This is because each beat total length will change because the base note is different */
            /* For example, if the original base note was 4 and now it is 8, divide the tempo by 2 because each 8th note is half the time of a  */
            /* quarter note                                                                                                                     */
            if (noteSheet.Time.baseNote != originalBaseNote)
                noteSheet.Tempo *= (float)originalBaseNote / (float)noteSheet.Time.baseNote;
        }

        private void ParseTempo(bool endWithNewline) /* tempo -> TEMPO COLON NUMBER EQUAL NUMBER NEWLINE  | TEMPO COLON NUMBER  | TEMPO COLON ID NEWLINE */
        {
            /* Constants */
            const float SECONDS_PER_MINUTE = 60f;   /* Second-Minute conversion */
            /* / Constants */

            /* Local Variables */
            NoteSheet referenceSheet;               /* Referenced accompaniment note sheet      */
            Token     next;                         /* Next token                               */
            float     baseNoteRatio;                /* Magnitude change in base note            */
            float     firstNumber;                  /* First number in command                  */
            float     secondNumber;                 /* Second number in command                 */
            float     secondsPerBeat;               /* Seconds per base beat (second number)    */
            string    idName;                       /* Name of accompaniment reference          */
            /* / Local Variables */

            /* Parse keyword tokens */
            Expect(TokenType.TEMPO);
            Expect(TokenType.COLON);

            /* Value is numeric (literal) */
            next = lexer.GetToken();
            if (next.Type == TokenType.NUMBER)
            {
                firstNumber = int.Parse(next.Content);
                next = lexer.GetToken();

                /* Two integers specified, so use these values to calculate how many seconds each base note of the time signature would take */
                if (next.Type == TokenType.EQUAL) /* NUMBER EQUAL NUMBER */
                {
                    next = Expect(TokenType.NUMBER);
                    secondNumber = int.Parse(next.Content);

                    /* first number: tempo's base note; second number: beats per minute */
                    baseNoteRatio  = firstNumber / noteSheet.Time.baseNote;
                    secondsPerBeat = SECONDS_PER_MINUTE / secondNumber;

                    /* Multiply the seconds per beat by the ratio of the base notes to get the seconds per beat in the time signature's base note */
                    noteSheet.Tempo = baseNoteRatio * secondsPerBeat;
                }

                /* One integer specified, so just multiply/divide the current tempo by this coefficient */
                else
                {
                    if (firstNumber >= 0)
                        noteSheet.Tempo *= firstNumber;
                    else /* If the number is negative, multiply by the negative reciprocal (Example: -2 becomes 1/2) */
                        noteSheet.Tempo /= -firstNumber; /* if the number is negative, it slows down time so divide by absolute value */

                    lexer.PutToken(next); /* NUMBER */
                }
            }

            /* If the value is a refrence, search for the referred value */
            else if (next.Type == TokenType.ID)
            {
                if (!ignoreContext)
                {
                    /* Make sure the reference is a valid accompaniment name */
                    idName = next.Content;
                    if (!noteSheet.Accompaniments.ContainsKey(idName)) /* Make sure accompaniment exists */
                        throw new ContextError(ContextError.INVALID_ACC_REFERENCE_ERROR);

                    /* Look up the referred value and set the tempo to it */
                    referenceSheet = noteSheet.Accompaniments[idName];
                    noteSheet.Tempo = referenceSheet.Tempo;

                    if (noteSheet.Time.baseNote != referenceSheet.Time.baseNote) /* Adjust tempo for accompaniment sheet's different time signature */
                        noteSheet.Tempo *= referenceSheet.Time.baseNote / noteSheet.Time.baseNote;
                }
            }

            else throw new SyntaxError(next.Type, TokenType.NUMBER, TokenType.ID);

            /* Continue parsing */
            if (endWithNewline)
                Expect(TokenType.NEWLINE);
        }

        private void ParseOctave() /* octave -> OCTAVE COLON NUMBER NEWLINE | OCTAVE COLON PLUS NUMBER NEWLINE | OCTAVE COLON ID NEWLINE */
        {
            /* Local Variables */
            NoteSheet referenceSheet;   /* Referenced accompaniment note sheet                                           */
            Token     next;             /* Next token                                                                    */
            int       newOctave;        /* Numerical value of new main octave                                            */
            string    idName;           /* Name of accompaniment reference                                               */
            /* / Local Variables */

            /* Parse keyword tokens */
            Expect(TokenType.OCTAVE);
            Expect(TokenType.COLON);

            /* Check the next token */
            next = lexer.GetToken();

            /* Value is a plus sign */
            if (next.Type == TokenType.PLUS)
            {
                /* Check that the next value is a number */
                next = lexer.GetToken();

                if (next.Type == TokenType.NUMBER)
                {
                    /* Get the number value */
                    newOctave = int.Parse(next.Content);

                    /* Increment current octave by the specified value */
                    noteSheet.Octave += newOctave;

                    /* Throw an error if the new octave is below 0 */
                    if (noteSheet.Octave < 0 && !ignoreContext)
                        throw new ContextError(ContextError.OCTAVE_ERROR);
                }

                /* Plus was followed by a non-number token: throw a syntax error */
                else
                    throw new SyntaxError(next.Type, TokenType.NUMBER);
            }

            /* Value is an integer */
            else if (next.Type == TokenType.NUMBER)
            {
                newOctave = int.Parse(next.Content);

                if (newOctave >= 0) /* Value is positive, so set the octave to this value */
                    noteSheet.Octave = newOctave;
                else if (newOctave < 0) /* Value is negative or has a plus sign, so add the current octave by the specified value  */
                    noteSheet.Octave += newOctave;

                if (noteSheet.Octave < 0 && !ignoreContext)
                    throw new ContextError(ContextError.OCTAVE_ERROR);
            }

            /* If the value is a refrence, search for the referred value */
            else if (next.Type == TokenType.ID)
            {
                if (!ignoreContext)
                {
                    /* Make sure the reference is a valid accompaniment name */
                    idName = next.Content;
                    if (!noteSheet.Accompaniments.ContainsKey(idName))
                        throw new ContextError(ContextError.INVALID_ACC_REFERENCE_ERROR);

                    /* Look up the referred value and set the octave to it */
                    referenceSheet = noteSheet.Accompaniments[idName];
                    noteSheet.Octave = referenceSheet.Octave;
                }
            }

            else throw new SyntaxError(next.Type, TokenType.NUMBER, TokenType.ID);
        }

        private void ParsePcDefinition() /* pc_definition -> PATTERN L_BRACKET ID R_BRACKET COLON NEWLINE NEWLINE* music | CHORD ID IS chord_type */
        {
            /* Local Variables */
            Token            nameTok;       /* Token containing the pattern/chord name      */
            Token            next;          /* Next token                                   */
            NoteSet          noteList;      /* Note set returned by ParseChordType          */
            Sheet            music;         /* Music sheet returned by ParseMusic           */
            TimeSignature    timeBuffer;    /* Original time signature buffer               */
            int              keyBuffer;     /* Original key signature buffer                */
            int              octaveBuffer;  /* Original octave buffer                       */
            string           chordName;     /* Name of specified chord                      */
            string           patternName;   /* Name of specified pattern                    */
            float            tempoBuffer;   /* Original tempo buffer                        */
            /* / Local Variables */

            /* Parse a pattern defintition */
            next = lexer.GetToken();
            if (next.Type == TokenType.PATTERN)
            {
                /* Parse definition */
                Expect(TokenType.LBRACKET);

                nameTok = Expect(TokenType.ID);

                Expect(TokenType.RBRACKET);
                Expect(TokenType.COLON);
                Expect(TokenType.NEWLINE);
                ConsumeNewlines();

                /* Changes to key, time, tempo, and octave will be temporary; so save the original values in buffers */
                keyBuffer       = noteSheet.Key;
                timeBuffer      = noteSheet.Time;
                tempoBuffer     = noteSheet.Tempo;
                octaveBuffer    = noteSheet.Octave;
                patternName     = nameTok.Content;

                /* Reset note count tracker */
                ClearNoteCountTracker();

                /* Parse Body */
                music = ParseMusic(out _, patternName: patternName);

                /* Make sure the pattern name does not exist */
                if (noteSheet.Patterns.ContainsKey(patternName))
                {
                    throw new ContextError(ContextError.DUPLICATE_NAME_ERROR);
                }

                /* Add the generated sheet to the list of patterns */
                noteSheet.Patterns.Add(patternName, music);

                /* Restore info */
                noteSheet.Key    = keyBuffer;
                noteSheet.Time   = timeBuffer;
                noteSheet.Tempo  = tempoBuffer;
                noteSheet.Octave = octaveBuffer;
            }

            /* Parse a chord definition */
            else if (next.Type == TokenType.CHORD)
            {
                /* Get the chord name */
                nameTok   = Expect(TokenType.ID);
                chordName = nameTok.Content;

                Expect(TokenType.IS);

                /* Parse the chord value */
                noteList = ParseChordType();

                if (!noteSheet.Chords.ContainsKey(chordName))
                {
                    /* Store the name-list pair in the chords dictionary */
                    noteSheet.Chords.Add(chordName, noteList);
                }
                else
                {
                    throw new ContextError(ContextError.DUPLICATE_NAME_ERROR);
                }
            }

            else throw new SyntaxError(next.Type, TokenType.PATTERN, TokenType.CHORD);
        }

        private NoteSet ParseChordType() /* chord_type -> NOTE | NOTE octave_change | NOTE SEMICOLON chord_type | NOTE octave_change SEMICOLON chord_type */
        {
            /* Local Variables */
            Token    next;          /* Next token                                           */
            Token    note;          /* Token containing a specified note                    */
            NoteSet  returnValue;   /* Chord to be returned                                 */
            bool     endOfChord;    /* Marked true if the end of the chord has been reached */
            float    frequency;     /* Note frequency                                       */
            int      octave;        /* Specified octave                                     */
            string   noteName;      /* String name of specified note                        */
            /* / Local Variables */

            /* Parse through the chord */
            returnValue   = new NoteSet();
            endOfChord = false;

            while (!endOfChord)
            {
                /* Store the current note's name and octave (there must be at least one note in the chord) */
                note = Expect(TokenType.NOTE);
                noteName = note.Content;
                octave = noteSheet.Octave;

                /* Offset octave if there is a change */
                octave += ParseOctaveChange();

                /* Find the frequency of the specified note at the given octave and add it to the note list */
                frequency = noteSheet.GetFrequency(noteName, octave);
                returnValue.Add(new Note { note = noteName, frequency = frequency, length = 0 });

                /* Stop parsing if there is no semicolon next (i.e. the last note in the chord was parsed) */
                next = lexer.GetToken();
                if (next.Type != TokenType.SEMICOLON)
                {
                    endOfChord = true;
                    lexer.PutToken(next);
                }
            }

            return returnValue;
        }

        private Sheet ParseMusicElement(List<PositionSheetPair> layerPositionSheetPairs, string patternName = null) /* music_element -> function | riff */
        {
            Token next = lexer.PeekToken();

            /* Parse function if the next token is in the function first set */
            if (TokenTypeFactory.Tier3Keywords.Contains(next.Type)) /* This is also the first set of the function grammar rule */
                return ParseFunction(layerPositionSheetPairs, patternName: patternName);

            /* Parse riff if the next token is in the riff first set */
            else if (riffFirstSet.Contains(next.Type))
                return ParseRiff(layerPositionSheetPairs);

            /* If the next token was not in the function or riff first set, throw a syntax error */
            else throw new SyntaxError(next.Type, TokenType.REPEAT, TokenType.LAYER, TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG);
        }

        private Sheet ParseFunction(List<PositionSheetPair> layerPositionSheetPairs, string patternName = null) /* function -> repeat | layer */
        {
            Token next = lexer.PeekToken();

            /* Parse the repeat rule if the next token is the "repeat" keyword (repeat rule first set) */
            if (next.Type == TokenType.REPEAT)
                return ParseRepeat();

            /* Parse the layer rule if the next token is the "layer" keyword (layer rule first set) */
            else if (next.Type == TokenType.LAYER)
            {
                /* Parse the layer and save both the music and its relative position in the song */
                PositionSheetPair layerReturn = ParseLayer(patternName: patternName);
                layerPositionSheetPairs.Add(layerReturn);

                return null; /* A layer does not produce any note sheets for the main sheet */
            }

            else throw new SyntaxError(next.Type, TokenType.REPEAT, TokenType.LAYER);
        }

        private Sheet ParseRepeat() /* repeat -> REPEAT L_PAREN NUMBER R_PAREN L_BRACE music R_BRACE */
        {
            /* Local Variables */
            Sheet              returnValue;     /* Sheet music to be returned (music the repeat number of times)    */
            Sheet              music;           /* Sheet music returend from ParseMusic                             */
            SheetSet newLayerList;    /* List of newly defined layers in repeat                           */
            Token              numberTok;       /* Token for number of repetitions                                  */
            int                i;               /* Repeat loop index                                                */
            int                position;        /* Specified layer position                                         */
            int                repeatNum;       /* Specified number of repeats                                      */
            /* / Local Variables */

            /* Parse repeat declaration */
            Expect(TokenType.REPEAT);
            Expect(TokenType.LPAREN);

            numberTok = Expect(TokenType.NUMBER);

            Expect(TokenType.RPAREN);

            ConsumeNewlines();
            Expect(TokenType.LBRACE);

            ConsumeNewlines();

            /* Initialize list and store the specified repeat number */
            returnValue = new Sheet();
            repeatNum = int.Parse(numberTok.Content);

            /* 0 allows someone to completely block off a section of music (effectively commenting it out) */
            if (repeatNum == 0)
                return null;
            else if (repeatNum < 0)
                throw new ContextError(ContextError.INVALID_REPEAT_NUM_ERROR);

            /* Add a layer to the note count tracker to count only the notes in the repeat section */
            AddNoteCountTrackerLayer();

            /* Parse the music in the repeat block */
            music = ParseMusic(out PositionSheetMap layerReference);

            /* Remove new note count layer */
            RemoveNoteCountTrackerLayer();

            /* Add the layers to the layer dictionary */
            foreach (PositionSheetPair pair in layerReference)
            {
                for (i = 1; i < repeatNum; ++i) /* Skip the 0th position because that was already done in the ParseLayer call of ParseMusic */
                {
                    /* The next position is equal to relative position in the repeat block + the number of notes that have already been added */
                    position = pair.Key + (music.Count * i);

                    /* Add the layer to the layer dictionary */
                    if (noteSheet.Layers.ContainsKey(position))
                        noteSheet.Layers[position].Add(pair.Value);
                    else
                    {
                        newLayerList = new SheetSet
                        {
                            pair.Value
                        };
                        noteSheet.Layers.Add(position, newLayerList);
                    }
                }
            }

            /* Add the music sheet to the return value X times whereas X is the repeat number */
            for (i = 0; i < repeatNum; ++i)
                returnValue.AddRange(music);

            /* Finish parsing and return list */
            ConsumeNewlines();
            Expect(TokenType.RBRACE);

            return returnValue;
        }

        private PositionSheetPair ParseLayer(string patternName = null) /* layer -> LAYER L_PAREN callback R_PAREN */
        {
            /* Local Variables */
            CallbackType      idPatternRef;     /* Type of id callback                                                      */
            PositionSheetPair returnValue;      /* Layer as position-sheet music pair to return                             */
            Sheet             pattern;          /* Layered music                                                            */
            SheetSet          newLayerList;     /* Layer list to set as the layer list if not already initialized           */
            int               position;         /* Position of layered music in song                                        */
            int               absolutePosition; /* Position of a layer call in the invoked pattern                          */
            /* / Local Variables */

            /* Parse declaration */
            Expect(TokenType.LAYER);
            Expect(TokenType.LPAREN);

            if (!ignoreContext)
            {
                /* Parse the reference, store the name, and store the name of the accompaniment if there is one */
                idPatternRef = ParseCallback(out string accname, out string name);

                /* Get the pattern based off of the reference */
                switch (idPatternRef)
                {
                    case CallbackType.ACCOMPANIMENT_SHEET:
                        pattern = noteSheet.Accompaniments[name].Sheet;
                        break;
                    case CallbackType.ACCOMPANIMENT_PATTERN:
                        pattern = noteSheet.Accompaniments[accname].Patterns[name];
                        break;
                    case CallbackType.LOCAL_PATTERN:
                        pattern = noteSheet.Patterns[name];
                        break;

                    /* Cannot use a chord or invalid callback for a layer */
                    case CallbackType.INVALID_CALLBACK:
                    case CallbackType.ACCOMPANIMENT_CHORD:
                    case CallbackType.LOCAL_CHORD:
                        throw new ContextError(ContextError.LAYER_NO_PATTERN_ERROR);

                    /* THIS SHOULD NEVER HAPPEN SINCE ALL CALLBACK TYPES SHOULD BE COVERED */
                    default:
                        throw new ContextError(ContextError.NULL_PATTERN_ERROR);
                }

                /* Set the position to the current note count so that the layer begins when that many notes have passed */
                position = GetNoteCountTrackerNoteCount();

                /* If parsing a pattern, add layer to relative position map */
                if (patternName != null)
                {
                    if (noteSheet.Layers.ContainsKey(position))
                    {
                        noteSheet.Layers[position].Add(pattern);
                    }
                    else
                    {
                        newLayerList = new SheetSet
                        {
                            pattern
                        };

                        noteSheet.Layers.Add(position, newLayerList);
                    }

                    if (noteSheet.RelativeLayerPositions.ContainsKey(patternName))
                    {
                        noteSheet.RelativeLayerPositions[patternName].Add(new PositionSheetPair(position, pattern));
                    }
                    else
                    {
                        noteSheet.RelativeLayerPositions.Add(patternName, new PositionSheetMap()
                        {
                            new PositionSheetPair(position, pattern)
                        });
                    }

                    /* Add any relative patterns from the layered pattern (nested layer call) */
                    switch (idPatternRef)
                    {
                        case CallbackType.ACCOMPANIMENT_SHEET:
                            foreach (KeyValuePair<int, SheetSet> posSheetSetPair in noteSheet.Accompaniments[name].Layers)
                            {
                                absolutePosition = position + posSheetSetPair.Key;

                                foreach (Sheet sheet in posSheetSetPair.Value)
                                {
                                    noteSheet.RelativeLayerPositions[patternName].Add(new PositionSheetPair(absolutePosition, sheet));
                                }
                            }
                            break;

                        case CallbackType.ACCOMPANIMENT_PATTERN:
                            if (noteSheet.Accompaniments[accname].RelativeLayerPositions.ContainsKey(name))
                            {
                                foreach (PositionSheetPair posSheetPair in noteSheet.RelativeLayerPositions[name])
                                {
                                    absolutePosition = position + posSheetPair.Key;
                                    noteSheet.RelativeLayerPositions[patternName].Add(new PositionSheetPair(absolutePosition, posSheetPair.Value));
                                }
                            }
                            break;

                        case CallbackType.LOCAL_PATTERN:
                            if (noteSheet.RelativeLayerPositions.ContainsKey(name))
                            {
                                foreach (PositionSheetPair posSheetPair in noteSheet.RelativeLayerPositions[name])
                                {
                                    absolutePosition = position + posSheetPair.Key;
                                    noteSheet.RelativeLayerPositions[patternName].Add(new PositionSheetPair(absolutePosition, posSheetPair.Value));
                                }
                            }
                            break;
                    }

                    /* Do not return an explicit value: return empty pair */
                    returnValue = new PositionSheetPair(-1, null);
                }
                else
                {
                    /* Add the pattern to the layer list at the stored position */
                    if (noteSheet.Layers.ContainsKey(position))
                        noteSheet.Layers[position].Add(pattern);
                    else
                    {
                        newLayerList = new SheetSet
                        {
                            pattern
                        };
                        noteSheet.Layers.Add(position, newLayerList);
                    }

                    /* Construct the position-pattern pair from the calculated position and received pattern */
                    returnValue = new PositionSheetPair(position, pattern);
                }
            }

            /* If we are ignoring context, then all we need to do is parse the rest of the layer call and return an empty pair */
            else
            {
                ParseCallback(out _, out _);
                returnValue = new PositionSheetPair(-1, null);
            }

            /* Finish parsing and return */
            Expect(TokenType.RPAREN);
            return returnValue;
        }

        private Sheet ParseRiff(List<PositionSheetPair> layerPositionSheetPairs) /* riff -> riff_element NEWLINE* riff  | riff_element NEWLINE* */
        {
            /* Local Variables */
            Sheet returnValue; /* Sheet music riff to be returned   */
            Token next;        /* Next token                        */
            int   i;           /* Repeat last not loop index        */
            /* / Local Variables */

            /* Initialize the return list and note buffer */
            returnValue = new Sheet();

            /* Parse each riff element */
            next = lexer.PeekToken();
            while (riffElementFirstSet.Contains(next.Type))
            {
                /* Parse and retrieve a list of notes/chords OR a number of times to repeat last note */
                dynamic elementReturn = ParseRiffElement(layerPositionSheetPairs, returnValue.Count);

                /* If value returned was a list, add all notes/chords to the return list */
                if (elementReturn is Sheet)
                    returnValue.AddRange(elementReturn);

                /* If the return value was an integer, repeat the last note in the return list the specified number of times */
                else if (elementReturn is int)
                {
                    /* Make sure a last note exists */
                    if (returnValue.Count == 0 && !ignoreContext)
                        throw new ContextError(ContextError.REPEAT_NOTHING_ERROR);

                    else if (!ignoreContext)
                        for (i = 0; i < elementReturn - 1; ++i) /* subtract 1 because it is already in the list once */
                            returnValue.Add(returnValue[returnValue.Count - 1]);
                }

                /* Continue parsing */
                ConsumeNewlines();
                next = lexer.PeekToken();
            }

            /* return the list of notes/chords */
            return returnValue;
        }

        private dynamic ParseRiffElement(List<PositionSheetPair> layerPositionSheetPairs, int currentNoteCount)     /* riff_element -> NOTE dot_set | NOTE octave_change dot_set | callback | callback dot_set */
                                                                                                                    /* | CARROT NUMBER | BANG key \ NEWLINE BANG | BANG time \ NEWLINE BANG                    */
        {
            /* Constants */
            const bool NO_END_NEWLINE = false; /* Do not expect a newline */
            /* / Constants */

            /* Local Variables */
            CallbackType idRefPattern;              /* Type of id callback                                                  */
            NoteSet      chordCopy;                 /* Copy of the referenced chord (to add if element is chord)            */
            NoteSet      chordRef;                  /* Chord referenced by id                                               */
            NoteSet      singleNoteList;            /* Element to add if element is a single note                           */
            SheetSet     newLayerList;              /* Set of layers at a newly-specified position if needed                */
            Token        next;                      /* Next token                                                           */
            Token        numberToken;               /* Token containing shorthand repeat number                             */
            int          i;                         /* Chord copy loop index                                                */
            int          numDots;                   /* Duration of note/chord in dots (base rhythms)                        */
            int          octave;                    /* Specified octave                                                     */
            int          layerAbsolutePosition;     /* Absolute position of layer if it was called in a pattern reference   */
            string       noteName;                  /* Name of specified note                                               */
            float        duration;                  /* Duration of note/chord in seconds                                    */
            float        frequency;                 /* Frequency of specified note                                          */
            dynamic      returnValue;               /* Return value (either Sheet or int type)                              */
            /* / Local Variables */

            /* Assume return value is a list and initialize (since most paths return a list) */
            returnValue = new Sheet();

            next = lexer.GetToken();

            /* If the element is a note, get the note information and add to the list */
            if (next.Type == TokenType.NOTE)
            {
                /* Store the initial information */
                noteName = next.Content;
                octave   = noteSheet.Octave;

                /* Offset the octave if specified */
                octave += ParseOctaveChange();

                /* Find the number of basenotes to hold (number of dots). This determines the length of the note */
                numDots = ParseDotSet();

                /* With updated information, find the note frequency and duration */
                frequency = noteSheet.GetFrequency(noteName, octave);
                duration  = noteSheet.Tempo * numDots;

                /* Store the note information into a note instance and store in the return value as a single note */
                singleNoteList = new NoteSet
                {
                    new Note { note = noteName, frequency = frequency, length = duration } /* stored in a list by itself since it is not a chord */
                };
                returnValue.Add(singleNoteList);
            }

            /* If the element is a reference, add the chord, pattern, or sheet to the return value */
            else if (next.Type == TokenType.ID)
            {
                /* Parse the callback and store its type and names */
                lexer.PutToken(next);
                idRefPattern = ParseCallback(out string accName, out string idName);
                next = lexer.PeekToken();

                if (ignoreContext) /* Should only be used for testing and should never be run for release */
                {
                    if (next.Type == TokenType.DOT)
                        _ = ParseDotSet();
                }
                else /* Should always run for release */
                {
                    /* If the ID refers to a chord, store the chord in the return value */
                    if (idRefPattern == CallbackType.LOCAL_CHORD || idRefPattern == CallbackType.ACCOMPANIMENT_CHORD)
                    {
                        /* Find the length of the chord */
                        numDots = ParseDotSet();
                        duration = noteSheet.Tempo * numDots;

                        /* Find the chord reference from the notesheet */
                        if (idRefPattern == CallbackType.LOCAL_CHORD)
                            chordRef = noteSheet.Chords[idName];
                        else
                            chordRef = noteSheet.Accompaniments[accName].Chords[idName];

                        /* Create a copy of the chord reference, settting the length of each note to the calculated length */
                        chordCopy = new NoteSet();
                        chordCopy.AddRange(chordRef);
                        for (i = 0; i < chordCopy.Count; ++i)
                            chordCopy[i] = new Note { note = chordCopy[i].note, frequency = chordCopy[i].frequency, length = duration }; ;

                        /* Add chord to return value */
                        returnValue.Add(chordCopy);
                    }

                    /* If the ID refers to a pattern, add the pattern sheet to the return value */
                    else
                    {
                        switch (idRefPattern)
                        {
                            case CallbackType.LOCAL_PATTERN:
                                returnValue.AddRange(noteSheet.Patterns[idName]);

                                /* Add layers to layer dictionary */
                                if (noteSheet.RelativeLayerPositions.ContainsKey(idName))
                                {
                                    foreach (PositionSheetPair posSheetPair in noteSheet.RelativeLayerPositions[idName])
                                    {
                                        /* Compute the layer's absolute position based off of its relative position in the pattern */
                                        layerAbsolutePosition = GetNoteCountTrackerNoteCount() + currentNoteCount + posSheetPair.Key;

                                        /* Add the layer to the song */
                                        if (noteSheet.Layers.ContainsKey(layerAbsolutePosition))
                                            noteSheet.Layers[layerAbsolutePosition].Add(posSheetPair.Value);
                                        else
                                        {
                                            newLayerList = new SheetSet
                                            {
                                                posSheetPair.Value
                                            };

                                            noteSheet.Layers.Add(layerAbsolutePosition, newLayerList);
                                        }

                                        /* Add the absolute position and layer sheet to the layer position sheet pair structure */
                                        layerPositionSheetPairs.Add(new PositionSheetPair(layerAbsolutePosition, posSheetPair.Value));
                                    }
                                }
                                break;

                            case CallbackType.ACCOMPANIMENT_PATTERN:
                                returnValue.AddRange(noteSheet.Accompaniments[accName].Patterns[idName]);

                                /* Add layers to layer dictionary */
                                if (noteSheet.Accompaniments[accName].RelativeLayerPositions.ContainsKey(idName))
                                {
                                    foreach (PositionSheetPair posSheetPair in noteSheet.Accompaniments[accName].RelativeLayerPositions[idName])
                                    {
                                        /* Compute the layer's absolute position based off of its relative position in the pattern */
                                        layerAbsolutePosition = GetNoteCountTrackerNoteCount() + currentNoteCount + posSheetPair.Key;

                                        /* Add the layer to the song */
                                        if (noteSheet.Layers.ContainsKey(layerAbsolutePosition))
                                            noteSheet.Layers[layerAbsolutePosition].Add(posSheetPair.Value);
                                        else
                                        {
                                            newLayerList = new SheetSet
                                            {
                                                posSheetPair.Value
                                            };

                                            noteSheet.Layers.Add(layerAbsolutePosition, newLayerList);
                                        }

                                        /* Add the absolute position and layer sheet to the layer position sheet pair structure */
                                        layerPositionSheetPairs.Add(new PositionSheetPair(layerAbsolutePosition, posSheetPair.Value));
                                    }
                                }
                                break;

                            case CallbackType.ACCOMPANIMENT_SHEET:
                                returnValue.AddRange(noteSheet.Accompaniments[idName].Sheet);

                                /* Add layers to layer dictionary */
                                if (noteSheet.Accompaniments[idName].Layers.Count > 0)
                                {
                                    foreach (KeyValuePair<int, SheetSet> posSheetSetPair in noteSheet.Accompaniments[idName].Layers)
                                    {
                                        foreach (Sheet layerSheet in posSheetSetPair.Value)
                                        {
                                            /* Compute the layer's absolute position based off of its relative position in the pattern */
                                            layerAbsolutePosition = GetNoteCountTrackerNoteCount() + currentNoteCount + posSheetSetPair.Key;

                                            /* Add the layer to the song */
                                            if (noteSheet.Layers.ContainsKey(layerAbsolutePosition))
                                                noteSheet.Layers[layerAbsolutePosition].Add(layerSheet);
                                            else
                                            {
                                                newLayerList = new SheetSet
                                                {
                                                    layerSheet
                                                };

                                                noteSheet.Layers.Add(layerAbsolutePosition, newLayerList);
                                            }

                                            /* Add the absolute position and layer sheet to the layer position sheet pair structure */
                                            layerPositionSheetPairs.Add(new PositionSheetPair(layerAbsolutePosition, layerSheet));
                                        }
                                    }
                                }
                                break;

                            default:
                                throw new ContextError(ContextError.PC_REFERENCE_ERROR);
                        }
                    }
                }
            }

            /* If the element is shorthand repeat, return the integer value */
            else if (next.Type == TokenType.CARROT)
            {
                numberToken = Expect(TokenType.NUMBER);
                returnValue = int.Parse(numberToken.Content);
            }

            /* If the element is an info change call, parse and change the info accordingly; return value will be an empty list */
            else if (next.Type == TokenType.BANG)
            {
                next = lexer.PeekToken();

                switch (next.Type)
                {
                    case TokenType.KEY:
                        ParseKey(NO_END_NEWLINE);
                        break;

                    case TokenType.TIME:
                        ParseTime(NO_END_NEWLINE);
                        break;

                    case TokenType.TEMPO:
                        ParseTempo(NO_END_NEWLINE);
                        break;

                    case TokenType.OCTAVE:
                        ParseOctave();
                        break;

                    default:
                        throw new SyntaxError(next.Type, TokenType.KEY, TokenType.TIME, TokenType.TEMPO, TokenType.OCTAVE);
                }

                /* Finish parsing */
                Expect(TokenType.BANG);
            }
            else throw new SyntaxError(next.Type, TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG);

            return returnValue;
        }

        private int ParseDotSet() /* dot_set -> DOT dot_set | DOT */
        {
            /* Expect a minimum of 1 dot */
            Expect(TokenType.DOT);

            /* Initialize local variables */
            Token next  = lexer.GetToken(); /* Next token               */
            int   count = 1;                /* Number of dots counted   */

            /* Increment the count for all the remaining dots parsed */
            while (next.Type == TokenType.DOT)
            {
                ++count;
                next = lexer.GetToken();
            }

            /* Put the non-dot token back and return */
            lexer.PutToken(next);
            return count;
        }

        private int ParseOctaveChange() /* octave_change -> ( COMMA | APOS )* */
        {
            /* Initialize local variables */
            Token next   = lexer.GetToken();
            int offset   = 0;

            /* Continue parsing while the next token is in the first set */
            while (octaveChangeFirstSet.Contains(next.Type))
            {
                offset += (next.Type == TokenType.COMMA) ? -1 : 1; /* Octave increases by 1 for each apostrophe and decreases by 1 for each comma */
                next = lexer.GetToken();
            }

            /* Put the next token away and return */
            lexer.PutToken(next);

            return offset;
        }

        private CallbackType ParseCallback(out string accname, out string name) /* callback -> ID | ID GREATER ID */
        {
            /* Local Variables */
            Token  calleeToken; /* Token containing id reference    */
            Token  next;        /* Next token                       */
            string calleeName;  /* Id reference                     */
            /* / Local Variables */

            /* Parse the ID name and store */
            next = Expect(TokenType.ID);
            name = next.Content;
            accname = "";

            next = lexer.GetToken();

            /* This means the ID is an accompaniment reference and the next ID is a pattern or chord */
            if (next.Type == TokenType.GREATER)
            {
                /* Parse the callee ID */
                calleeToken = Expect(TokenType.ID);

                if (!ignoreContext)
                {
                    calleeName = calleeToken.Content;

                    /* Check that the caller name is a valid accompaniment name */
                    if (!noteSheet.Accompaniments.ContainsKey(name))
                        return CallbackType.INVALID_CALLBACK;

                    /* Check that the callee name is a valid chord or pattern in the referenced accompaniment sheet */
                    if (!noteSheet.Accompaniments[name].Patterns.ContainsKey(calleeName) && !noteSheet.Accompaniments[name].Chords.ContainsKey(calleeName))
                        return CallbackType.INVALID_CALLBACK;

                    /* Names are valid, so store them in the appropriate return parameters */
                    accname = name;
                    name = calleeName;

                    /* Return the appropriate callback type */
                    if (noteSheet.Accompaniments[accname].Patterns.ContainsKey(name))
                        return CallbackType.ACCOMPANIMENT_PATTERN; /* Accompaniment pattern */
                    else
                        return CallbackType.ACCOMPANIMENT_CHORD; /* Accompaniment chord */
                }

                /* Only used when the parser ignores context -- should never happen in release */
                else return CallbackType.INVALID_CALLBACK;
            }

            /* Callback must be a local pattern, a local chord, or an accompaniment sheet */
            else
            {
                /* Put the previous token away since it was not the accompaniment callback token (>) */
                lexer.PutToken(next);

                /* Figure out which callback type this is based off of which dictionary contains the name key */
                CallbackType retVal = CallbackType.INVALID_CALLBACK;

                if (noteSheet.Patterns.ContainsKey(name))
                    retVal = CallbackType.LOCAL_PATTERN;
                else if (noteSheet.Chords.ContainsKey(name))
                    retVal = CallbackType.LOCAL_CHORD;
                else if (noteSheet.Accompaniments.ContainsKey(name))
                    retVal = CallbackType.ACCOMPANIMENT_SHEET;

                /* Return the appropriate callback type */
                return retVal;
            }
        }

        /* GRAMMAR PARSER METHODS */
    }
}
