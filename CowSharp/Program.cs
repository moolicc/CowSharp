using System;
using System.IO;

namespace CowSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var vm = new VirtualMachine();

            var file = "";
            bool debug = false;
            bool step = false;
            foreach (var item in args)
            {
                if (File.Exists(item))
                {
                    file = item;
                }
                else if (string.Equals(item, "/d", StringComparison.OrdinalIgnoreCase))
                {
                    debug = true;
                }
                else if(string.Equals(item, "/s", StringComparison.OrdinalIgnoreCase))
                {
                    step = true;
                }
            }

            if(string.IsNullOrEmpty(file))
            {
                PrintHelp();
            }
            else
            {
                using (var stream = new StreamReader(file, System.Text.Encoding.ASCII))
                {
                    vm.LoadStream(stream);
                }

                if(debug && step)
                {
                    vm.DebugExecuteStep();
                }
                else if(debug)
                {
                    vm.DebugExecute();
                }
                else
                {
                    vm.Execute();
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage: CowSharp.exe FILE [/d] [/s]");
            Console.WriteLine("/d - Start in debug mode. Debug mode prints a bunch of garbage in an attempt to help you analyze the program's state.");
            Console.WriteLine("/s - Step mode. Step mode requires user interaction before executing each instruction.");
        }
    }
}
