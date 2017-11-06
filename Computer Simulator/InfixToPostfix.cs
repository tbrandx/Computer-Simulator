using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Computer_Simulator
{
    public class InfixToPostfix
    {
        private static bool debug = false;
        private static char[] operators = { '(', ')', '^', '*', '/', '+', '-' };
        private static char[] validOperators = { '(', '+', '-', '*', '/', '^' };
        private static int[] precedence = { 0, 10, 10, 20, 20, 30 };

        //------------------------------------------------------------------------------------------------------------
        public static string Convert(string infix, bool allowLetters = true)
        {
            string name = "InfixToPostfix.Convert";
            if (infix == null) { throw new ArgumentNullException($"{name} infix value cannot be null"); }
            if (infix.Trim() == "") { throw new ArgumentException($"{name} infix value must not be empty"); }
            if (infix.Trim().Length <= 0) { throw new ArgumentException($"{name} infix value must have at least one character"); }
            Toolbelt.StripWhiteSpace(ref infix);

            Stack<char> infixStack = new Stack<char>(infix.ToCharArray().Reverse());
            StringBuilder postFix = new StringBuilder();
            Stack<string> operators = new Stack<string>();
            int counter = 0;
            while (infixStack.Count > 0)
            {
                ++counter;

                if (debug) {
                    Toolbelt.DisplayStack(infixStack, $"{counter}::  infixStack");
                    Toolbelt.DisplayStack(operators,  $"{counter}::   operators");
                    Console.WriteLine(counter + "::     postfix: " + postFix.ToString() + "\n");
                }
                char ch = infixStack.Peek();
                StringBuilder newChars = new StringBuilder();
                if (Char.IsNumber(ch))
                {
                    StringBuilder number = new StringBuilder();
                    number.Append(infixStack.Pop());
                    while (infixStack.Count > 0 && Char.IsNumber(infixStack.Peek()))
                    {
                        number.Append(infixStack.Pop());
                    }
                    newChars.Append(number + " ");
                }
                else if (allowLetters && Char.IsLetter(ch))
                {
                    newChars.Append(infixStack.Pop() + " ");
                    if (infixStack.Count > 0 && Char.IsLetter(infixStack.Peek())) { throw new SystemException($"{name} variables in infix value can only be one character"); }
                }
                else if (ch == '(')
                {
                    operators.Push(infixStack.Pop().ToString());
                }
                else if (ch == ')')
                {
                    string op = operators.Pop();
                    while (op != "(")
                    {
                        newChars.Append(op + " ");
                        if (operators.Count == 0) { break; }
                        op = operators.Pop();
                    }

                    infixStack.Pop(); // discard the left parenthesis
                }
                else
                {
                    if (operators.Count > 0 && comparePrecedence(operators.Peek(), ch) >= 0)
                    {
                        string op = operators.Pop();
                        while (true)
                        {
                            newChars.Append(op + " ");
                            if (operators.Count == 0) { break; }
                            if (comparePrecedence(operators.Peek(), ch) < 0) { break; }
                            op = operators.Pop();
                        }
                        operators.Push(infixStack.Pop().ToString());
                    }
                    else
                    {
                        operators.Push(infixStack.Pop().ToString());
                    }
                    
                }
                int oldPFLengh = postFix.Length;
                int newCharsLength = newChars.Length;
                postFix.Append(newChars);
                if (postFix.Length != (oldPFLengh + newCharsLength))
                {
                    Console.WriteLine("Error appending to postfix");
                    Toolbelt.DisplayStack(operators, "operators");
                    Console.WriteLine($"postfix: {postFix}");
                }
            }

            while (operators.Count > 0) {
                postFix.Append(operators.Pop() + " ");
            }
            if (infixStack.Count > 0 || operators.Count > 0)
            {
                Toolbelt.DisplayStack(infixStack, "infix");
                Toolbelt.DisplayStack(operators, "operators");
            }
            return postFix.ToString().Trim();
        }

        //------------------------------------------------------------------------------------------------------------
        private static int comparePrecedence(string of, char to)
        {
            string name = "InfixToPostfix.comparePrecedence";
            if (of == null) { throw new ArgumentException($"{name} of: ({of}) cannot be null"); }
            if (of.Length <= 0) { throw new ArgumentException($"{name} of: ({of}) must contain at least one character"); }
            return comparePrecedence(of[0], to);
        }

        //------------------------------------------------------------------------------------------------------------
        private static int comparePrecedence(char of, char to)
        {
            if (!isValidOperator(of)) { return -1; }
            if (!isValidOperator(to)) { return -1; }
            int ofPrecedence = precedence[Array.IndexOf(validOperators, of)];
            int toPrecedence = precedence[Array.IndexOf(validOperators, to)];
            if (ofPrecedence < toPrecedence) { return -1; }
            if (ofPrecedence > toPrecedence) { return 1; }
            return 0;
        }

        //------------------------------------------------------------------------------------------------------------
        private static Boolean validPostfix(string postfix)
        {
            if (postfix == null) { return false; }
            if (postfix.Trim() == "" || postfix.Length <= 0) { return false; }
            if (!isOperator(postfix[postfix.Length - 1])) { return false; } // if expression does not end in an operator
            if (!Char.IsNumber(postfix[0])) { return false; } // if expression does not start with a number

            return true;
        }

        //------------------------------------------------------------------------------------------------------------
        private static Boolean validInfix(string infix)
        {
            if (infix == null) { return false; }
            if (infix.Trim() == "" || infix.Length <= 0) { return false; }
            if (!(Char.IsNumber(infix[0]) || infix[0] == '(')) { return false; } // first character should be a number or '('
            if (infix.Length < 3) { return false; } // expression must contain at least 3 characters

            return true;
        }

        //------------------------------------------------------------------------------------------------------------
        private static Boolean isOperator(char c)
        {
            return operators.Contains(c);
        }

        //------------------------------------------------------------------------------------------------------------
        private static Boolean isValidOperator(char c)
        {
            return validOperators.Contains(c);
        }

        //------------------------------------------------------------------------------------------------------------
        private static Boolean isValidOperator(string s)
        {
            string name = "InfixToPostfix.isValidOperator";
            if (s == null) { throw new ArgumentNullException($"{name} s value cannot be null"); }
            s = s.Trim();
            if (s == "" || s.Length != 1) { throw new ArgumentException($"{name} s value must contain at one character"); }
            return isValidOperator(s[0]);
        }

    }
}
