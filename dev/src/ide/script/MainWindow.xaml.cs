using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ide
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /*
         *  ---------------- TEXT EDITOR STYLE HANDLERS ----------------
        */

        private void Editor_KeyDown(object sender, RoutedEventArgs e)
        {
            string editorText = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd).Text;

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
            /* Make a blank new file */
            Editor.Document.Blocks.Clear();
        }

        /*
         *  ---------------- / MENU BAR CLICK HANDLERS ----------------
        */
    }
}
