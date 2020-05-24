using AntimatterAnnihilation.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public class ThunderGunTop : AATurretTop
    {
        public static Material BarrelMat;

        public RecoilManager RecoilLeft, RecoilRight;
        public uint ShootFlipFlop;

        public ThunderGunTop(Building_AATurret parentTurret) : base(parentTurret)
        {
            RecoilLeft = new RecoilManager();
            RecoilRight = new RecoilManager();

            ConfigRecoil(RecoilLeft);
            ConfigRecoil(RecoilRight);

            void ConfigRecoil(RecoilManager m)
            {
                m.RecoveryMultiplier = 0.75f;
                m.VelocityMultiplier = 0.85f;
            }
        }

        public override void Tick()
        {
            base.Tick();
            RecoilLeft.Tick();
            RecoilRight.Tick();
        }

        public override void Draw()
        {
            if(BarrelMat == null)
            {
                BarrelMat = MaterialPool.MatFrom("AntimatterAnnihilation/Buildings/ThunderGunBarrel");
            }

            const float RATIO = 57f / 257f;
            const float HEIGHT = 2.5f;
            const float WIDTH = HEIGHT * RATIO;
            const float SPACING = 0.36f;
            const float DEFAULT_FORWARDS = 1.35f;

            Vector3 rightPos = (new Vector3(this.parentTurret.def.building.turretTopOffset.x + SPACING, 0f, this.parentTurret.def.building.turretTopOffset.y - RecoilRight.CurrentRecoil + DEFAULT_FORWARDS)).RotatedBy(this.CurrentRotation);
            Matrix4x4 matrixRight = default(Matrix4x4);
            matrixRight.SetTRS(this.parentTurret.DrawPos + Altitudes.AltIncVect * 0.5f + rightPos, (this.CurrentRotation + TurretTop.ArtworkRotation).ToQuat(), new Vector3(HEIGHT, 1f, WIDTH));
            Graphics.DrawMesh(MeshPool.plane10, matrixRight, BarrelMat, 0);

            Vector3 leftPos = (new Vector3(this.parentTurret.def.building.turretTopOffset.x - SPACING, 0f, this.parentTurret.def.building.turretTopOffset.y - RecoilLeft.CurrentRecoil + DEFAULT_FORWARDS)).RotatedBy(this.CurrentRotation);
            Matrix4x4 matrixLeft = default(Matrix4x4);
            matrixLeft.SetTRS(this.parentTurret.DrawPos + Altitudes.AltIncVect * 0.5f + leftPos, (this.CurrentRotation + TurretTop.ArtworkRotation).ToQuat(), new Vector3(HEIGHT, 1f, WIDTH));
            Graphics.DrawMesh(MeshPool.plane10, matrixLeft, BarrelMat, 0);

            base.Draw();
        }

        public override void OnShoot()
        {
            ShootFlipFlop++;

            bool left = ShootFlipFlop % 2 == 0;
            if(left)
                RecoilLeft.AddRecoil(25f);
            else
                RecoilRight.AddRecoil(25f);
        }
    }
}
