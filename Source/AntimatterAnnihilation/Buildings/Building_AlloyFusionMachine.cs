using AntimatterAnnihilation.ThingComps;
using AntimatterAnnihilation.Utils;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_AlloyFusionMachine : Building_MultiRefuelable, IConditionalGlower
    {
        public const int TICKS_PER_FRAME = 3;
        public const int FRAME_COUNT = 20;
        public const int FRAME_STEP = 3;

        public const int TICKS_PER_HYPER = 2500; // 1 hour.

        public const int GOLD_PER_HYPER = 4;
        public const int COMPOSITE_PER_HYPER = 3;
        public const int URANIUM_PER_HYPER = 2;

        public const float POWER_DRAW_NORMAL = 4500; // 1x speed
        public const float POWER_DRAW_OVERCLOCKED = POWER_DRAW_NORMAL * 2 + 1000; // 2x speed
        public const float POWER_DRAW_INSANITY = POWER_DRAW_NORMAL * 4 + 5000; // 4x speed

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

        public CompGlower CompGlower
        {
            get
            {
                if (_compGlower == null)
                    _compGlower = this.TryGetComp<CompGlower>();

                return _compGlower;
            }
        }
        private CompGlower _compGlower;
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
        public int GoldToFill
        {
            get
            {
                var f = GetFuelComp(1);
                return (int)(f.Props.fuelCapacity - f.Fuel);
            }
        }
        public int CompositeToFill
        {
            get
            {
                var f = GetFuelComp(2);
                return (int)(f.Props.fuelCapacity - f.Fuel);
            }
        }
        public int UraniumToFill
        {
            get
            {
                var f = GetFuelComp(3);
                return (int)(f.Props.fuelCapacity - f.Fuel);
            }
        }
        public int StoredGold
        {
            get
            {
                return (int) GetFuelComp(1).Fuel;
            }
        }
        public int StoredComposite
        {
            get
            {
                return (int)GetFuelComp(2).Fuel;
            }
        }
        public int StoredUranium
        {
            get
            {
                return (int)GetFuelComp(3).Fuel;
            }
        }
        public bool ShouldBeGlowingNow
        {
            get
            {
                return IsActive;
            }
        }

        private Graphic activeGraphic;
        private bool isActiveInt = true;
        private int frameNumber;
        private long tickCount;
        private int outputSide = 2;
        private byte reasonNotRunning;
        private int ticksUntilOutput = -1;
        private int powerMode;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isActiveInt, "isFusionActive");
            Scribe_Values.Look(ref outputSide, "outputSide", 2);
            Scribe_Values.Look(ref ticksUntilOutput, "ticksUntilOutput", -1);
            Scribe_Values.Look(ref powerMode, "powerMode");
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

            tickCount++;

            // Change power draw based on active status.
            UpdatePowerDraw();

            // Update active/running state.
            bool oldActive = this.IsActive;
            UpdateActiveState();
            if (oldActive != this.IsActive)
            {
                CompGlower?.ReceiveCompSignal("PowerTurnedOn"); // Obviously the power hasn't actually just been turned on, but it's just a way to trigger UpdateLit to be called.
            }

            if (!IsActive)
            {
                frameNumber = 0;
                activeGraphic = base.DefaultGraphic;
                return;
            }

            // Update output.
            UpdateOutput();

            // Update active state visuals.
            UpdateActiveGraphics();
        }

        private void UpdatePowerDraw()
        {
            PowerTraderComp.PowerOutput = this.IsActive ? -GetActivePowerDraw() : PowerTraderComp.Props.PowerConsumption;
        }

        public float GetActivePowerDraw()
        {
            switch (powerMode)
            {
                case 0:
                    return POWER_DRAW_NORMAL;
                case 1:
                    return POWER_DRAW_OVERCLOCKED;
                case 2:
                    return POWER_DRAW_INSANITY;
                default:
                    return POWER_DRAW_NORMAL;
            }
        }

        /// <summary>
        /// Gets the speed multiplier based on current power mode.
        /// </summary>
        public int GetActiveSpeed()
        {
            switch (powerMode)
            {
                case 0:
                    return 1;
                case 1:
                    return 2;
                case 2:
                    return 4;
                default:
                    return 1;
            }
        }

        private void UpdateActiveState()
        {
            IsActive = true; // Assume running for now.
            reasonNotRunning = 0;
            if (!PowerTraderComp.PowerOn)
            {
                reasonNotRunning = 1;
                IsActive = false;
            }
            else if (StoredGold < GOLD_PER_HYPER || StoredComposite < COMPOSITE_PER_HYPER || StoredUranium < URANIUM_PER_HYPER)
            {
                reasonNotRunning = 2;
                IsActive = false;
            }
        }

        private void UpdateOutput()
        {
            if (ticksUntilOutput < 0)
            {
                ticksUntilOutput = TICKS_PER_HYPER / GetActiveSpeed();
            }
            if (ticksUntilOutput > 0)
            {
                ticksUntilOutput--;
                if (ticksUntilOutput == 0)
                {
                    // Give output.
                    PlaceOutput(1);

                    // Remove internal resources.
                    SubtractFuel(GetFuelComp(1), GOLD_PER_HYPER);
                    SubtractFuel(GetFuelComp(2), COMPOSITE_PER_HYPER);
                    SubtractFuel(GetFuelComp(3), URANIUM_PER_HYPER);

                    // Flag timer to be reset.
                    ticksUntilOutput = -1;
                }
            }

            void SubtractFuel(CompRefuelableMulti f, float amountLess)
            {
                f.SetFuelLevel(f.Fuel - amountLess);
            }
        }

        private void UpdateActiveGraphics()
        {
            if (activeGraphics == null)
            {
                LoadGraphics(this);
            }

            activeGraphic = activeGraphics[frameNumber];
            if (tickCount % TICKS_PER_FRAME == 0)
            {
                frameNumber++;
                if (frameNumber >= FRAME_COUNT)
                    frameNumber = 0;
            }
        }

        public void PlaceOutput(int count)
        {
            if (count <= 0)
                return;

            Thing thing = ThingMaker.MakeThing(AADefOf.HyperAlloy_AA);
            thing.stackCount = count;

            GenPlace.TryPlaceThing(thing, this.Position + GetSpotOffset(outputSide), this.Map, ThingPlaceMode.Near);
        }

        public IntVec3 GetSpotOffset(int rot)
        {
            switch (rot)
            {
                case 0:
                    return IntVec3.North * 2;
                case 1:
                    return IntVec3.East * 2;
                case 2:
                    return IntVec3.South * 2;
                case 3:
                    return IntVec3.West * 2;
                default:
                    return IntVec3.Zero;
            }
        }

        public override string GetInspectString()
        {
            string cardRot = RotToCardinalString(outputSide).CapitalizeFirst();
            string runningStatus;

            if (IsActive)
            {
                runningStatus = "AA.AFMRunning".Translate((ticksUntilOutput / 2500f).ToString("F2"));
            }
            else
            {
                switch (reasonNotRunning)
                {
                    case 1:
                        runningStatus = "AA.AFMNotRunningNoPower".Translate();
                        break;
                    case 2:
                        runningStatus = "AA.AFMNotRunningMissingStuff".Translate(GOLD_PER_HYPER, COMPOSITE_PER_HYPER, URANIUM_PER_HYPER);
                        break;
                    default:
                        runningStatus = "Not Running: Reason unknown.";
                        break;
                }
            }

            return base.GetInspectString() + $"\n{runningStatus}\n{"AA.AFMOutputSide".Translate(cardRot)}";
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

            var cmd2 = new Command_Action();
            cmd2.defaultLabel = "AA.AFMPowerLevel".Translate();
            cmd2.defaultDesc = "AA.AFMPowerLevelDesc".Translate(GetActiveSpeed() * 100, GetActivePowerDraw());
            cmd2.icon = powerMode == 2 ? Content.PowerLevelHigh : powerMode == 1 ? Content.PowerLevelMedium : Content.PowerLevelLow;
            cmd2.action = () =>
            {
                powerMode++;
                if (powerMode >= 3)
                    powerMode = 0;
            };

            yield return cmd2;
        }
    }
}
