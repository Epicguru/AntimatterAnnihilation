using System.Diagnostics;

namespace RunRimworld
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string PATH = @"D:\Steam Games\steamapps\common\RimWorld\RimWorldWin64.exe";

            ProcessStartInfo info = new ProcessStartInfo(PATH);
            Process.Start(info);
        }
    }
}
