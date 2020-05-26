using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_AlloyFusionMachine : Building_TrayPuller
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
                    this._powerTraderComp = GetComp<CompPowerTrader>();
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
        private int outputSide;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isActiveInt, "isFusionActive");
            Scribe_Values.Look(ref outputSide, "outputSide");
        }

        public string RotToCardinalString(int rot)
        {
            switch (rot)
            {
                case 0:
                    return "AA.North".Translate();
                case 1:
                    return "AA.East".Translate();
                case 2:
                    return "AA.South".Translate();
                case 3:
                    return "AA.West".Translate();
                default:
                    return $"Unknown rotation: {rot}";
            }
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

        public override string GetInspectString()
        {
            string cardRot = RotToCardinalString(outputSide).CapitalizeFirst();
            return base.GetInspectString() + $"\n{"AA.AFMOutputSide".Translate(cardRot)}";
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            var cmd = new Command_Action();
            cmd.defaultLabel = "AA.AFMChangeOutputSide".Translate();
            cmd.defaultDesc = "AA.AFMChangeOutputSideDesc".Translate();
            cmd.icon = Content.ArrowIcon;
            cmd.iconAngle = outputSide * 90f;
            cmd.action = () =>
            {
                outputSide++;
                if (outputSide >= 4)
                    outputSide = 0;
            };

            yield return cmd;
        }
    }
}
