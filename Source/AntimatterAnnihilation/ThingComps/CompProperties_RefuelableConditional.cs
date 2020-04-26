using RimWorld;

namespace AntimatterAnnihilation.ThingComps
{
    public class CompProperties_RefuelableConditional : CompProperties_Refuelable
    {
        public CompProperties_RefuelableConditional()
        {
            base.compClass = typeof(CompRefuelableConditional);
        }
    }
}
