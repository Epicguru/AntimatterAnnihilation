using HarmonyLib;
using RimWorld;
using Verse;

namespace AntimatterAnnihilation.Patches
{
    [HarmonyPatch(typeof(ThingListGroupHelper), "Includes")]
    static class Patch_RefuelableFix
    {
        static bool Prefix(ref bool __result, ThingRequestGroup group, ThingDef def)
        {
            if (group == ThingRequestGroup.Refuelable)
            {
                // Vanilla behaviour:
                // return def.HasComp(typeof(CompRefuelable));
                // Doesn't allow for sub-classes of CompRefuelable

                __result = def.HasAssignableCompFrom(typeof(CompRefuelable));

                return false;
            }

            return true;
        }
    }
}
