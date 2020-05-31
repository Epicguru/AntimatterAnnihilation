using RimWorld;

namespace AntimatterAnnihilation.ThingComps
{
    public class CompProperties_RefuelableSpecial : CompProperties_Refuelable
    {
        public int id;
        public int fuelPriority;

        public CompProperties_RefuelableSpecial()
        {
            base.compClass = typeof(CompRefuelableSpecial);
        }
    }
}
