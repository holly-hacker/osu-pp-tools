using System;
using System.IO;
using System.Linq;
using System.Threading;
using osu.Helpers;
using osu_database_reader.BinaryFiles;
using OppaiSharp;
using GameMode = osu.Shared.GameMode;

namespace osu_pp_tools
{
    internal static class LivePPDisplay
    {
        public static void Run()
        {
            //get IPC
            var ipc = (InterProcessOsu)Activator.GetObject(typeof(InterProcessOsu), "ipc://osu!/loader");

            //load osudb
            string osuPath = Helpers.GetOsuFolder();
            var db = OsuDb.Read(Path.Combine(osuPath, "osu!.db"));

            string hash = new string('0', 32);
            while (Helpers.CheckOsuExists()) {
                try {
                    var data = ipc.GetBulkClientData();
                    if (data.BeatmapChecksum != hash) {
                        //get beatmap
                        hash = data.BeatmapChecksum;
                        var mapTemp = db.Beatmaps.FirstOrDefault(a => a.BeatmapChecksum == hash);
                        if (mapTemp == null) throw new Exception("Map not found. If you recently added it, reload both osu! and this program.");
                        if (mapTemp.GameMode != GameMode.Standard) throw new Exception("Only osu!std maps supported for now!");

                        //open beatmap
                        using (var sr = new StreamReader(File.OpenRead(Path.Combine(osuPath, "Songs", mapTemp.FolderName, mapTemp.BeatmapFileName)))) {
                            var map = new Parser().Map(sr);
                            
                            Console.Clear();
                            Console.WriteLine("MD5: " + hash);
                            Helpers.PrintPP(map, new[] {Mods.Hidden, Mods.Hardrock, Mods.DoubleTime});
                        }
                    }
                } catch (Exception e) {
                    Console.Clear();
                    Console.WriteLine(e.Message);
                }

                Thread.Sleep(100);
            }

            Console.WriteLine("osu! closed, we're closing too...");
        }
    }
}
