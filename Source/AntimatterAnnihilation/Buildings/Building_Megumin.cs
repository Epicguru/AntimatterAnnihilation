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
        [TweakValue("AntimatterAnnihilation")]
        public static bool DoSolarFlare = true;
        [TweakValue("AntimatterAnnihilation", 0f, 1f)]
        public static float EasterEggChance = 0.1f;
        public static int COOLDOWN_TICKS = 2500 * 24 * 4; // 4 in-game days.
        public static int POWER_UP_TICKS = 1276; // Made to correspond to audio queue.
        public static int TICKS_BEFORE_FIRING_LASER = 1200; // Made to correspond to audio queue.
        public static float RADIUS = 15;
        public static int DURATION_TICKS = 310;
        public static float EXPLOSION_RADIUS = 18;
        public static int EXPLOSION_DAMAGE = 50;
        public static float EXPLOSION_PEN = 0.7f;
        public static float CHARGE_WATT_DAYS = 600 * 5; // Requires 5 fully-powered batteries to charge (semi-instantly). Otherwise it will take longer depending on power production.
        public static float IDLE_WATTS = 50; // Power consumption when not charging.

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
        public CompGreedyBattery BatComp
        {
            get
            {
                if (_batComp == null)
                    _batComp = this.TryGetComp<CompGreedyBattery>();
                return _batComp;
            }
        }
        private CompGreedyBattery _batComp;
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
        public bool IsPoweringUp
        {
            get
            {
                return PoweringUpTicks < POWER_UP_TICKS;
            }
        }
        public bool IsChargingUp
        {
            get
            {
                return isChargingUp;
            }
            protected set
            {
                isChargingUp = value;
            }
        }
        public int CooldownTicks;
        public int PoweringUpTicks;

        private bool isChargingUp;
        private UpBeam beam;
        private LocalTargetInfo localTarget;
        private Sustainer soundSustainer;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                PoweringUpTicks = POWER_UP_TICKS;
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
            else if (IsPoweringUp)
            {
                cmd.Disable("CannotFire".Translate() + $": Already powering up.");
            }
            else if (IsChargingUp)
            {
                cmd.Disable("CannotFire".Translate() + $": Already charging to fire.");
            }
            else if (FuelComp.FuelPercentOfMax != 1f)
            {
                cmd.Disable("CannotFire".Translate() + $": Missing antimatter canisters.");
            }
            else if (!HasSkyAccess())
            {
                cmd.Disable("CannotFire".Translate() + $": Blocked by roof.");
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
            IsChargingUp = true;
        }

        private void StartPowerUpSequence()
        {
            // Then enter the powering up phase.
            PoweringUpTicks = 0;

            // Remove antimatter fuel. At his point the firing cannot be cancelled.
            FuelComp.SetFuelLevel(0);

            // Play the long attack sound.
            SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
            soundSustainer = AADefOf.LaserStrike_AA.TrySpawnSustainer(info);
        }

        private void StartRealAttack()
        {
            // Spawn sky beam of death.
            AttackVerb.TryStartCastOn(localTarget);

            // Put on cooldown.
            CooldownTicks = COOLDOWN_TICKS;

            // Delete target position.
            localTarget = null;

            // Spawn a solar flare event on the map that it was fired from.
            if (DoSolarFlare)
            {
                IncidentParms param = new IncidentParms();
                param.forced = true;
                param.target = this.Map;

                bool canFire = AADefOf.SolarFlare.Worker.CanFireNow(param);
                if (!canFire)
                    Log.Warning($"SolarFare Worker says that it cannot fire under the current conditions - forcing it to anyway, from M3G_UMIN.");

                bool worked = AADefOf.SolarFlare.Worker.TryExecute(param);
                if (!worked)
                    Log.Error($"SolarFare Worker failed to execute (returned false), from M3G_UMIN.");
            }
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

        private void DoEasterEgg()
        {
            // It's a trash anime btw. Genuine waste of time.
            // You're better off watching JoJo or Cowboy Bebob or KillLaKill.
            AADefOf.Explosion_Voice_AA.PlayOneShotOnCamera();
        }

        public bool HasSkyAccess()
        {
            IntVec3[] cells = new IntVec3[4];
            cells[0] = Position;
            cells[1] = Position + new IntVec3(1, 0, 0);
            cells[2] = Position + new IntVec3(0, 0, 1);
            cells[3] = Position + new IntVec3(1, 0, 1);

            foreach (var cell in cells)
            {
                if (cell.Roofed(this.Map))
                    return false;
            }

            return true;
        }

        internal void OnStrikeEnd(CustomOrbitalStrike _)
        {
            StopFireLaser();
        }

        public override string GetInspectString()
        {
            string cooldown = IsOnCooldown ? $"Cooldown: {GetCooldownPretty(CooldownTicks)}" : $"Ready to fire";
            string status = IsPoweringUp ? $"Powering up: {PoweringUpTicks / (float)POWER_UP_TICKS * 100f:F0}%" : (FuelComp.FuelPercentOfMax == 1f ? $"{cooldown}" : $"Missing {8 - FuelComp.Fuel} antimatter canisters.");
            status = IsChargingUp ? $"Charging: {BatComp.WattDaysUntilFull:F0} watt-days remaining, {BatComp.StoredEnergyPct * 100f:F0}%." : status;
            return base.GetInspectString() + $"\n{status}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_TargetInfo.Look(ref localTarget, "localTarget");
            Scribe_Values.Look(ref CooldownTicks, "cooldownTicks");
            Scribe_Values.Look(ref PoweringUpTicks, "powerUpTicks");
            Scribe_Values.Look(ref isChargingUp, "isChargingUp");

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

            if (!IsChargingUp && IsPoweringUp)
            {
                PoweringUpTicks++;

                if (PoweringUpTicks == TICKS_BEFORE_FIRING_LASER - 40 && Rand.Chance(EasterEggChance))
                {
                    DoEasterEgg();
                }

                if (PoweringUpTicks == TICKS_BEFORE_FIRING_LASER)
                {
                    StartFireLaser();
                }

                if (PoweringUpTicks == POWER_UP_TICKS)
                {
                    StartRealAttack();
                }
            }

            if (IsChargingUp)
            {
                BatComp.MaxStoredEnergy = CHARGE_WATT_DAYS;
                if (Math.Abs(BatComp.StoredEnergyPct - 1f) < 0.005f)
                {
                    // Now full on power!
                    IsChargingUp = false;
                    StartPowerUpSequence();
                }
            }
            else
            {
                BatComp.SetStoredEnergyPct(0f);
                BatComp.MaxStoredEnergy = 0;
            }
            if (IsOnCooldown)
            {
                CooldownTicks--;
            }

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
