using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace compiler
{
    /* Serializes and deserializes note sheets to allow storage and access in persistent memory */
    partial class Serializer
    {
        private static readonly string SERIALIZE_EXT = ".mkc"; /* File extension of serialized NoteSheet objects */

        public static void Serialize(NoteSheet instance, string filepath, string filename) /* Takes a NoteSheet instance, file name, and file path and uses them to */
                                                                                           /* serialize the instance and store it at the path with the name         */
        {
            /* Set up the formatter and stream object s */
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filepath + filename + SERIALIZE_EXT, FileMode.Create, FileAccess.Write, FileShare.None);

            /* Serialize the given obejct */
            formatter.Serialize(stream, instance);
            stream.Close();
        }

        public static NoteSheet Deserialize(string filepath, string filename)   /* Takes a file name and file path, deserializes the file at that path, and returns the */
                                                                                /* NoteSheet instance                                                                   */
        {
            try
            {
                /* Set up the formatter and stream object */
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(filepath + filename + SERIALIZE_EXT, FileMode.Open, FileAccess.Read, FileShare.Read);

                /* Deserialize the object and return it */
                NoteSheet instance = (NoteSheet)formatter.Deserialize(stream);
                return instance;
            }

            /* Return null if the given file was not found */
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }

    /* Intermediate representation of a Musika file. Contains notes laid out in order */
    [Serializable]
    partial class NoteSheet
    {
        /* CONSTANTS */

        private static readonly string NOTE_FREQUENCY_FILE  = "../../../../../src/lang/data/note_frequency.xml";    /* Note frequency XML chart address */

        public static readonly Dictionary<TokenType, TimeSignature> TimeSignatureDict = new Dictionary<TokenType,TimeSignature>() /* Common time signatures (with keywords associated */
        {
              { TokenType.COMMON, new TimeSignature { baseNote = 4, beatsPerMeasure = 4 } },
              { TokenType.CUT,    new TimeSignature { baseNote = 2, beatsPerMeasure = 2 } }
        };

        public static readonly Dictionary<string, int> KeyConversion = new Dictionary<string, int>() /* Converts a key signature into the number of sharps or flats it has. */
                                                                                                     /* (positive number: sharps, negative number: flats)                   */
        {
            /* Major Keys */
            { "Cmaj",    0 }, { "Gmaj",    1 }, { "Dmaj",    2 }, { "Amaj",    3 },
            { "Emaj",    4 }, { "Bmaj",    5 }, { "F#maj",   6 }, { "C#maj",   7 },
            { "Fmaj",   -1 }, { "Bbmaj",  -2 }, { "Ebmaj",  -3 }, { "Abmaj",  -4 },

            /* Minor Keys */
            { "Am",      0 }, { "Em",      1 }, { "Bm",      2 }, { "F#m",     3 },
            { "C#m",     4 }, { "Dm",     -1 }, { "Gm",     -2 }, { "Cm",     -3 },
            { "Fm",     -4 }, { "Bbm",    -5 }, { "Ebm",    -6 }, { "Abm",    -7 }
        };

        /* / CONSTANTS */

        /* PUBLIC UTILITY FUNCTIONS */

        public bool KeySignuatureExists(string key) /* Verifies if a given key signature is defined */
        {
            return KeyConversion.ContainsKey(key);
        }

        public float GetFrequency(string noteName, int octave) /* Takes a note and its octave and returns the frequency of that sound */
        {
            /* Local Variables */
            XmlDocument noteFrequencyChart; /* XML freq chart reference         */
            XmlNodeList chartSearchResults; /* XML freq chart query results     */
            float       returnValue;        /* Note frequency (return value)    */
            string      name;               /* Formatted note name              */
            /* / Local Variables */

            /* Return 0 frequency for "rest" note */
            if (noteName == "|") returnValue = 0;

            else
            {
                /* Adjust note by key and format it as a valid element name in the XML frequency lookup file */
                name = NoteFactory.GetFormattedNote(noteName, key, octave);

                /* Load XML frequency lookup file */
                noteFrequencyChart = new XmlDocument();
                noteFrequencyChart.Load(NOTE_FREQUENCY_FILE);

                /* Get XML result */
                chartSearchResults = noteFrequencyChart.GetElementsByTagName(name);

                /* Return an "inaudible" frequency if the octave is too high or low or the returned frequency value otherwise */
                if (chartSearchResults.Count == 0)
                    returnValue = 0;
                else
                    returnValue = float.Parse(chartSearchResults[0].InnerText);
            }

            /* Return frequency */
            return returnValue;
        }

        /* / PUBLIC UTILITY FUNCTIONS */

        /* MUSIC REPRESENTATION */

        private string title; /* Song title */
        public string Title   /* Can only be set once */
        {
            set { if (title == null) title = value; }
            get { return title; }
        }

        private string author; /* Composer of the song */
        public string Author   /* Can only be set once */
        {
            set { if (author == null) author = value; }
            get { return author; }
        }

        private string[] coauthors; /* Any other co-composers if there are any */
        public string[] Coauthors   /* Can only be set once */
        {
            set { if (coauthors == null) coauthors = value; }
            get { return coauthors; }
        }

        public int                           key;                           /* Song's key signature                                               */
        public TimeSignature                 time;                          /* Song's time signature                                              */
        public float                         tempo;                         /* Song's tempo (seconds per base beat)                               */
        public int                           octave;                        /* Song's main octave                                                 */
        public int                           noteCount;                     /* Number of notes in main music                                      */
        public bool                          positionCounting;              /* Set to "true" if the note position of the song is being tracked    */

        public Sheet                         Sheet          { get; set; }   /* Main music sheet                                                   */
        public Dictionary<string, NoteSheet> Accompaniments { get; set; }   /* Collection of accompaniment sheets                                 */
        public Dictionary<string, Sheet>     Patterns       { get; set; }   /* Collection of local pattern sheets                                 */
        public Dictionary<string, NoteSet>   Chords         { get; set; }   /* Collection of local chord sheets                                   */
        public Dictionary<int,    SheetSet>  Layers         { get; set; }   /* Collection of layered music at specified positions                 */

        /* / MUSIC REPRESENTATION */

        /* CONSTRUCTOR */

        public NoteSheet()
        {
            /* Initialize all instance objects */
            Sheet          = new Sheet();
            Patterns       = new Dictionary<string, Sheet>();
            Chords         = new Dictionary<string, NoteSet>();
            Accompaniments = new Dictionary<string, NoteSheet>();
            Layers         = new Dictionary<int,    SheetSet>();
        }

        /* / CONSTRUCTOR */
    }

    /* Indicates that contextual error has been made */
    partial class ContextError : Exception
    {
        private const string BASE_STRING                = "CONTEXT ERROR: ";

        public const string CROSS_REFERENCE_ERROR       = "cross-referencing files is not allowed.";
        public const string INVALID_ACC_REFERENCE_ERROR = "invalid accompaniment reference!";
        public const string INVALID_FILENAME_ERROR      = "invalid Musika file name in accompaniment!";
        public const string INVALID_PA_REF_ERROR        = "invalid pattern or accompaniment reference name!";
        public const string INVALID_REPEAT_NUM_ERROR    = "you cannot repeat a negative number of times!";
        public const string KEY_ERROR                   = "Invalid key signature specified.";
        public const string LAYER_NO_PATTERN_ERROR      = "a pattern or accompaniment name must be specified in a layer call!";
        public const string NULL_PATTERN_ERROR          = "A callback type case was not covered because the layer pattern was set to null";
        public const string OCTAVE_ERROR                = "Octave cannot be negative!";
        public const string PATTERN_DOT_ERROR           = "pattern cannot be followed by dots!";
        public const string PC_REFERENCE_ERROR          = "Invalid pattern or chord reference!";
        public const string REPEAT_NOTHING_ERROR        = "cannot use shorthand repeat on nothing";
        public const string SELF_REFERENCE_ERROR        = "cannot reference the current file!";
        public const string TIER_2_DICT_ERROR           = "Tier 2 keyword read but content invalid. Check lexical analyzer";
        public const string TIME_ERROR                  = "Make sure to set the beats per measure and the base beat!";
        public const string ZERO_TIME_ERROR             = "time base note and beats per measure cannot be set to zero!";

        public ContextError(string text) : base(BASE_STRING + text) { }
    }

    /* Parses a Musika program to check for correct syntax and synthesize the intermediate representation */
    partial class Parser
    {
        /* PROPERTIES */

        private HashSet<string> doNotCompileSet;    /* Do not use any of these files in an accompaniment       */
        private LexicalAnalyzer lexer;              /* Token manager                                           */
        private NoteSheet       noteSheet;          /* Intermediate representation of the compiled Musika file */
        private string          filename;           /* File name of the current Musika file                    */
        private string          filepath;           /* File path of the current Musika file                    */
        private string          program;            /* Musika program text                                     */
        private bool            ignoreContext;      /* Flag marked true if only syntax is checked              */

        /* / PROPERTIES */

        /* CONSTANTS */

        public const string MUSIKA_FILE_EXT = ".ka";

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
            if (!filename.EndsWith(MUSIKA_FILE_EXT))
                filename += MUSIKA_FILE_EXT;
            this.filename = filename;

            /* Add the current file to the do-not-compile set */
            doNotCompileSet.Add(filepath + filename);

            /* Initialize other instance variables */
            this.program = program;
            Reset();
        }

        /* / CONSTRUCTOR */

        /* HELPER FUNCTIONS */

        public void IgnoreContext() /* Sets parser to only check for syntax: this will NOT generate a NoteSheet instance */
        {
            ignoreContext = true;
            noteSheet     = null;
        }

        public void Reset() /* Restore the program and refresh/restart the lexical analyzer and notesheet */
        {
            lexer     = new LexicalAnalyzer(program);
            noteSheet = new NoteSheet();
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

        /* / HELPER FUNCTIONS */

        /* GRAMMAR PARSER FUNCTIONS */

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
            /* Local Variables */
            Token     fileNameToken;    /* Accompaniment file name token                   */
            Token     nameToken;        /* Reference name of the accompaniment token       */
            Parser    accParser;        /* Parses accompanied Musika file                  */
            NoteSheet accSheet;         /* Noteh sheet from accompanied file               */
            string    accProgram;       /* Accompanied file program text                   */
            string    file;             /* Accompanied file path + accompanied file name   */
            string    filename;         /* Accompanied file path                           */
            /* / Local Variables */

            /* Parse The Accompany Statement */
            Expect(TokenType.ACCOMPANY);
            Expect(TokenType.LBRACKET);

            fileNameToken = Expect(TokenType.ID);

            Expect(TokenType.RBRACKET);
            Expect(TokenType.NAME);

            nameToken = Expect(TokenType.ID);

            /* Construct the literal file (filepath + filename) */
            filename = fileNameToken.Content + ".ka";
            file     = filepath + filename;

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
            int key                      = noteSheet.key;
            TimeSignature time           = noteSheet.time;
            float tempo                  = noteSheet.tempo;
            int octave                   = noteSheet.octave;
            Sheet noteList;
            noteSheet.positionCounting   = true; /* These notes are used in the actual note sheet, so keep track of position for any potential layer placement */

            /* Enter a child scope to quickly deallocate the discard variable */
            {
                PositionSheetMap discard;

                /* Generate a note sheet from the main music */
                noteList = ParseMusic(out discard);
            }

            /* The ParseMusic() function returns the main music representation */
            noteSheet.Sheet = noteList;

            /* Turn off position countning as the main music has been generated */
            noteSheet.positionCounting = false;

            /* Restore key, time, tempo, and octave info */
            noteSheet.key    = key;
            noteSheet.time   = time;
            noteSheet.tempo  = tempo;
            noteSheet.octave = octave;

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

        private Sheet ParseMusic(out PositionSheetMap layerDict) /* music -> music_element NEWLINE* music | music_element | EPSILON */
        {
            /* Local Variables */
            Sheet layerSheet    = null;         /* Layered music             */
            Sheet returnValue   = new Sheet();  /* Main music return value   */
            Token next;                         /* Next token                */
            int   layerPosition = -1;           /* Position of layered music */
            /* / Local Variables */

            /* Initialize the dictionary of layer sheets */
            layerDict = new PositionSheetMap();

            /* Parse each music element */
            next = lexer.PeekToken();
            while (musicFirstSet.Contains(next.Type))
            {
                /* Parse the music element and store the Sheet instance and layer data it returns */
                Sheet elementList = ParseMusicElement(ref layerPosition, ref layerSheet);

                /* If a layer was present, add it to the layer dicitonary */
                if (layerPosition != -1 && layerSheet != null) /* Layer was called */
                    layerDict.Add(new PositionSheetPair(layerPosition, layerSheet));

                /* If a list of notes was returned from parsing the music element, add each note to returned note sheet  */
                if (elementList != null)
                    foreach (NoteSet element in elementList)
                    {
                        returnValue.Add(element);
                        if (noteSheet.positionCounting)
                            ++noteSheet.noteCount;
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
                        noteSheet.key = NoteSheet.KeyConversion[next.Content];
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
                        noteSheet.key = referenceSheet.key;
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
            originalBaseNote = noteSheet.time.baseNote;

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

                    noteSheet.time = new TimeSignature { baseNote = secondNumber, beatsPerMeasure = firstNumber };
                }

                /* If the value is a single integer, set the base note to the specified value and adjust the beats per measure accordingly */
                else
                {
                    /* Check that the time signature was initialized (there must be an initial value before you can modify it) */
                    if (noteSheet.time.baseNote == 0 && noteSheet.time.beatsPerMeasure == 0 && !ignoreContext)
                        throw new ContextError(ContextError.TIME_ERROR);
                    else
                    {
                        /* Check that the time was not set to 0 (because that would not make much sense) */
                        if (firstNumber == 0)
                            throw new ContextError(ContextError.ZERO_TIME_ERROR);

                        /* Increase/Decrease the beats per measure by the same order of magnitude (so that the true time signature remains the same */
                        /* Example: if the original time signature is 4 / 4, and the base note is set to 8, the new time signature is 8 / 8         */
                        baseNoteRatio = (float)firstNumber / (float)noteSheet.time.baseNote;
                        noteSheet.time.baseNote = firstNumber;
                        noteSheet.time.beatsPerMeasure *= baseNoteRatio;
                    }

                    lexer.PutToken(next); /* NUMBER */
                }
            }

            /* If the value specified is a keyword (a standard time signature), find the time signature values from the keyword and store the new time */
            else if (TokenTypeFactory.Tier2Keywords.Contains(next.Type))
            {
                if (NoteSheet.TimeSignatureDict.ContainsKey(next.Type))
                    noteSheet.time = NoteSheet.TimeSignatureDict[next.Type];

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
                    noteSheet.time = referenceSheet.time;
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
            if (noteSheet.time.baseNote != originalBaseNote)
                noteSheet.tempo *= (float)originalBaseNote / (float)noteSheet.time.baseNote;
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
                    baseNoteRatio  = firstNumber / noteSheet.time.baseNote;
                    secondsPerBeat = SECONDS_PER_MINUTE / secondNumber;

                    /* Multiply the seconds per beat by the ratio of the base notes to get the seconds per beat in the time signature's base note */
                    noteSheet.tempo = baseNoteRatio * secondsPerBeat;
                }

                /* One integer specified, so just multiply/divide the current tempo by this coefficient */
                else
                {
                    if (firstNumber >= 0)
                        noteSheet.tempo *= firstNumber;
                    else /* If the number is negative, multiply by the negative reciprocal (Example: -2 becomes 1/2) */
                        noteSheet.tempo /= -firstNumber; /* if the number is negative, it slows down time so divide by absolute value */

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
                    noteSheet.tempo = referenceSheet.tempo;

                    if (noteSheet.time.baseNote != referenceSheet.time.baseNote) /* Adjust tempo for accompaniment sheet's different time signature */
                        noteSheet.tempo *= referenceSheet.time.baseNote / noteSheet.time.baseNote;
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
            bool      increment;        /* Determines if octave number is a raw value or an increment (with a plus sign) */
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
                    noteSheet.octave += newOctave;

                    /* Throw an error if the new octave is below 0 */
                    if (noteSheet.octave < 0 && !ignoreContext)
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
                    noteSheet.octave = newOctave;
                else if (newOctave < 0) /* Value is negative or has a plus sign, so add the current octave by the specified value  */
                    noteSheet.octave += newOctave;

                if (noteSheet.octave < 0 && !ignoreContext)
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
                    noteSheet.octave = referenceSheet.octave;
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
            PositionSheetMap discard;       /* Not used, but needed to pass into ParseMusic */
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
                keyBuffer    = noteSheet.key;
                timeBuffer   = noteSheet.time;
                tempoBuffer  = noteSheet.tempo;
                octaveBuffer = noteSheet.octave;

                /* Parse Body */
                music = ParseMusic(out discard);

                /* Add the generated sheet to the list of patterns */
                patternName = nameTok.Content;
                noteSheet.Patterns.Add(patternName, music);

                /* Restore info */
                noteSheet.key    = keyBuffer;
                noteSheet.time   = timeBuffer;
                noteSheet.tempo  = tempoBuffer;
                noteSheet.octave = octaveBuffer;
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

                /* Store the name-list pair in the chords dictionary */
                noteSheet.Chords.Add(chordName, noteList);
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
                octave = noteSheet.octave;

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

        private Sheet ParseMusicElement(ref int position, ref Sheet sheet) /* music_element -> function | riff */
        {
            Token next = lexer.PeekToken();

            /* Parse function if the next token is in the function first set */
            if (TokenTypeFactory.Tier3Keywords.Contains(next.Type)) /* This is also the first set of the function grammar rule */
                return ParseFunction(ref position, ref sheet);

            /* Parse riff if the next token is in the riff first set */
            else if (riffFirstSet.Contains(next.Type))
                return ParseRiff();

            /* If the next token was not in the function or riff first set, throw a syntax error */
            else throw new SyntaxError(next.Type, TokenType.REPEAT, TokenType.LAYER, TokenType.NOTE, TokenType.ID, TokenType.CARROT, TokenType.BANG);
        }

        private Sheet ParseFunction(ref int position, ref Sheet sheet) /* function -> repeat | layer */
        {
            Token next = lexer.PeekToken();

            /* Parse the repeat rule if the next token is the "repeat" keyword (repeat rule first set) */
            if (next.Type == TokenType.REPEAT)
                return ParseRepeat();

            /* Parse the layer rule if the next token is the "layer" keyword (layer rule first set) */
            else if (next.Type == TokenType.LAYER)
            {
                /* Parse the layer and save both the music and its relative position in the song */
                PositionSheetPair layerReturn = ParseLayer();
                position = layerReturn.Key;
                sheet = layerReturn.Value;

                return null; /* A layer does not produce any note sheets for the main sheet */
            }

            else throw new SyntaxError(next.Type, TokenType.REPEAT, TokenType.LAYER);
        }

        private Sheet ParseRepeat() /* repeat -> REPEAT L_PAREN NUMBER R_PAREN L_BRACE music R_BRACE */
        {
            /* Local Variables */
            Sheet              returnValue;     /* Sheet music to be returned (music the repeat number of times)    */
            Sheet              music;           /* Sheet music returend from ParseMusic                             */
            PositionSheetMap   layerReference;  /* Stores a detected layer reference in the music                   */
            SheetSet           newLayerList;    /* List of newly defined layers in repeat                           */
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

            /* Parse the music in the repeat block */
            music = ParseMusic(out layerReference);

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
                        newLayerList = new SheetSet();
                        newLayerList.Add(pair.Value);
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

        private PositionSheetPair ParseLayer() /* layer -> LAYER L_PAREN callback R_PAREN */
        {
            /* Local Variables */
            CallbackType      idPatternRef; /* Type of id callback                                              */
            PositionSheetPair returnValue;  /* Layer as position-sheet music pair to return                     */
            Sheet             pattern;      /* Layered music                                                    */
            SheetSet          newLayerList; /* Layer list to set as the layer list if not already initialized   */
            string            accname;      /* Accompaniment name                                               */
            string            name;         /* Name of pattern/accompaniment                                    */
            int               position;     /* Position of layered music in song                                */
            /* / Local Variables */

            /* Parse declaration */
            Expect(TokenType.LAYER);
            Expect(TokenType.LPAREN);

            if (!ignoreContext)
            {
                /* Parse the reference, store the name, and store the name of the accompaniment if there is one */
                idPatternRef = ParseCallback(out accname, out name);

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
                position = noteSheet.noteCount;

                /* Add the pattern to the layer list at the stored position */
                if (noteSheet.Layers.ContainsKey(position))
                    noteSheet.Layers[position].Add(pattern);
                else
                {
                    newLayerList = new SheetSet();
                    newLayerList.Add(pattern);
                    noteSheet.Layers.Add(position, newLayerList);
                }

                /* Construct the position-pattern pair from the calculated position and received pattern */
                returnValue = new PositionSheetPair(position, pattern);
            }

            /* If we are ignoring context, then all we need to do is parse the rest of the layer call and return an empty pair */
            else
            {
                ParseCallback(out accname, out name);
                returnValue = new PositionSheetPair(-1, null);
            }

            /* Finish parsing and return */
            Expect(TokenType.RPAREN);
            return returnValue;
        }

        private Sheet ParseRiff() /* riff -> riff_element NEWLINE* riff  | riff_element NEWLINE* */
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
                dynamic elementReturn = ParseRiffElement();

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

        private dynamic ParseRiffElement() /* riff_element -> NOTE dot_set | NOTE octave_change dot_set | callback | callback dot_set */
                                           /* | CARROT NUMBER | BANG key \ NEWLINE BANG | BANG time \ NEWLINE BANG                    */
        {
            /* Constants */
            const bool NO_END_NEWLINE = false; /* Do not expect a newline */
            /* / Constants */

            /* Local Variables */
            CallbackType idRefPattern;      /* Type of id callback                                          */
            NoteSet      chordCopy;         /* Copy of the referenced chord (to add if element is chord)    */
            NoteSet      chordRef;          /* Chord referenced by id                                       */
            NoteSet      singleNoteList;    /* Element to add if element is a single note                   */
            Token        next;              /* Next token                                                   */
            Token        numberToken;       /* Token containing shorthand repeat number                     */
            int          i;                 /* Chord copy loop index                                        */
            int          numDots;           /* Duration of note/chord in dots (base rhythms)                */
            int          octave;            /* Specified octave                                             */
            string       accName;           /* Name of specified accompaniment                              */
            string       idName;            /* Name of specified id                                         */
            string       noteName;          /* Name of specified note                                       */
            float        duration;          /* Duration of note/chord in seconds                            */
            float        frequency;         /* Frequency of specified note                                  */
            dynamic      returnValue;       /* Return value (either Sheet or int type)                      */
            /* / Local Variables */

            /* Assume return value is a list and initialize (since most paths return a list) */
            returnValue = new Sheet();

            next = lexer.GetToken();

            /* If the element is a note, get the note information and add to the list */
            if (next.Type == TokenType.NOTE)
            {
                /* Store the initial information */
                noteName = next.Content;
                octave   = noteSheet.octave;
                numDots  = 0;

                next = lexer.PeekToken();

                /* Offset the octave if specified */
                octave += ParseOctaveChange();

                /* Find the number of basenotes to hold (number of dots). This determines the length of the note */
                numDots = ParseDotSet();

                /* With updated information, find the note frequency and duration */
                frequency = noteSheet.GetFrequency(noteName, octave);
                duration  = noteSheet.tempo * numDots;

                /* Store the note information into a note instance and store in the return value as a single note */
                singleNoteList = new NoteSet();
                singleNoteList.Add(new Note { note = noteName, frequency = frequency, length = duration }); /* stored in a list by itself since it is not a chord */
                returnValue.Add(singleNoteList);
            }

            /* If the element is a reference, add the chord, pattern, or sheet to the return value */
            else if (next.Type == TokenType.ID)
            {
                /* Parse the callback and store its type and names */
                lexer.PutToken(next);
                idRefPattern = ParseCallback(out accName, out idName);
                next = lexer.PeekToken();

                if (ignoreContext) /* Should only be used for testing and should never be run for release */
                {
                    if (next.Type == TokenType.DOT)
                        numDots = ParseDotSet();
                }
                else /* Should always run for release */
                {
                    /* If the ID refers to a chord, store the chord in the return value */
                    if (idRefPattern == CallbackType.LOCAL_CHORD || idRefPattern == CallbackType.ACCOMPANIMENT_CHORD)
                    {
                        /* Find the length of the chord */
                        numDots = ParseDotSet();
                        duration = noteSheet.tempo * numDots;

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
                                break;

                            case CallbackType.ACCOMPANIMENT_PATTERN:
                                returnValue.AddRange(noteSheet.Accompaniments[accName].Patterns[idName]);
                                break;

                            case CallbackType.ACCOMPANIMENT_SHEET:
                                returnValue.AddRange(noteSheet.Accompaniments[idName].Sheet);
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

        /* GRAMMAR PARSER FUNCTIONS */
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
        private const int BREAK_DASH_COUNT = 3;

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
                else if (nextChar == '=')
                {
                    nextChar = input.GetChar();

                    /* Multi-line comment */
                    if (nextChar == '>')
                    {
                        /* Stop at the end of file to prevent infinite loop */
                        while (nextChar != '\0')
                        {
                            nextChar = input.GetChar();

                            /* Check for closing multiline comment */
                            if (nextChar == '<')
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
                    while (nextChar != '\n' && char.IsWhiteSpace(nextChar))
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
            if (nextChar == '\n')
            {
                /* Store the newline in the return string */
                returnTokenString += nextChar;
                nextChar = input.GetChar();

                /* Count the number of dashes present (max 3) */
                int dashCount = 0;
                while (dashCount < BREAK_DASH_COUNT && nextChar == '-')
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                    ++dashCount;
                }

                /* If the dash count hit the maximum, ensure there is a newline following to return a BREAK token */
                if (dashCount == BREAK_DASH_COUNT)
                {
                    TokenType type = TokenType.UNKNOWN;

                    /* Ignore all other characters until a newline or EOF is reached */
                    while (nextChar != '\n' && nextChar != '\0')
                        nextChar = input.GetChar();

                    /* If a newline was reached, the BREAK is \n---\n, ignoring all other characters in between */
                    if (nextChar == '\n')
                    {
                        returnTokenString += nextChar;
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
                        TokenType returnType = TokenType.ID;
                        if (returnTokenString.Contains("#") || returnTokenString.Contains("*"))
                            returnType = TokenType.UNKNOWN; /* If this is an ID, make sure there's no # or * in it */
                        return new Token(returnTokenString, returnType);
                }
            }

            /* String */
            else if (nextChar == '\"')
            {
                /* Keep adding content to the return string until the closing quote or newline is reached */
                nextChar = input.GetChar();
                while (nextChar != '\"' && nextChar != '\n')
                {
                    returnTokenString += nextChar;
                    nextChar = input.GetChar();
                }

                /* If the final character was a close quote, it's a valid string; if it was a newline, it is an unknown token (string was never closed in the same line) */
                bool quoteClosed = true;
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
                int discard; /* Unused but needed to pass in TryParse function */

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
                return new Token(returnTokenString, (int.TryParse(returnTokenString, out discard)) ? TokenType.NUMBER : TokenType.UNKNOWN);
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
            /* Return a null terminator to allow other objects to know there is no more input */
            if (programText.Length == 0)
                return '\0';

            /* Return the first character in the input buffer and remove it from the buffer */
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

    /* Converts a note to an appropriate string */
    class NoteFactory
    {
        private static readonly Dictionary<char, string> SharpToXML = new Dictionary<char, string>()
        {
            { 'A', "As" },
            { 'B', "C"  },
            { 'C', "Cs" },
            { 'D', "Ds" },
            { 'E', "F"  },
            { 'F', "Fs" },
            { 'G', "Gs" }
        };

        private static readonly Dictionary<char, string> FlatToXML = new Dictionary<char, string>()
        {
            { 'A', "Gs" },
            { 'B', "As" },
            { 'C', "B"  },
            { 'D', "Cs" },
            { 'E', "Ds" },
            { 'F', "E"  },
            { 'G', "Fs" }
        };

        private static readonly Dictionary<char, string> DoubleSharpToXML = new Dictionary<char, string>()
        {
            { 'A', "B"  },
            { 'B', "Cs" },
            { 'C', "D"  },
            { 'D', "E"  },
            { 'E', "Fs" },
            { 'F', "G"  },
            { 'G', "A"  }
        };

        private static readonly Dictionary<char, string> DoubleFlatToXML = new Dictionary<char, string>()
        {
            { 'A', "G"  },
            { 'B', "A"  },
            { 'C', "Bb" },
            { 'D', "C"  },
            { 'E', "D"  },
            { 'F', "Eb" },
            { 'G', "F"  }
        };

        private static readonly char[] sharpOrder = { 'F', 'C', 'G', 'D', 'A', 'E', 'B' };
        private static readonly char[] flatOrder =  { 'B', 'E', 'A', 'D', 'G', 'C', 'F' };

        /* Adjust note by key and format it as a valid element name in the XML file    */
        /* XML element names consist of the note name followed by the octave number    */
        /* The accidental symbols are replased with an "s" for sharp or nothing at all */
        /* For example, a 5th octave B in D minor will be converted to "As5"           */
        public static string GetFormattedNote(string name, int numSharpsOrFlats, int octave)
        {
            /* Local Variables */
            char[] order;
            char   finalChar;
            char   nameCharCopy;
            string noteName;
            int    i;
            bool   flatKey;
            /* / Local Variables */

            finalChar = name[name.Length - 1];

            noteName = "ERROR"; /* This initial value should not be returned */

            switch (finalChar)
            {
                /* Just remove the $ */
                case '$':
                    noteName = name[0].ToString();
                    break;

                /* Just replace # with s */
                case '#':
                    noteName = SharpToXML[name[0]];
                    break;

                case 'b':
                    /* If the note is a double flat, convert using the double flat table */
                    if (name[name.Length - 2] == 'b')
                        noteName = DoubleFlatToXML[name[0]];

                    /* Otherwise convert using the single flat table */
                    else
                        noteName = FlatToXML[name[0]];

                    break;

                /* Convert using the double sharp table */
                case '*':
                    noteName = DoubleSharpToXML[name[0]];
                    break;

                /* No accidental, so adjust to key signature */
                default:
                    /* We only need the first character in the name string */
                    noteName     = name[0].ToString();
                    nameCharCopy = noteName[0]; /* Used for readability */

                    /* A flat key in this compiler is represented by a negative number of sharps */
                    flatKey = numSharpsOrFlats < 0;

                    /* Figure out the order of flats/sharps to use */
                    order            = (flatKey) ? flatOrder : sharpOrder;
                    numSharpsOrFlats = Math.Abs(numSharpsOrFlats);

                    /* Iterate until the number of sharps/flats has been reached                                                                                */
                    /* (Example: Bb Major (a 2-flat key) would only check the first 2 values (Bb and Eb) and leave the rest unchecked since they are natural)   */
                    for (i = 0; i < numSharpsOrFlats; ++i)
                    {
                        /* If there is a match, it means that the note needs to be adjusted, so use the appropriate table to adjust it */
                        if (nameCharCopy == order[i])
                        {
                            noteName = (flatKey) ? FlatToXML[nameCharCopy] : SharpToXML[nameCharCopy];
                            break;
                        }
                    }

                    break;
            }

            /* Return the name concatenated with the octave for valid XML frequency table element name */
            return noteName + octave;
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

    /* Callback types */
    enum CallbackType
    {
        INVALID_CALLBACK, ACCOMPANIMENT_PATTERN, ACCOMPANIMENT_CHORD, ACCOMPANIMENT_SHEET, LOCAL_PATTERN, LOCAL_CHORD
    }

    /* All the possible token types */
    enum TokenType
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

    /* Custom types and substitute type names */
    struct PositionSheetPair /* Replacement for KeyValuePair<int, Sheet> */
    {
        public int   Key;
        public Sheet Value;

        public PositionSheetPair(int pos, Sheet s)
        {
            Key   = pos;
            Value = s;
        }
    }

    [Serializable]
    class NoteSet           : List<Note>               { }

    [Serializable]
    class Sheet             : List<NoteSet>            { }

    class SheetSet          : List<Sheet>              { }

    class PositionSheetMap  : List<PositionSheetPair>  { }

    /* Represents a musical note with its data */
    [Serializable]
    internal struct Note
    {
        internal string note;
        internal float frequency;
        internal float length;
    }

    /* Represents a time signature (a number for beats per measure and number for base beat rhythm) */
    [Serializable]
    internal struct TimeSignature
    {
        internal int baseNote;
        internal float beatsPerMeasure;
    }
}
