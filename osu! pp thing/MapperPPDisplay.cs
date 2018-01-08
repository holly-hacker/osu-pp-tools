using System;
using System.IO;
using System.Linq;
using System.Threading;
using osu.Helpers;
using osu_database_reader.BinaryFiles;
using OppaiSharp;
using GameMode = osu.Shared.GameMode;

namespace osu_pp_thing
{
    internal class MapperPPDisplay
    {
        public static void Run()
        {
            try {
                //get IPC
                var ipc = (InterProcessOsu)Activator.GetObject(typeof(InterProcessOsu), "ipc://osu!/loader");

                //load osudb
                string osuPath = Helpers.GetOsuFolder();
                var db = OsuDb.Read(Path.Combine(osuPath, "osu!.db"));

                //get beatmap hash
                string hash = ipc.GetBulkClientData().BeatmapChecksum;

                //get beatmap
                var mapTemp = db.Beatmaps.FirstOrDefault(a => a.BeatmapChecksum == hash);
                if (mapTemp == null) throw new Exception("Map not found. If you recently added it, reload both osu! and this program.");
                if (mapTemp.GameMode != GameMode.Standard) throw new Exception("Only osu!std maps supported for now!");

                //get path
                string beatmapFile = Path.Combine(osuPath, "Songs", mapTemp.FolderName);

                //set handler to run code every time 
                var fs = new FileSystemWatcher(beatmapFile, "*.osu");
                void OnChanged(object sender, FileSystemEventArgs args) => Print(args.FullPath);
                //fs.Created += OnChanged;
                fs.Changed += OnChanged;
                fs.EnableRaisingEvents = true;

                //run this in a loop
                while (Helpers.CheckOsuExists())
                    Thread.Sleep(1000);

                //unsub from event handlers
                fs.Changed -= OnChanged;
            } catch (Exception e) {
                Console.Clear();
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }


            Console.WriteLine("osu! closed, we're closing too...");
        }

        private static void Print(string path)
        {
            //open beatmap
            Thread.Sleep(100);
            using (var sr = new StreamReader(File.OpenRead(path))) {
                var map = new Parser().Map(sr);

                Console.Clear();
                LivePPDisplay.Print(map, new[] {Mods.Hidden, Mods.Hardrock, Mods.DoubleTime});
            }
        }
    }
}
