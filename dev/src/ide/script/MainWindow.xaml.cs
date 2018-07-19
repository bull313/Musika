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
    /* Style class to style special words/chars */
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

    public partial class MainWindow : Window
    {
        private static Dictionary<string, Style> wordStyleDict;
        private List<Tag> colorTagList = new List<Tag>();

        private static string program;

        new private struct Tag
        {
            internal TextPointer Start, End;
            internal string Word;
            internal Style Style;
        }

        static MainWindow()
        {
            /* Create Styles */
            Style tier1 = new Style();
            tier1.AddColor(Colors.Red);
            tier1.AddWeight(FontWeights.Bold);

            Style tier2 = new Style();
            tier2.AddColor(Colors.OrangeRed);

            Style tier3 = new Style();
            tier3.AddColor(Colors.Green);

            Style breakStyle = new Style();
            breakStyle.AddColor(Colors.Olive);
            breakStyle.AddWeight(FontWeights.Bold);

            Style signStyle = new Style();
            signStyle.AddColor(Colors.SandyBrown);

            Style noteStyle = new Style();
            noteStyle.AddColor(Colors.CadetBlue);

            wordStyleDict = new Dictionary<string, Style>();

            /* Tier 1 Keywords */
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

            /* Tier 2 Keywords */
            wordStyleDict.Add("common", tier2);
            wordStyleDict.Add("cut", tier2);

            /* Tier 2 Keywords */
            wordStyleDict.Add("repeat", tier3);
            wordStyleDict.Add("layer", tier3);

            /* The break token */
            wordStyleDict.Add("---", breakStyle);

            /* Key signatures */
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

            /* Notes */
            wordStyleDict.Add("A", noteStyle);
            wordStyleDict.Add("B", noteStyle);
            wordStyleDict.Add("C", noteStyle);
            wordStyleDict.Add("D", noteStyle);
            wordStyleDict.Add("E", noteStyle);
            wordStyleDict.Add("F", noteStyle);
            wordStyleDict.Add("G", noteStyle);
            wordStyleDict.Add("A#", noteStyle);
            wordStyleDict.Add("B#", noteStyle);
            wordStyleDict.Add("C#", noteStyle);
            wordStyleDict.Add("D#", noteStyle);
            wordStyleDict.Add("E#", noteStyle);
            wordStyleDict.Add("F#", noteStyle);
            wordStyleDict.Add("G#", noteStyle);
            wordStyleDict.Add("Ab", noteStyle);
            wordStyleDict.Add("Bb", noteStyle);
            wordStyleDict.Add("Cb", noteStyle);
            wordStyleDict.Add("Db", noteStyle);
            wordStyleDict.Add("Eb", noteStyle);
            wordStyleDict.Add("Fb", noteStyle);
            wordStyleDict.Add("Gb", noteStyle);
            wordStyleDict.Add("A$", noteStyle);
            wordStyleDict.Add("B$", noteStyle);
            wordStyleDict.Add("C$", noteStyle);
            wordStyleDict.Add("D$", noteStyle);
            wordStyleDict.Add("E$", noteStyle);
            wordStyleDict.Add("F$", noteStyle);
            wordStyleDict.Add("G$", noteStyle);
            wordStyleDict.Add("A*", noteStyle);
            wordStyleDict.Add("B*", noteStyle);
            wordStyleDict.Add("C*", noteStyle);
            wordStyleDict.Add("D*", noteStyle);
            wordStyleDict.Add("E*", noteStyle);
            wordStyleDict.Add("F*", noteStyle);
            wordStyleDict.Add("G*", noteStyle);
            wordStyleDict.Add("Abb", noteStyle);
            wordStyleDict.Add("Bbb", noteStyle);
            wordStyleDict.Add("Cbb", noteStyle);
            wordStyleDict.Add("Dbb", noteStyle);
            wordStyleDict.Add("Ebb", noteStyle);
            wordStyleDict.Add("Fbb", noteStyle);
            wordStyleDict.Add("Gbb", noteStyle);
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        /*
         *  ---------------- HELPER FUNCTIONS ----------------
        */

        private string ContainsKey(string word)
        {
            if (word.Length > 0 && !char.IsLetterOrDigit(word[word.Length - 1]) && word[word.Length - 1] != '"')
            {
                if (wordStyleDict.ContainsKey(word.Substring(0, word.Length - 1)))
                    return word.Substring(0, word.Length - 1);
                else
                    return null;
            }
            else
            {
                if (wordStyleDict.ContainsKey(word))
                    return word;
                else return null;
            }
        }

        private void CheckWordsInRun(Run r)
        {
            int startIndex = 0, endIndex = 0;
            string keyResult;

            /* Find special words */
            for (int i = 0; i < program.Length; ++i)
            {
                if (char.IsWhiteSpace(program[i]))
                {
                    if (i > 0 && !(char.IsWhiteSpace(program[i - 1])))
                    {
                        endIndex = i - 1;
                        string word = program.Substring(startIndex, endIndex - startIndex + 1);

                        keyResult = ContainsKey(word);
                        if (keyResult != null)
                        {
                            Tag t = new Tag();
                            t.Start = r.ContentStart.GetPositionAtOffset(startIndex, LogicalDirection.Forward);
                            if (keyResult == word)
                                t.End = r.ContentStart.GetPositionAtOffset(endIndex + 1, LogicalDirection.Backward);
                            else
                                t.End = r.ContentStart.GetPositionAtOffset(endIndex, LogicalDirection.Backward);
                            t.Word = word;
                            t.Style = wordStyleDict[keyResult];
                            colorTagList.Add(t);
                        }
                    }
                    startIndex = i + 1;
                }
            }

            string lastWord = program.Substring(startIndex, program.Length - startIndex);
            keyResult = ContainsKey(lastWord);
            if (keyResult != null)
            {
                Tag t = new Tag();
                t.Start = r.ContentStart.GetPositionAtOffset(startIndex, LogicalDirection.Forward);
                if (keyResult == lastWord)
                    t.End = r.ContentStart.GetPositionAtOffset(endIndex + 1, LogicalDirection.Backward);
                else
                    t.End = r.ContentStart.GetPositionAtOffset(endIndex, LogicalDirection.Backward);
                t.Word = lastWord;
                t.Style = wordStyleDict[keyResult];
                colorTagList.Add(t);
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
            colorTagList.Clear();

            TextRange docRange = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            docRange.ClearAllProperties(); /* Remove all formatting properties (to be readded) */

            /* Find keywords in program */
            TextPointer nav = Editor.Document.ContentStart;
            while (nav.CompareTo(Editor.Document.ContentEnd) < 0)
            {
                TextPointerContext ctxt = nav.GetPointerContext(LogicalDirection.Backward);
                if (ctxt == TextPointerContext.ElementStart && nav.Parent is Run)
                {
                    program = ((Run)nav.Parent).Text;
                    if (program != "") /* Only check words if program is not empty */
                        CheckWordsInRun((Run)nav.Parent);
                }
                nav = nav.GetNextContextPosition(LogicalDirection.Forward);
            }

            /* Highlight keywords in program */
            for (int i = 0; i < colorTagList.Count; ++i)
            {
                try
                {
                    TextRange range = new TextRange(colorTagList[i].Start, colorTagList[i].End);
                    foreach (KeyValuePair<DependencyProperty, object> style in colorTagList[i].Style.GetDict())
                        range.ApplyPropertyValue(style.Key, style.Value);
                }
                catch { }
            }

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
}
