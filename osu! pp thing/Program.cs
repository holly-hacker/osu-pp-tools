using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using osu.Helpers;
using osu_database_reader.BinaryFiles;
using OppaiSharp;
using GameMode = osu.Shared.GameMode;

namespace osu__pp_thing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            for (Console.Write("Waiting for osu!."); !CheckOsuExists(); Thread.Sleep(1000))
                Console.Write(".");
            Console.WriteLine(" Found osu!");

            //get IPC
            var ipc = (InterProcessOsu)Activator.GetObject(typeof(InterProcessOsu), "ipc://osu!/loader");

            //load osudb
            string exePath = GetOsu().MainModule.FileName;
            string osuPath = exePath.Substring(0, exePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            var db = OsuDb.Read(Path.Combine(osuPath, "osu!.db"));

            string hash = new string('0', 32);
            while (CheckOsuExists()) {
                try {
                    var data = ipc.GetBulkClientData();
                    if (data.BeatmapChecksum != hash) {
                        //get beatmap
                        hash = data.BeatmapChecksum;
                        var mapTemp = db.Beatmaps.FirstOrDefault(a => a.BeatmapChecksum == hash);
                        if (mapTemp?.GameMode != GameMode.Standard) continue;

                        //open beatmap
                        using (var sr = new StreamReader(File.OpenRead(Path.Combine(osuPath, "Songs", mapTemp.FolderName, mapTemp.BeatmapFileName)))) {
                            var map = new Parser().Map(sr);

                            Console.Clear();
                            Console.WriteLine("MD5: " + hash);
                            Console.WriteLine($"Map: {map.Artist} - {map.Title} [{map.Version}]");
                            Console.WriteLine($"Mapper: {map.Creator}");
                            Console.WriteLine();
                            Console.WriteLine($"CS{map.CS} AR{map.AR} OD{map.OD} HP{map.HP}"); //TODO: use calculated
                            Console.WriteLine();
                            for (int i = 0; i <= 0b111; i++) {
                                var mods = Mods.NoMod;
                                if ((i & 0b001) != 0) mods |= Mods.Hidden;
                                if ((i & 0b010) != 0) mods |= Mods.Hardrock;
                                if ((i & 0b100) != 0) mods |= Mods.DoubleTime;
                                var diff = new DiffCalc().Calc(map, mods);

                                var modStr = Helpers.ModsToString(mods);
                                if (string.IsNullOrWhiteSpace(modStr)) modStr = "NoMod";

                                void ShowWithPercent(double percent)
                                {
                                    var acc = new Accuracy(percent, map.Objects.Count, 0);
                                    var pp = new PPv2(new PPv2Parameters {Beatmap = map, AimStars = diff.Aim, SpeedStars = diff.Speed, Mods = mods, Count100 = acc.Count100, Count50 = acc.Count50});
                                    Console.Write($" | {(pp.Total.ToString("F2") + "pp").PadRight(9)} {("(" + percent.ToString(CultureInfo.CurrentCulture) + "%)").PadLeft(4 + 3)}");
                                }

                                Console.Write($"{modStr.PadRight(6)}");
                                ShowWithPercent(92.5);
                                ShowWithPercent(95);
                                ShowWithPercent(97.5);
                                ShowWithPercent(100);
                                Console.WriteLine();
                            }
                        }
                    }
                } catch (Exception e) {
                    Console.WriteLine(e);
                }

                Thread.Sleep(100);
            }

            Console.WriteLine("osu! closed, we're closing too...");
        }

        private static bool CheckOsuExists() => Process.GetProcessesByName("osu!").Any();
        private static Process GetOsu() => Process.GetProcessesByName("osu!").First();
    }
}
