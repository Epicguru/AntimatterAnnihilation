using System;
using RimWorld;
using System.Collections.Generic;
using AntimatterAnnihilation.Commands;
using UnityEngine;
using Verse;
using AntimatterAnnihilation.ThingComps;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_Megumin : Building
    {
        public static float RADIUS = 15;
        public static int DURATION_TICKS = 600;
        public static int COOLDOWN = 2500 * 24 * 4; // 4 in-game days.
        public static float EXPLOSION_RADIUS = 18;
        public static int EXPLOSION_DAMAGE = 50;
        public static float EXPLOSION_PEN = 0.7f;

        public CompEquippable GunComp
        {
            get
            {
                if(_gunCompEq == null)
                    _gunCompEq = this.gun.TryGetComp<CompEquippable>();
                return _gunCompEq;
            }
        }
        private CompEquippable _gunCompEq;
        public CompRefuelableConditional FuelComp
        {
            get
            {
                if (_compRefuelable == null)
                    _compRefuelable = this.GetComp<CompRefuelableConditional>();
                return _compRefuelable;
            }
        }
        private CompRefuelableConditional _compRefuelable;
        public Verb AttackVerb
        {
            get
            {
                return this.GunComp.PrimaryVerb;
            }
        }
        public bool IsOnCooldown
        {
            get
            {
                return CooldownTicks > 0;
            }
        }
        public int CooldownTicks;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            // Attack on local map.
            Command_AttackLocation cmd = new Command_AttackLocation();
            cmd.defaultLabel = "Strike location";
            cmd.defaultDesc = "Fires the MEG-Umin at a specific position on this map.";
            cmd.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true);
            cmd.hotKey = KeyBindingDefOf.Misc4;
            cmd.targetingParams = new TargetingParameters() {canTargetBuildings = true, canTargetLocations = true, canTargetPawns = true, canTargetAnimals = true};
            cmd.onTargetSelected = OnTargetSelected;
            if (IsOnCooldown)
            {
                cmd.Disable("CannotFire".Translate() + $": Weapon is cooling down.");
            }
            else if (FuelComp.FuelPercentOfMax != 1f)
            {
                cmd.Disable("CannotFire".Translate() + $": Missing antimatter canisters.");
            }
            yield return cmd;

            // Debug gizmos.
            if (Prefs.DevMode)
            {
                Command_Action resetCooldown = new Command_Action();
                resetCooldown.action = () =>
                {
                    CooldownTicks = 0;
                };
                resetCooldown.defaultLabel = "Debug: Reset cooldown";
                resetCooldown.defaultDesc = "Resets cooldown.";
                yield return resetCooldown;
            }
        }

        private void OnTargetSelected(LocalTargetInfo target)
        {
            //Log.Message($"Start attack on {target}");
            AttackVerb.TryStartCastOn(target);

            FuelComp.SetFuelLevel(0);
            CooldownTicks = COOLDOWN;
        }

        public override string GetInspectString()
        {
            string cooldown = IsOnCooldown ? $"Cooldown: {GetCooldownPretty(CooldownTicks)}" : $"Ready to fire";
            string status = FuelComp.FuelPercentOfMax == 1f ? $"{cooldown}" : $"Missing {8 - FuelComp.Fuel} antimatter canisters.";
            return base.GetInspectString() + $"\n{status}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.CooldownTicks, "cooldownTicks");
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

            if (IsOnCooldown)
            {
                CooldownTicks--;
            }

            if(this.Spawned && !this.Destroyed)
                this.GunComp?.verbTracker?.VerbsTick();
        }

        /// <summary>
        /// Gets the cooldown time in the form "2.1 Days" or "12.6 Hours".
        /// </summary>
        /// <param name="ticksLeft">The number of ticks left.</param>
        /// <returns></returns>
        public string GetCooldownPretty(int ticksLeft)
        {
            const float HOUR = 2500;
            const float DAY = HOUR * 24;
            const float QUADRUM = DAY * 15;

            if(ticksLeft >= QUADRUM)
            {
                return $"{ticksLeft / QUADRUM:F1} Quadrums";
            }

            if (ticksLeft >= DAY)
            {
                return $"{ticksLeft / DAY:F1} Days";
            }

            return $"{ticksLeft / HOUR:F1} Hours";
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
