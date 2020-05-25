using RimWorld;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_AlloyFusionMachine : Building
    {
        #region Static stuff
        public static int TICKS_PER_FRAME = 3;
        public static int FRAME_COUNT = 20;
        public static int FRAME_STEP = 3;

        private static Graphic[] activeGraphics;

        private static void LoadGraphics(Building thing)
        {
            activeGraphics = new Graphic[FRAME_COUNT];
            for (int i = 0; i < FRAME_COUNT; i++)
            {
                var gd = thing.DefaultGraphic.data;
                string num = (i * FRAME_STEP).ToString();
                while (num.Length < 4)
                    num = '0' + num;
                activeGraphics[i] = GraphicDatabase.Get(gd.graphicClass, $"AntimatterAnnihilation/Buildings/AlloyFusionMachine/{num}", gd.shaderType.Shader, gd.drawSize, Color.white, Color.white);
            }
        }

        #endregion

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

        public override Graphic Graphic
        {
            get
            {
                return activeGraphic ?? base.DefaultGraphic;
            }
        }

        public bool IsActive
        {
            get
            {
                return isActiveInt;
            }
            protected set
            {
                isActiveInt = value;
            }
        }

        private Graphic activeGraphic;
        private bool isActiveInt = true;
        private int frameNumber;
        private long tickCount;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isActiveInt, "isFusionActive");
        }

        public override void Tick()
        {
            base.Tick();

            if (activeGraphics == null)
            {
                LoadGraphics(this);
            }

            if (IsActive)
            {
                activeGraphic = activeGraphics[frameNumber];
            }
            else
            {
                frameNumber = 0;
                activeGraphic = base.DefaultGraphic;
                return;
            }

            tickCount++;

            if (tickCount % TICKS_PER_FRAME == 0)
            {
                frameNumber++;
                if (frameNumber >= FRAME_COUNT)
                    frameNumber = 0;
            }
        }
    }
}
