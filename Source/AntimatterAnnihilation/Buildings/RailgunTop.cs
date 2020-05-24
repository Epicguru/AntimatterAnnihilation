using AntimatterAnnihilation.Utils;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public class RailgunTop : AATurretTop
    {
        public RecoilManager Recoil;

        public RailgunTop(Building_AATurret parentTurret) : base(parentTurret)
        {
            Recoil = new RecoilManager();
            Recoil.RecoveryMultiplier = 0.9f;
        }

        public override void Tick()
        {
            base.Tick();
            Recoil.Tick();
            base.LocalDrawOffset = new Vector3(0f, 0f, -Recoil.CurrentRecoil);
        }

        public override void OnShoot()
        {
            Recoil.AddRecoil(60f);
        }

        public bool BaseCanShootNow()
        {
            return base.CanShootNow();
        }

        public bool IsCharged()
        {
            if (parentTurret == null)
                return false;

            return (parentTurret as Building_AntimatterRailgun).CurrentChargeTicks >= Building_AntimatterRailgun.CHARGE_TICKS;
        }

        public override bool CanShootNow()
        {
            return BaseCanShootNow() && IsCharged();
        }

        public Vector3 GetMuzzlePos()
        {
            const float LENGTH = 4f;
            Vector3 b = new Vector3(0f, 0f, LENGTH).RotatedBy(base.CurrentRotation);
            return parentTurret.DrawPos + b;
        }
    }
}
