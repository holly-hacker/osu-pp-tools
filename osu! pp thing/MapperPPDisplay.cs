using System;
using System.IO;
using System.Threading;
using OppaiSharp;

namespace osu_pp_tools
{
    internal class MapperPPDisplay
    {
        public static void Run(string folderPath)
        {
            try {
                //set handler to run code every time 
                var fs = new FileSystemWatcher(folderPath, "*.osu");
                void OnChanged(object sender, FileSystemEventArgs args) => Print(args.FullPath);
                fs.Changed += OnChanged;
                fs.EnableRaisingEvents = true;

                //run this in a loop
                while (true) Thread.Sleep(1000);
            } catch (Exception e) {
                Console.Clear();
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }

        private static void Print(string path)
        {
            //open beatmap
            Thread.Sleep(100);
            using (var sr = new StreamReader(File.OpenRead(path))) {
                var map = new Parser().Map(sr);

                Console.Clear();
                Helpers.PrintPP(map, new[] {Mods.Hidden, Mods.Hardrock, Mods.DoubleTime});
            }
        }
    }
}
