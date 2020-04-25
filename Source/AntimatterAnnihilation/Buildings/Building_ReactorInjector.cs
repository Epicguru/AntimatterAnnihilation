using AntimatterAnnihilation.ThingComps;
using RimWorld;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_ReactorInjector : Building
    {
        private static Graphic normal, running;

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
        public CompFlickable FlickComp
        {
            get
            {
                if (_flickComp == null)
                    this._flickComp = base.GetComp<CompFlickable>();
                return _flickComp;
            }
        }
        private CompFlickable _flickComp;

        public bool IsRunning
        {
            get
            {
                Log.Message($"{FuelComp.IsConditionPassed}, {FuelComp.HasFuel}, {FlickComp.SwitchIsOn}");
                return FuelComp.IsConditionPassed && FuelComp.HasFuel && FlickComp.SwitchIsOn;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (normal == null)
                {
                    var gd = base.DefaultGraphic.data;
                    normal = GraphicDatabase.Get(gd.graphicClass, gd.texPath, gd.shaderType.Shader, gd.drawSize, base.DrawColor, base.DrawColorTwo);
                    running = GraphicDatabase.Get(gd.graphicClass, gd.texPath + "_running", gd.shaderType.Shader, gd.drawSize, base.DrawColor, base.DrawColorTwo);
                }

                bool isRunning = IsRunning;
                if (isRunning)
                {
                    return running;
                }
                else
                {
                    return normal;
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            FuelComp.FuelConsumeCondition = _ => PowerTraderComp.PowerOn; // We need to have power to consume fuel, and we only fire beam when we consume fuel.
            FuelComp.OnRefueled += (f) =>
            {
                Log.Message("Refueled.");
                CauseRedraw();
            };
            FuelComp.OnRunOutOfFuel += (f) =>
            {
                Log.Message("Out of fuel.");
                CauseRedraw();
            };
        }

        public void CauseRedraw(Map map = null)
        {
            if (map == null)
                map = base.Map;

            map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
        }
    }
}
