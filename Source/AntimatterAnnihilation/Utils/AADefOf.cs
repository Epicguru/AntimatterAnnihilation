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
        public static ThingDef CustomOrbitalStrike_AA;
    }
}
