using UnityEngine;
using Verse;

namespace AntimatterAnnihilation
{
    public class Settings : ModSettings
    {
        public static float PowerGenMulti = 1f;
        public static float FuelConsumeRate = 1f;
        public static bool EnableEasterEggs = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref PowerGenMulti, "PowerGenMulti", 1f);
            Scribe_Values.Look(ref FuelConsumeRate, "FuelConsumeRate", 1f);
            Scribe_Values.Look(ref EnableEasterEggs, "EnableEasterEggs", true);
        }

        public static void DoWindow(Rect window)
        {
            DoLabel("AA.PowerGenMulti".Translate($"{PowerGenMulti*100f:F0}"));
            PowerGenMulti = Widgets.HorizontalSlider(new Rect(window.x, window.y, window.width * 0.5f, 32), PowerGenMulti, 0.1f, 5f, leftAlignedLabel: "10%", rightAlignedLabel: "500%", roundTo: 0.05f);
            MoveDown(32 + 10f);

            DoLabel("AA.FuelConsumeRate".Translate($"{FuelConsumeRate * 100f:F0}"));
            FuelConsumeRate = Widgets.HorizontalSlider(new Rect(window.x, window.y, window.width * 0.5f, 32), FuelConsumeRate, 0.05f, 5f, leftAlignedLabel: "5%", rightAlignedLabel: "500%", roundTo: 0.05f);
            MoveDown(32 + 10f);

            Widgets.CheckboxLabeled(new Rect(window.x, window.y, 350, 32), "AA.EnableEasterEggs".Translate(), ref EnableEasterEggs, placeCheckboxNearText: true);
            MoveDown(32 + 10f);

            void DoLabel(string label)
            {
                const float HEIGHT = 32;
                Widgets.Label(new Rect(window.x, window.y, window.width, HEIGHT), label);
                MoveDown(HEIGHT + 10);
            }

            void MoveDown(float amount)
            {
                window.y += amount;
                window.height -= amount;
            }
        }
    }
}
