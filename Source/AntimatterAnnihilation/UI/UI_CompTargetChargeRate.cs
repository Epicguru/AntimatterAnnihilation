using AntimatterAnnihilation.ThingComps;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.UI
{
    class UI_CompTargetChargeRate : Window
    {
        public override Vector2 InitialSize => new Vector2(330, 80);
        public CompTargetChargeRate Comp;

        public UI_CompTargetChargeRate(CompTargetChargeRate comp)
        {
            this.Comp = comp;
            doCloseX = true;
            onlyOneOfTypeAllowed = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Comp == null)
            {
                Close();
                return;
            }

            Comp.Watts = Widgets.HorizontalSlider(inRect, Comp.Watts, Comp.MinRate, Comp.MaxRate, true, $"Rate: {Comp.Watts:F0} Watts", Comp.MinRate.ToString("F0"), Comp.MaxRate.ToString("F0"), Comp.Interval);
        }
    }
}
