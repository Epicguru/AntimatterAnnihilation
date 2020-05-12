using System.IO;
using System.Text;
using UnityEngine;
using Verse;

namespace InGameWiki
{
    public static class PageParser
    {
        public static void AddAllFromDirectory(ModWiki wiki, string dir)
        {
            if (wiki == null)
                return;
            if (!Directory.Exists(dir))
                return;

            string[] files = Directory.GetFiles(dir, "*.txt", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var page = Parse(File.ReadAllText(file));
                if (page == null)
                {
                    Log.Error($"Failed to load wiki page from {file}");
                    continue;
                }

                wiki.Pages.Add(page);
            }
        }

        public static WikiPage Parse(string rawText)
        {
            string[] lines = rawText.Split('\n');

            if (lines.Length < 3)
            {
                Log.Error("Expected minimum 3 lines for title, icon and description.");
                return null;
            }

            string title = string.IsNullOrWhiteSpace(lines[0].Trim()) ? null : lines[0].Trim();
            Texture2D icon = ContentFinder<Texture2D>.Get(lines[1].Trim(), false);
            string desc = string.IsNullOrWhiteSpace(lines[1].Trim()) ? null : lines[1].Trim();

            WikiPage p = new WikiPage();
            p.Title = title;
            p.Icon = icon;
            p.ShortDescription = desc;

            bool largeText = false;
            StringBuilder str = new StringBuilder();
            for (int i = 3; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line[0] == '-')
                {
                    AddText(str.ToString(), largeText);
                    largeText = line.Length >= 2 && line[1] == '-';
                    str.Clear();
                    str.Append(line.Substring(largeText ? 2 : 1));
                }
                else
                {
                    str.Append(line);
                }
            }
            AddText(str.ToString(), largeText); // TODO change to allow images and other types.
            str.Clear();

            void AddText(string txt, bool large)
            {
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    var text = WikiElement.Create(txt);
                    text.FontSize = large ? GameFont.Medium : GameFont.Small;
                    p.Elements.Add(text);
                }
            }

            return p;
        }
    }
}
