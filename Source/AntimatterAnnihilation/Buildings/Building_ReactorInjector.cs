using AntimatterAnnihilation.Damage;
using AntimatterAnnihilation.Effects;
using AntimatterAnnihilation.ThingComps;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                return FuelComp.IsConditionPassed && FuelComp.HasFuel && FlickComp.SwitchIsOn;
            }
        }
        public EnergyBeam Beam { get; private set; }
        public IntVec3 BeamExploreDirection { get; private set; }

        public int MaxBeamLength = 10;

        public int UpdateInterval = 20; // Every 20 ticks, so 3 times a second.
        public int BuildingDamage = 15;
        public int PawnDamage = 4;

        private long tick;

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
                Beam.Visible = isRunning;
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
                CauseRedraw();
            };
            FuelComp.OnRunOutOfFuel += (f) =>
            {
                CauseRedraw();
            };
            PowerTraderComp.powerStartedAction += () =>
            {
                CauseRedraw();
            };
            PowerTraderComp.powerStoppedAction += () =>
            {
                CauseRedraw();
            };

            Vector3 offset = GetOffset(out float angle);
            Beam = new EnergyBeam(Position.ToVector3() + offset, angle);
            ModCore.Log(Position.ToString());
            Beam.Length = 5;
            if (Rotation == Rot4.South)
            {
                Beam.LengthOffset = 0.25f;
            }
        }

        private Vector3 GetOffset(out float angle)
        {
            var rot = base.Rotation;
            if (rot == Rot4.East)
            {
                angle = 90f;
                BeamExploreDirection = new IntVec3(1, 0, 0);
                return new Vector3(2, 0, 0.15f);
            }
            if (rot == Rot4.North)
            {
                angle = 0f;
                BeamExploreDirection = new IntVec3(0, 0, 1);
                return new Vector3(1f, 0f, 2f);
            }
            if (rot == Rot4.South)
            {
                angle = 180f;
                BeamExploreDirection = new IntVec3(0, 0, -1);
                return new Vector3(0f, 0f, -0.75f);
            }
            if (rot == Rot4.West)
            {
                angle = 270f;
                BeamExploreDirection = new IntVec3(-1, 0, 0);
                return new Vector3(-1f, 0, 1.15f);
            }

            angle = 0f;
            return Vector3.zero;
        }

        public override void Tick()
        {
            base.Tick();

            if (Beam == null)
                return;

            Beam.Tick();

            if (!IsRunning)
                return;

            tick++;
            if (tick % UpdateInterval == 0)
            {
                Beam.Length = UpdateDamageAndInjection(MaxBeamLength);
            }
        }

        private List<Thing> tempThings = new List<Thing>();
        public float UpdateDamageAndInjection(float maxDst) // The action is a hacky workaround to not being able to do 'out int realDst'
        {
            bool horizontal = Rotation.IsHorizontal;
            IntVec3 basePos = Position;
            var rot = base.Rotation;
            if (rot == Rot4.East)
            {
                basePos += BeamExploreDirection * 2 + new IntVec3(0, 0, -1);
            }
            if (rot == Rot4.North)
            {
                basePos += BeamExploreDirection * 2;
            }
            if (rot == Rot4.South)
            {
                basePos += BeamExploreDirection * 2 + new IntVec3(-1, 0, 0);
            }
            if (rot == Rot4.West)
            {
                basePos += BeamExploreDirection * 2;
            }

            tempThings.Clear();

            float i;

            for (i = 0; i < maxDst; i++)
            {
                IntVec3 posLower = basePos + BeamExploreDirection * (int)i;
                IntVec3 posUpper = basePos + BeamExploreDirection * (int)i + (horizontal ? new IntVec3(0, 0, 1) : new IntVec3(1, 0, 0));

                bool done = false;
                var pawn = Map.thingGrid.ThingAt(posLower, ThingCategory.Pawn);
                if (pawn != null)
                {
                    done = true;
                    tempThings.Add(pawn);
                }
                var pawn2 = Map.thingGrid.ThingAt(posUpper, ThingCategory.Pawn);
                if (pawn2 != null)
                {
                    done = true;
                    tempThings.Add(pawn2);
                }

                if (done)
                    break;

                var build = Map.thingGrid.ThingAt(posLower, ThingCategory.Building);
                if (build != null)
                {
                    bool shouldDamage = build.def.altitudeLayer == AltitudeLayer.Building || build.def.altitudeLayer == AltitudeLayer.BuildingOnTop;
                    if (shouldDamage)
                    {
                        done = true;
                        tempThings.Add(build);
                    }


                }
                var build2 = Map.thingGrid.ThingAt(posUpper, ThingCategory.Building);
                if (build2 != null)
                {
                    bool shouldDamage = build2.def.altitudeLayer == AltitudeLayer.Building || build2.def.altitudeLayer == AltitudeLayer.BuildingOnTop;
                    if (shouldDamage)
                    {
                        done = true;
                        tempThings.Add(build2);
                    }
                }

                if (done)
                    break;
            }

            bool doneReactor = false;

            foreach (var thing in tempThings)
            {
                if (thing.def.defName == "AntimatterReactor")
                {
                    // This might be the reactor that needs to have power injected into it.
                    // However, the power needs to be injected from the correct side, and in the correct position.

                    var reactor = thing as Building_AntimatterReactor;
                    bool valid = true;
                    Vector3 tc = this.TrueCenter();
                    Vector3 rtc = reactor.TrueCenter();

                    if (horizontal && !reactor.IsHorizontal)
                        valid = false;
                    else if (!horizontal && reactor.IsHorizontal)
                        valid = false;
                    else if (horizontal && tc.z != rtc.z)
                        valid = false;
                    else if (!horizontal && tc.x != rtc.x)
                        valid = false;

                    if (!doneReactor && valid && Rotation == Rot4.North) // When firing the beam into the bottom, needs to further than the edge due to the camera angle.
                        i += 0.815f;

                    if (valid || doneReactor)
                    {
                        doneReactor = true;
                        continue;
                    }
                }

                if (thing is Pawn pawn)
                {
                    if (pawn.Downed)
                        continue; // Don't damage downed pawns, animals etc.
                }

                thing.TakeDamage(new DamageInfo(AADefOf.Annihilate, thing.def.category == ThingCategory.Building ? BuildingDamage : PawnDamage, 90, instigator: this));
            }

            return i;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);

            Beam?.Dispose();
            Beam = null;
        }

        public void CauseRedraw(Map map = null)
        {
            if (map == null)
                map = base.Map;

            map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
        }
    }
}
