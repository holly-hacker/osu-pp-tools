using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using OppaiSharp;

namespace osu_pp_tools
{
    internal static class Helpers
    {
        public static void WaitForOsu()
        {
            //fuck you, Green
            for (Console.Write("Waiting for osu!."); !CheckOsuExists(); Thread.Sleep(1000))
                Console.Write(".");
            Console.WriteLine(" Found osu!");
        }

        /// <summary>
        /// Returns the directory the running osu! executable is in.
        /// Requires osu! to be open.
        /// </summary>
        /// <returns></returns>
        public static string GetOsuFolder()
        {
            string exePath = GetOsu()?.MainModule.FileName;
            return exePath?.Substring(0, exePath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }

        public static bool CheckOsuExists() => Process.GetProcessesByName("osu!").Any();
        public static Process GetOsu() => Process.GetProcessesByName("osu!").First();

        internal static void PrintPP(Beatmap map, Mods[] allMods)
        {
            Console.WriteLine($"Map: {map.Artist} - {map.Title} [{map.Version}]");
            Console.WriteLine($"Mapper: {map.Creator}");
            Console.WriteLine();
            Console.WriteLine($"CS{map.CS} AR{map.AR} OD{map.OD} HP{map.HP}");
            Console.WriteLine();


            for (int i = 0; i <= (1 << allMods.Length) - 1; i++)
            {
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
                    var pp = new PPv2(new PPv2Parameters { Beatmap = map, AimStars = diff.Aim, SpeedStars = diff.Speed, Mods = mods, Count100 = acc.Count100, Count50 = acc.Count50 });
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
