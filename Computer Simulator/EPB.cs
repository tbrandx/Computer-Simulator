using System.Collections.Generic;

namespace Computer_Simulator
{
    public class EPB
    {
        //------------------------------------------------------------------------------------------------------------
        public static EPB Make()
        {
            return new EPB();
        }

        //------------------------------------------------------------------------------------------------------------
        public static EPB Make(string[] lines)
        {
            return new EPB(lines);
        }

        private Queue<string> _codeLines;
        public string[] CodeLines { get { return _codeLines.ToArray(); } }

        //------------------------------------------------------------------------------------------------------------
        private EPB()
        {
            _codeLines = new Queue<string>();
        }

        //------------------------------------------------------------------------------------------------------------
        private EPB(string[] lines)
        {
            _codeLines = new Queue<string>(lines);
        }

        //------------------------------------------------------------------------------------------------------------
        public EPB AddLine(string code)
        {
            _codeLines.Enqueue(code);
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public bool MoreLines()
        {
            return _codeLines.Count > 0;
        }

        //------------------------------------------------------------------------------------------------------------
        public string NextLine()
        {
            return _codeLines.Dequeue();
        }
    }
}
