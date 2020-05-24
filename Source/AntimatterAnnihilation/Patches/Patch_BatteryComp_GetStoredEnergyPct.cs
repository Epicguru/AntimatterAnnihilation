using AntimatterAnnihilation.ThingComps;
using HarmonyLib;
using RimWorld;

namespace AntimatterAnnihilation.Patches
{
    [HarmonyPatch(typeof(CompPowerBattery), "get_StoredEnergyPct")]
    static class Patch_BatteryComp_GetStoredEnergyPct
    {
        static bool Prefix(CompPowerBattery __instance, ref float __result)
        {
            if(__instance is CompGreedyBattery gb)
            {
                __result = __instance.StoredEnergy / gb.realMax;
                return false;
            }

            return true;
        }
    }
}
