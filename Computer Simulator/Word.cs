using System;

namespace Computer_Simulator
{
    public class Word
    {
        //------------------------------------------------------------------------------------------------------------
        public static void Extract(decimal from, out int code, out int operand)
        {
            code = (int)(from / 100);
            operand = (int)(from % 100);
        }

        //------------------------------------------------------------------------------------------------------------
        public static decimal Build(int code, int operand)
        {
            return Decimal.Add(Decimal.Multiply(code, 100), operand);
        }
    }
}
