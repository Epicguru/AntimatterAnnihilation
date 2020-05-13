using System;
using System.IO;
using System.Linq;
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

            var files = Directory.GetFiles(dir, "*.txt", SearchOption.AllDirectories).ToList();

            files.Sort((a, b) =>
            {
                string nameA = new FileInfo(a).Name;
                string nameB = new FileInfo(b).Name;

                return -string.Compare(nameA, nameB, StringComparison.Ordinal);
            });

            foreach (var file in files)
            {
                var page = Parse(File.ReadAllText(file));
                if (page == null)
                {
                    Log.Error($"Failed to load wiki page from {file}");
                    continue;
                }

                Log.Message("Added " + file);
                wiki.Pages.Add(page);
            }
        }

        public static WikiPage Parse(string rawText)
        {
            string[] lines = rawText.Split('\n');

            if (lines.Length < 4)
            {
                Log.Error("Expected minimum 4 lines for title, icon, background and description.");
                return null;
            }

            string title = string.IsNullOrWhiteSpace(lines[0].Trim()) ? null : lines[0].Trim();
            Texture2D icon = ContentFinder<Texture2D>.Get(lines[1].Trim(), false);
            Texture2D bg = ContentFinder<Texture2D>.Get(lines[2].Trim(), false);
            string desc = string.IsNullOrWhiteSpace(lines[3].Trim()) ? null : lines[3].Trim();

            WikiPage p = new WikiPage();
            p.Title = title;
            p.Icon = icon;
            p.ShortDescription = desc;
            p.Background = bg;

            bool largeText = false;
            StringBuilder str = new StringBuilder();
            for (int i = 4; i < lines.Length; i++)
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
                else if(line[0] == '*')
                {
                    string imagePath = line.Substring(1).Trim();
                    Texture2D loaded = ContentFinder<Texture2D>.Get(imagePath, false);
                    p.Elements.Add(new WikiElement()
                    {
                        Image = loaded,
                        AutoFitImage = true
                    });
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
