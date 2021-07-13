using AntimatterAnnihilation.Attacks;
using AntimatterAnnihilation.Commands;
using AntimatterAnnihilation.Effects;
using AntimatterAnnihilation.ThingComps;
using AntimatterAnnihilation.Utils;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using Object = UnityEngine.Object;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_Megumin : Building, IConditionalGlower
    {
        [TweakValue("AntimatterAnnihilation")]
        public static bool DoSolarFlare = true;
        [TweakValue("AntimatterAnnihilation", 0f, 1f)]
        public static float EasterEggChance = 0.333f;
        public static int COOLDOWN_TICKS = 2500 * 24 * 4; // 4 in-game days.
        public static int POWER_UP_TICKS = 1276; // Made to correspond to audio queue.
        public static int TICKS_BEFORE_FIRING_LASER = 1200; // Made to correspond to audio queue.
        public static float RADIUS = 16;
        public static int DURATION_TICKS = 310;
        public static float EXPLOSION_RADIUS = 18;
        public static int EXPLOSION_DAMAGE = 50;
        public static float EXPLOSION_PEN = 0.7f;
        public static float CHARGE_WATT_DAYS = 600 * 1f; // Requires 1 fully-powered batteries to charge (semi-instantly). Otherwise it will take longer depending on power production.
        public static int WORLD_MAP_RANGE = 150;

        public bool ShouldBeGlowingNow
        {
            get
            {
                return IsPoweringUp;
            }
        }
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
        public CompGlower CompGlower
        {
            get
            {
                if (_compGlower == null)
                    this._compGlower = base.GetComp<CompGlower>();
                return _compGlower;
            }
        }
        private CompGlower _compGlower;
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
        public Map TargetMap
        {
            get
            {
                if (globalTarget.IsValid && globalTarget.Map != null)
                {
                    // The target map is in the global target.
                    return globalTarget.Map;
                }

                // If there is no global target, the target must be this map.
                return this.Map;
            }
        }

        private IntVec3 lastKnownThingLoc; // Used to prevent bugs where target is destroyed and laser does not spawn.
        private bool isChargingUp;
        private UpBeam beam;
        private LocalTargetInfo localTarget = LocalTargetInfo.Invalid;
        private GlobalTargetInfo globalTarget = GlobalTargetInfo.Invalid;
        private Sustainer soundSustainer;
        private ParticleSystem chargeEffect;
        private Map globalTargetMapLastKnown;

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

            if(chargeEffect == null)
            {
                chargeEffect = Object.Instantiate(Content.MeguminChargePrefab).GetComponent<ParticleSystem>();
                chargeEffect.transform.position = this.TrueCenter() + new Vector3(0f, 0f, 1.1f);
                chargeEffect.transform.eulerAngles = new Vector3(90f, 0f, 0f);
            }

            if (!IsPoweringUp)
                chargeEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            // Attack on local map.
            Command_AttackLocation cmd = new Command_AttackLocation();
            cmd.defaultLabel = "AA.MegStrikeLocation".Translate();
            cmd.defaultDesc = "AA.MegStrikeLocationDesc".Translate();
            cmd.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true);
            cmd.hotKey = KeyBindingDefOf.Misc4;
            cmd.targetingParams = new TargetingParameters() {canTargetBuildings = true, canTargetLocations = true, canTargetPawns = true, canTargetAnimals = true };
            cmd.onTargetSelected = (local) =>
            {
                StartAttackSequence(GlobalTargetInfo.Invalid, local);
            };

            // Attack on world map.
            Command_Action cmd2 = new Command_Action();
            cmd2.icon = Content.GlobalStrikeIcon;
            cmd2.defaultLabel = "AA.MegStrikeWorld".Translate();
            cmd2.defaultDesc = "AA.MegStrikeWorldDesc".Translate();
            cmd2.action = () =>
            {
                Find.WorldSelector.ClearSelection();
                CameraJumper.TryJump(CameraJumper.GetWorldTarget(this));
#if V12
                Find.WorldTargeter.BeginTargeting_NewTemp(OnChoseWorldTarget, false, Content.AutoAttackIcon, true,
                    () =>
                    {
                        GenDraw.DrawWorldRadiusRing(this.Map.Tile, WORLD_MAP_RANGE);
                    });
#elif V13
                Find.WorldTargeter.BeginTargeting(OnChoseWorldTarget, false, Content.AutoAttackIcon, true,
                    () =>
                    {
                        GenDraw.DrawWorldRadiusRing(this.Map.Tile, WORLD_MAP_RANGE);
                    });
#endif
            };

            if (IsOnCooldown)
            {
                cmd.Disable("CannotFire".Translate() + $": {"AA.MegCoolingDown".Translate(GetCooldownPretty(CooldownTicks))}");
                cmd2.Disable("CannotFire".Translate() + $": {"AA.MegCoolingDown".Translate(GetCooldownPretty(CooldownTicks))}");
            }
            else if (IsPoweringUp)
            {
                cmd.Disable("CannotFire".Translate() + $": {"AA.MegAlreadyPoweringUp".Translate()}");
                cmd2.Disable("CannotFire".Translate() + $": {"AA.MegAlreadyPoweringUp".Translate()}");
            }
            else if (IsChargingUp)
            {
                cmd.Disable("CannotFire".Translate() + $": {"AA.MegAlreadyCharging".Translate()}");
                cmd2.Disable("CannotFire".Translate() + $": {"AA.MegAlreadyCharging".Translate()}");
            }
            else if (FuelComp.FuelPercentOfMax < 0.999f)
            {
                cmd.Disable("CannotFire".Translate() + $": {"AA.MegMissingCanisters".Translate()}");
                cmd2.Disable("CannotFire".Translate() + $": {"AA.MegMissingCanisters".Translate()}");
            }
            else if (!HasSkyAccess())
            {
                cmd.Disable("CannotFire".Translate() + $": {"AA.MegBlockedByRoof".Translate()}");
                cmd2.Disable("CannotFire".Translate() + $": {"AA.MegBlockedByRoof".Translate()}");
            }

            yield return cmd;
            yield return cmd2;

            if (IsChargingUp)
            {
                var cancel = new Command_Action();
                cancel.defaultLabel = "AA.MegCancelStrike".Translate();
                cancel.defaultDesc = "AA.MegCancelStrikeDesc".Translate();
                cancel.icon = Content.CancelIcon;
                cancel.defaultIconColor = Color.red;
                cancel.action = () =>
                {
                    localTarget = LocalTargetInfo.Invalid;
		    globalTarget = GlobalTargetInfo.Invalid;
                    IsChargingUp = false;
                };
                yield return cancel;
            }

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

        private bool OnChoseWorldTarget(GlobalTargetInfo target)
        {
            // Invalid target.
            if (!target.IsValid)
            {
                Messages.Message("AA.MegInvalidWorldTarget".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            // Out of range.
            if (Find.WorldGrid.TraversalDistanceBetween(base.Map.Tile, target.Tile) > WORLD_MAP_RANGE)
            {
                Messages.Message("AA.MegOutOfRange".Translate(), this, MessageTypeDefOf.RejectInput);
                return false;
            }

            // Make sure that the map is generated. Cannot fire into a null map.
            MapParent mapParent = target.WorldObject as MapParent;
            if (mapParent == null || !mapParent.HasMap)
            {
                Messages.Message("AA.MegNeedsMap".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            // Can't attack own map.
            var targetMap = mapParent.Map;
            if (targetMap == this.Map)
            {
                Messages.Message("AA.MegCantTargetSelf".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            // Allow targeting everything.
            var targParams = new TargetingParameters
			{
				canTargetPawns = true,
				canTargetBuildings = true,
				canTargetLocations = true,
                canTargetAnimals = true
			};

            Current.Game.CurrentMap = targetMap;
            Find.Targeter.BeginTargeting(targParams, (localTarg) =>
            {
                GlobalTargetInfo globalTargetInfo = localTarg.ToGlobalTargetInfo(targetMap);
                StartAttackSequence(globalTargetInfo, LocalTargetInfo.Invalid);
            });

            //Messages.Message("Not working yet.", MessageTypeDefOf.RejectInput, true);
            return true;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);

            beam?.Dispose();
            beam = null;
        }

        private void StartAttackSequence(GlobalTargetInfo global, LocalTargetInfo local)
        {
            if (!local.IsValid && !global.IsValid)
            {
                Log.Error($"Tried to start M3G_UMIN attack with invalid target(s): Local {local}, Global {global}.");
                return;
            }
            if (local.IsValid && global.IsValid)
            {
                Log.Error("Passed in valid global and local targets to attack sequence. This is not valid. Must be one or the other.");
                return;
            }

            // Set local target.
            if (local.IsValid)
            {
                Log.Message("Starting local attack.");
                this.localTarget = local;
                lastKnownThingLoc = local.Cell;
            }
            //Set global target.
            if (global.IsValid)
            {
                Log.Message("Starting global attack.");
                this.globalTarget = global;
                lastKnownThingLoc = global.Cell;
            }

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

            // Do particle effects.
            chargeEffect?.Play(true);

            // Enable glow start.
            CompGlower?.ReceiveCompSignal("PowerTurnedOn");
        }

        private void StartRealAttack()
        {
            bool cast = true;
            if (globalTarget.IsValid)
            {
                Log.Message($"Using global target to create local target. ({globalTarget})");
                if (globalTarget.Map == null)
                {
                    if(globalTargetMapLastKnown != null)
                    {
                        Log.Warning($"Global target has null map (most likely missing pawn target). Falling back to last known map ({globalTargetMapLastKnown}).");
                        localTarget = new LocalTargetInfo(lastKnownThingLoc);
                        globalTarget = new GlobalTargetInfo(lastKnownThingLoc, globalTargetMapLastKnown);
                    }
                    else
                    {
                        Log.Error("Global target is valid, but has null map. Last known map is also null. Probably caused by map being unloaded prematurely, or immediately after the updated that added this feature. Strike cancelled to avoid damage to wrong map.");
                        cast = false;
                    }
                }
                else
                {
                    // Convert to local target. Note that Thing may be non-null be destroyed, that is handled below.
                    localTarget = globalTarget.HasThing ? new LocalTargetInfo(globalTarget.Thing) : new LocalTargetInfo(globalTarget.Cell);
                }
            }
            // Make sure that the local target is completely valid: it is possible that thing is destroyed and Target.IsValid is still true.
            if (localTarget.HasThing && localTarget.ThingDestroyed)
            {
                Log.Warning($"M3G_UMIN appears to have been targeting a Thing but that Thing was destroyed. Using last known location: {lastKnownThingLoc}");
                localTarget = new LocalTargetInfo(lastKnownThingLoc);
            }

            // Spawn sky beam of death.
            if (cast)
            {
                bool worked = AttackVerb.TryStartCastOn(localTarget);
                if (!worked)
                {
                    Log.Error("Megumin verb cast failed!");
                    cast = false;
                }
            }

            if (!cast)
            {
                // Cast failed. Stop be beam effect immediately.
                OnStrikeEnd(null);
            }
            

            chargeEffect?.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            // Put on cooldown.
            CooldownTicks = COOLDOWN_TICKS;

            // Delete target position.
            localTarget = LocalTargetInfo.Invalid;
            globalTarget = GlobalTargetInfo.Invalid;
            lastKnownThingLoc = IntVec3.Invalid;
            globalTargetMapLastKnown = null;

            // Spawn a solar flare event on the map that it was fired from.
            if (DoSolarFlare && cast)
            {
                IncidentParms param = new IncidentParms();
                param.forced = true;
                param.target = this.Map;

                bool canFire = AADefOf.SolarFlare.Worker.CanFireNow(param);
                if (!canFire)
                    Log.Warning("SolarFare Worker says that it cannot fire under the current conditions - forcing it to anyway, from M3G_UMIN.");

                bool worked = AADefOf.SolarFlare.Worker.TryExecute(param);
                if (!worked)
                    Log.Error("SolarFare Worker failed to execute (returned false), from M3G_UMIN.");
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

            // Turn off glow.
            CompGlower?.ReceiveCompSignal("PowerTurnedOn");
        }

        private void DoEasterEgg()
        {
            if (!Settings.EnableEasterEggs)
                return;

            // It's a trash anime btw. Genuine waste of time.
            // You're better off watching JoJo or Cowboy Bebop or KillLaKill.
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
            string cooldown = IsOnCooldown ? "AA.MegCooldown".Translate(GetCooldownPretty(CooldownTicks)) : "AA.MegReadyToFire".Translate();
            string status = IsPoweringUp ? "AA.MegPoweringUp".Translate($"{PoweringUpTicks / (float)POWER_UP_TICKS * 100f:F0}").ToString() : FuelComp.FuelPercentOfMax == 1f ? cooldown : "AA.MegMissingXCanisters".Translate(8 - FuelComp.Fuel).ToString();
            status = IsChargingUp ? "AA.MegCharging".Translate($"{BatComp.WattDaysUntilFull:F0}", $"{BatComp.StoredEnergyPct * 100f:F0}").ToString() : status;
            return base.GetInspectString() + $"\n{status}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_TargetInfo.Look(ref localTarget, "localTarget", LocalTargetInfo.Invalid);
            Scribe_TargetInfo.Look(ref globalTarget, "globalTarget", GlobalTargetInfo.Invalid);
            Scribe_Values.Look(ref CooldownTicks, "cooldownTicks");
            Scribe_Values.Look(ref PoweringUpTicks, "powerUpTicks");
            Scribe_Values.Look(ref isChargingUp, "isChargingUp");
            Scribe_Values.Look(ref lastKnownThingLoc, "lastKnownThingLoc");

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

            // Save the last known global target map. This is necessary for when targeting a pawn that is destroyed in a foreign map.
            // Note that this isn't a perfect fix: this isn't saved, so reloading the game will still cause a 'fake strike' where the laser will not spawn.
            if (globalTarget.IsValid && globalTarget.Map != null)
            {
                globalTargetMapLastKnown = globalTarget.Map;
            }

            if (globalTarget.HasThing && !globalTarget.ThingDestroyed)
                lastKnownThingLoc = globalTarget.Cell;
            else if (localTarget.HasThing && !localTarget.ThingDestroyed)
                lastKnownThingLoc = localTarget.Cell;

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

            if(this.chargeEffect != null)
            {
                bool isActive = chargeEffect.gameObject.activeSelf;
                bool shouldBeActive = Find.CurrentMap == this.Map;

                if(isActive != shouldBeActive)
                    chargeEffect.gameObject.SetActive(shouldBeActive);
            }
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
                return $"{ticksLeft / QUADRUM:F1} {"AA.Quadrums".Translate()}";
            }

            if (ticksLeft >= DAY)
            {
                return $"{ticksLeft / DAY:F1} {"AA.Days".Translate()}";
            }

            return $"{ticksLeft / HOUR:F1} {"AA.Hours".Translate()}";
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
