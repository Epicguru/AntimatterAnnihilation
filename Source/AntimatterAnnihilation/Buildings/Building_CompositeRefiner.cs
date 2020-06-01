using AntimatterAnnihilation.Utils;
using RimWorld;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_CompositeRefiner : Building_MultiRefuelable, IConditionalGlower
    {
        public bool ShouldBeGlowingNow
        {
            get
            {
                return GetShouldBeRunning();
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
        public IntVec3 OutputPos
        {
            get
            {
                IntVec3 pos = this.Position + new IntVec3(0, 0, -1);
                return pos;
            }
        }

        // Recipe: 60 plasteel + 1 canister is 30 antimatter composite.

        public float CurrentPlasteelCount
        {
            get
            {
                return GetFuelComp(1).Fuel;
            }
        }
        public float CurrentAntimatterCount
        {
            get
            {
                return GetFuelComp(2).Fuel;
            }
        }
        public float MaxPlasteel
        {
            get
            {
                return GetFuelComp(1).Props.fuelCapacity;
            }
        }
        public float MaxAntimatter
        {
            get
            {
                return GetFuelComp(2).Props.fuelCapacity;
            }
        }
        public float MissingPlasteel
        {
            get
            {
                var f = GetFuelComp(1);
                return f.Props.fuelCapacity - f.Fuel;
            }
        }
        public float MissingAntimatter
        {
            get
            {
                var f = GetFuelComp(2);
                return f.Props.fuelCapacity - f.Fuel;
            }
        }

        public int TicksToProduceOutput = 15000; // 6 in-game hours.
        public int OutputAmount = 30;

        public int ProductionTicks;

        private bool lastFrameRunning;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                CreateZone();
            }
        }

        private void CreateZone()
        {
            var zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, Map.zoneManager);
            zone.settings.filter.SetDisallowAll();
            zone.settings.Priority = StoragePriority.Low;
            zone.settings.filter.SetAllow(AADefOf.AntimatterComposite_AA, true);
            Map.zoneManager.RegisterZone(zone);
            zone.AddCell(OutputPos);
        }

        public override void Tick()
        {
            base.Tick();

            bool isRunning = GetShouldBeRunning();
            if (isRunning)
            {
                ProductionTicks++;
                if (ProductionTicks >= TicksToProduceOutput)
                {
                    ProductionTicks = 0;

                    // Delete fuel from comps.
                    GetFuelComp(1).SetFuelLevel(0);
                    GetFuelComp(2).SetFuelLevel(0);

                    PlaceOutput(OutputAmount);
                }
            }

            if (lastFrameRunning != isRunning)
            {
                CompGlower?.ReceiveCompSignal("PowerTurnedOn"); // Obviously the power hasn't actually just been turned on, but it's just a way to trigger UpdateLit to be called.
            }
            lastFrameRunning = isRunning;
        }

        public void PlaceOutput(int count)
        {
            if (count <= 0)
                return;

            Thing thing = ThingMaker.MakeThing(AADefOf.AntimatterComposite_AA);
            thing.stackCount = count;

            GenPlace.TryPlaceThing(thing, OutputPos, Find.CurrentMap, ThingPlaceMode.Near);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref ProductionTicks, "productionTicks");
        }

        public bool GetShouldBeRunning()
        {
            return GetReasonNotRunning() == null;
        }

        public string GetReasonNotRunning()
        {
            if (!PowerTraderComp.PowerOn)
                return "AA.NotEnoughPower".Translate();

            if (MissingPlasteel > 0)
                return "AA.MissingPlasteel".Translate(MissingPlasteel);

            if (MissingAntimatter > 0)
                return "AA.MissingAntimatter".Translate(MissingAntimatter);

            return null;
        }

        public override string GetInspectString()
        {
            string reasonNotRunning = GetReasonNotRunning();
            string hours = $"{(TicksToProduceOutput - ProductionTicks) / 2500f:F1}";
            return base.GetInspectString() + $"\n{(reasonNotRunning == null ? "AA.RunningInfo".Translate(hours) : "AA.NotRunningInfo".Translate(reasonNotRunning))}";
        }
    }
}
