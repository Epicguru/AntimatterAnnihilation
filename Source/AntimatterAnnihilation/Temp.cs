using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation
{
    [StaticConstructorOnStartup]
    internal static class Temp
    {
        [HarmonyPatch(typeof(UIRoot_Entry), "Init")]
        class OnMainMenuShow
        {
            static void Prefix()
            {
                // Disabled until I want to harass someone else.

                //try
                //{
                //    Log.Message($"Name: '{SteamUtility.SteamPersonaName}'");
                //    Log.Message($"WindowStack == null: '{Find.WindowStack == null}'");
                //    if (SteamUtility.SteamPersonaName == "Epicguru" || SteamUtility.SteamPersonaName == "Roll1D2Games")
                //    {
                //        if (!Settings.dontShowAgain)
                //            ShowMsgToRoll1D2();
                //    }
                //}
                //catch (Exception e)
                //{
                //    Log.Warning("Oops: " + e);
                //}
            }
        }

        static void ShowMsgToRoll1D2()
        {
            Find.WindowStack.Add(new CustomWindow());
        }

        class CustomWindow : Window
        {
            private bool dontShowAgain = false;

            public CustomWindow()
            {
                base.doCloseButton = true;
            }

            public override void DoWindowContents(Rect inRect)
            {
                Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 150);
                Widgets.Label(titleRect, "<size=54><color=#9aff47><b>Merry Christmas Mr. Streamer!</b></color></size>");

                Rect textRect = new Rect(inRect.x, inRect.y + 150, inRect.width, inRect.height - 150);
                Widgets.Label(textRect, "Seeing you play with my mod has made me want to take up mod development again!\nKeep up the great content, but try to get some rest too, it's christmas after all.\nI was gonna include a funny Joris here too but my artistic skills are even worse than yours :)\n\nTake care!\nEpicguru <i>(Antimatter Annihilation)</i>");

                Widgets.CheckboxLabeled(new Rect(inRect.x, inRect.yMax - 72, inRect.width * 0.5f, 32), "Don't show again", ref dontShowAgain);
            }

            public override void PreClose()
            {
                base.PreClose();

                if (dontShowAgain)
                {
                    Settings.dontShowAgain = true;
                    ModCore.Instance.WriteSettings();
                }
            }
        }
    }
}
