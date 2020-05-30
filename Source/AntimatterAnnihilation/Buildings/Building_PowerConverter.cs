using RimWorld;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_PowerConverter : Building
    {
        public static float BasePowerOutput = 30000;
        public static float PowerOutputMulti { get; set; } = 1f;

        private float PowerOutput
        {
            get
            {
                float reactorMulti = this.CurrentReactor?.CurrentInjector?.PowerOutputMultiplier ?? 1f;

                return BasePowerOutput * PowerOutputMulti * reactorMulti;
            }
        }

        private static Graphic normal, running;
        private const int MAX_TICKS_SINCE_INPUT = 30;

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

                bool isRunning = HasInput;
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
        public bool HasInput
        {
            get
            {
                return ticksSinceHasInput < MAX_TICKS_SINCE_INPUT;
            }
        }
        public Building_AntimatterReactor CurrentReactor { get; private set; }

        private int ticksSinceHasInput;

        public override void Tick()
        {
            if (CurrentReactor != null && CurrentReactor.Destroyed)
                CurrentReactor = null;

            if (ticksSinceHasInput < MAX_TICKS_SINCE_INPUT)
            {
                ticksSinceHasInput++;
            }
            else 
            {
                CurrentReactor = null;
                if (ticksSinceHasInput != 69420)
                {
                    ticksSinceHasInput = 69420;
                    CauseRedraw();
                }
            }

            var trader = PowerComp as CompPowerTrader;
            trader.PowerOutput = HasInput ? (PowerOutput) : 0;

            base.Tick();
        }

        public void GiveInput(Building_AntimatterReactor r)
        {
            ticksSinceHasInput = 0;
            CurrentReactor = r;
            if (!HasInput)
            {
                CauseRedraw();
            }
        }

        public void CauseRedraw(Map map = null)
        {
            if (map == null)
                map = base.Map;

            map?.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
        }
    }
}
