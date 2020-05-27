using AntimatterAnnihilation.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public class MBDTop : AATurretTop
    {
        const float RATIO = 31f / 280f;
        const float HEIGHT = 2.5f;
        const float WIDTH = HEIGHT * RATIO;
        const float DEFAULT_FORWARDS = 1.7f;

        public static Material BarrelMat;

        public float BarrelRotation;
        public float BarrelVelocity;

        public MBDTop(Building_AATurret parentTurret) : base(parentTurret)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref this.BarrelRotation, "top_barrelsRotation");
            Scribe_Values.Look(ref this.BarrelVelocity, "top_barrelsVelocity");
        }

        public override void Tick()
        {
            base.Tick();

            const float DT = 1f / 60f;
            const float BARRELS_VEL = -2880f;
            const float BARRELS_VEL_DECAY = 0.99f;

            if (parentTurret.IsInBurst)
            {
                BarrelVelocity = BARRELS_VEL;
            }
            else
            {
                BarrelVelocity *= BARRELS_VEL_DECAY;
            }

            BarrelRotation += BarrelVelocity * DT;
            if (BarrelRotation > 99999f) // Just to avoid running out of precision. Should not be noticed too much.
                BarrelRotation = 0f;
        }

        public override void Draw()
        {
            if(BarrelMat == null)
            {
                BarrelMat = MaterialPool.MatFrom("AntimatterAnnihilation/Buildings/MBDBarrel");
            }

            const int BARREL_COUNT = 4;
            const float RADIUS = 0.185f;

            for (int i = 0; i < BARREL_COUNT; i++)
            {
                float angle = BarrelRotation + i * (360f / BARREL_COUNT);
                DrawBarrel(angle, RADIUS);
            }

            base.Draw();
        }

        private void DrawBarrel(float angle, float radius)
        {
           

            angle = angle % 360f;
            float rads = angle * Mathf.Deg2Rad;

            float horizontalOffset = Mathf.Cos(rads) * radius;
            float verticalOffset = Mathf.Sin(rads) * 1f;

            // Altitudes:
            // -- Top (+1) --
            // -- Barrels (+0.1 to +0.9f) --
            // -- Base --

            Vector3 drawPos = (new Vector3(this.parentTurret.def.building.turretTopOffset.x + horizontalOffset, 0f, this.parentTurret.def.building.turretTopOffset.y + DEFAULT_FORWARDS)).RotatedBy(this.CurrentRotation);
            Matrix4x4 matrixRight = default(Matrix4x4);
            float heightIncrease = 0.5f + (0.4f * verticalOffset);
            matrixRight.SetTRS(this.parentTurret.DrawPos + (Altitudes.AltIncVect * heightIncrease) + drawPos, (this.CurrentRotation + TurretTop.ArtworkRotation).ToQuat(), new Vector3(HEIGHT, 1f, WIDTH));
            Graphics.DrawMesh(MeshPool.plane10, matrixRight, BarrelMat, 0);
        }

        public override void OnShoot()
        {
            base.OnShoot();

            Vector3 drawPos = this.parentTurret.DrawPos + (new Vector3(this.parentTurret.def.building.turretTopOffset.x, 0f, this.parentTurret.def.building.turretTopOffset.y + DEFAULT_FORWARDS + 2f)).RotatedBy(this.CurrentRotation);
            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();

            DoMuzzleFlash(drawPos);
        }

        private void DoMuzzleFlash(Vector3 pos)
        {
            Mote mote = (Mote)ThingMaker.MakeThing(AADefOf.Mote_LargeMuzzleFlashFast_AA, null);
            mote.Scale = 2.3f;
            mote.exactRotation = base.CurrentRotation;
            mote.exactPosition = pos;
            GenSpawn.Spawn(mote, base.parentTurret.Position, base.parentTurret.Map, WipeMode.Vanish);
        }
    }
}
