using System;
using System.Collections.Generic;
using System.Linq;

namespace Computer_Simulator
{
    class Memory
    {
        private static Memory _instance = new Memory();
        public static Memory Instance { get { return _instance; } }

        const decimal DEFAULT = +0000;
        public static string FORMAT = "+####;-####;+0000";

        private List<decimal> _block;
        private int _size = 100;
        public List<decimal> Block
        {
            get { return _block; }
        }

        //------------------------------------------------------------------------------------------------------------
        private Memory() { }

        //------------------------------------------------------------------------------------------------------------
        public int Size()
        {
            return _size;
        }

        //------------------------------------------------------------------------------------------------------------
        public Memory Size(int value)
        {
            if (value > 0) { _size = value; }
            else { throw new ArgumentOutOfRangeException($"Memory Size of {value} must be greater than zero."); }
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public Memory Initialize()
        {
            _block = new List<decimal>(Enumerable.Repeat(DEFAULT, _size).ToList());
            
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public Memory Clear()
        {
            if (_block == null) { throw new NullReferenceException("The Memory block is not initialized.  Nothing to clear."); }
            int length = _block.Count;
            for (int i = 0; i < length; ++i)
            {
                _block[i] = DEFAULT;
            }
            return this;
        }


        //------------------------------------------------------------------------------------------------------------
        public Memory Put<T>(T data, int location)
        {
            _block[location] = Convert.ToDecimal(data);
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public decimal Get(int location)
        {
            return _block[location];
        }

        //------------------------------------------------------------------------------------------------------------
        public string Dump()
        {
            if (_block == null) { return "Memory Stack not initialized"; }
            string response = "Memory:\n";
            int cols = 10;
            int inCol = 0;
            // column headings
            response += "        ";
            for (int i = 0; i < cols; ++i) // numbered column headings
            {
                response += String.Format("{0,8}", i.ToString("D2"));
            }
            response += "\n";
            for (int i = 0; i < _size; ++i)
            {
                if (inCol == 0) // first column of new row
                {
                    response += $"{i.ToString("D2"),8}";
                }
                decimal value = _block[i];
                if (EPC.ValidWord(value))
                {
                    response += String.Format("{0,8}", value.ToString(FORMAT));
                }
                else
                {
                    response += String.Format("{0,8}", value.ToString());
                }
                ++inCol;
                if (inCol == cols) // last column of row
                {
                    response += "\n";
                    inCol = 0;
                }
            }
            return response;
        }

    }
}
