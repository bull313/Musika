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
    /* STYLE CLASS TO STYLE SPECIAL WORDS/CHARS */
    internal class Style
    {
        private Dictionary<DependencyProperty, object> styleDict;

        /* CONSTRUCTOR */
        internal Style()
        {
            styleDict = new Dictionary<DependencyProperty, object>();
        }
        /* / CONSTRUCTOR */

        internal void AddColor(Color c)
        {
            AddStyle(TextElement.ForegroundProperty, new SolidColorBrush(c));
        }

        internal void AddWeight(FontWeight f)
        {
            AddStyle(TextElement.FontWeightProperty, f);
        }

        internal void AddStyle(DependencyProperty dp, object val)
        {
            styleDict.Add(dp, val);
        }

        internal Dictionary<DependencyProperty, object> GetDict()
        {
            return styleDict;
        }
    }
    /* / STYLE CLASS TO STYLE SPECIAL WORDS/CHARS */

    /* MAIN CLASS */
    public partial class MainWindow : Window
    {
        /* CONSTANTS */
        public const int EXIT_OK_CODE = 0;                                              /* Application quitting on its own with no issues   */
        public const string MUSIKA_FILE_FILTER = "Musika Files(*.ka)|*.ka|All(*.*)|*";  /* Filters Musika files in file dialogs             */
        public const string NOTESHEET_FILE_EXT = ".mkc";                                /* Serialized note sheet file extension             */
        public const string WAV_FILE_EXT = ".wav";                                      /* WAV audio file extension                         */
        /*/CONSTANTS */


        /* PROPERTIES */
        private readonly Dictionary<string, Style>      wordStyleDict;      /* Determines which words are styled and how they are styled            */
        private readonly List<StyleText>                keywordBuffer;      /* Used to keep track of the keywords in in the text box                */

        private SongPlayer                              songPlayer;         /* Environment object to play a WAV song                                */
        private string                                  currentDirectory;   /* Keeps track of the directory of the current file                     */
        private string                                  currentText;        /* Buffer to store Run text that will be checked for keywords           */
                                                                            /* (initialized to the bin directory                                    */
        private string                                  savedFilename;      /* Name of file we are writing to                                       */
        /* / PROPERTIES */


        /* Style text struct that includes its location, content, and style */
        private struct StyleText
        {
            internal TextPointer Start, End;
            internal Style       Style;
            internal string      Word;
        }

        public MainWindow()
        {
            InitializeComponent();
            keywordBuffer = new List<StyleText>();

            /* Create Styles */

            /* Tier-1 keywords */
            Style tier1 = new Style();
            tier1.AddColor(Colors.Red);
            tier1.AddWeight(FontWeights.Bold);

            /* Tier-2 keywords */
            Style tier2 = new Style();
            tier2.AddColor(Colors.OrangeRed);

            /* Tier-3 keywords */
            Style tier3 = new Style();
            tier3.AddColor(Colors.Green);

            /* Break token (---) */
            Style breakStyle = new Style();
            breakStyle.AddColor(Colors.Olive);
            breakStyle.AddWeight(FontWeights.Bold);

            /* Key signature keywords */
            Style signStyle = new Style();
            signStyle.AddColor(Colors.SandyBrown);

            /* Note keywords */
            Style noteStyle = new Style();
            noteStyle.AddColor(Colors.CadetBlue);

            /* Populate dictionary */
            wordStyleDict = new Dictionary<string, Style>()
            {
                /* Tier 1 Keywords */
                {"accompany", tier1}, {"name",      tier1}, {"author",    tier1}, {"coauthors", tier1}, {"title",     tier1}, {"key",       tier1},
                {"time",      tier1}, {"tempo",     tier1}, {"octave",    tier1}, {"pattern",   tier1}, {"chord",     tier1}, {"is",        tier1},

                /* Tier 2 Keywords */
                {"common", tier2}, {"cut", tier2},

                /* Tier 3 Keywords */
                {"repeat", tier3}, {"layer", tier3},

                /* BREAK Token */
                {"---", breakStyle},

                /* Key Signatures */
                {"Cmaj",  signStyle}, {"Gmaj",  signStyle}, {"Dmaj",  signStyle}, {"Amaj",  signStyle}, {"Emaj",  signStyle}, {"Bmaj",  signStyle},
                {"F#maj", signStyle}, {"C#maj", signStyle}, {"Fmaj",  signStyle}, {"Bbmaj", signStyle}, {"Ebmaj", signStyle}, {"Abmaj", signStyle},
                {"Cm",    signStyle}, {"Gm",    signStyle}, {"Dm",    signStyle}, {"Am",    signStyle}, {"Em",    signStyle}, {"Bm",    signStyle},
                {"F#m",   signStyle}, {"C#m",   signStyle}, {"Fm",    signStyle}, {"Bbm",   signStyle}, {"Ebm",   signStyle}, {"Abm",   signStyle},

                /* Notes */
                {"A",   noteStyle}, {"B",   noteStyle}, {"C",   noteStyle}, {"D",   noteStyle}, {"E",   noteStyle}, {"F",   noteStyle}, {"G",   noteStyle},
                {"A#",  noteStyle}, {"B#",  noteStyle}, {"C#",  noteStyle}, {"D#",  noteStyle}, {"E#",  noteStyle}, {"F#",  noteStyle}, {"G#",  noteStyle},
                {"Ab",  noteStyle}, {"Bb",  noteStyle}, {"Cb",  noteStyle}, {"Db",  noteStyle}, {"Eb",  noteStyle}, {"Fb",  noteStyle}, {"Gb",  noteStyle},
                {"A$",  noteStyle}, {"B$",  noteStyle}, {"C$",  noteStyle}, {"D$",  noteStyle}, {"E$",  noteStyle}, {"F$",  noteStyle}, {"G$",  noteStyle},
                {"A*",  noteStyle}, {"B*",  noteStyle}, {"C*",  noteStyle}, {"D*",  noteStyle}, {"E*",  noteStyle}, {"F*",  noteStyle}, {"G*",  noteStyle},
                {"Abb", noteStyle}, {"Bbb", noteStyle}, {"Cbb", noteStyle}, {"Dbb", noteStyle}, {"Ebb", noteStyle}, {"Fbb", noteStyle}, {"Gbb", noteStyle}
            };
        }

        /*
         *  ---------------- HELPER FUNCTIONS ----------------
        */

        private string HasStyle(string word) /* Checks if a word is marked to be styled in the dictionary */
        {
            while (word != "")
            {
                if (wordStyleDict.ContainsKey(word))
                    return word;
                else
                    word = word.Substring(0, word.Length - 1); /* We are checking every prefix of the word too in case there are symbols following the keyword (e.g. title:) */
            }
            return null;
        }

        private void CheckWordsInRun(Run r) /* Scans a run for keywords and stores the keywords with their information in the buffer */
        {
            /* Local Variables */
            StyleText styleText;
            int       startIndex;
            int       endIndex;
            int       i;
            string    hasStyleReturn;
            string    word;
            string    lastWord;
            /* / Local Variables */

            startIndex = 0;
            endIndex   = 0;

            /* Find special words */
            for (i = 0; i < currentText.Length; ++i)
            {
                if (char.IsWhiteSpace(currentText[i]))
                {
                    /* Check to see if we're at the end of a word (because we're on whitespace and the character before is not whitespace) */
                    if (i > 0 && !(char.IsWhiteSpace(currentText[i - 1])))
                    {
                        endIndex = i - 1;
                        word = currentText.Substring(startIndex, endIndex - startIndex + 1);

                        hasStyleReturn = HasStyle(word);

                        /* If the word is marked to be styled */
                        if (hasStyleReturn != null)
                        {
                            styleText = new StyleText();
                            styleText.Start = r.ContentStart.GetPositionAtOffset(startIndex, LogicalDirection.Forward); /* Get starting position in textbox */

                            /* Get ending position in textbox */
                            if (hasStyleReturn == word)
                                styleText.End = r.ContentStart.GetPositionAtOffset(endIndex + 1, LogicalDirection.Backward);
                            else
                                styleText.End = r.ContentStart.GetPositionAtOffset(endIndex + 1 - (word.Length - hasStyleReturn.Length), LogicalDirection.Backward);

                            /* Store the word itself and the styler instance (lookup from dictionary) */
                            styleText.Word = word;
                            styleText.Style = wordStyleDict[hasStyleReturn];

                            /* Add the new StyleText instance containing the word's position, content, and style to the buffer */
                            keywordBuffer.Add(styleText);
                        }
                    }
                    startIndex = i + 1;
                }
            }

            /* The previous loop doesn't deal with the last word */
            lastWord = currentText.Substring(startIndex, currentText.Length - startIndex);

            /* Process is the same as above */
            hasStyleReturn = HasStyle(lastWord);
            if (hasStyleReturn != null)
            {
                styleText = new StyleText();
                styleText.Start = r.ContentStart.GetPositionAtOffset(startIndex, LogicalDirection.Forward);
                if (hasStyleReturn == lastWord)
                    styleText.End = r.ContentStart.GetPositionAtOffset(endIndex + 1, LogicalDirection.Backward);
                else
                    styleText.End = r.ContentStart.GetPositionAtOffset(endIndex + 1 - (lastWord.Length - hasStyleReturn.Length), LogicalDirection.Backward);
                styleText.Word = lastWord;
                styleText.Style = wordStyleDict[hasStyleReturn];
                keywordBuffer.Add(styleText);
            }
        }

        public string GetProgramText()
        {
            string text = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text;
            return text.Substring(0, text.Length - 2); /* Remove final added whitespace character */
        }

        /*
         *  ---------------- / HELPER FUNCTIONS --------------
        */


        /*
         *  ---------------- TEXT EDITOR STYLE HANDLERS ----------------
        */

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            /* Don't do anything if the document is not set up yet */
            if (Editor.Document == null) return;

            /* Temporarily disable TextChanged event handler */
            Editor.TextChanged -= Editor_TextChanged;

            /* Clear the buffer that holds the words to be styled */
            keywordBuffer.Clear();

            /* Remove all current formatting properties */
            TextRange docRange = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            docRange.ClearAllProperties();

            /* Find keywords in program by checking each run */
            TextPointer nav = Editor.Document.ContentStart;
            while (nav.CompareTo(Editor.Document.ContentEnd) < 0)
            {
                TextPointerContext ctxt = nav.GetPointerContext(LogicalDirection.Backward);
                if (ctxt == TextPointerContext.ElementStart && nav.Parent is Run)
                {
                    currentText = ((Run)nav.Parent).Text;

                    /* Only check words if the Run is not empty */
                    if (currentText != "")
                        CheckWordsInRun((Run)nav.Parent);
                }
                nav = nav.GetNextContextPosition(LogicalDirection.Forward);
            }
            /* / Find keywords in program */

            /* Highlight keywords in program */
            for (int i = 0; i < keywordBuffer.Count; ++i)
            {
                try
                {
                    /* Find the desired keyword in the text box using the current word's location data */
                    TextRange range = new TextRange(keywordBuffer[i].Start, keywordBuffer[i].End);

                    /* Apply every style property from the Style data */
                    foreach (KeyValuePair<DependencyProperty, object> style in keywordBuffer[i].Style.GetDict())
                        range.ApplyPropertyValue(style.Key, style.Value);
                }

                /* This shouldn't ever happen */
                catch
                {
                    MessageBox.Show("There was a problem with the editor's color highlighting feature\nSorry for the inconvenience....");
                    System.Environment.Exit(1);
                }
            }
            /* / Highlight keywords in program */

            Editor.TextChanged += Editor_TextChanged; /* Re-enable TextChanged event handler */
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
            OpenFileDialog ofDialog;    /* File chooser to open a file                  */
            bool? validFileRequested;   /* Was a valid file selected from the dialog?   */
            string fileText;            /* Text inside a valid selected file            */
            string filename;            /* Name of the valid selected file              */
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
            SaveFileDialog dialog;  /* File chooser to select a location/file name to save to   */
            bool? fileSelected;     /* Was a valid file selected?                               */
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

        private void Build_BuildNoteSheet_Click(object sender, RoutedEventArgs e) /* Build the code as a note sheet instance */
        {
            /* Local Variables */
            Compiler compiler;      /* Object to compile source code down to a note sheet   */
            string outputMessage;   /* Message to ouput status to user                      */
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
            Compiler compiler;              /* Object to compile source code down to a note sheet   */
            string noteSheetFilename;       /* Name + extension of the note sheet file              */
            string noteSheetFileAddress;    /* Location of the note sheet file                      */
            string outputMessage;           /* Message to display to the user after build           */
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
