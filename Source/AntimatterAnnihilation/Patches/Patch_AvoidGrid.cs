﻿using AntimatterAnnihilation.AI;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AntimatterAnnihilation.Patches
{

    [HarmonyPatch(typeof(PawnUtility), "GetAvoidGrid")]
    static class Patch_AvoidGrid
    {
        static void Postfix(Pawn p, ref ByteGrid __result)
        {
            AI_AvoidGrid.DoAvoidGrid(p, ref __result);
        }
    }
    
}