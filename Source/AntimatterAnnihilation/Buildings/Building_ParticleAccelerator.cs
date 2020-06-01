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

        public float ProductionPerDay = 2;
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
        }

        public override void Tick()
        {
            base.Tick();

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

        public void OutputAntimatter(int count)
        {
            Thing thing = ThingMaker.MakeThing(AADefOf.AntimatterCanister_AA);
            thing.stackCount = count;

            GenPlace.TryPlaceThing(thing, OutputPos, Find.CurrentMap, ThingPlaceMode.Near);
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
        }
    }
}
