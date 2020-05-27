namespace AntimatterAnnihilation.Buildings
{
    public class Building_MBD : Building_AATurret
    {
        public Building_MBD()
        {
            this.ScreenShakeOnShoot = 0.03f;
        }

        public override AATurretTop CreateTop()
        {
            return new MBDTop(this);
        }
    }
}
