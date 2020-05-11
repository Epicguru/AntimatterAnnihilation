using Verse;

namespace AntimatterAnnihilation.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_AntimatterRailgun : Building_AATurret
    {
        public Building_AntimatterRailgun()
        {
            this.ScreenShakeOnShoot = 0.2f;
        }

        public override AATurretTop CreateTop()
        {
            return new RailgunTop(this);
        }
    }
}
