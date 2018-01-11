using System;
using System.IO;
using System.Reflection;

namespace osu_pp_tools
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string folder = null;

            //check if the argument is a folder to be used with Mapper mode
            if (args.Length == 1) {
                if (Directory.Exists(args[0]))
                    folder = args[0];
                else if (File.Exists(args[0]) && args[0].EndsWith(".osu"))
                    folder = Path.GetDirectoryName(args[0]);
                else {
                    Console.WriteLine("Parameter is not a path");
                    return;
                }
            }
            
            if (folder == null) {
                Helpers.WaitForOsu();
                SetAsmResolver();
                LivePPDisplay.Run();
            }
            else {
                MapperPPDisplay.Run(folder);
            }
        }

        private static bool _resolverSet = false;
        public static void SetAsmResolver()
        {
            //add assembly resolver
            if (_resolverSet) return;
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => 
                new AssemblyName(args.Name).Name == "osu!" && Helpers.CheckOsuExists()
                    ? Assembly.LoadFile(Helpers.GetOsu().MainModule.FileName)
                    : null;
            _resolverSet = true;
        }
    }
}
