using RimWorld;

namespace AntimatterAnnihilation.ThingComps
{
    /// <summary>
    /// This component (property) is a refuelable object that only consumes fuel while turned on and also powered.
    /// </summary>
    public class CompProperties_RefuelableConditional : CompProperties_Refuelable
    {
        public CompProperties_RefuelableConditional()
        {
            this.compClass = typeof(CompRefuelableConditional);
        }
    }
}
