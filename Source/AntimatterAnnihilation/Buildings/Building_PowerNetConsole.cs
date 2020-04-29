using AntimatterAnnihilation.UI;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_PowerNetConsole : Building
    {
        private static Graphic normal, running;

        public bool IsRunning
        {
            get
            {
                return PowerTraderComp.PowerOn && PowerTraderComp.PowerNet != null;
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

        public event Action OnDestroyed;

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

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            PowerTraderComp.powerStartedAction += () =>
            {
                CauseRedraw();
            };
            PowerTraderComp.powerStoppedAction += () =>
            {
                CauseRedraw();
            };
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);

            OnDestroyed?.Invoke();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(var thing in base.GetGizmos())
            {
                yield return thing;
            }

            yield return new Command_Action()
            {
                action = () =>
                {
                    UI_PowerNetConsole.Open(this);
                },
                defaultLabel = "Open Console",
                defaultDesc = "Shows detailed information about the connected power network, and allows you to turn power on and off.",
                icon = Content.PowerNetGraph
            };
        }

        public void CauseRedraw(Map map = null)
        {
            if (map == null)
                map = base.Map;

            map?.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
        }
    }
}
