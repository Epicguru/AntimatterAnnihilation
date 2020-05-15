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

                return string.Compare(nameA, nameB, StringComparison.Ordinal);
            });

            foreach (var file in files)
            {
                var info = new FileInfo(file);
                string fileName = info.Name;
                fileName = fileName.Substring(0, fileName.Length - info.Extension.Length);
                if (fileName.StartsWith("Thing_"))
                {
                    string thingDefName = fileName.Substring(6);
                    var existing = wiki.GetPage(thingDefName);
                    if (existing != null)
                    {
                        Parse(File.ReadAllText(file), existing);
                        //Log.Message("Added to existing " + file);
                    }
                    else
                    {
                        Log.Error("Failed to find Thing wiki entry for wiki page: Thing_" + thingDefName);
                    }
                    continue;
                }

                var page = Parse(File.ReadAllText(file), null);
                if (page == null)
                {
                    Log.Error($"Failed to load wiki page from {file}");
                    continue;
                }

                //Log.Message("Added " + file);
                wiki.Pages.Insert(0, page);
            }
        }

        public static WikiPage Parse(string rawText, WikiPage existing)
        {
            string[] lines = rawText.Split('\n');

            if (lines.Length < 4)
            {
                Log.Error("Expected minimum 4 lines for title, icon, background and description.");
                return null;
            }

            WikiPage p = existing ?? new WikiPage();
            if (existing == null)
            {
                string title = string.IsNullOrWhiteSpace(lines[0].Trim()) ? null : lines[0].Trim();
                Texture2D icon = ContentFinder<Texture2D>.Get(lines[1].Trim(), false);
                Texture2D bg = ContentFinder<Texture2D>.Get(lines[2].Trim(), false);
                string desc = string.IsNullOrWhiteSpace(lines[3].Trim()) ? null : lines[3].Trim();

                p.Title = title;
                p.Icon = icon;
                p.ShortDescription = desc;
                p.Background = bg;
            }
            else
            {
                Texture2D bg = ContentFinder<Texture2D>.Get(lines[0].Trim(), false);
                p.Background = bg;
            }

            StringBuilder str = new StringBuilder();
            CurrentlyParsing parsing = CurrentlyParsing.None;
            for (int i = (existing == null ? 4 : 1); i < lines.Length; i++)
            {
                string line = lines[i];
                line += '\n';

                char last = char.MinValue;
                foreach (var c in line)
                {
                    int add = 0;

                    // Text
                    string final = CheckParseChar('#', CurrentlyParsing.Text, last, i, c, ref parsing, ref add);
                    if(final != null)
                    {
                        if (final.StartsWith("!"))
                            AddText(final.Substring(1), true);
                        else
                            AddText(final, false);
                        continue;
                    }

                    // Images
                    final = CheckParseChar('$', CurrentlyParsing.Image, last, i, c, ref parsing, ref add);
                    if (final != null)
                    {
                        Texture2D loaded = ContentFinder<Texture2D>.Get(final, false);
                        p.Elements.Add(new WikiElement()
                        {
                            Image = loaded,
                            AutoFitImage = true
                        });
                        continue;
                    }

                    if (add > 0)
                        str.Append(c);
                    last = c;
                }
            }
            str.Clear();

            string CheckParseChar(char tag, CurrentlyParsing newState, char last, int i, char c, ref CurrentlyParsing currentParsing, ref int shouldAppend)
            {
                if (c != tag)
                {
                    if (currentParsing != CurrentlyParsing.None)
                        shouldAppend++;
                    return null;
                }

                if (last == '\\')
                {
                    // Escape char. This must be added and the slash removed.
                    if (currentParsing != CurrentlyParsing.None)
                    {
                        str.Remove(str.Length - 1, 1);
                        shouldAppend++;
                    }
                    return null;
                }

                // This is either an opening or closing tag, either way it should definitely not be added.
                shouldAppend = -1000;

                if (currentParsing == CurrentlyParsing.None)
                {
                    // Start state.
                    currentParsing = newState;
                    str.Clear();
                    return null;
                }
                else if (currentParsing == newState)
                {
                    // End state.
                    currentParsing = CurrentlyParsing.None;
                    string s = str.ToString();
                    str.Clear();
                    return s;
                }
                else
                {
                    // Invalid.
                    Log.Error($"Error parsing wiki on line {i + 1}: got '{c}' which is invalid since {currentParsing} is currently active.");
                    return null;
                }
            }

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

        public enum CurrentlyParsing
        {
            None,
            Text,
            Image
        }
    }
}
