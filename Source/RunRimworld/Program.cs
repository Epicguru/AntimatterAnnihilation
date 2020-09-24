using System.Diagnostics;

namespace RunRimworld
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string PATH = @"C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64.exe";

            ProcessStartInfo info = new ProcessStartInfo(PATH);
            Process.Start(info);
        }
    }
}
