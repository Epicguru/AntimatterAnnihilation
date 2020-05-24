using RimWorld;

namespace AntimatterAnnihilation.ThingComps
{
    public class CompProperties_AutoAttack : CompProperties_Explosive
    {
        public bool defaultAutoAttack = true;

        public CompProperties_AutoAttack()
        {
            base.compClass = typeof(CompAutoAttack);
        }
    }
}