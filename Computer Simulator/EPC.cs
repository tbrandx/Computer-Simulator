using System;

namespace Computer_Simulator
{
    public class EPC
    {
        private bool debug = false;

        public const int MEMORY_DEFAULT = 100;

        //------------------------------------------------------------------------------------------------------------
        public static EPC Make(int memoryLimit = MEMORY_DEFAULT, bool debugging = false)
        {
            return new EPC(memoryLimit, debugging);
        }

        Memory _memory;

        public enum Code : int
        {
            READ = 10,              // Read a word from the keyboard into a specific location in memory
            WRITE = 11,             // Write a word from a specific location in memory to the screen
            LOAD = 20,              // Load a word from a specific location in memory into the accumulator
            STORE = 21,             // Store a word from the accumulator into a specific location in memory
            MULTIPLY = 30,          // Multiply a word from a specific location in memory by the word in the accumulator(leave the result in the accumulator).
            DIVIDE = 31,            // Divide a word from a specific location in memory into the word in the accumulator(leave result in the accumulator)
            ADD = 32,               // Add a word from a specific location in memory to the word in the accumulator(leave the result in the accumulator)
            SUBTRACT = 33,          // Subtract a word from a specific location in memory from the word in the accumulator(leave the result in the accumulator)
            BRANCH = 40,            // Branch to a specific location in memory
            BRANCHNEG = 41,         // Branch to a specific location in memory if the accumulator is negative
            BRANCHZERO = 42,        // Branch to a specific location in memory if the accumulator is zero
            HALT = 50,              // Halt. The program has completed its task
            PAUSE = 51              // Pause the program, have user press enter to continues
        };

        public static Array Codes
        {
            get
            {
                return Enum.GetValues(typeof(Code)); 
            }
        }

        //------------------------------------------------------------------------------------------------------------
        public static bool ValidWord(decimal instruction)
        {
            if (instruction == +0000M) // This is the default entry +0000
            {
                return true;
            }
            if (instruction < 1000M)
            {
                return false;
            }
            Word.Extract(instruction, out int intCode, out int intOp);
            var codes = EPC.Codes;
            foreach (var code in codes)
            {
                if ((int)code == intCode)
                {
                    return true;
                }
            }
            return false;
        }


        const int PADDING = 50;
        public static string WELCOME =
            $"*** {("Welcome to EPC!").PadRight(PADDING)} ***\n" +
            $"*** {("Please enter your program one instruction").PadRight(PADDING)} ***\n" +
            $"*** {("( or data word) at a time into the input").PadRight(PADDING)} ***\n" +
            $"*** {("text field. I will display the location").PadRight(PADDING)} ***\n" +
            $"*** {("number and a question mark (?). You then").PadRight(PADDING)} ***\n" +
            $"*** {("type the word for that location. Enter").PadRight(PADDING)} ***\n" +
            $"*** {("-99999 to stop entering your program").PadRight(PADDING)} ***\n";


        const decimal WORD_MIN = -9999;
        const decimal WORD_MAX = 9999;
        const string SENTINEL = "-99999";

        private int _instructionCounter = 0;
        private int _currentInstruction = 0;
        private decimal _instructionRegister = 0;
        private int _operationCode = 0;
        private int _operand = 0;
        private decimal _accumulator = 0;
        public int MemoryLimit { get { return _memory.Size(); } }

        //------------------------------------------------------------------------------------------------------------
        private EPC(int memoryLimit, bool debugging)
        {
            debug = debugging;
            Console.WriteLine(EPC.WELCOME);
            _memory = Memory.Instance.Size(memoryLimit).Initialize();
        }

        //------------------------------------------------------------------------------------------------------------
        public EPC Run()
        {
            _memory.Clear();
            return Prompt();
        }

        //------------------------------------------------------------------------------------------------------------
        public EPC Run(EPML program)
        {
            _memory.Clear();
            return loadProgram(program);
        }

        //------------------------------------------------------------------------------------------------------------
        private EPC loadProgram(EPML program)
        {
            Console.WriteLine($"*** Loading {program.Name} into memory ***");
            if (program.Description() != null && program.Description() != "")
            {
                Console.WriteLine($"*** {program.Description()} ***");
            }
            int max = _memory.Size();
            for (int i=0; i<max; ++i)
            {
                var instruction = program.Instructions[i];
                _memory.Put(instruction, i);
            }
            Console.WriteLine("*** Program loading completed ***");
            Console.WriteLine("*** Program execution begins ***");

            execute();
            return this;
        }

        //------------------------------------------------------------------------------------------------------------
        private EPC Prompt()
        {
            Console.Write($"{_instructionCounter.ToString("D2")} ? ");
            string input = Console.ReadLine();
            if (input == SENTINEL)
            {
                Console.WriteLine("*** Program loading completed ***");
                Console.WriteLine("*** Program execution begins ***");

                execute();
                return this;

            }
            try
            {
                decimal value = Convert.ToDecimal(input);
                _memory.Put(value, _instructionCounter);
                if (_memory.Get(_instructionCounter) == value) // Successfully stored in memory
                {
                    ++_instructionCounter;
                }
                return Prompt();
            }
            catch (FormatException)
            {
                return Prompt();
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private void execute()
        {
            try
            {
                Code code = 0;
                while (code != Code.HALT && _currentInstruction < _memory.Size())
                {
                    ++_instructionCounter;

                    _instructionRegister = _memory.Get(_currentInstruction);
                    Word.Extract(_instructionRegister, out _operationCode, out _operand);
                    code = getCode(_operationCode);
                    process(code);
                    if (debug) { Dump(); }
                    ++_currentInstruction;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Dump();
            }
        }


        //------------------------------------------------------------------------------------------------------------
        private void goTo(int location)
        {
            _currentInstruction = location;
            execute();
        }


        //------------------------------------------------------------------------------------------------------------
        private void process(Code code)
        {
            switch (code)
            {
                case Code.READ:
                    readLine();
                    break;
                case Code.WRITE:
                    writeLine();
                    break;
                case Code.LOAD:
                    load();
                    break;
                case Code.STORE:
                    store();
                    break;
                case Code.MULTIPLY:
                    multiply();
                    break;
                case Code.DIVIDE:
                    divide();
                    break;
                case Code.ADD:
                    add();
                    break;
                case Code.SUBTRACT:
                    subtract();
                    break;
                case Code.BRANCH:
                    branch();
                    break;
                case Code.BRANCHNEG:
                    branchneg();
                    break;
                case Code.BRANCHZERO:
                    branchzero();
                    break;
                case Code.HALT:
                    halt();
                    break;
                case Code.PAUSE:
                    pause();
                    break;
                default:
                    break;
            }
        }

        private void pause()
        {
            Console.Write("Program Paused.  View memory dump (N/y)? ");
            if (Console.ReadLine().ToUpper() == "Y") { Dump(); }
        }

        //------------------------------------------------------------------------------------------------------------
        private Code extractCode(decimal instruction)
        {
            Word.Extract(instruction, out int code, out int operand);
            return getCode(code);
        }

        //------------------------------------------------------------------------------------------------------------
        private Code getCode(int code)
        {
            return (Code)Enum.ToObject(typeof(Code), code);
        }

        //------------------------------------------------------------------------------------------------------------
        private void readLine()
        {
            Console.Write("Enter an integer ? ");
            string response = Console.ReadLine();
            try
            {
                int value = Convert.ToInt32(response);
                _memory.Put(value, _operand);
            }
            catch (FormatException)
            {
                readLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Dump();
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private void writeLine()
        {
            Console.WriteLine("\noutput:  " + _memory.Get(_operand).ToString() + '\n');
        }

        //------------------------------------------------------------------------------------------------------------
        private void load()
        {
            _accumulator = _memory.Get(_operand);
        }

        //------------------------------------------------------------------------------------------------------------
        private void store()
        {
            _memory.Put(_accumulator, _operand);
            _accumulator = 0;

        }

        //------------------------------------------------------------------------------------------------------------
        private void multiply()
        {
            _accumulator *= _memory.Get(_operand);
        }

        //------------------------------------------------------------------------------------------------------------
        private void divide()
        {
            _accumulator /= _memory.Get(_operand);
        }

        //------------------------------------------------------------------------------------------------------------
        private void add()
        {
            _accumulator += _memory.Get(_operand);
        }

        //------------------------------------------------------------------------------------------------------------
        private void subtract()
        {
            _accumulator -= _memory.Get(_operand);
        }

        //------------------------------------------------------------------------------------------------------------
        private void branch()
        {
            goTo(_operand);
        }

        //------------------------------------------------------------------------------------------------------------
        private void branchneg()
        {
            if (_accumulator < 0) { goTo(_operand); }
        }

        //------------------------------------------------------------------------------------------------------------
        private void branchzero()
        {
            if (_accumulator == 0) { goTo(_operand); }
        }

        //------------------------------------------------------------------------------------------------------------
        private void halt()
        {
            Console.WriteLine("*** EPC execution terminated");
            Dump();
        }

        //------------------------------------------------------------------------------------------------------------
        public void Dump()
        {
            string output = "";
            try
            {
                output = Output();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine(output);
            Console.WriteLine("-------------------------------------------------------------------------------------------------\n");
        }

        //------------------------------------------------------------------------------------------------------------
        public string Output()
        {
            return "REGISTERS:\n" +
                    String.Format("{0,-20}{1,8}\n", "accumulator", _accumulator.ToString()) +
                    String.Format("{0,-20}{1,8}\n", "instructionCounter", _instructionCounter.ToString("D2")) +
                    String.Format("{0,-20}{1,8}\n", "instructionRegister", _instructionRegister.ToString(Memory.FORMAT)) +
                    String.Format("{0,-20}{1,8}\n", "operationCode", _operationCode.ToString("D2")) +
                    String.Format("{0,-20}{1,8}\n", "operand", _operand.ToString("D2"))
                    + "\n\n" +
                    _memory.Dump();
        }

    }
}
