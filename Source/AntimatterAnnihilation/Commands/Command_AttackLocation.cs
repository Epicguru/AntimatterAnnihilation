using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AntimatterAnnihilation.Commands
{
    public class Command_AttackLocation : Command
    {
        public Action<LocalTargetInfo> onTargetSelected;
        public TargetingParameters targetingParams;

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
            Find.Targeter.BeginTargeting(this.targetingParams, delegate (LocalTargetInfo target)
            {
                this.onTargetSelected(target);
            }, null, null, null);
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            return false;
        }
    }
}
