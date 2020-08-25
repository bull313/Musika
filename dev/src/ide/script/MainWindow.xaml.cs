using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using Musika;

namespace MusikaIDE
{
    /* Used to style special words and characters */
    internal class Style
    {
        /*
        *  ---------------- PROPERTIES ----------------
        */
        private readonly Dictionary<DependencyProperty, object> styleDict;
        /*
        *  ---------------- / PROPERTIES ----------------
        */

        /*
        *  ---------------- CONSTRUCTOR ----------------
        */
        internal Style()
        {
            styleDict = new Dictionary<DependencyProperty, object>();
        }
        /*
        *  ---------------- / CONSTRUCTOR ----------------
        */

        /*
        *  ---------------- INTERNAL METHODS ----------------
        */
        internal void AddColor(Color c) /* Add the specified font color */
        {
            AddStyle(TextElement.ForegroundProperty, new SolidColorBrush(c));
        }

        internal void AddWeight(FontWeight f) /* Add the specified font weight (e.g. bold) */
        {
            AddStyle(TextElement.FontWeightProperty, f);
        }

        internal void AddStyle(FontStyle s) /* Add the specified font syle (e.g. oblique, italic) */
        {
            AddStyle(TextElement.FontStyleProperty, s);
        }

        internal void AddStyle(DependencyProperty dp, object val) /* Add a miscellaneous style */
        {
            styleDict.Add(dp, val);
        }

        internal Dictionary<DependencyProperty, object> GetDict() /* Getter for style dictionary */
        {
            return styleDict;
        }
        /*
        *  ---------------- / INTERNAL METHODS ----------------
        */
    }
    /*
        *  ---------------- / STYLE CLASS TO STYLE SPECIAL WORDS/CHARS ----------------
    */

    /*
        *  ---------------- MAIN CLASS ----------------
    */
    public partial class MainWindow : Window
    {
        /*
        *  ---------------- CONSTANTS ----------------
        */
        public const int    EXIT_OK_CODE                                = 0;                                                                                                                                                                                    /* Application quitting on its own with no issues                       */
        public const string MUSIKA_FILE_FILTER                          = "Musika Files(*.ka)|*.ka|All(*.*)|*";                                                                                                                                                 /* Filters Musika files in file dialogs                                 */
        public const string NOTESHEET_FILE_EXT                          = ".mkc";                                                                                                                                                                               /* Serialized note sheet file extension                                 */
        public const string UKNOWN_ISSUE_MESSAGE                        = "Musika IDE ran into a problem processing your last request.\nPlease consult your nearest Musika engineer.\nSorry for the inconvenience.";                                            /* Error message if an unknown exception occurrs                        */
        public const string VIEW_TOGGLE_SYNTAX_HIGHLIGHTING_WARNING     = "WARNING:\nThe syntax highlighting feature allows you to view Musika code in a more visual style, but currently the process of highlighting is VERY slow.\nPlease use with caution";  /* Syntax highlighting is slow warning message when user turns it on    */
        public const string WAV_FILE_EXT                                = ".wav";                                                                                                                                                                               /* WAV audio file extension                                             */
        /*
        *  ---------------- / CONSTANTS ----------------
        */


        /*
        *  ---------------- PROPERTIES ----------------
        */
        private readonly Dictionary<TokenType, Style>   tokenTypeStyleDict;             /* Determines which token types are styled and how they are styled                                  */
        private readonly Dictionary<string, Style>      wordStyleDict;                  /* Determines which words are styled and how they are styled                                        */
        private readonly List<StyleText>                styledTextRanges;               /* Used to keep track of the keywords in in the text box                                            */
        private readonly Style                          multiLineCommentStyle;          /* Styling data for multi-line comments                                                             */
        private readonly Style                          singleLineCommentStyle;         /* Styling data for single line comments                                                            */


        private SongPlayer                              songPlayer;                     /* Environment object to play a WAV song                                                            */
        bool                                            inMultiLineComment;             /* Checks if the current text is in a multi line comment based on the context of the previous run   */
        bool                                            viewSyntaxHighlighting;         /* If and only if enabled, syntax highlighting functionality is on                                  */
        private string                                  currentDirectory;               /* Keeps track of the directory of the current file                                                 */
        private string                                  savedFilename;                  /* Name of file we are writing to (initialized to the bin directory of the executable               */
        /*
        *  ---------------- / PROPERTIES ----------------
        */


        /*
        *  ---------------- DATA TYPES ----------------
        */
        private struct StyleText /* Style text struct that includes its location, content, and style */
        {
            internal TextPointer Start, End;
            internal Style Style;
        }
        /*
        *  ---------------- / DATA TYPES ----------------
        */

        /*
        *  ---------------- CONSTRUCTOR ----------------
        */
        public MainWindow()
        {
            /* Initialize properties */
            InitializeComponent();
            styledTextRanges = new List<StyleText>();

            /* Create Styles */

            /* Intialize all style Variables */
            Style bangStyle             = new Style(),  /* Bangs !                                      */
                    breakStyle          = new Style(),  /* Break token (\n---\n)                        */
                    caretStyle          = new Style(),  /* Carets ^                                     */
                    dotStyle            = new Style(),  /* Dots .                                       */
                    greaterThanStyle    = new Style(),  /* Greater than symbol >                        */
                    keySignStyle        = new Style(),  /* Key signature keywords (Cmaj, Am, etc.)      */
                    noteStyle           = new Style(),  /* Note names (C, D, G, etc.)                   */
                    numberStyle         = new Style(),  /* Numbers 1234567890                           */
                    octaveModifierStyle = new Style(),  /* Commas, and apostrophes' (octave modifiers)  */
                    stringStyle         = new Style(),  /* "Strings"                                    */
                    tier1               = new Style(),  /* Tier-1 keywords (see grammar.html)           */
                    tier2               = new Style(),  /* Tier-2 keywords (see grammar.html)           */
                    tier3               = new Style();  /* Tier-3 keywords (see grammar.html)           */

            multiLineCommentStyle       = new Style();  /* => Multi Line Comments <=                    */
            singleLineCommentStyle      = new Style();  /* & Signle Line Comments                       */

            /* Add Colors */
            bangStyle.AddColor(Colors.DarkTurquoise);
            breakStyle.AddColor(Colors.Olive);
            caretStyle.AddColor(Colors.Firebrick);
            dotStyle.AddColor(Colors.Teal);
            greaterThanStyle.AddColor(Colors.HotPink);
            keySignStyle.AddColor(Colors.SandyBrown);
            noteStyle.AddColor(Colors.CadetBlue);
            numberStyle.AddColor(Colors.Indigo);
            octaveModifierStyle.AddColor(Colors.DarkSlateBlue);
            stringStyle.AddColor(Colors.Purple);
            tier1.AddColor(Colors.Red);
            tier2.AddColor(Colors.OrangeRed);
            tier3.AddColor(Colors.Green);

            singleLineCommentStyle.AddColor(Colors.CornflowerBlue);
            multiLineCommentStyle.AddColor(Colors.DarkSlateGray);

            /* Add Weight (Bold, Light, etc.) */
            breakStyle.AddWeight(FontWeights.Bold);
            tier1.AddWeight(FontWeights.Bold);

            /* Add Styles (Italic, Oblique, etc.) */
            singleLineCommentStyle.AddStyle(FontStyles.Oblique);
            multiLineCommentStyle.AddStyle(FontStyles.Oblique);

            /* Populate dictionary */
            tokenTypeStyleDict = new Dictionary<TokenType, Style>()
            {
                /* Tier 1 Keywords */
                {TokenType.ACCOMPANY, tier1}, {TokenType.NAME,      tier1}, {TokenType.AUTHOR,    tier1}, {TokenType.COAUTHORS, tier1}, {TokenType.TITLE,     tier1}, {TokenType.KEY,       tier1},
                {TokenType.TIME,      tier1}, {TokenType.TEMPO,     tier1}, {TokenType.OCTAVE,    tier1}, {TokenType.PATTERN,   tier1}, {TokenType.CHORD,     tier1}, {TokenType.IS,        tier1},

                /* Tier 2 Keywords */
                {TokenType.COMMON, tier2}, {TokenType.CUT, tier2},

                /* Tier 3 Keywords */
                {TokenType.REPEAT, tier3}, {TokenType.LAYER, tier3},

                /* Octave Modifiers */
                { TokenType.COMMA, octaveModifierStyle }, { TokenType.APOS, octaveModifierStyle },

                /* Individual Styles */
                {TokenType.BANG, bangStyle},        {TokenType.BREAK, breakStyle},                  {TokenType.CARROT, caretStyle},
                {TokenType.DOT, dotStyle},          {TokenType.GREATER, greaterThanStyle},          {TokenType.SIGN,  keySignStyle},
                {TokenType.NOTE,   noteStyle},      {TokenType.NUMBER, numberStyle},                {TokenType.STRING, stringStyle }
            };

            wordStyleDict = new Dictionary<string, Style>()
            {
                /* BREAK Token sans newlines */
                {"-", breakStyle}
            };
        }
        /*
        *  ---------------- / CONSTRUCTOR ----------------
        */

        /*
         *  ---------------- HELPER FUNCTIONS ----------------
        */

        private void CheckWordsInRun(Run r) /* Scans a run for keywords and stores the keywords with their information in the buffer */
        {
            /* Local Variables */
            StyleText       styleText;                  /* Contains the beginning and end positions of a styled token along with the style object to style the token    */
            LexicalAnalyzer lexer;                      /* Identifies tokens in the text to be colored                                                                  */
            Token           nextToken;                  /* Pointer to the current token                                                                                 */
            int             textOffset;                 /* Start position of the current text ignoring the prefix under a multi-line comment                            */
            int             leftRangePointer;           /* Pointer to the beginning position of the current token in the text                                           */
            int             previousLeftRangePointer;   /* Stores the left range pointer for adjusting BREAK tokens                                                     */
            int             rightRangePointer;          /* Pointer to the end position of the current token in the text                                                 */
            string          currentText;                /* Current text to process                                                                                      */
            /* / Local Variables */

            /* Create new lexical analyzer and start reading tokens */
            currentText         = r.Text;
            lexer               = new LexicalAnalyzer(currentText);
            nextToken           = lexer.GetToken();
            textOffset          = 0;
            rightRangePointer   = textOffset + lexer.GetTextPosition() + 1;       /* Initial range is from the beginning of the text  */
            leftRangePointer    = rightRangePointer - nextToken.Content.Length;   /* to the end of the first read token               */

            /* If this comes from an unclosed multi-line comment, style everything accordingly */
            if (inMultiLineComment)
            {
                /* Identify the end of the multi-line comment (either where there is a comment closer or at the end of the string if that is not present) */
                if (currentText.Contains("<="))
                {
                    rightRangePointer   = currentText.IndexOf("<=") + 2;
                    inMultiLineComment  = false;
                }
                else
                {
                    rightRangePointer = currentText.Length;
                }

                /* Build style text object and add it to the words to style buffer */
                styleText = new StyleText
                {
                    Start   = r.ContentStart.GetPositionAtOffset(leftRangePointer),
                    End     = r.ContentStart.GetPositionAtOffset(rightRangePointer),
                    Style   = multiLineCommentStyle
                };

                styledTextRanges.Add(styleText);

                /* Reset lexer to ignore the commented out section */
                currentText         = currentText.Substring(rightRangePointer);
                lexer               = new LexicalAnalyzer(currentText);
                textOffset          = rightRangePointer;
                nextToken           = lexer.GetToken();
                rightRangePointer   = textOffset + lexer.GetTextPosition() + 1;
                leftRangePointer    = rightRangePointer - nextToken.Content.Length;
            }

            /* Find keywords in program by checking each run */
            while (nextToken.Type != TokenType.EOF)
            {
                /* Adjust range pointers if this is a BREAK token */
                if (nextToken.Type == TokenType.BREAK)
                {
                    /* Store the current value of the left range pointer */
                    previousLeftRangePointer = leftRangePointer;

                    /* Move the left range pointer further left until it points to a newline (the first newline in the BREAK token) */
                    while (r.Text[leftRangePointer] != '\n' && r.Text[leftRangePointer] != '\r')
                    {
                        --leftRangePointer;
                    }

                    /* Offset the right range pointer the same amount (add 1 in case a newline is \r\n) */
                    if (previousLeftRangePointer > leftRangePointer)
                    {
                        rightRangePointer -= previousLeftRangePointer - leftRangePointer + 1;
                    }
                }

                /* Adjust range poitners if this is a STRING token */
                if (nextToken.Type == TokenType.STRING)
                {
                    leftRangePointer -= 2; /* Offset for the two quotes in the string */
                }

                /* Add any tokens with a recognized type to the words to style buffer */
                if (tokenTypeStyleDict.ContainsKey(nextToken.Type))
                {
                    styleText = new StyleText
                    {
                        Start   = r.ContentStart.GetPositionAtOffset(leftRangePointer),
                        End     = r.ContentStart.GetPositionAtOffset(rightRangePointer),
                        Style   = tokenTypeStyleDict[nextToken.Type]
                    };

                    styledTextRanges.Add(styleText);
                }

                /* Add any other recognized strings to the words to style buffer */
                else if (wordStyleDict.ContainsKey(nextToken.Content))
                {
                    styleText = new StyleText
                    {
                        Start   = r.ContentStart.GetPositionAtOffset(leftRangePointer),
                        End     = r.ContentStart.GetPositionAtOffset(rightRangePointer),
                        Style   = wordStyleDict[nextToken.Content]
                    };

                    styledTextRanges.Add(styleText);
                }

                /* Get the next token and update the left and right pointers accordingly */
                nextToken           = lexer.GetToken();
                rightRangePointer   = textOffset + lexer.GetTextPosition() + 1;
                leftRangePointer    = rightRangePointer - nextToken.Content.Length;
            }

            /* Style comments */
            foreach (KeyValuePair<int, string> positionCommentPair in lexer.SingleLineCommentStartPositionMap)
            {
                styleText = new StyleText
                {
                    Start = r.ContentStart.GetPositionAtOffset(positionCommentPair.Key),
                    End = r.ContentStart.GetPositionAtOffset(textOffset + positionCommentPair.Key + positionCommentPair.Value.Length),
                    Style = singleLineCommentStyle
                };

                styledTextRanges.Add(styleText);
            }

            foreach (KeyValuePair<int, string> positionCommentPair in lexer.MultiLineCommentStartPositionMap)
            {
                /* Check if the multi-line comment ends */
                inMultiLineComment = !positionCommentPair.Value.Contains("<=");

                styleText = new StyleText
                {
                    Start = r.ContentStart.GetPositionAtOffset(positionCommentPair.Key),
                    End = r.ContentStart.GetPositionAtOffset(textOffset + positionCommentPair.Key + positionCommentPair.Value.Length),
                    Style = multiLineCommentStyle,
                };

                styledTextRanges.Add(styleText);
            }
        }

        private string GetProgramText() /* Get the text currently in the editor */
        {
            string text = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text;
            return text.Substring(0, text.Length - 2); /* Remove final added whitespace character */
        }

        private void ClearDocumentFormatting() /* Remove all current formatting properties */
        {
            /* Local Variables */
            TextRange docRange; /* Text range obeject containing the text of the entire document */
            /* / Local Variables */

            docRange = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            docRange.ClearAllProperties();
            inMultiLineComment = false; /* Reset checker for multi-line comments */
        }

        private void IdentifyTextRangesToStyle() /* Identify all strings to be styled in the document */
        {
            /* Local Variables */
            TextPointer nav; /* Text pointer to the next "content position" in the document */
            /* / Local Variables */

            nav = Editor.Document.ContentStart;

            /* Iterate through each text pointer in the document */
            while (nav.CompareTo(Editor.Document.ContentEnd) < 0)
            {
                /* Document text is only in TextElements that are children to Runs */
                if (nav.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart && nav.Parent is Run)
                {
                    CheckWordsInRun((Run)nav.Parent);
                }

                /* Get next text pointer */
                nav = nav.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        private void AddStylesToDocument() /* Highlight keywords in program */
        {
            /* Local Variables */
            TextRange styleBuffer; /* Text pointer to a string in the document to be styled    */
            /* / Local Variables */

            foreach (StyleText styleTextrange in styledTextRanges)
            {
                /* Find the desired keyword in the text box using the current word's location data */
                styleBuffer = new TextRange(styleTextrange.Start, styleTextrange.End);

                /* Apply every style property from the Style data */
                foreach (KeyValuePair<DependencyProperty, object> style in styleTextrange.Style.GetDict())
                {
                    styleBuffer.ApplyPropertyValue(style.Key, style.Value);
                }
            }
        }

        /*
         *  ---------------- / HELPER FUNCTIONS --------------
        */


        /*
         *  ---------------- TEXT EDITOR STYLE HANDLERS ----------------
        */

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                /* Temporarily disable TextChanged event handler */
                Editor.TextChanged -= Editor_TextChanged;

                /* Clear the buffer that holds the words to be styled */
                styledTextRanges.Clear();

                /* Remove all styles from the document, identify where to replace them, then replace all styles in the document */
                ClearDocumentFormatting();

                /* Only perform syntax highlighting if enabled */
                if (viewSyntaxHighlighting)
                {
                    IdentifyTextRangesToStyle();
                    AddStylesToDocument();
                }

                /* Re-enable TextChanged event handler */
                Editor.TextChanged += Editor_TextChanged;
            }
            /* This shouldn't ever happen */
            catch
            {
                MessageBox.Show(UKNOWN_ISSUE_MESSAGE);
                System.Environment.Exit(1);
            }
        }

        /*
        *   ---------------- / TEXT EDITOR STYLE HANDLERS ----------------
        */


        /*
         *  ---------------- MENU BAR CLICK HANDLERS ----------------
        */

        private void File_New_Click(object sender, RoutedEventArgs e) /* Clear text, directory, and compile information */
        {
            /* Clear text */
            Editor.Document.Blocks.Clear();
            savedFilename = null; /* Clear the saved file buffer to always prompt a filename on first save */
        }

        private void File_Open_Click(object sender, RoutedEventArgs e) /* Open a file's content and store it's location */
        {
            /* Local Variables */
            OpenFileDialog  ofDialog;           /* File chooser to open a file                  */
            bool?           validFileRequested; /* Was a valid file selected from the dialog?   */
            string          fileText;           /* Text inside a valid selected file            */
            string          filename;           /* Name of the valid selected file              */
            /* / Local Variables */

            /* Open a Musika file */
            ofDialog = new OpenFileDialog()
            {
                /* Set up the open file dialog */
                InitialDirectory = currentDirectory,
                Filter = MUSIKA_FILE_FILTER,
                RestoreDirectory = true             /* Set the default open directory to the location of the last valid open */
            };

            /* If a valid file was requested to open, store the filename */
            validFileRequested = ofDialog.ShowDialog();
            filename = (validFileRequested == true) ? ofDialog.FileName : null;

            /* If a filename was stored, open the file */
            if (filename != null)
            {
                /* Get the text from the file */
                fileText = File.ReadAllText(filename);

                /* Perform a New operation, then add the new text */
                File_New_Click(sender, e); /* Use "New" command to clear */
                Editor.Document.Blocks.Add(new Paragraph(new Run(fileText)));

                /* Set the new directory to the directory of the opened file */
                currentDirectory = Path.GetDirectoryName(filename);
                savedFilename = Path.GetFileName(filename);
            }
        }

        private void File_Save_Click(object sender, RoutedEventArgs e) /* Save the editor's text content to the stored location */
        {
            /* Re-save a saved file or save a new one */
            if (currentDirectory != null && savedFilename != null)
            {
                File.WriteAllText(Path.Combine(currentDirectory, savedFilename), GetProgramText());
            }

            /* This is an unsaved file, so treat this function just like Save As */
            else
            {
                File_SaveAs_Click(sender, e);
            }
        }

        private void File_SaveAs_Click(object sender, RoutedEventArgs e) /* Prompt the user for a save location/file and performa a Save operation */
        {
            /* Local Variables */
            SaveFileDialog  dialog;         /* File chooser to select a location/file name to save to   */
            bool?           fileSelected;   /* Was a valid file selected?                               */
            /* / Local Variables */

            /* File or create a file to save to */
            dialog = new SaveFileDialog()
            {
                Filter = MUSIKA_FILE_FILTER
            };

            fileSelected = dialog.ShowDialog();

            /* Store the file name and location to save more quickly later */
            if (fileSelected == true)
            {
                currentDirectory = Path.GetDirectoryName(dialog.FileName);
                savedFilename = Path.GetFileName(dialog.FileName);

                File_Save_Click(sender, e); /* Perform a Save operation */
            }
        }

        private void File_Exit_Click(object sender, RoutedEventArgs e) /* Quit the application */
        {
            /* Exit the program */
            System.Environment.Exit(0);
        }

        private void View_ToggleSyntaxHighlighting_Click(object sender, RoutedEventArgs e)
        {
            /* Show warning message if toggling syntax highlighting from OFF to ON */
            if (!viewSyntaxHighlighting)
            {
                MessageBox.Show(VIEW_TOGGLE_SYNTAX_HIGHLIGHTING_WARNING);
            }

            /* Invert toggle flag */
            viewSyntaxHighlighting = !viewSyntaxHighlighting;

            /* Invoke TextChanged event to turn syntax highlighting on */
            Editor_TextChanged(sender, null);
        }

        private void Build_BuildNoteSheet_Click(object sender, RoutedEventArgs e) /* Build the code as a note sheet instance */
        {
            /* Local Variables */
            Compiler    compiler;       /* Object to compile source code down to a note sheet   */
            string      outputMessage;  /* Message to ouput status to user                      */
            /* / Local Variables */

            outputMessage = null;

            /* Attempt to build current file */
            try
            {
                /* Get the current file */
                if (currentDirectory != null && savedFilename != null)
                {
                    /* Build source code and save/serialize binary if build is successful */
                    compiler = new Compiler(currentDirectory, savedFilename);
                    compiler.CompileToNoteSheet();

                    /* Build success output message */
                    outputMessage = "BUILD SUCCESSFUL\n\nCompiled sheet saved to " + Path.ChangeExtension(savedFilename, NOTESHEET_FILE_EXT);
                }
                else
                {
                    /* No file was selected */
                    outputMessage = "Cannot build: No file is selected. Please open and/or save a file.";
                }
            }

            /* Syntax Error thrown by parser */
            catch (SyntaxError se)
            {
                outputMessage = "SYNTAX ERROR\n\n" + se.Message;
            }

            /* Context Error thrown by parser */
            catch (ContextError ce)
            {
                outputMessage = "CONTEXTUAL ERROR\n\n" + ce.Message;
            }

            catch
            {
                outputMessage = "UNKNOWN ERROR\n\n" + "An unkown error occurred when constructing your WAV file. Please consult your nearest tech support engineer.";
            }

            /* Output message to user */
            finally
            {
                if (outputMessage != null)
                {
                    MessageBox.Show(outputMessage);
                }
            }
        }

        private void Build_BuildWAVFile_Click(object sender, RoutedEventArgs e) /* Construct a note sheet as a WAV file */
        {
            /* Local Variables */
            Compiler    compiler;               /* Object to compile source code down to a note sheet   */
            string      noteSheetFilename;      /* Name + extension of the note sheet file              */
            string      noteSheetFileAddress;   /* Location of the note sheet file                      */
            string      outputMessage;          /* Message to display to the user after build           */
            /* / Local Variables */

            outputMessage = null;

            try
            {
                /* Make sure the Note Sheet exists first */
                if (currentDirectory != null && savedFilename != null)
                {
                    noteSheetFilename = Path.ChangeExtension(savedFilename, NOTESHEET_FILE_EXT);
                    noteSheetFileAddress = Path.Combine(currentDirectory, noteSheetFilename);

                    /* Build the note sheet if it does not exist */
                    if (File.Exists(noteSheetFileAddress) == false)
                    {
                        Build_BuildNoteSheet_Click(sender, e);
                    }

                    /* If it does not exist after initial check, there was a build error */
                    if (File.Exists(noteSheetFileAddress) == true)
                    {
                        /* Compile note sheet to WAV file */
                        compiler = new Compiler(currentDirectory, savedFilename);

                        compiler.CompileToWAV();
                        outputMessage = "WAV CONSTRUCTION SUCCESSFUL\n\nFile has been saved to " + Path.Combine(currentDirectory, Path.ChangeExtension(savedFilename, WAV_FILE_EXT));
                    }
                }
                else
                {
                    /* If the directory and/or file name do not exist, the file has not been saved yet. */
                    /* Perform a Build Note Sheet operation to show that error message                  */
                    Build_BuildNoteSheet_Click(sender, e);
                }
            }

            /* There is no audio to play, so no WAV can be constructed */
            catch (WAVConstructionError wce)
            {
                outputMessage += "WAV CONSTRUCTION FAILED\n\n" + wce.Message;
            }

            catch
            {
                /* SHOULD NEVER OCCURR UNLESS SOMETHING WENT HORRIBLY WRONG */
                outputMessage += "WAV CONSTRUCTION FAILED\n\n" + "An unkown error occurred when constructing your WAV file. Please consult your nearest tech support engineer.";
            }

            /* Show output message if it exists */
            finally
            {
                if (outputMessage != null)
                {
                    MessageBox.Show(outputMessage);
                }
            }
        }

        private void Play_PlaySong_Click(object sender, RoutedEventArgs e) /* Play the compiled WAV file */
        {
            /* Local Variables */
            string wavFileAddress;  /* Location of the WAV file                     */
            string outputMessage;   /* Message to display to the user after build   */
            /* / Local Variables */

            outputMessage = null;

            /* Make sure the file is saved */
            try
            {
                if (currentDirectory != null && savedFilename != null)
                {
                    wavFileAddress = Path.Combine(currentDirectory, Path.ChangeExtension(savedFilename, WAV_FILE_EXT));

                    /* Make sure the WAV file exists */
                    if (File.Exists(wavFileAddress))
                    {
                        songPlayer = new SongPlayer(currentDirectory, savedFilename);
                        songPlayer.PlayWAVFile();
                    }
                    else
                    {
                        outputMessage = "Must create a WAV file to play first!";
                    }
                }
                else
                {
                    outputMessage = "Please save the file before playing the song!";
                }
            }

            /* Handle any error playing the WAV music */
            catch
            {
                outputMessage = "Song Play Failed\n\nThere was a problem playing the WAV file. Please check if the WAV file is corrupted";
            }

            /* Show Ouput Message if it exists */
            finally
            {
                if (outputMessage != null)
                {
                    MessageBox.Show(outputMessage);
                }
            }
        }

        private void Play_StopSong_Click(object sender, RoutedEventArgs e) /* Stop a song that is currently playing */
        {
            /* Stop playnig any song that is currently playing */
            if (songPlayer != null)
            {
                songPlayer.StopWAVFile();
            }
        }

        /*
         *  ---------------- / MENU BAR CLICK HANDLERS ----------------
        */
    }
    /* / MAIN CLASS */
}
