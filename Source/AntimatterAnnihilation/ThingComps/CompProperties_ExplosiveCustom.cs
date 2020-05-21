using RimWorld;
using Verse;

namespace AntimatterAnnihilation.ThingComps
{
    public class CompProperties_ExplosiveCustom : CompProperties_Explosive
    {
        public CompProperties_ExplosiveCustom()
        {
            base.compClass = typeof(CompExplosiveCustom);
        }

        public int wickTicksToPlayAudio = 30;
        public SoundDef customExplodeAudio;
    }
}