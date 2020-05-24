using Verse;

namespace AntimatterAnnihilation.ThingComps
{
    public class CompProperties_TargetChargeRate : CompProperties
    {
        public CompProperties_TargetChargeRate()
        {
            base.compClass = typeof(CompTargetChargeRate);
        }

        public float defaultRate = 1000f;
        public float interval = 100f;
        public FloatRange rateRange = new FloatRange(100, 5000);
    }
}