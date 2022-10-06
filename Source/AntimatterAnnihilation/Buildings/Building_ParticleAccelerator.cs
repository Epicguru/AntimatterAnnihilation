using AntimatterAnnihilation.ThingComps;
using AntimatterAnnihilation.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_ParticleAccelerator : Building
    {
        // Becomes less efficient at higher power levels.
        public const float POWER_DRAW_NORMAL = 4000; // 1x speed
        public const float POWER_DRAW_OVERCLOCKED = POWER_DRAW_NORMAL * 2 + 500; // 2x speed
        public const float POWER_DRAW_INSANITY = POWER_DRAW_NORMAL * 4 + 1000; // 4x speed

        public bool IsRunning
        {
            get
            {
                return PowerTraderComp.PowerOn && FuelComp.HasFuel;
            }
        }

        public CompPowerTrader PowerTraderComp
        {
            get
            {
                if (_powerTraderComp == null)
                    this._powerTraderComp = base.GetComp<CompPowerTrader>();
                return _powerTraderComp;
            }
        }
        private CompPowerTrader _powerTraderComp;
        public CompRefuelableConditional FuelComp
        {
            get
            {
                if (_fuelComp == null)
                    this._fuelComp = base.GetComp<CompRefuelableConditional>();
                return _fuelComp;
            }
        }
        private CompRefuelableConditional _fuelComp;

        public float ProductionPerDay
        {
            get
            {
                return 2 * GetActiveSpeed();
            }
        }
        public IntVec3 OutputPos
        {
            get
            {
                IntVec3 dir;
                switch (outputRotation)
                {
                    case 0:
                        dir = IntVec3.North;
                        break;
                    case 1:
                        dir = IntVec3.East;
                        break;
                    case 2:
                        dir = IntVec3.South;
                        break;
                    case 3:
                        dir = IntVec3.West;
                        break;
                    default:
                        dir = IntVec3.South;
                        break;
                }

                IntVec3 pos = Position + dir * 5;
                return pos;
            }
        }
        public int ProductionTickInterval
        {
            get
            {
                return Math.Max((int)(60000 / ProductionPerDay), 1);
            }
        }

        private int outputRotation = 2;

        private int tickCounter;
        private int powerMode;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            FuelComp.FuelConsumeCondition = _ => PowerTraderComp.PowerOn;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref tickCounter, "canisterProductionTick");
            Scribe_Values.Look(ref outputRotation, "outputRotation", 2);
            Scribe_Values.Look(ref powerMode, "powerMode", 0);
        }

        public override void Tick()
        {
            base.Tick();

            // Change power draw based on active status and speed.
            UpdatePowerDraw();

            // Change fuel consumption rate based on speed.
            FuelComp.CustomFuelBurnMultiplier = GetActiveSpeed();

            if (IsRunning)
            {
                tickCounter++;
                if (tickCounter >= ProductionTickInterval)
                {
                    OutputAntimatter(1);
                    tickCounter = 0;
                }
            }
        }

        private void UpdatePowerDraw()
        {
#if V13
            PowerTraderComp.PowerOutput = IsRunning ? -GetActivePowerDraw() : PowerTraderComp.Props.basePowerConsumption;
#else
            PowerTraderComp.PowerOutput = IsRunning ? -GetActivePowerDraw() : PowerTraderComp.Props.PowerConsumption;
#endif
        }

        public float GetActivePowerDraw()
        {
            switch (powerMode)
            {
                case 0:
                    return POWER_DRAW_NORMAL;
                case 1:
                    return POWER_DRAW_OVERCLOCKED;
                case 2:
                    return POWER_DRAW_INSANITY;
                default:
                    return POWER_DRAW_NORMAL;
            }
        }

        /// <summary>
        /// Gets the speed multiplier based on current power mode.
        /// </summary>
        public int GetActiveSpeed()
        {
            switch (powerMode)
            {
                case 0:
                    return 1;
                case 1:
                    return 2;
                case 2:
                    return 4;
                default:
                    return 1;
            }
        }

        public void OutputAntimatter(int count)
        {
            Thing thing = ThingMaker.MakeThing(AADefOf.AntimatterCanister_AA);
            thing.stackCount = count;

            GenPlace.TryPlaceThing(thing, OutputPos, this.Map, ThingPlaceMode.Near);
        }

        public override string GetInspectString()
        {
            string whenRunning = "AA.PAProducing".Translate(ProductionPerDay, $"{(ProductionTickInterval - tickCounter) / 2500f:F1}");
            string whenNotRunning = PowerTraderComp.PowerOn ? "AA.PANotRunningNoPlasteel".Translate() : "AA.PANotRunningNoPower".Translate();
            return base.GetInspectString() + '\n' + (!IsRunning ? whenNotRunning : whenRunning);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
                yield return g;

            var cmd = new Command_Action();
            cmd.defaultLabel = "AA.AFMChangeOutputSide".Translate();
            cmd.defaultDesc = "AA.AFMChangeOutputSideDesc".Translate();
            cmd.icon = Content.ArrowIcon;
            cmd.iconAngle = outputRotation * 90f;
            cmd.action = () =>
            {
                outputRotation++;
                if (outputRotation >= 4)
                    outputRotation = 0;
            };

            yield return cmd;

            var cmd2 = new Command_Action();
            cmd2.defaultLabel = "AA.AFMPowerLevel".Translate();
            cmd2.defaultDesc = "AA.AFMPowerLevelDesc".Translate(GetActiveSpeed() * 100, GetActivePowerDraw());
            cmd2.icon = powerMode == 2 ? Content.PowerLevelHigh : powerMode == 1 ? Content.PowerLevelMedium : Content.PowerLevelLow;
            cmd2.action = () =>
            {
                powerMode++;
                if (powerMode >= 3)
                    powerMode = 0;
            };

            yield return cmd2;
        }
    }
}
