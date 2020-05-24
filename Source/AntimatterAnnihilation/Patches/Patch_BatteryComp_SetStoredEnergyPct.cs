using AntimatterAnnihilation.ThingComps;
using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace AntimatterAnnihilation.Patches
{
    [HarmonyPatch(typeof(CompPowerBattery), "SetStoredEnergyPct")]
    static class Patch_BatteryComp_SetStoredEnergyPct
    {
        static bool Prefix(CompPowerBattery __instance, float pct, ref float ___storedEnergy)
        {
            if(__instance is CompGreedyBattery gb)
            {
                pct = Mathf.Clamp01(pct);
                ___storedEnergy = gb.realMax * pct;
                return false;
            }

            return true;
        }
    }
}
