using AntimatterAnnihilation.ThingComps;
using AntimatterAnnihilation.Utils;
using RimWorld;
using System;
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
                IntVec3 pos = this.Position + new IntVec3(0, 0, -5);
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

        private int tickCounter;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                CreateZone();
            }

            FuelComp.FuelConsumeCondition = _ => PowerTraderComp.PowerOn;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref tickCounter, "canisterProductionTick");
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

        private void CreateZone()
        {
            var zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, Map.zoneManager);
            zone.settings.filter.SetDisallowAll();
            zone.settings.Priority = StoragePriority.Low;
            zone.settings.filter.SetAllow(AADefOf.AntimatterCanister_AA, true);
            Map.zoneManager.RegisterZone(zone);
            zone.AddCell(OutputPos);
        }

        public void OutputAntimatter(int count)
        {
            Thing thing = ThingMaker.MakeThing(AADefOf.AntimatterCanister_AA);
            thing.stackCount = count;

            GenPlace.TryPlaceThing(thing, OutputPos, Find.CurrentMap, ThingPlaceMode.Near);
        }

        public override string GetInspectString()
        {
            string whenRunning = $"\nProducing {ProductionPerDay} canisters per day.\n{(ProductionTickInterval - tickCounter) / 2500f:F1} hours until next antimatter canister.";
            string whenNotRunning = $"\nNot running: {(!PowerTraderComp.PowerOn ? "No power" : "No plasteel fuel")}";
            return base.GetInspectString() + (!IsRunning ? whenNotRunning : whenRunning);
        }
    }
}
