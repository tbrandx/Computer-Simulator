using System;
using System.Collections.Generic;
using System.Linq;

namespace Computer_Simulator
{
    public class PostfixEvaluator
    {
        //------------------------------------------------------------------------------------------------------------
        public static decimal Evaluate(string postfix)
        {
            bool debug = false;
            if (Toolbelt.HasLetters(postfix)) { return 0M; }
            Stack<string> expression = new Stack<string>(postfix.Split(' ').Reverse());
            Stack<decimal> operands = new Stack<decimal>();

            while (expression.Count > 0)
            {
                if (debug)
                {
                    Toolbelt.DisplayStack(expression, "expression");
                    Toolbelt.DisplayStack(operands, "operands");
                }

                string working = expression.Pop();
                decimal value;
                if (Decimal.TryParse(working, out value))
                {
                    operands.Push(value);
                }
                else if(working.Length == 1 && Toolbelt.IsOperator(working[0]))
                {
                    if (operands.Count >= 2)
                    {
                        var x = operands.Pop();
                        var y = operands.Pop();
                        operands.Push(calculate(y, working[0], x));
                    }
                }
            }

            return operands.Pop();
        }

        //------------------------------------------------------------------------------------------------------------
        private static decimal calculate(decimal y, char op, decimal x)
        {
            string name = "PostfixEvaluator calculate";
            if (!Toolbelt.IsOperator(op)) { throw new ArgumentException($"{name} op value is not a valid operator ({op})"); }
            switch(op)
            {
                case '^':
                    return (int)y ^ (int)x;
                case '+':
                    return y + x;
                case '-':
                    return y - x;
                case '*':
                    return y * x;
                case '/':
                    return y / x;
            }
            return 0m;
        }


    }
}
