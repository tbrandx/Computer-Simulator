using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Computer_Simulator
{
    public class Toolbelt
    {
        //------------------------------------------------------------------------------------------------------------
        public static void StripWhiteSpace(ref string strRef)
        {
            StringBuilder sb = new StringBuilder();
            int length = strRef.Length;
            for (int i = 0; i < length; ++i)
            {
                if (strRef[i] != ' ')
                {
                    sb.Append(strRef[i]);
                }
            }
            strRef = sb.ToString();
        }

        //------------------------------------------------------------------------------------------------------------
        public static void DisplayStack<T>(Stack<T> stack, string name)
        {
            string response = $"{name}:[  ";
            foreach (var c in stack)
            {
                response += $"{c}, ";
            }
            response = response.Substring(0, response.Length - 2);
            response += "  ]";
            Console.WriteLine(response);

        }

        //------------------------------------------------------------------------------------------------------------
        public static bool HasLetters(string expression)
        {
            foreach (char c in expression)
            {
                if (Char.IsLetter(c))
                {
                    return true;
                }
            }
            return false;
        }

        //------------------------------------------------------------------------------------------------------------
        public static bool IsOperator(string s)
        {
            return (s.Length == 1 && IsOperator(Convert.ToChar(s)));
        }

        //------------------------------------------------------------------------------------------------------------
        public static bool IsOperator(char c)
        {
            char[] operators = { '-', '+', '*', '/', '^' };
            return operators.Contains(c);
        }

        //------------------------------------------------------------------------------------------------------------
        public static bool IsNumeric(string expression)
        {
            return Decimal.TryParse(expression, out decimal result);
        }

        //------------------------------------------------------------------------------------------------------------
        public static bool IsValidVariable(string expression)
        {
            return expression.Length == 1 && expression.All(char.IsLetter) && expression == expression.ToLower();
        }

        //------------------------------------------------------------------------------------------------------------
        public static bool IsComparator(string expression)
        {
            string[] comparators = { "==", "<", ">", "<=", ">=", "!=" };
            return comparators.Contains(expression);
        }

    }
}
