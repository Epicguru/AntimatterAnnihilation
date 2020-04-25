using AntimatterAnnihilation.Effects;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation
{
    [StaticConstructorOnStartup]
    public class Building_AntimatterReactor : Building
    {
        public CompPowerTrader PowerTraderComp
        {
            get
            {
                if(_powerTraderComp == null)
                    this._powerTraderComp = base.GetComp<CompPowerTrader>();
                return _powerTraderComp;
            }
        }
        private CompPowerTrader _powerTraderComp;

        public EnergyBall EnergyBall { get; set; }
        public override Graphic Graphic
        {
            get
            {

                bool powerOn = PowerTraderComp?.PowerOn ?? false;
                if (powerOn)
                {
                    if (true) // This is the condition to be running as opposed to just powered.
                    {
                        if (runningGraphic == null)
                        {
                            var gd = base.Graphic.data;
                            runningGraphic = GraphicDatabase.Get(gd.graphicClass, gd.texPath + "Running", gd.shaderType.Shader, gd.drawSize, base.DrawColor, base.DrawColorTwo);
                        }

                        return runningGraphic;
                    }
                    else
                    {
                        if (poweredGraphic == null)
                        {
                            var gd = base.Graphic.data;
                            poweredGraphic = GraphicDatabase.Get(gd.graphicClass, gd.texPath + "Powered", gd.shaderType.Shader, gd.drawSize, base.DrawColor, base.DrawColorTwo);
                        }

                        return poweredGraphic;
                    }
                }
                else
                {
                    return base.DefaultGraphic;
                }
            }
        }

        private Graphic poweredGraphic;
        private Graphic runningGraphic;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Log.Message("Spawn. After load: " + respawningAfterLoad + ", has power: " + PowerTraderComp.PowerOn);
            Log.Message("Position: " + this.Position);

            if (PowerTraderComp.PowerOn)
                OnPowerStart(map);
            else
                OnPowerStop(map);

            var thing = map.thingGrid.ThingAt(Position, ThingCategory.Building);
            Log.Message(thing);

            PowerTraderComp.powerStartedAction += () => OnPowerStart(base.Map);
            PowerTraderComp.powerStoppedAction += () => OnPowerStop(base.Map);

            EnergyBall = new EnergyBall(Position.ToVector3() + new Vector3(1f, 0f, 1.4f));
        }

        public void OnPowerStart(Map map)
        {
            //Log.Message("Power started!");
            map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
            EnergyBall.Visible = true;
        }

        public void OnPowerStop(Map map)
        {
            //Log.Message("Power stopped...");
            map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
            EnergyBall.Visible = false;
        }

        public override void Tick()
        {
            base.Tick();

            EnergyBall.Tick();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var thing in base.GetGizmos())
            {
                yield return thing;
            }
        }
    }
}
