using RimWorld;

namespace AntimatterAnnihilation.ThingComps
{
    public class CompProperties_RefuelableMulti : CompProperties_Refuelable
    {
        public int id;
        public int fuelPriority;

        public CompProperties_RefuelableMulti()
        {
            base.compClass = typeof(CompRefuelableMulti);
        }
    }
}
