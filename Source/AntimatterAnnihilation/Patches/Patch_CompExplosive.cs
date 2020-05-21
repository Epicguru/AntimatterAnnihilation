using AntimatterAnnihilation.ThingComps;
using HarmonyLib;
using RimWorld;

namespace AntimatterAnnihilation.Patches
{
    [HarmonyPatch(typeof(CompExplosive), "Detonate")]
    static class Patch_CompExplosive
    {
        static void Postfix(CompExplosive __instance)
        {
            if (__instance is CompExplosiveCustom custom)
            {
                custom.OnPostDetonated();
            }
        }
    }
}
