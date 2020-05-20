using AntimatterAnnihilation.Attacks;
using AntimatterAnnihilation.Commands;
using AntimatterAnnihilation.Effects;
using AntimatterAnnihilation.ThingComps;
using AntimatterAnnihilation.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_Megumin : Building
    {
        public static int COOLDOWN_TICKS = 2500 * 24 * 4; // 4 in-game days.
        public static int CHARGE_TICKS = 1276; // Made to correspond to audio queue.
        public static int TICKS_BEFORE_FIRING_LASER = 1200; // Made to correspond to audio queue.
        public static float RADIUS = 15;
        public static int DURATION_TICKS = 310;
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
        public bool IsCharging
        {
            get
            {
                return ChargingTicks < CHARGE_TICKS;
            }
        }
        public int CooldownTicks;
        public int ChargingTicks;

        private UpBeam beam;
        private LocalTargetInfo localTarget;
        private Sustainer soundSustainer;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                ChargingTicks = CHARGE_TICKS;
            }

            if (beam == null)
            {
                beam = new UpBeam(map, this.TrueCenter() + new Vector3(0f, 0f, 1.2f));
                beam.IsActive = false;
            }
        }

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
            cmd.onTargetSelected = StartAttackSequence;
            if (IsOnCooldown)
            {
                cmd.Disable("CannotFire".Translate() + $": Weapon is cooling down.");
            }
            else if (FuelComp.FuelPercentOfMax != 1f)
            {
                cmd.Disable("CannotFire".Translate() + $": Missing antimatter canisters.");
            }
            else if (IsCharging)
            {
                cmd.Disable("CannotFire".Translate() + $": Already charging to fire.");
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

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);

            beam?.Dispose();
            beam = null;
        }

        private void StartAttackSequence(LocalTargetInfo target)
        {
            // Set local target.
            this.localTarget = target;

            // Enter the charging phase.
            ChargingTicks = 0;

            // Remove antimatter fuel.
            FuelComp.SetFuelLevel(0);

            // Play the long attack sound.
            SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
            soundSustainer = AADefOf.LaserStrike_AA.TrySpawnSustainer(info);

            // Force game to run at 1x speed, so that the audio matches up with the laser beam.
            // Note that if the player pauses the game at this point, the audio messes up anyway... TODO fix this.
            // There are mods that disable this forced slow behaviour, so they will make this not work properly but there isn't much I can do about that.
            //Find.TickManager.slower.SignalForceNormalSpeedShort();
        }

        private void StartRealAttack()
        {
            AttackVerb.TryStartCastOn(localTarget);
            CooldownTicks = COOLDOWN_TICKS;
            localTarget = null;
        }

        private void StartFireLaser()
        {
            beam.IsActive = true;
        }

        private void StopFireLaser()
        {
            beam.IsActive = false;
            soundSustainer?.End();
        }

        internal void OnStrikeEnd(CustomOrbitalStrike strike)
        {
            StopFireLaser();
        }

        public override string GetInspectString()
        {
            string cooldown = IsOnCooldown ? $"Cooldown: {GetCooldownPretty(CooldownTicks)}" : $"Ready to fire";
            string status = IsCharging ? $"Charging: {ChargingTicks / (float)CHARGE_TICKS * 100f:F0}%" : (FuelComp.FuelPercentOfMax == 1f ? $"{cooldown}" : $"Missing {8 - FuelComp.Fuel} antimatter canisters.");
            return base.GetInspectString() + $"\n{status}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_TargetInfo.Look(ref localTarget, "localTarget");
            Scribe_Values.Look(ref CooldownTicks, "cooldownTicks");
            Scribe_Values.Look(ref ChargingTicks, "chargeTicks");

            Scribe_Deep.Look(ref gun, "gun", Array.Empty<object>());

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

            beam?.Tick();

            if (IsCharging)
            {
                ChargingTicks++;
                if(ChargingTicks == TICKS_BEFORE_FIRING_LASER)
                {
                    StartFireLaser();
                }

                if (ChargingTicks == CHARGE_TICKS)
                {
                    StartRealAttack();
                }
            }
            if (IsOnCooldown)
            {
                CooldownTicks--;
            }

            // If charging up or firing, slow down the game to 1x speed to make the audio match up with the visuals.
            // This forces the player to sit through ~25 seconds of real-time audio, but otherwise the audio doesn't make any sense.
            //if (isFiringBeam || IsCharging)
            //{
            //    Find.TickManager.slower.SignalForceNormalSpeedShort();
            //}

            if (soundSustainer != null)
            {
                if (soundSustainer.Ended)
                    soundSustainer = null;
                soundSustainer?.Maintain();
            }

            if (this.Spawned && !this.Destroyed)
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
