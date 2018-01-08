using System;
using System.Reflection;
using System.Threading;

namespace osu_pp_thing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string mode = args.Length < 1 ? "live" : args[0];

            Helpers.WaitForOsu();

            switch (mode) {
                case "live":
                    SetAsmResolver();
                    LivePPDisplay.Run();
                    break;
                case "mapper":
                    SetAsmResolver();
                    MapperPPDisplay.Run();
                    break;
                case "help":
                    Console.WriteLine("Modes are 'live' and 'help'");
                    break;
            }
        }

        public static void SetAsmResolver()
        {
            //add assembly resolver
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return new AssemblyName(args.Name).Name == "osu!" && Helpers.CheckOsuExists()
                ? Assembly.LoadFile(Helpers.GetOsu().MainModule.FileName)
                : null;
        }
    }
}
