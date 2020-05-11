namespace AntimatterAnnihilation.Buildings
{
    public class Building_ThunderGun : Building_AATurret
    {
        public Building_ThunderGun()
        {
            this.ScreenShakeOnShoot = 0.05f;
        }

        public override AATurretTop CreateTop()
        {
            return new ThunderGunTop(this);
        }
    }
}
