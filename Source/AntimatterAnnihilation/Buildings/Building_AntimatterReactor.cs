using AntimatterAnnihilation.AI;
using AntimatterAnnihilation.Effects;
using AntimatterAnnihilation.Utils;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_AntimatterReactor : Building, IAvoidanceProvider
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
        public EnergyBeam EnergyBeam { get; set; }
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
                    bool isRunning = IsRunning;
                    EnergyBall.BallVisible = isRunning;

                    if (isRunning)
                        return running;
                    else
                        return powered;
                }
                else
                {
                    EnergyBall.BallVisible = false;
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
        public int TicksToShutdownWithNoInput { get; set; } = 30;
        public bool IsRunning { get; private set; }
        public float BuildingDamage = 25;
        public float PawnDamage = 6.5f;
        public int UpdateInterval = 20; // Every 20 ticks, so 3 times a second.

        private List<(IntVec3 cell, byte weight)> avoidance = new List<(IntVec3 cell, byte weight)>();
        private Building_ReactorInjector currentInjector;
        private int injectorRot;
        private int tickCounter;
        private long updateTick;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            EnergyBall = new EnergyBall(map, Position.ToVector3() + GetEnergyBallOffset(), Rotation.IsHorizontal ? 0f : 90f);
            EnergyBeam = new EnergyBeam(map, Position.ToVector3(), 0f, false);

            if (PowerTraderComp.PowerOn)
                OnPowerStart(map);
            else
                OnPowerStop(map);

            PowerTraderComp.powerStartedAction += () => OnPowerStart();
            PowerTraderComp.powerStoppedAction += () => OnPowerStop();

            // Register with avoidance grid system.
            var grid = AI_AvoidGrid.Ensure(map);
            if (!grid.AvoidanceProviders.Contains(this))
            {
                grid.AvoidanceProviders.Add(this);
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            // Remove from avoidance grid system.
            var grid = AI_AvoidGrid.Ensure(EnergyBall.Map);
            if (grid.AvoidanceProviders.Contains(this))
            {
                grid.AvoidanceProviders.Remove(this);
            }

            EnergyBall?.Dispose();
            EnergyBall = null;
            EnergyBeam?.Dispose();
            EnergyBeam = null;

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
            CauseRedraw(map);
        }

        public void OnPowerStop(Map map = null)
        {
            Log.Message("Power stopped...");
            CauseRedraw(map);
        }

        public void CauseRedraw(Map map = null)
        {
            if (map == null)
                map = base.Map;

            map?.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
        }

        public void RegisterInput(Building_ReactorInjector injector, int injRot)
        {
            // Assumes that the injector is valid (correct side, aligned properly).

            tickCounter = 0;
            if (currentInjector != null && currentInjector != injector)
            {
                // Two injectors?!
                // TODO handle (explode?)
            }

            currentInjector = injector;
            this.injectorRot = injRot;
            tickCounter = 0;
            if (!IsRunning)
            {
                IsRunning = true;
                CauseRedraw();
            }
        }

        public void RemoveInput(Building_ReactorInjector injector)
        {
            if (currentInjector != injector)
                return;

            currentInjector = null;

            if (IsRunning)
            {
                IsRunning = false;
                CauseRedraw();
            }
        }

        public override void Tick()
        {
            base.Tick();

            if(tickCounter < TicksToShutdownWithNoInput && currentInjector != null)
                tickCounter++;
            if (tickCounter == TicksToShutdownWithNoInput)
            {
                if (IsRunning)
                {
                    IsRunning = false;
                    currentInjector = null;
                    CauseRedraw();
                }
            }

            updateTick++;
            if (updateTick % UpdateInterval == 0 && EnergyBeam != null)
            {
                EnergyBeam.Length = UpdateOutputBeam(16);
            }

            EnergyBall?.Tick();
            EnergyBeam?.Tick();
        }

        private List<Thing> tempThings = new List<Thing>();
        public float UpdateOutputBeam(float maxDst)
        {
            avoidance.Clear();

            bool horizontal = Rotation.IsHorizontal;

            if (!IsRunning)
            {
                EnergyBeam.BeamVisible = false;
                return 0f;
            }
            EnergyBeam.BeamVisible = true;
            IntVec3 beamExploreDirection = IntVec3.Zero;
            switch (injectorRot)
            {
                case 0:
                    beamExploreDirection = new IntVec3(0, 0, 1);
                    EnergyBeam.Rotation = 0f;
                    break;
                case 1:
                    beamExploreDirection = new IntVec3(1, 0, 0);
                    EnergyBeam.Rotation = 90;
                    break;
                case 2:
                    beamExploreDirection = new IntVec3(0, 0, -1);
                    EnergyBeam.Rotation = 180;
                    break;
                case 3:
                    beamExploreDirection = new IntVec3(-1, 0, 0);
                    EnergyBeam.Rotation = 270;
                    break;
            }

            IntVec3 basePos;
            if (horizontal)
            {
                Vector3 baseOffset = new Vector3(injectorRot == 3 ? -1 : 0, 0f, -1);
                basePos = new IntVec3(this.TrueCenter() + baseOffset) + beamExploreDirection * 6;
                EnergyBeam.Position = basePos.ToVector3() + new Vector3(injectorRot == 3 ? 1 : 0, 0, 1.25f);
            }
            else 
            {
                basePos = new IntVec3(this.TrueCenter() - new Vector3(0.1f, 0f, injectorRot == 2 ? 0.1f : 0f)) + beamExploreDirection * 6;
                EnergyBeam.Position = basePos.ToVector3() + new Vector3(1, 0, 1f + (injectorRot == 2 ? 0.8f : 0f) + (injectorRot == 0 ? -1 : 0));
                EnergyBeam.LengthOffset = (injectorRot == 2 ? 0.8f : 0f) + (injectorRot == 0 ? 0.43f : 0f);
            }

            tempThings.Clear();

            float i;

            for (i = 0; i < maxDst; i++)
            {
                IntVec3 posLower = basePos + beamExploreDirection * (int)i;
                IntVec3 posUpper = basePos + beamExploreDirection * (int)i + (horizontal ? new IntVec3(0, 0, 1) : new IntVec3(1, 0, 0));

                avoidance.Add((posLower, AI_AvoidGrid.WEIGHT_ENERGY_BEAM));
                avoidance.Add((posUpper, AI_AvoidGrid.WEIGHT_ENERGY_BEAM));

                bool done = false;
                var pawn = Map.thingGrid.ThingAt(posLower, ThingCategory.Pawn);
                if (pawn != null && !(pawn as Pawn).Downed) // Don't damage downed pawns, because it's too punishing.
                {
                    done = true;
                    tempThings.Add(pawn);
                }
                var pawn2 = Map.thingGrid.ThingAt(posUpper, ThingCategory.Pawn);
                if (pawn2 != null && !(pawn2 as Pawn).Downed)
                {
                    done = true;
                    tempThings.Add(pawn2);
                }

                if (done)
                    break;

                var things = Map.thingGrid.ThingsListAtFast(posLower);
                foreach (var build in things)
                {
                    bool shouldDamage = build.def.altitudeLayer == AltitudeLayer.Building || build.def.altitudeLayer == AltitudeLayer.BuildingOnTop;
                    if (!shouldDamage)
                        continue;

                    done = true;
                    tempThings.Add(build);
                    break;
                }
                things = Map.thingGrid.ThingsListAtFast(posUpper);
                foreach (var build in things)
                {
                    bool shouldDamage = build.def.altitudeLayer == AltitudeLayer.Building || build.def.altitudeLayer == AltitudeLayer.BuildingOnTop;
                    if (!shouldDamage)
                        continue;

                    done = true;
                    tempThings.Add(build);
                    break;
                }

                if (done)
                    break;
            }

            foreach (var thing in tempThings)
            {
                if (thing.def.defName == "AntimatterReactorPowerConverter_AA")
                {
                    var converter = thing as Building_PowerConverter;
                    Vector3 tc = this.TrueCenter();
                    Vector3 rtc = converter.TrueCenter();

                    bool valid = true;
                    if (horizontal && tc.z != rtc.z)
                        valid = false;
                    else if (!horizontal && tc.x != rtc.x)
                        valid = false;
                    else if (injectorRot == 0 && converter.Rotation != Rot4.South)
                        valid = false;
                    else if (injectorRot == 1 && converter.Rotation != Rot4.West)
                        valid = false;
                    else if (injectorRot == 2 && converter.Rotation != Rot4.North)
                        valid = false;
                    else if (injectorRot == 3 && converter.Rotation != Rot4.East)
                        valid = false;

                    if (valid)
                    {
                        converter.GiveInput(this);
                        continue;
                    }
                }

                float damage = thing.def.category == ThingCategory.Building ? BuildingDamage : PawnDamage;
                thing.TakeDamage(new DamageInfo(AADefOf.EnergyBurn_AA, damage, 15, instigator: this));
            }

            return i;
        }

        public IEnumerable<(IntVec3 cell, byte weight)> GetAvoidance(Map map)
        {
            if (map == null || map != this.Map)
                yield break;

            foreach (var point in avoidance)
            {
                yield return point;
            }
        }
    }
}
