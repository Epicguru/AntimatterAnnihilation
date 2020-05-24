using RimWorld;

namespace AntimatterAnnihilation.ThingComps
{
    public class CompProperties_GreedyBattery : CompProperties_Battery
    {
        public bool doInspectorInfo = false; // Should the default battery inspector string be used?
        public bool doSelfDischarge = false; // Should the battery self discharge at the fixed 5W rate?
        public float maxPull = 500000; // Max rate of energy pull from regular batteries.

        public CompProperties_GreedyBattery()
        {
            base.compClass = typeof(CompGreedyBattery);
        }
    }
}