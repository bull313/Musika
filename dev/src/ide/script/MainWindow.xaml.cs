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
    public partial class MainWindow : Window
    {
        private static Color DEFAULT_COLOR = Colors.Black; /* Set the default text color to black */
        private static Color TIER_1_KEYWORD_COLOR = Colors.Red;

        private Paragraph body;
        private Dictionary<string, Color> syntaxHighlightDict;

        public MainWindow()
        {
            InitializeComponent();
            ResetEditor();

            /* Populate Text Highlight Dictionary */
            syntaxHighlightDict = new Dictionary<string, Color>();
            syntaxHighlightDict.Add("accompany",    TIER_1_KEYWORD_COLOR);
            syntaxHighlightDict.Add("name",         TIER_1_KEYWORD_COLOR);
            syntaxHighlightDict.Add("author",       TIER_1_KEYWORD_COLOR);
            syntaxHighlightDict.Add("coauthors",    TIER_1_KEYWORD_COLOR);
            syntaxHighlightDict.Add("title",        TIER_1_KEYWORD_COLOR);
            syntaxHighlightDict.Add("key",          TIER_1_KEYWORD_COLOR);
            syntaxHighlightDict.Add("time",         TIER_1_KEYWORD_COLOR);
            syntaxHighlightDict.Add("tempo",        TIER_1_KEYWORD_COLOR);
            syntaxHighlightDict.Add("octave",       TIER_1_KEYWORD_COLOR);
            syntaxHighlightDict.Add("pattern",      TIER_1_KEYWORD_COLOR);
            syntaxHighlightDict.Add("chord",        TIER_1_KEYWORD_COLOR);
            syntaxHighlightDict.Add("is",           TIER_1_KEYWORD_COLOR);
        }

        /*
         *  ---------------- HELPER FUNCTIONS ----------------
        */

        private string GetText()
        {
            return new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text;
        }

        private void ResetEditor() /* Make a blank new file */
        {
            Editor.Document.Blocks.Clear();
            body = new Paragraph(); /* Insert a parent paragraph into the document */
            Editor.Document.Blocks.Add(body);
        }

        /*
         *  ---------------- / HELPER FUNCTIONS --------------
        */


        /*
         *  ---------------- TEXT EDITOR STYLE HANDLERS ----------------
        */

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {

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
            ResetEditor();
        }

        /*
         *  ---------------- / MENU BAR CLICK HANDLERS ----------------
        */
    }
}
