using System.Collections.Generic;
using AntimatterAnnihilation.Effects;
using RimWorld;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_AntimatterReactor : Building
    {
        private static Graphic normal, powered, running;

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
                if(normal == null)
                {
                    var gd = base.DefaultGraphic.data;
                    normal = GraphicDatabase.Get(gd.graphicClass, gd.texPath, gd.shaderType.Shader, gd.drawSize, base.DrawColor, base.DrawColorTwo);
                    powered = GraphicDatabase.Get(gd.graphicClass, gd.texPath + "_powered", gd.shaderType.Shader, gd.drawSize, base.DrawColor, base.DrawColorTwo);
                    running = GraphicDatabase.Get(gd.graphicClass, gd.texPath + "_running", gd.shaderType.Shader, gd.drawSize, base.DrawColor, base.DrawColorTwo);
                }

                bool powerOn = PowerTraderComp?.PowerOn ?? false;
                if (powerOn)
                {
                    bool isRunning = true;
                    if (isRunning)
                        return running;
                    else
                        return powered;
                }
                else
                {
                    return normal;
                }
            }
        }
        public bool IsHorizontal
        {
            get
            {
                return Rotation.IsHorizontal;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            //Log.Message("Spawn. After load: " + respawningAfterLoad + ", has power: " + PowerTraderComp.PowerOn);

            EnergyBall = new EnergyBall(Position.ToVector3() + GetEnergyBallOffset(), Rotation.IsHorizontal ? 0f : 90f);

            if (PowerTraderComp.PowerOn)
                OnPowerStart(map);
            else
                OnPowerStop(map);

            //var thing = map.thingGrid.ThingAt(Position, ThingCategory.Building);
            //Log.Message(thing?.ToString() ?? "null");

            PowerTraderComp.powerStartedAction += () => OnPowerStart();
            PowerTraderComp.powerStoppedAction += () => OnPowerStop();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            EnergyBall.Dispose();
            base.DeSpawn(mode);
        }

        private Vector3 GetEnergyBallOffset()
        {
            if(Rotation == Rot4.North)
                return new Vector3(1f, 0f, 1.4f);
            if (Rotation == Rot4.East)
                return new Vector3(1f, 0f, 0.4f);
            if (Rotation == Rot4.South)
                return new Vector3(0f, 0f, 0.4f);
            if (Rotation == Rot4.West)
                return new Vector3(0f, 0f, 1.4f);

            Log.Error("Rotation is not cardinal!");
            return Vector3.zero;
        }

        public void OnPowerStart(Map map = null)
        {
            //Log.Message("Power started!");
            EnergyBall.Visible = true;
            CauseRedraw(map);
        }

        public void OnPowerStop(Map map = null)
        {
            //Log.Message("Power stopped...");
            EnergyBall.Visible = false;
            CauseRedraw(map);
        }

        public void CauseRedraw(Map map = null)
        {
            if (map == null)
                map = base.Map;

            map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
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
