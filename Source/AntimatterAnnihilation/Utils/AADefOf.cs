using RimWorld;
using Verse;

namespace AntimatterAnnihilation.Utils
{
    [DefOf]
    public static class AADefOf
    {
        static AADefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AADefOf));
        }

        public static DamageDef Annihilate_AA;
        public static DamageDef EnergyBurn_AA;
        public static ThingDef AntimatterCanister_AA;
        public static ThingDef AntimatterComposite_AA;
        public static ThingDef HyperAlloy_AA;
        public static ThingDef CustomOrbitalStrike_AA;
        public static IncidentDef SolarFlare;
        public static ThingDef Mote_MeguminBeam_AA;
        public static SoundDef Explosion_Antimatter_Large_AA;
        public static SoundDef Explosion_Voice_AA;
        public static SoundDef LaserStrike_AA;
        public static SoundDef RailgunShoot_AA;
        public static ResearchProjectDef InstantFlick_AA;
        public static ThingDef Mote_LargeMuzzleFlash_AA;
        public static ThingDef Mote_LargeMuzzleFlashFast_AA;
    }
}
