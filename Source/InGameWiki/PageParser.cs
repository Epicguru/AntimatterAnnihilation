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

                return -string.Compare(nameA, nameB, StringComparison.InvariantCultureIgnoreCase);
            });

            foreach (var file in files)
            {
                var info = new FileInfo(file);
                string fileName = info.Name;
                fileName = fileName.Substring(0, fileName.Length - info.Extension.Length);
                if (fileName.StartsWith("Thing_"))
                {
                    string thingDefName = fileName.Substring(6);
                    var existing = wiki.FindPageFromDef(thingDefName);
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

            if (existing == null && lines.Length < 5)
            {
                Log.Error("Expected minimum 5 lines for ID, title, icon, background and description.");
                return null;
            }
            if (existing != null && lines.Length < 1)
            {
                Log.Error($"Expected minimum 1 lines for background when appending to Thing {existing.Def.defName}");
                return null;
            }

            WikiPage p = existing ?? new WikiPage();
            if (existing == null)
            {
                string id = string.IsNullOrWhiteSpace(lines[0].Trim()) ? null : lines[0].Trim();
                string title = string.IsNullOrWhiteSpace(lines[1].Trim()) ? null : lines[1].Trim();
                Texture2D icon = ContentFinder<Texture2D>.Get(lines[2].Trim(), false);
                Texture2D bg = ContentFinder<Texture2D>.Get(lines[3].Trim(), false);
                string desc = string.IsNullOrWhiteSpace(lines[4].Trim()) ? null : lines[4].Trim();

                if (id == null)
                    Log.Warning($"External wiki page with title {title} has a null ID. It may break things.");

                p.ID = id;
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
            for (int i = (existing == null ? 5 : 1); i < lines.Length; i++)
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
                        Vector2? size = null;
                        if (final.Contains(':'))
                        {
                            var split = final.Split(':');
                            final = split[0];
                            if (split[1].Contains(","))
                            {
                                bool workedX = float.TryParse(split[1].Split(',')[0], out float x);
                                bool workedY = float.TryParse(split[1].Split(',')[1], out float y);

                                if (workedX && workedY)
                                {
                                    size = new Vector2(x, y);
                                }
                                else
                                {
                                    Log.Error($"Error in wiki parse: failed to parse Vector2 for image size, '{split[1]}', failed to parse {(workedX ? "y" : "x")} value as a float.");
                                }
                            }
                            else
                            {
                                Log.Error($"Error in wiki parse: failed to parse Vector2 for image size, '{split[1]}', expected format 'x, y'.");
                            }
                        }
                        Texture2D loaded = ContentFinder<Texture2D>.Get(final, false);
                        p.Elements.Add(new WikiElement()
                        {
                            Image = loaded,
                            AutoFitImage = size == null,
                            ImageSize = size ?? new Vector2(-1, -1)
                        });
                        continue;
                    }

                    // ThingDef links
                    final = CheckParseChar('@', CurrentlyParsing.ThingDefLink, last, i, c, ref parsing, ref add);
                    if (final != null)
                    {
                        string label = null;
                        if (final.Contains(':'))
                        {
                            var split = final.Split(':');
                            final = split[0];
                            label = split[1];
                        }

                        Def def = ThingDef.Named(final);
                        if (def != null)
                        {
                            p.Elements.Add(new WikiElement()
                            {
                                DefForIconAndLabel = def,
                                Text = label
                            });
                        }
                        else
                        {
                            AddText($"<i>MissingDefLink [{final}]</i>", false);
                        }
                        continue;
                    }

                    // Page links
                    final = CheckParseChar('~', CurrentlyParsing.PageLink, last, i, c, ref parsing, ref add);
                    if (final != null)
                    {
                        p.Elements.Add(new WikiElement()
                        {
                            PageLink = final,
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
            Image,
            ThingDefLink,
            PageLink
        }
    }
}
