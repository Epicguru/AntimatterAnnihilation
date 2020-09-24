using AntimatterAnnihilation.AI;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AntimatterAnnihilation.Patches
{
    [HarmonyPatch(typeof(PawnUtility), "GetAvoidGrid")]
    static class Patch_AvoidGrid
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(Pawn p, ref ByteGrid __result)
        {
            if(Settings.EnableCustomPathfinding)
                AI_AvoidGrid.DoAvoidGrid(p, ref __result);
        }
    }
}
