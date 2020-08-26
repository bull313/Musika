using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

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
}
