using System;
using System.Collections.Generic;
using System.Linq;

namespace Computer_Simulator
{
    public class EPML
    {
        //------------------------------------------------------------------------------------------------------------
        public static EPML Make(string name = "EPB Program")
        {
            return new EPML(name);
        }

        public static string[] Commands { get { return _commands; } }
        private static string[] _commands = {
            "rem",      // '50 rem this is a remark'    Any text following the command rem is for documentation purposes only and is ignored by the compiler
            "input",    // '30 input x'                 Display a question mark to prompt the user to enter an integer.  Read that integer from the keyboard and store the integer in x
            "let",      // '80 let u = 4 * (j - 56)'    Assign u the value of 4 * (j-56).  Note that an arbitrarily complex expression can appear to the right of the equal sign
            "print",    // '10 print w'                 Display the value of w
            "goto",     // '70 goto 45'                 Transfer program control to line 45
            "if",       // '35 if i == z goto 80'       Compare i and z for equality and transfer program control to line 80 if the condition is true: otherwise, continue execution with the next statement
            "for",      // '40 for x = 1 to 5'          Loop from 1 to 5 increment by 1 until.  Requires 'next' command
            "next",     // '60 next'                    End of for loop
            "pause",    // '65 pause'                   Pause program
            "end"       // '99 end'                     Terminate program execution
        };

        const decimal DEFAULT = +0000M;
        private List<decimal> _instructions;
        private string _name;
        private string _description;
        public List<decimal> Instructions { get { return _instructions; } }
        public string Name { get { return _name; } }

        //------------------------------------------------------------------------------------------------------------
        private EPML(string name)
        {
            _name = name;
            _description = "";
            _instructions = new List<decimal>();
        }

        //------------------------------------------------------------------------------------------------------------
        public EPML FillTo(int max)
        {
            _instructions = new List<decimal>(Enumerable.Repeat(DEFAULT, max).ToList());
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public EPML Load(string[] lines)
        {
            if (lines.Length > 0)
            {
                if (lines.First().Split(' ').Length > 1) // instruction line includes memory location
                {
                    if (_instructions.Count <= 0) { throw new SystemException($"EPML program instruction set is empty.  You must use EPML.FillTo to populate the instructions with defaults"); }
                    foreach(string line in lines)
                    {
                        var tokens = line.Split(' ');
                        string location = tokens[0];
                        string instruction = tokens[1];
                        if (Int32.TryParse(location, out int intLocation))
                        {
                            if (intLocation == -1) // line contains program description
                            {
                                for (int i=1; i<tokens.Length; ++i)
                                {
                                    _description += tokens[i] + " ";
                                }
                            }
                            if (Decimal.TryParse(instruction, out decimal decInstruction))
                            {
                                Add(decInstruction, intLocation);
                            }

                        }
                    }
                }
                else
                {
                    foreach (string line in lines)
                    {
                        Add(line);
                    }

                }
            }
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public EPML Description(string description)
        {
            _description = description;
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public string Description()
        {
            return _description;
        }

        //------------------------------------------------------------------------------------------------------------
        public EPML Add(string instruction)
        {
            if (Decimal.TryParse(instruction, out decimal value))
            {
                return Add(value);
            }
            else
            {
                return Description(instruction);
            }
        }

        //------------------------------------------------------------------------------------------------------------
        public EPML Optimize()
        {
            // Search the instructions for a STORE instruction followed immediately by a LOAD instruction to the same location in memory
            int max = _instructions.Count() - 1;  // each iteration looks at the current instruction and sometimes the next one
            for (int i=0; i<max; ++i)
            {
                var current = _instructions[i];
                Word.Extract(current, out int currentCode, out int currentOperand);
                if (currentCode == (int)EPC.Code.STORE)
                {
                    var next = _instructions[i + 1];
                    var following = _instructions[i + 2];
                    Word.Extract(next, out int nextCode, out int nextOperand);
                    Word.Extract(following, out int followingCode, out int followingOperand);
                    if (currentOperand == nextOperand && nextCode == (int)EPC.Code.LOAD)
                    {
                        if (followingCode == (int)EPC.Code.STORE)
                        {
                            removeInstructionPair(i);
                            return Optimize();
                        }
                    }
                }
            }
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        private void removeInstructionPair(int index)
        {
            remove(index); // remove the first instruction
            remove(index); // after all instructions shift up, remove the second instruction
        }

        //------------------------------------------------------------------------------------------------------------
        private void remove(int index) // Removes an instruction from the list and shifts remaining instructions up
        {
            // Set requested location to default
            _instructions[index] = DEFAULT;
            updateBranchInstructions(minIndex: index); // update branch pointers to account for target instruction shifting up
            // shift remaining instructions up
            int max = _instructions.Count() - 1;
            int i = index;
            int halt = findHalt();
            while (i < max && (halt > 0 && i <= halt))
            {
                _instructions[i] = _instructions[i + 1];
                ++i;
            }
            // Set previous halt instruction to default
            _instructions[halt] = DEFAULT;
        }

        private void updateBranchInstructions(int minIndex)
        {
            // update branch
            int max = _instructions.Count();
            for (int i=0; i<max; ++i)
            {
                Word.Extract(_instructions[i], out int code, out int operand);
                if (code == (int)EPC.Code.BRANCH ||
                    code == (int)EPC.Code.BRANCHNEG ||
                    code == (int)EPC.Code.BRANCHZERO)
                {
                    if (operand >= minIndex)
                    {
                        Replace(i, Word.Build(code, operand - 1));
                    }
                }
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private bool isHalt(decimal instruction)
        {
            Word.Extract(instruction, out int code, out int op);
            return code == (int)EPC.Code.HALT;
        }

        //------------------------------------------------------------------------------------------------------------
        private int findHalt()
        {
            int max = _instructions.Count();
            for (int i=0; i<max; ++i)
            {
                var instruction = _instructions[i];
                if (isHalt(instruction)) { return i; }
            }
            return -1;
        }

        //------------------------------------------------------------------------------------------------------------
        public EPML Replace(int index, string instruction)
        {
            if (Decimal.TryParse(instruction, out decimal value))
            {
                return Replace(index, value);
            }
            else
            {
                return this;
            }
        }

        //------------------------------------------------------------------------------------------------------------
        public EPML Replace(int index, decimal instruction)
        {
            _instructions[index] = instruction;
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public decimal Get(int index)
        {
            return _instructions[index];
        }

        //------------------------------------------------------------------------------------------------------------
        public EPML Add(decimal instruction)
        {
            _instructions.Add(instruction);
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public EPML Add(decimal instruction, int location)
        {
            _instructions[location] = instruction;
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public int Count()
        {
            return _instructions.Count;
        }

        //------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return Output();
        }

        //------------------------------------------------------------------------------------------------------------
        public string Output()
        {
            string response = "";
            if (_description.Trim() != "") { response += $"-1 {_description}\n"; }
            int count = _instructions.Count();
            for (int i=0; i<count; ++i)
            {
                var instruction = _instructions[i];
                string line = $"{i.ToString("D2")} ";
                if (EPC.ValidWord(instruction))
                {
                    line += $"{instruction.ToString(Memory.FORMAT)}";
                }
                else // Assume this is a variable or constant field
                {
                    line += $"{instruction.ToString()}";
                }
                response += line;
                if (i < count-1) { response += '\n'; }
            }
            return response;
        }

        //------------------------------------------------------------------------------------------------------------
        public string MinOutput()
        {
            string response = $"{_description}\n";
            int count = _instructions.Count();
            for (int i = 0; i < count; ++i)
            {
                var instruction = _instructions[i];
                response += $"{instruction.ToString(Memory.FORMAT)}";
                if (i < count - 1) { response += '\n'; }
            }
            return response;
        }
    }
}
