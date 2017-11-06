using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Computer_Simulator
{
    class Program
    {
        //------------------------------------------------------------------------------------------------------------
        static void Main(string[] args)
        {
            Console.WriteLine("-----------------------------------------------------------------------------\n");
            Console.WriteLine("-   Computer Simulator          IS 318 C# Programmming          Tim Brand   -\n");
            Console.WriteLine("-----------------------------------------------------------------------------\n");
            bool running = true;
            if (args.Length > 0)
            {
                runProgramWithArgs(args);
                Console.Write("Press the [Enter] key to exit.");
                Console.ReadLine();
            }
            else
            {
                while (running)
                {
                    Console.Clear();
                    running = selectAssignment();
                    if (running)
                    {
                        Console.Write("\n Run again (Y/n)? ");
                        string response = Console.ReadLine();
                        running = response == "" || response.ToUpper() == "Y";
                    }
                }
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private static void runProgramWithArgs(string[] args)
        {
            bool debug = false;
            string filename = "";
            try
            {
                int fileArg = Array.IndexOf(args, "--file");
                debug = Array.IndexOf(args, "--debug") >= 0;
                if (fileArg >= 0)
                {
                    filename = args[fileArg + 1];
                    if (filename.StartsWith(".."))
                    {
                        filename = Directory.GetCurrentDirectory() + '\\' + filename;
                    }

                    // Check to make sure filename is an EPO file
                    string extension = Path.GetExtension(filename).ToLower();
                    if (extension != ".epo")
                    {
                        throw new IOException($"Invalid file type ({extension}).  Only .epo files allowed");
                    }
                    runProgramFromFile(filename, debug);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (debug) { Console.WriteLine(ex.StackTrace); }
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private static void runProgramFromFile(string filename, bool debugging = false)
        {
            // Assume that each line of the program file contains a location and instruction   eg. 01 +1009
            try
            {
                string[] lines = File.ReadAllLines(filename);
                var vm = EPC.Make(debugging: debugging);
                var program = EPML.Make().FillTo(vm.MemoryLimit).Load(lines);
                Console.Clear();
                vm.Run(program);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        //------------------------------------------------------------------------------------------------------------
        static bool selectAssignment()
        {
            Console.WriteLine("*** Enter (1) to run Project ***");
            Console.WriteLine("*** Enter (6) to run PA06 ***");
            Console.WriteLine("*** Enter (9) to run PA09 ***");
            Console.Write("Selection? ");
            string response = Console.ReadLine();
            Console.Clear();
            try
            {
                int value = Convert.ToInt32(response);
                switch (value)
                {
                    case 1:
                        return runProject();
                    case 6:
                        return runPA06();
                    case 9:
                        return runPA09();
                }
                Console.WriteLine("*** Selection not recognized.  Try again (Y/n)?");
                if (Console.ReadLine().ToUpper() == "Y")
                {
                    return selectAssignment();
                }
                return false;
            }
            catch
            {
                return false;
            }

        }

        //------------------------------------------------------------------------------------------------------------
        private static string promptForFile()
        {
            List<string> files = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.epo")
                .Select(Path.GetFileName)
                .ToList();
            for (int i=0; i<files.Count(); ++i)
            {
                Console.WriteLine($"*** Enter ({i + 1}) to run {files[i]} ***");
            }
            Console.Write("Selection? ");
            string response = Console.ReadLine();
            Console.Clear();
            try
            {
                int value = Convert.ToInt32(response);
                if (value > files.Count())
                {
                    Console.WriteLine("*** Selection not recognized.  Try again (Y/n)?");
                    if (Console.ReadLine().ToUpper() == "Y")
                    {
                        return promptForFile();
                    }
                }
                else
                {
                    return files[value - 1];
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        //------------------------------------------------------------------------------------------------------------
        private static bool runProject()
        {
            Console.WriteLine("\n**********************IS318-Project********************\n");
            string filename = promptForFile();
            runProgramFromFile(Directory.GetCurrentDirectory() + '\\' + filename);
            return true;
        }

        //------------------------------------------------------------------------------------------------------------
        private static bool runPA09()
        {
            Console.WriteLine("\n**********************IS318-PA09********************\n");
            PostFixTest.RunTests();
            return true;
        }

        //------------------------------------------------------------------------------------------------------------
        private static bool runPA06()
        {
            Console.WriteLine("\n**********************IS318-PA06********************\n");
            var vm = EPC.Make();

            var program = offerProgramChoices();
            if (program != null)
            {
                vm.Run(program);
            }
            else
            {
                vm.Run();
            }
            return true;
        }

        //------------------------------------------------------------------------------------------------------------
        private static EPML offerProgramChoices()
        {
            var programOne = testProgramOne();
            var programTwo = testProgramTwo();
            var programThree = testProgramThree();
            Console.WriteLine($"*** Enter (1) for {programOne.Name} ***");
            Console.WriteLine($"*** Enter (2) for {programTwo.Name} ***");
            Console.WriteLine($"*** Enter (3) for {programThree.Name} ***");
            Console.WriteLine("*** Enter anything else to input code manually ***");
            Console.Write("Selection?");
            string response = Console.ReadLine();
            Console.Clear();
            try
            {
                int value = Convert.ToInt32(response);
                switch (value)
                {
                    case 1:
                        return programOne;
                    case 2:
                        return programTwo;
                    case 3:
                        return programThree;
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        //------------------------------------------------------------------------------------------------------------
        private static EPML testProgramOne()
        {
            return EPML.Make("Test Program One")
                .Description("EPML program that reads two integers and computes their sum.")
                .Add(+1007)     // Read A
                .Add(+1008)     // Read B
                .Add(+2007)     // Load A
                .Add(+3208)     // Add B
                .Add(+2109)     // Store C
                .Add(+1109)     // Write C
                .Add(+5000)     // Halt
                .Add(+0000)     // Variable A
                .Add(+0000)     // Variable B
                .Add(+0000);    // Result C
        }

        //------------------------------------------------------------------------------------------------------------
        private static EPML testProgramTwo()
        {
            return EPML.Make("Test Program Two")
                .Description("EPML program that reads two numbers and displays the larger value")
                .Add(+1009)     // Read A
                .Add(+1010)     // Read B
                .Add(+2009)     // Load A
                .Add(+3310)     // Subtract B
                .Add(+4107)     // Branch negative to 07
                .Add(+1109)     // Write A
                .Add(+5000)     // Halt
                .Add(+1110)     // Write B
                .Add(+5000)     // Halt
                .Add(+0000)     // Variable A
                .Add(+0000);    // Variable B
        }

        //------------------------------------------------------------------------------------------------------------
        private static EPML testProgramThree()
        {
            return EPML.Make("Test Program Three")
                .Description("EPML program that calculates several integers")
                .Add(+1098)
                .Add(+2098)
                .Add(+3397)
                .Add(+4211)
                .Add(+2098)
                .Add(+3098)
                .Add(+2195)
                .Add(+2095)
                .Add(+2196)
                .Add(+1196)
                .Add(+4000)
                .Add(+5000);
        }

    }
}
