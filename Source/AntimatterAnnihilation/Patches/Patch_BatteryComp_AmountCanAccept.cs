using AntimatterAnnihilation.ThingComps;
using HarmonyLib;
using RimWorld;

namespace AntimatterAnnihilation.Patches
{
    [HarmonyPatch(typeof(CompPowerBattery), "get_AmountCanAccept")]
    static class Patch_BatteryComp_AmountCanAccept
    {
        static bool Prefix(CompPowerBattery __instance, ref float __result)
        {
            if(__instance is CompGreedyBattery gb)
            {
                if (__instance.parent.IsBrokenDown())
                {
                    __result = 0f;
                    return false;
                }

                CompProperties_Battery props = __instance.Props;
                __result = (gb.realMax - __instance.StoredEnergy) / props.efficiency;
                return false;
            }

            return true;
        }
    }
}
