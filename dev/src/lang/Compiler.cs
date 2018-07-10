using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compiler
{
    class Compiler
    {

    }

    enum TokenType
    {

    }

    class Token
    {
        public string content { get { return content; } set { } }
        public TokenType type { get { return type; } set { } }

        public Token(string content, TokenType type)
        {
            this.content = content;
            this.type = type;
        }
    }

    class LexicalAnalyzer
    {

    }
}
