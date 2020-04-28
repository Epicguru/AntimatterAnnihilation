using RimWorld;
using Verse;

namespace AntimatterAnnihilation.Damage
{
    [DefOf]
    public static class AADefOf
    {
        static AADefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(AADefOf));
        }

        public static DamageDef Annihilate;
        public static DamageDef EnergyBurn;
    }
}
