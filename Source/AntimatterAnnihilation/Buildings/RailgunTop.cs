using AntimatterAnnihilation.Utils;
using UnityEngine;

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
    }
}
