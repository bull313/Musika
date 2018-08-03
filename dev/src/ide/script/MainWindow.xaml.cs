using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ide
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
        private Dictionary<string, Style> wordStyleDict; /* Determines which words are styled and how they are styled   */
        private List<StyleText>           keywordBuffer; /* Used to keep track of the keywords in in the text box       */
        private string                    savedFilename; /* Name of file we are writing to                              */
        private string                    currentText;   /* Buffer to store Run text that will be checked for keywords  */

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

        private void File_Exit_Click(object sender, RoutedEventArgs e)
        {
            /* Exit the program */
            System.Environment.Exit(0);
        }

        private void File_New_Click(object sender, RoutedEventArgs e)
        {
            /* Clear text */
            Editor.Document.Blocks.Clear();
            savedFilename = null; /* Clear the saved file buffer to always prompt a filename on first save */
        }

        private void File_Save_Click(object sender, RoutedEventArgs e)
        {
            /* Re-save a saved file or save a new one */
            if (savedFilename != null)
                File.WriteAllText(savedFilename, GetProgramText());

            /* This is an unsaved file, so treat this function just like Save As */
            else
                File_SaveAs_Click(sender, e);
        }

        private void File_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            /* Save a new file */
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "Musika Files(*.ka)|*.ka|All(*.*)|*"
            };

            bool? fileSelected = dialog.ShowDialog();

            if (fileSelected == true)
            {
                savedFilename = dialog.FileName; /* Store the file name into a variable to save more quickly later */
                File.WriteAllText(savedFilename, GetProgramText());
            }
        }

        /*
         *  ---------------- / MENU BAR CLICK HANDLERS ----------------
        */
    }
    /* / MAIN CLASS */
}
