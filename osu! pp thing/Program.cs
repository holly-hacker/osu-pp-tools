using System;
using System.Threading;

namespace osu_pp_thing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string mode = args.Length < 1 ? "live" : args[0];

            switch (mode) {
                case "live":
                    LivePPDisplay.SetAsmResolver();
                    LivePPDisplay.Run();
                    break;
                case "help":
                    Console.WriteLine("Modes are 'live' and 'help'");
                    break;
            }
        }
    }
}
