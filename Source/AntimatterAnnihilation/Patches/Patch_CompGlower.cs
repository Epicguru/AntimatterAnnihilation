using System;
using AntimatterAnnihilation.Utils;
using HarmonyLib;
using Verse;

namespace AntimatterAnnihilation.Patches
{

    [HarmonyPatch(typeof(CompGlower), "get_ShouldBeLitNow")]
    static class Patch_CompGlower
    {
        static bool Prefix(CompGlower __instance, ref bool __result)
        {
            var ins = __instance;
            if (ins.parent is IConditionalGlower c)
            {
                try
                {
                    __result = c.ShouldBeGlowingNow;
                }
                catch (Exception e)
                {
                    Log.Error($"Exception during conditional glower patch processing: {e}");
                    return false;
                }
                return false;
            }

            return true;
        }
    }
}
