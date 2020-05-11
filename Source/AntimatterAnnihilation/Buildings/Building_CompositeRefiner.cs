using AntimatterAnnihilation.Utils;
using RimWorld;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_CompositeRefiner : Building_Storage
    {
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
        public IntVec3 OutputPos
        {
            get
            {
                IntVec3 pos = this.Position + new IntVec3(0, 0, -1);
                return pos;
            }
        }

        public int MissingPlasteel
        {
            get
            {
                return MaxPlasteel - CurrentPlasteelCount;
            }
        }
        public int MissingAntimatter
        {
            get
            {
                return MaxAntimatter - CurrentAntimatterAmount;
            }
        }

        public int TicksToProduceOutput = 15000; // 6 in-game hours.
        public int MaxPlasteel = 60;
        public int MaxAntimatter = 1;
        public int OutputAmount = 30;

        public int CurrentPlasteelCount;
        public int CurrentAntimatterAmount;
        public int ProductionTicks;

        private ulong tickCount;

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
            zone.settings.filter.SetAllow(AADefOf.AntimatterComposite, true);
            Map.zoneManager.RegisterZone(zone);
            zone.AddCell(OutputPos);
        }

        public override void Tick()
        {
            base.Tick();

            tickCount++;
            if (tickCount % 120 == 0)
            {
                Building_InputTray lt = null;
                Building_InputTray rt = null;
                if (MissingPlasteel != 0)
                {
                    lt = GetLeftTray();
                    rt = GetRightTray();

                    TryGet(rt, "Plasteel", MissingPlasteel, ref CurrentPlasteelCount);
                    TryGet(lt, "Plasteel", MissingPlasteel, ref CurrentPlasteelCount);
                }

                if (MissingAntimatter != 0)
                {
                    if(lt == null)
                    {
                        lt = GetLeftTray();
                        rt = GetRightTray();
                    }

                    TryGet(lt, "AntimatterCanister", MissingAntimatter, ref CurrentAntimatterAmount);
                    TryGet(rt, "AntimatterCanister", MissingAntimatter, ref CurrentAntimatterAmount);
                }
            }

            bool isRunning = GetShouldBeRunning();
            if(isRunning)
            {
                ProductionTicks++;
                if (ProductionTicks >= TicksToProduceOutput)
                {
                    ProductionTicks = 0;
                    CurrentAntimatterAmount = 0;
                    CurrentPlasteelCount = 0;

                    PlaceOutput(OutputAmount);
                }
            }
        }

        public void PlaceOutput(int count)
        {
            if (count <= 0)
                return;

            Thing thing = ThingMaker.MakeThing(AADefOf.AntimatterComposite);
            thing.stackCount = count;

            GenPlace.TryPlaceThing(thing, OutputPos, Find.CurrentMap, ThingPlaceMode.Near);
        }

        public void TryGet(Building_InputTray tray, string defName, int amount, ref int counter)
        {
            if (amount <= 0)
                return;
            if (tray == null)
                return;
            if (defName == null)
                return;

            var removed = tray.TryRemove(defName, amount);
            if (removed > 0)
                counter += removed;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref CurrentAntimatterAmount, "currentAntimatter");
            Scribe_Values.Look(ref CurrentPlasteelCount, "currentPlasteel");
            Scribe_Values.Look(ref ProductionTicks, "productionTicks");
        }

        public Building_InputTray GetLeftTray()
        {
            return GetTray(new IntVec3(-2, 0, 0));
        }

        public Building_InputTray GetRightTray()
        {
            return GetTray(new IntVec3(2, 0, 0));
        }

        public Building_InputTray GetTray(IntVec3 offset)
        {
            var thing = Map?.thingGrid.ThingAt(base.Position + offset, ThingCategory.Building);

            return thing as Building_InputTray;
        }

        public bool GetShouldBeRunning()
        {
            return GetReasonNotRunning() == null;
        }

        public string GetReasonNotRunning()
        {
            if (!PowerTraderComp.PowerOn)
                return "Not enough power.";

            if (MissingPlasteel > 0)
                return $"Missing {MissingPlasteel} plasteel.";

            if (MissingAntimatter > 0)
                return $"Missing {MissingAntimatter} antimatter.";

            return null;
        }

        public override string GetInspectString()
        {
            string reasonNotRunning = GetReasonNotRunning();
            return base.GetInspectString() + $"\n{(reasonNotRunning == null ? $"Running: {(TicksToProduceOutput - ProductionTicks)/2500f:F1} hours until output" : $"Not running: {reasonNotRunning}")}\nPlasteel: {CurrentPlasteelCount}/{MaxPlasteel}\nAntimatter: {CurrentAntimatterAmount}/{MaxAntimatter}";
        }
    }
}
