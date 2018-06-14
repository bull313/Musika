using System.Windows;

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

        

        /*
        *   ---------------- / TEXT EDITOR STYLE HANDLERS ----------------
        */

        /*
         *  ---------------- MENU BAR CLICK HANDLERS ----------------
        */

        private void File_Exit_Click(object sender, RoutedEventArgs e)
        {
            /* Exit the program */
            System.Environment.Exit(0); // TODO: ask for save
        }

        /*
         *  ---------------- / MENU BAR CLICK HANDLERS ----------------
        */
    }
}
