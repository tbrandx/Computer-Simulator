using System;

namespace Computer_Simulator
{
    public class PostFixTest
    {
        private static string[] infix = 
        {
            "4 * 4 - 4 / 4 * (4 - 4) + 4 / (4 * 4)",
            "22*55-33",
            "123 * (34 + 22)",
            "555 - (55 * 5)",
            "(9*(1-3))-4*(3+3*4)",
            "(5+(9*3))",
            "((42+55)*(22+89))",
            "((48+59)*((33+(20+66))-(19+42)))",
            "(A+(B*C))",
            "((A+B)*(Z+X))",
            "((A+T)*((B+(A+C))^(C+D)))",
            "(A+(123*C))",
            "((A+44)*(98+X))",
            "((74+T)*((B+(A+905))^(186+D)))",
            "4 * r - 4 / a * (4 - 4) + x / (4 * 4)",
            "22*t-33",
            "123 * (34 + p)",
        };
        private static string[] postfix = 
        {
            "4 4 * 4 4 / 4 4 - * - 4 4 4 * / +",
            "22 55 * 33 -",
            "123 34 22 + *",
            "555 55 5 * -",
            "9 1 3 - * 4 3 3 4 * + * -",
            "5 9 3 * +",
            "42 55 + 22 89 + *",
            "48 59 + 33 20 66 + + 19 42 + - *",
            "A B C * +",
            "A B + Z X + *",
            "A T + B A C + + C D + ^ *",
            "A 123 C * +",
            "A 44 + 98 X + *",
            "74 T + B A 905 + + 186 D + ^ *",
            "4 r * 4 a / 4 4 - * - x 4 4 * / +",
            "22 t * 33 -",
            "123 34 p + *"
        };
        private static decimal[] results = 
        {
            16.25M,
            1177M,
            6888M,
            280M,
            -78M,
            32M,
            10767M,
            6206M,
            0M,
            0M,
            0M,
            0M,
            0M,
            0M,
            0M,
            0M,
            0M
        };


        //------------------------------------------------------------------------------------------------------------
        public static void RunTests()
        {
            int length = infix.Length;
            bool[] responses = new bool[length];
            for (int i = 0; i < length; ++i)
            {
                try
                {
                    string conversion = InfixToPostfix.Convert(infix: infix[i]);
                    responses[i] = String.Compare(conversion, postfix[i]) == 0;
                    decimal result = 0M;
                    if (responses[i])
                    {
                        result = PostfixEvaluator.Evaluate(conversion);
                    }
                    bool correct = result == results[i];
                    Console.WriteLine(
                        $"  PROBLEM # {i}\n" +
                        $"     infix: {infix[i]}, \n" +
                        $"conversion: {conversion}, \n" +
                        $"  expected: {postfix[i]}, \n" +
                        $"    PASSED: {responses[i].ToString()}\n" +
                        $"    RESULT: {result.ToString()}\n" +
                        $"    ANSWER: {results[i].ToString()}\n" +
                        $"   CORRECT: {correct.ToString()}\n" +
                        "\n\n"
                        );

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }

        }

    }
}
