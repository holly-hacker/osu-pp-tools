using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace osu_pp_thing
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
    }
}
