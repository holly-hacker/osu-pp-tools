using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using osu.Helpers;
using osu_database_reader.BinaryFiles;
using OppaiSharp;
using GameMode = osu.Shared.GameMode;

namespace osu_pp_thing
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
                            Print(map, new[] {Mods.Hidden, Mods.Hardrock, Mods.DoubleTime});
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

        internal static void Print(Beatmap map, Mods[] allMods)
        {
            Console.WriteLine($"Map: {map.Artist} - {map.Title} [{map.Version}]");
            Console.WriteLine($"Mapper: {map.Creator}");
            Console.WriteLine();
            Console.WriteLine($"CS{map.CS} AR{map.AR} OD{map.OD} HP{map.HP}");
            Console.WriteLine();


            for (int i = 0; i <= (1 << allMods.Length) - 1; i++) {
                var mods = Mods.NoMod;
                for (int j = 0; j <= allMods.Length; j++)
                    if ((i & 1 << j) != 0)
                        mods |= allMods[j];

                var diff = new DiffCalc().Calc(map, mods);

                var modStr = OppaiSharp.Helpers.ModsToString(mods);
                if (string.IsNullOrWhiteSpace(modStr)) modStr = "NoMod";

                void ShowWithPercent(double percent)
                {
                    var acc = new Accuracy(percent, map.Objects.Count, 0);
                    var pp = new PPv2(new PPv2Parameters {Beatmap = map, AimStars = diff.Aim, SpeedStars = diff.Speed, Mods = mods, Count100 = acc.Count100, Count50 = acc.Count50});
                    Console.Write($" | {(pp.Total.ToString("F2") + "pp").PadRight(9)} {("(" + percent.ToString(CultureInfo.CurrentCulture) + "%)").PadLeft(4 + 3)}");
                }

                Console.Write($"{modStr.PadRight(6)}| {diff.Total:F2}* (Aim: {diff.Aim:F2}*, Speed: {diff.Speed:F2}*)");
                ShowWithPercent(92.5);
                ShowWithPercent(95);
                ShowWithPercent(97.5);
                ShowWithPercent(100);
                Console.WriteLine();
            }
        }
    }
}
