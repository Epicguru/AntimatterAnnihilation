using System;
using RimWorld;
using System.Collections.Generic;
using AntimatterAnnihilation.Commands;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_Megumin : Building
    {
        public const float RADIUS = 15;
        public const int DURATION_TICKS = 600;

        public CompEquippable GunCompEq
        {
            get
            {
                return this.gun.TryGetComp<CompEquippable>();
            }
        }

        public Verb AttackVerb
        {
            get
            {
                return this.GunCompEq.PrimaryVerb;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            Command_AttackLocation cmd = new Command_AttackLocation();
            cmd.defaultLabel = "Strike location";
            cmd.defaultDesc = "Fires the MEG-Umin at a specific position on this map.";
            cmd.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true);
            cmd.hotKey = KeyBindingDefOf.Misc4;
            cmd.targetingParams = new TargetingParameters() {canTargetBuildings = true, canTargetLocations = true, canTargetPawns = true, canTargetAnimals = true};
            cmd.onTargetSelected = OnTargetSelected;
            //if (condition)
            //{
            //    command_VerbTarget.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
            //}
            
            yield return cmd;
        }

        private void OnTargetSelected(LocalTargetInfo target)
        {
            Log.Message($"Start attack on {target}");
            AttackVerb.TryStartCastOn(target);
        }

        public override string GetInspectString()
        {
            return base.GetInspectString() + $"\nVerb: {AttackVerb}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref this.gun, "gun", Array.Empty<object>());
            BackCompatibility.PostExposeData(this);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.UpdateGunVerbs();
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            this.MakeGun();
        }

        public override void Tick()
        {
            base.Tick();

            if(this.Spawned && !this.Destroyed)
                this.GunCompEq?.verbTracker?.VerbsTick();
        }

        public void MakeGun()
        {
            this.gun = ThingMaker.MakeThing(this.def.building.turretGunDef, null);
            UpdateGunVerbs();
            //Log.Message($"Created gun: {gun}");
        }

        private void UpdateGunVerbs()
        {
            List<Verb> allVerbs = this.gun.TryGetComp<CompEquippable>().AllVerbs;
            for (int i = 0; i < allVerbs.Count; i++)
            {
                Verb verb = allVerbs[i];
                verb.caster = this;
                //verb.castCompleteCallback = new Action(this.BurstComplete);
            }
        }

        private Thing gun;
    }
}
