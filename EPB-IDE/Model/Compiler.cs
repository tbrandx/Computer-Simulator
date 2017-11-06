using Computer_Simulator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EPB_IDE.Model
{
    public class Compiler : ICompiler
    {
        private string[] _commands = EPML.Commands;

        private Flags _flags;

        private EPB _programCode;
        private EPML _compiledCode;
        private SymbolTable _symbolTable;
        public SymbolTable Symbols { get { return _symbolTable; } }
        public Flags Flags { get { return _flags; } }
        private string _programDescription = "";
        private Stack<ForLoopEntry> _forLoopEntries;
        private int _workingLineNumber;
        private const int FOR_LOOP_INCREMENT = 1;

        //------------------------------------------------------------------------------------------------------------
        public Compiler(int max = 100)
        {
            _symbolTable = SymbolTable.Make();
            _programCode = EPB.Make();
            _compiledCode = EPML.Make().FillTo(max);
            _flags = Flags.Make();
            _workingLineNumber = 0;
            resetForLoopVariables();
        }

        //------------------------------------------------------------------------------------------------------------
        private void resetForLoopVariables()
        {
            _forLoopEntries = new Stack<ForLoopEntry>();
        }

        public Compiler LoadProgram(string[] lines)
        {
            return LoadProgram(EPB.Make(lines));
        }

        //------------------------------------------------------------------------------------------------------------
        public Compiler LoadProgram(EPB program)
        {
            _programCode = program;
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public Compiler Compile()
        {
            while (_programCode.MoreLines())
            {
                processFirstPass(_programCode.NextLine());
            }
            processSecondPass();
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        public Compiler Optimize()
        {
            optimize();
            return this;
        }


        //------------------------------------------------------------------------------------------------------------
        private void processSecondPass()
        {
            foreach (var flag in _flags.All)
            {
                int operationCode, operand;
                var current = _compiledCode.Get(flag.Index);
                Word.Extract(current, out operationCode, out operand);
                var location = _symbolTable.FindLineNumber(flag.Value).Location();
                _compiledCode.Replace(flag.Index, Word.Build(operationCode, location));
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private void optimize()
        {
            _compiledCode.Optimize();
        }

        //------------------------------------------------------------------------------------------------------------
        public EPML CompiledCode()
        {
            return this._compiledCode;
        }

        //------------------------------------------------------------------------------------------------------------
        private void processFirstPass(string line)
        {
            line = line.Replace("\r", "");
            string[] tokens = line.Split(' ');
            if (tokens.Length >= 2)
            {
                if (!Int32.TryParse(tokens[0], out int lineNumber)) { throw new SystemException($"Invalid token ({tokens[0]} at position 0"); }
                string command = tokens[1];
                if (Array.IndexOf(_commands, command) >= 0)
                {
                    _symbolTable.Add(lineNumber, TableEntry.LINE_NUMBER, _workingLineNumber);
                    int operationCode, operand;
                    switch (command.ToLower())
                    {
                        case "rem":
                            remStatement(tokens);
                            break;
                        case "input":
                            inputStatement(tokens, lineNumber, out operationCode, out operand);
                            break;
                        case "let":
                            letStatement(tokens, lineNumber);
                            break;
                        case "print":
                            printStatement(tokens, lineNumber, out operationCode, out operand);
                            break;
                        case "goto":
                            gotoStatement(tokens, lineNumber, out operationCode, out operand);
                            break;
                        case "if": 
                            ifStatement(tokens, lineNumber);
                            break;
                        case "for":
                            forStatement(tokens, lineNumber);
                            break;
                        case "next":
                            nextStatement(lineNumber);
                            break;
                        case "pause":
                            pauseStatement(lineNumber);
                            break;
                        case "end":
                            endStatement(out operationCode, out operand);
                            break;
                    }
                }
            }
        }

        private void pauseStatement(int lineNumber)
        {
            int operationCode = (int)EPC.Code.PAUSE;
            int operand = 00;
            _compiledCode.Add(Word.Build(operationCode, operand), _workingLineNumber++);
        }

        //------------------------------------------------------------------------------------------------------------
        private void nextStatement(int lineNumber)
        {
            // Check to make sure that a 'for' statement has processed and that the start of the loop has been set
            // If this has not been done the 'for' keyword is missing
            if (_forLoopEntries.Peek() != null)
            {
                var entry = _forLoopEntries.Pop();
                // format the 'if/goto' statement so it can be properly processed
                // if [LHS] != [RHS] goto [LOOP_START]
                TableEntry left = _symbolTable.AtLocation(entry.LHS);
                TableEntry right = _symbolTable.AtLocation(entry.RHS);
                // Assume that left is a Variable

                // Increment counter
                string strIncrement = $"00 let {left.CharVal} = {left.CharVal} + {entry.Increment}";
                letStatement(strIncrement.Split(' '), lineNumber);

                // Check condition
                string strCheck = $"00 if {left.CharVal} <= ";
                if (right.IsVar())
                {
                    strCheck += $"{right.CharVal}";
                }
                else if (right.IsConst())
                {
                    strCheck += $"{right.Val}";
                }
                else
                {
                    throw new SystemException($"Something bad happened -on code line {lineNumber}");
                }
                int start = _symbolTable.AtLocation(entry.Step).Val;
                strCheck += $" goto {start}";

                ifStatement(strCheck.Split(' '), lineNumber);
            }
            else
            {
                throw new SystemException($"'next' missing 'for' statement -on code line {lineNumber}");
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private void forStatement(string[] tokens, int lineNumber)
        {
            // Place increment value into the symbol table
            _symbolTable.AddConst(FOR_LOOP_INCREMENT);
            if (findNext(out int intNextLineLocation)) // validate there is a 'next' statement
            {
                // validate that the 'for' statement following the correct syntax
                // [2] = variable, [3] = comparator, [4] = var/const, [5] = 'to', [6] = var/const
                string strLHS = tokens[2];
                string strRHS = tokens[4];
                string strCheck = tokens[6];
                if (Toolbelt.IsValidVariable(strLHS) &&
                    tokens[3] == "=" &&
                    tokens[5] == "to")
                {
                    putInSymbolTable(tokens);
                    // Format the assignment string so it can be properly processed
                    string assignment = $"00 let {strLHS} = {strRHS}";
                    letStatement(assignment.Split(' '), lineNumber);
                    // after let statement is process, the next entry will be the start of the commands in the loop
                    _forLoopEntries.Push(ForLoopEntry.Make(
                        step: _workingLineNumber,
                        lhs: _symbolTable.Find(strLHS).Location(),
                        rhs: _symbolTable.Find(strCheck).Location(),
                        increment: FOR_LOOP_INCREMENT
                        ));
                }
                else
                {
                    throw new SystemException($"Invalid 'for' statement -on code line {lineNumber}");
                }

            }
            else
            {
                throw new SystemException($"'for' missing 'next' statment -on code line {lineNumber}");
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private void endStatement(out int operationCode, out int operand)
        {
            operationCode = (int)EPC.Code.HALT;
            operand = 00;
            _compiledCode.Add(Word.Build(operationCode, operand), _workingLineNumber++);
        }

        //------------------------------------------------------------------------------------------------------------
        private void remStatement(string[] tokens)
        {
            if (_programDescription == "") // use the first rem as the program description
            {
                for (int i = 2; i < tokens.Length; ++i)
                {
                    _programDescription += tokens[i];
                    if (i < tokens.Length - 1) { _programDescription += ' '; }
                }
                _compiledCode.Description(_programDescription);
            }
            // Do nothing, ignore comment in code
        }

        //------------------------------------------------------------------------------------------------------------
        private void inputStatement(string[] tokens, int lineNumber, out int operationCode, out int operand)
        {
            if (!Toolbelt.IsValidVariable(tokens[2])) { throw new SystemException($"{tokens[2]} not valid for variable name following 'input' keyword -on code line {lineNumber}"); }
            char variable = Convert.ToChar(tokens[2]);
            var entry = _symbolTable.FindOrAdd((int)variable, TableEntry.VARIABLE);
            // Generate EPML instruction
            operationCode = (int)EPC.Code.READ;
            operand = entry.Location();
            _compiledCode.Add(Word.Build(operationCode, operand), _workingLineNumber++);
        }

        //------------------------------------------------------------------------------------------------------------
        private void letStatement(string[] tokens, int lineNumber)
        {
            // Check that first character after 'let' is a valid variable
            if (!Toolbelt.IsValidVariable(tokens[2])) { throw new SystemException($"Invalid token ({tokens[2]}) after keyword 'let' -on code line {lineNumber}"); }
            // Check that the next character is the '=' sign
            if (!(tokens[3].Length == 1 && '=' == Convert.ToChar(tokens[3]))) { throw new SystemException($"Invalid token ({tokens[3]} after assignment variable in 'let' statement -on code line {lineNumber}"); }

            putInSymbolTable(tokens);

            // determine location of variable to store result in
            char storeVar = Convert.ToChar(tokens[2]);
            int storeLoc = _symbolTable.FindVariable((int)storeVar).Location();

            if (tokens.Length > 5) // expression follows '=' sign
            {
                compileExpression(tokens, lineNumber, storeLoc);
            }
            else // simple value or variable assignment follows '=' sign
            {
                compileAssignment(tokens, lineNumber, storeLoc);
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private void compileAssignment(string[] tokens, int lineNumber, int storeLoc)
        {
            var token = tokens[4];
            int loc = -1;
            if (Int32.TryParse(token, out int intValue)) // assigning a Constant
            {
                loc = _symbolTable.FindConstant(intValue).Location();
            }
            else if (Toolbelt.IsValidVariable(token)) // assigning a variable value
            {
                if (Char.TryParse(token, out char chVar))
                {
                    loc = _symbolTable.FindVariable((int)chVar).Location();
                }
                else
                {
                    throw new SystemException($"Something bad happened -on code line {lineNumber}");
                }
            }
            else
            {
                throw new SystemException($"Invalid character following '=' in 'let' statement -on code line {lineNumber}");
            }

            if (loc >= 0)
            {
                // load constant or variable value from memory and store in assignment variable
                _compiledCode.Add(Word.Build((int)EPC.Code.LOAD, loc), _workingLineNumber++);
                if (_symbolTable.HasLocation(storeLoc) && _symbolTable.AtLocation(storeLoc).IsConst()) { throw new SystemException($"Cannot override constant value at memory[{storeLoc}] -on code line {lineNumber}"); }
                _compiledCode.Add(Word.Build((int)EPC.Code.STORE, storeLoc), _workingLineNumber++);
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private void compileExpression(string[] tokens, int lineNumber, int storeLoc)
        {
            string infix = "";
            // build infix expression
            for (int i = 4; i < tokens.Length; ++i)
            {
                infix += tokens[i];
            }

            string postfix = InfixToPostfix.Convert(infix);
            Console.WriteLine($"Infix: {infix}, Postfix: {postfix}");
            int temp = evaluatePostfix(postfix, lineNumber);
            // load temp
            _compiledCode.Add(Word.Build((int)EPC.Code.LOAD, temp), _workingLineNumber++);

            if (_symbolTable.HasLocation(storeLoc) && _symbolTable.AtLocation(storeLoc).IsConst()) { throw new SystemException($"Cannot override constant value at memory[{storeLoc}] -on code line {lineNumber}"); }
            _compiledCode.Add(Word.Build((int)EPC.Code.STORE, storeLoc), _workingLineNumber++);
        }

        //------------------------------------------------------------------------------------------------------------
        private void printStatement(string[] tokens, int lineNumber, out int operationCode, out int operand)
        {
            if (tokens.Length == 2) { throw new SystemException($"Missing variable after 'print' keyword -on code line {lineNumber}"); }
            if (!Toolbelt.IsValidVariable(tokens[2])) { throw new SystemException($"Invalid token ({tokens[2]}) following 'print' statement. Single letter variable expected -on code line {lineNumber}"); }
            operationCode = (int)EPC.Code.WRITE;
            char printVar = Convert.ToChar(tokens[2]);
            var printEntry = _symbolTable.GetEntry((int)printVar);
            if (printEntry == null) { throw new SystemException($"Variable '{tokens[2]}' is missing from the Symbol Table"); }
            operand = printEntry.Location();
            _compiledCode.Add(Word.Build(operationCode, operand), _workingLineNumber++);
        }

        //------------------------------------------------------------------------------------------------------------
        private void gotoStatement(string[] tokens, int lineNumber, out int operationCode, out int operand)
        {
            if (!Int32.TryParse(tokens[2], out int lineNum)) { throw new SystemException($"Invalid token ({tokens[2]}) following 'goto' statement.  Integer value expected -on code line {lineNumber}"); }

            operationCode = (int)EPC.Code.BRANCH;
            operand = 00;
            var gotoEntry = _symbolTable.FindLineNumber(lineNum);
            if (gotoEntry != null)
            {
                operand = gotoEntry.Location();
                _compiledCode.Add(Word.Build(operationCode, operand), _workingLineNumber++);

            }
            else
            {
                _flags.Add(_workingLineNumber, lineNum);
                _compiledCode.Add(Word.Build(operationCode, operand), _workingLineNumber++);

            }
        }

        //------------------------------------------------------------------------------------------------------------
        private void ifStatement(string[] tokens, int lineNumber)
        {
            // Assume only one token to the left of the comparator  eg.  20 if x == y goto 50
            // [2] = Left, [3] = Comparator, [4] = Right, [5] = 'goto', [6] = LineNumber
            if (!tokens.Contains("goto")) { throw new SystemException($"'if' statement missing corresponding 'goto' -on code line {lineNumber}"); }
            string left = tokens[2];
            string comparator = tokens[3];
            string right = tokens[4];
            string goTo = tokens[5];
            string gotoLine = tokens[6];
            TableEntry leftEntry, rightEntry;
            //------------------Begin Validations--------Symbol table update------------
            // Validate left of comparator
            if (Int32.TryParse(left, out int intLeft)) // Third token is a number (constant)
            {
                leftEntry = _symbolTable.FindOrAddConst(intLeft);
                _compiledCode.Add(leftEntry.Symbol(), leftEntry.Location());
            }
            else if (Toolbelt.IsValidVariable(left)) // Third token is a valid variable
            {
                char leftVar = Convert.ToChar(left);
                leftEntry = _symbolTable.FindOrAddVar((int)leftVar);
            }
            else
            {
                throw new SystemException($"{left} not valid following 'if' keyword -on code line {lineNumber}");
            }
            // Validate comparator
            if (!Toolbelt.IsComparator(comparator)) // Fourth token is a valid comparator
            {
                throw new SystemException($"{comparator} is is not a valid comparator in 'if' statement -on code line {lineNumber}");
            }
            // Validate left of comparator
            if (Int32.TryParse(right, out int intRight)) // Third token is a number (constant)
            {
                rightEntry = _symbolTable.FindOrAddConst(intRight);
                _compiledCode.Add(rightEntry.Symbol(), rightEntry.Location());
            }
            else if (Toolbelt.IsValidVariable(right)) // Third token is a valid variable
            {
                char rightVar = Convert.ToChar(right);
                rightEntry = _symbolTable.FindOrAddVar((int)rightVar);
            }
            else
            {
                throw new SystemException($"{right} not valid following comparator in 'if' statement -on code line {lineNumber}");
            }
            // validate that 'goto' is in position 5
            if (goTo != "goto") { throw new SystemException($"Expected 'goto' after comparison in 'if' statement.  ({goTo}) -on code line {lineNumber}"); }
            // validate that following the 'goto' is a numeric value
            if (!Int32.TryParse(gotoLine, out int intGotoLine)) { throw new SystemException($"'{gotoLine}' invalid token following 'goto' in 'if' statement.  Integer value expected. -on code line {lineNumber}"); }
            //-------------End Validations--------------------

            if (leftEntry == null || rightEntry == null) { throw new SystemException($"Unknown error occurred in 'if/goto' statement -on code line {lineNumber}"); }

            // lookup goto line number in symbol table
            var gotoLineEntry = _symbolTable.FindLineNumber(intGotoLine);
            var gotoLineNumber = 00;
            bool needsFlag = false;
            if (gotoLineEntry != null)
            {
                gotoLineNumber = gotoLineEntry.Location();

            }
            else
            {
                needsFlag = true;
            }

            // perform calculations with right operand
            // add appropriate branch command

            switch (comparator)
            {
                case "==":
                    _compiledCode.Add(Word.Build((int)EPC.Code.LOAD, leftEntry.Location()), _workingLineNumber++);
                    _compiledCode.Add(Word.Build((int)EPC.Code.SUBTRACT, rightEntry.Location()), _workingLineNumber++);
                    if (needsFlag) { _flags.Add(_workingLineNumber, intGotoLine); }
                    _compiledCode.Add(Word.Build((int)EPC.Code.BRANCHZERO, gotoLineNumber), _workingLineNumber++);
                    break;
                case "!=":
                    // check if greater than
                    _compiledCode.Add(Word.Build((int)EPC.Code.LOAD, rightEntry.Location()), _workingLineNumber++);
                    _compiledCode.Add(Word.Build((int)EPC.Code.SUBTRACT, leftEntry.Location()), _workingLineNumber++);
                    if (needsFlag) { _flags.Add(_workingLineNumber, intGotoLine); }
                    _compiledCode.Add(Word.Build((int)EPC.Code.BRANCHNEG, gotoLineNumber), _workingLineNumber++);
                    // check if less than
                    _compiledCode.Add(Word.Build((int)EPC.Code.LOAD, leftEntry.Location()), _workingLineNumber++);
                    _compiledCode.Add(Word.Build((int)EPC.Code.SUBTRACT, rightEntry.Location()), _workingLineNumber++);
                    if (needsFlag) { _flags.Add(_workingLineNumber, intGotoLine); }
                    _compiledCode.Add(Word.Build((int)EPC.Code.BRANCHNEG, gotoLineNumber), _workingLineNumber++);
                    break;
                case "<":
                    _compiledCode.Add(Word.Build((int)EPC.Code.LOAD, leftEntry.Location()), _workingLineNumber++);
                    _compiledCode.Add(Word.Build((int)EPC.Code.SUBTRACT, rightEntry.Location()), _workingLineNumber++);
                    if (needsFlag) { _flags.Add(_workingLineNumber, intGotoLine); }
                    _compiledCode.Add(Word.Build((int)EPC.Code.BRANCHNEG, gotoLineNumber), _workingLineNumber++);
                    break;
                case ">":
                    _compiledCode.Add(Word.Build((int)EPC.Code.LOAD, rightEntry.Location()), _workingLineNumber++);
                    _compiledCode.Add(Word.Build((int)EPC.Code.SUBTRACT, leftEntry.Location()), _workingLineNumber++);
                    if (needsFlag) { _flags.Add(_workingLineNumber, intGotoLine); }
                    _compiledCode.Add(Word.Build((int)EPC.Code.BRANCHNEG, gotoLineNumber), _workingLineNumber++);
                    break;
                case "<=":
                    _compiledCode.Add(Word.Build((int)EPC.Code.LOAD, leftEntry.Location()), _workingLineNumber++);
                    _compiledCode.Add(Word.Build((int)EPC.Code.SUBTRACT, rightEntry.Location()), _workingLineNumber++);
                    if (needsFlag) { _flags.Add(_workingLineNumber, intGotoLine); }
                    _compiledCode.Add(Word.Build((int)EPC.Code.BRANCHNEG, gotoLineNumber), _workingLineNumber++);
                    if (needsFlag) { _flags.Add(_workingLineNumber, intGotoLine); }
                    _compiledCode.Add(Word.Build((int)EPC.Code.BRANCHZERO, gotoLineNumber), _workingLineNumber++);
                    break;
                case ">=":
                    _compiledCode.Add(Word.Build((int)EPC.Code.LOAD, rightEntry.Location()), _workingLineNumber++);
                    _compiledCode.Add(Word.Build((int)EPC.Code.SUBTRACT, leftEntry.Location()), _workingLineNumber++);
                    if (needsFlag) { _flags.Add(_workingLineNumber, intGotoLine); }
                    _compiledCode.Add(Word.Build((int)EPC.Code.BRANCHNEG, gotoLineNumber), _workingLineNumber++);
                    if (needsFlag) { _flags.Add(_workingLineNumber, intGotoLine); }
                    _compiledCode.Add(Word.Build((int)EPC.Code.BRANCHZERO, gotoLineNumber), _workingLineNumber++);
                    break;

            }
        }

        //------------------------------------------------------------------------------------------------------------
        private void putInSymbolTable(string[] tokens)
        {
            // place all variables and constants in the symbol table if missing
            for (int i = 2; i < tokens.Length; ++i)
            {
                var token = tokens[i];
                if (Int32.TryParse(token, out int intToken))
                {
                    var tempEntry = _symbolTable.FindOrAddConst(intToken);
                    _compiledCode.Add(tempEntry.Symbol(), tempEntry.Location());

                }
                else if (Toolbelt.IsValidVariable(token))
                {
                    _symbolTable.FindOrAddVar(Convert.ToChar(token));
                }
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private bool findNext(out int result)
        {
            int max = _programCode.CodeLines.Length;
            for (int i=0; i<max; ++i)
            {
                string entry = _programCode.CodeLines[i];
                if (entry.Contains("next"))
                {
                    result = i;
                    return true;
                }
            }
            result = -1;
            return false;
        }

        //------------------------------------------------------------------------------------------------------------
        private int evaluatePostfix(string postfix, int lineNumber)
        {
            Stack<string> expression = new Stack<string>(postfix.Split(' ').Reverse());
            Stack<int> operands = new Stack<int>();
            Stack<int> temps = new Stack<int>();
            int firstTemp = _symbolTable.TempLocation;
            while (expression.Count > 0)
            {
                string working = expression.Pop();
                if (Int32.TryParse(working, out int intWorking))
                {
                    // working string is an integer value
                    operands.Push(_symbolTable.FindConstant(intWorking).Location());
                }
                else if (Toolbelt.IsValidVariable(working))
                {
                    char workingChar = Convert.ToChar(working);
                    operands.Push(_symbolTable.FindVariable(Convert.ToInt32(workingChar)).Location());
                }
                else if (Toolbelt.IsOperator(working))
                {
                    int left, right;
                    char op = Convert.ToChar(working);
                    if (operands.Count() >= 2)
                    {
                        right = operands.Pop();
                        left = operands.Pop();
                    }
                    else if (operands.Count() == 1 && temps.Count >= 1)
                    {
                        right = temps.Pop();
                        left = operands.Pop();
                    }
                    else if (temps.Count >= 2)
                    {
                        right = temps.Pop();
                        left = temps.Pop();
                    }
                    else { return -1; }
                    // load 'left' from memory
                    _compiledCode.Add(Word.Build((int)EPC.Code.LOAD, left), _workingLineNumber++);
                    // perform calculation on 'left'
                    switch (op)
                    {
                        case '+':
                            _compiledCode.Add(Word.Build((int)EPC.Code.ADD, right), _workingLineNumber++);
                            break;
                        case '-':
                            _compiledCode.Add(Word.Build((int)EPC.Code.SUBTRACT, right), _workingLineNumber++);
                            break;
                        case '*':
                            _compiledCode.Add(Word.Build((int)EPC.Code.MULTIPLY, right), _workingLineNumber++);
                            break;
                        case '/':
                            _compiledCode.Add(Word.Build((int)EPC.Code.DIVIDE, right), _workingLineNumber++);
                            break;
                    }
                    // store result in 'temp'
                    temps.Push(firstTemp - temps.Count());
                    if (_symbolTable.HasLocation(temps.Peek()) && _symbolTable.AtLocation(temps.Peek()).IsConst()) { throw new SystemException($"Cannot override constant value at memory[{temps.Peek()}] -on code line {lineNumber}"); }
                    _compiledCode.Add(Word.Build((int)EPC.Code.STORE, temps.Peek()), _workingLineNumber++);

                }
                else { return -1; }
            }
            return temps.Pop();
        }
    }
}
