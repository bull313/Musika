using System;
using System.Collections.Generic;
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

        internal Style()
        {
            styleDict = new Dictionary<DependencyProperty, object>();
        }

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
        private Dictionary<string, Style> wordStyleDict; /* Determines which words are styled and how they are styled */
        private List<StyleText> keywordBuffer = new List<StyleText>(); /* Used to keep track of the keywords in in the text box */

        private static string currentText; /* Buffer to store Run text that will be checked for keywords */

        /* Style text struct that includes its location, content, and style */
        private struct StyleText
        {
            internal TextPointer Start, End;
            internal string Word;
            internal Style Style;
        }
        /* / Style text struct that includes its location, content, and style */

        public MainWindow()
        {
            InitializeComponent();

            /* Create Styles */
            Style tier1 = new Style(); /* Tier-1 keywords */
            tier1.AddColor(Colors.Red);
            tier1.AddWeight(FontWeights.Bold);

            Style tier2 = new Style(); /* Tier-2 keywords */
            tier2.AddColor(Colors.OrangeRed);

            Style tier3 = new Style(); /* Tier-3 keywords */
            tier3.AddColor(Colors.Green);

            Style breakStyle = new Style(); /* Break token (---) */
            breakStyle.AddColor(Colors.Olive);
            breakStyle.AddWeight(FontWeights.Bold);

            Style signStyle = new Style(); /* Key signature keywords */
            signStyle.AddColor(Colors.SandyBrown);

            Style noteStyle = new Style(); /* Note keywords */
            noteStyle.AddColor(Colors.CadetBlue);
            /* / Create Styles */

            wordStyleDict = new Dictionary<string, Style>();

            /* Add Tier 1 Keywords to Dictionary */
            wordStyleDict.Add("accompany",    tier1);
            wordStyleDict.Add("name",         tier1);
            wordStyleDict.Add("author",       tier1);
            wordStyleDict.Add("coauthors",    tier1);
            wordStyleDict.Add("title",        tier1);
            wordStyleDict.Add("key",          tier1);
            wordStyleDict.Add("time",         tier1);
            wordStyleDict.Add("tempo",        tier1);
            wordStyleDict.Add("octave",       tier1);
            wordStyleDict.Add("pattern",      tier1);
            wordStyleDict.Add("chord",        tier1);
            wordStyleDict.Add("is",           tier1);

            /* Add Tier 2 Keywords to Dictionary */
            wordStyleDict.Add("common", tier2);
            wordStyleDict.Add("cut", tier2);

            /* Add Tier 3 Keywords to Dictionary */
            wordStyleDict.Add("repeat", tier3);
            wordStyleDict.Add("layer", tier3);

            /* The break token to Dictionary */
            wordStyleDict.Add("---", breakStyle);

            /* Add Key signatures to Dictionary */
            wordStyleDict.Add("Cmaj",   signStyle);
            wordStyleDict.Add("Gmaj",   signStyle);
            wordStyleDict.Add("Dmaj",   signStyle);
            wordStyleDict.Add("Amaj",   signStyle);
            wordStyleDict.Add("Emaj",   signStyle);
            wordStyleDict.Add("Bmaj",   signStyle);
            wordStyleDict.Add("F#maj",  signStyle);
            wordStyleDict.Add("C#maj",  signStyle);
            wordStyleDict.Add("Fmaj",   signStyle);
            wordStyleDict.Add("Bbmaj",  signStyle);
            wordStyleDict.Add("Ebmaj",  signStyle);
            wordStyleDict.Add("Abmaj",  signStyle);
            wordStyleDict.Add("Cm",     signStyle);
            wordStyleDict.Add("Gm",     signStyle);
            wordStyleDict.Add("Dm",     signStyle);
            wordStyleDict.Add("Am",     signStyle);
            wordStyleDict.Add("Em",     signStyle);
            wordStyleDict.Add("Bm",     signStyle);
            wordStyleDict.Add("F#m",    signStyle);
            wordStyleDict.Add("C#m",    signStyle);
            wordStyleDict.Add("Fm",     signStyle);
            wordStyleDict.Add("Bbm",    signStyle);
            wordStyleDict.Add("Ebm",    signStyle);
            wordStyleDict.Add("Abm",    signStyle);

            /* Add Notes to Dictionary */
            wordStyleDict.Add("A",      noteStyle);
            wordStyleDict.Add("B",      noteStyle);
            wordStyleDict.Add("C",      noteStyle);
            wordStyleDict.Add("D",      noteStyle);
            wordStyleDict.Add("E",      noteStyle);
            wordStyleDict.Add("F",      noteStyle);
            wordStyleDict.Add("G",      noteStyle);
            wordStyleDict.Add("A#",     noteStyle);
            wordStyleDict.Add("B#",     noteStyle);
            wordStyleDict.Add("C#",     noteStyle);
            wordStyleDict.Add("D#",     noteStyle);
            wordStyleDict.Add("E#",     noteStyle);
            wordStyleDict.Add("F#",     noteStyle);
            wordStyleDict.Add("G#",     noteStyle);
            wordStyleDict.Add("Ab",     noteStyle);
            wordStyleDict.Add("Bb",     noteStyle);
            wordStyleDict.Add("Cb",     noteStyle);
            wordStyleDict.Add("Db",     noteStyle);
            wordStyleDict.Add("Eb",     noteStyle);
            wordStyleDict.Add("Fb",     noteStyle);
            wordStyleDict.Add("Gb",     noteStyle);
            wordStyleDict.Add("A$",     noteStyle);
            wordStyleDict.Add("B$",     noteStyle);
            wordStyleDict.Add("C$",     noteStyle);
            wordStyleDict.Add("D$",     noteStyle);
            wordStyleDict.Add("E$",     noteStyle);
            wordStyleDict.Add("F$",     noteStyle);
            wordStyleDict.Add("G$",     noteStyle);
            wordStyleDict.Add("A*",     noteStyle);
            wordStyleDict.Add("B*",     noteStyle);
            wordStyleDict.Add("C*",     noteStyle);
            wordStyleDict.Add("D*",     noteStyle);
            wordStyleDict.Add("E*",     noteStyle);
            wordStyleDict.Add("F*",     noteStyle);
            wordStyleDict.Add("G*",     noteStyle);
            wordStyleDict.Add("Abb",    noteStyle);
            wordStyleDict.Add("Bbb",    noteStyle);
            wordStyleDict.Add("Cbb",    noteStyle);
            wordStyleDict.Add("Dbb",    noteStyle);
            wordStyleDict.Add("Ebb",    noteStyle);
            wordStyleDict.Add("Fbb",    noteStyle);
            wordStyleDict.Add("Gbb",    noteStyle);
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
                    word = word.Substring(0, word.Length - 1);
            }
            return null;
        }

        private void CheckWordsInRun(Run r) /* Scans a run for keywords and stores the keywords with their information in the buffer */
        {
            int startIndex = 0, endIndex = 0;
            string HasStyleReturn;

            /* Find special words */
            for (int i = 0; i < currentText.Length; ++i)
            {
                if (char.IsWhiteSpace(currentText[i]))
                {
                    if (i > 0 && !(char.IsWhiteSpace(currentText[i - 1]))) /* Check to see if we're at the end of a word
                                                                            (because we're on whitespace and the character before is not whitespace) */
                    {
                        endIndex = i - 1;
                        string word = currentText.Substring(startIndex, endIndex - startIndex + 1);

                        HasStyleReturn = HasStyle(word);
                        if (HasStyleReturn != null) /* If the word is marked to be styled */
                        {
                            StyleText styleText = new StyleText();
                            styleText.Start = r.ContentStart.GetPositionAtOffset(startIndex, LogicalDirection.Forward); /* Get starting position in textbox */

                            if (HasStyleReturn == word) /* Get ending position in textbox */
                                styleText.End = r.ContentStart.GetPositionAtOffset(endIndex + 1, LogicalDirection.Backward);
                            else
                                styleText.End = r.ContentStart.GetPositionAtOffset(endIndex + 1 - (word.Length - HasStyleReturn.Length), LogicalDirection.Backward);

                            styleText.Word = word;
                            styleText.Style = wordStyleDict[HasStyleReturn]; /* Store the word itself and the styler instance (lookup from dictionary) */

                            keywordBuffer.Add(styleText); /* Add the new StyleText instance containing the word's position, content, and style to the buffer */
                        }
                    }
                    startIndex = i + 1;
                }
            }

            string lastWord = currentText.Substring(startIndex, currentText.Length - startIndex); /* The previous loop doesn't deal with the last word */
            HasStyleReturn = HasStyle(lastWord); /* Process is the same as above */
            if (HasStyleReturn != null)
            {
                StyleText styleText = new StyleText();
                styleText.Start = r.ContentStart.GetPositionAtOffset(startIndex, LogicalDirection.Forward);
                if (HasStyleReturn == lastWord)
                    styleText.End = r.ContentStart.GetPositionAtOffset(endIndex + 1, LogicalDirection.Backward);
                else
                    styleText.End = r.ContentStart.GetPositionAtOffset(endIndex + 1 - (lastWord.Length - HasStyleReturn.Length), LogicalDirection.Backward);
                styleText.Word = lastWord;
                styleText.Style = wordStyleDict[HasStyleReturn];
                keywordBuffer.Add(styleText);
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
            if (Editor.Document == null) return; /* Don't do anything if the document is not set up yet */

            Editor.TextChanged -= Editor_TextChanged; /* Temporarily disable TextChanged event handler */

            keywordBuffer.Clear(); /* Clear the buffer that holds the words to be styled */

            TextRange docRange = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            docRange.ClearAllProperties(); /* Remove all formatting properties (to be readded) */

            /* Find keywords in program */
            TextPointer nav = Editor.Document.ContentStart;
            while (nav.CompareTo(Editor.Document.ContentEnd) < 0)
            {
                TextPointerContext ctxt = nav.GetPointerContext(LogicalDirection.Backward);
                if (ctxt == TextPointerContext.ElementStart && nav.Parent is Run)
                {
                    currentText = ((Run)nav.Parent).Text;
                    if (currentText != "") /* Only check words if the Run is not empty */
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
                    TextRange range = new TextRange(keywordBuffer[i].Start, keywordBuffer[i].End); /* Find the desired keyword in the text box using the current word's location data */
                    foreach (KeyValuePair<DependencyProperty, object> style in keywordBuffer[i].Style.GetDict())
                        range.ApplyPropertyValue(style.Key, style.Value); /* Apply every style property from the Style data */
                }
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
            Editor.Document.Blocks.Clear();
        }

        /*
         *  ---------------- / MENU BAR CLICK HANDLERS ----------------
        */
    }
    /* / MAIN CLASS */
}
