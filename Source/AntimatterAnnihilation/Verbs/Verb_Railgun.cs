using AntimatterAnnihilation.Buildings;
using Verse;

namespace AntimatterAnnihilation.Verbs
{
    public class Verb_Railgun : Verb_LaunchProjectile
    {
        protected override int ShotsPerBurst
        {
            get
            {
                return this.verbProps.burstShotCount;
            }
        }

        protected override bool TryCastShot()
        {
            Building_AATurret turret = this.caster as Building_AATurret;

            bool flag = base.TryCastShot();
            if (flag)
            {
                // Gun has fired.
                if(turret.ScreenShakeOnShoot > 0f)
                    Find.CameraDriver.shaker.SetMinShake(turret.ScreenShakeOnShoot);
                turret.top.OnShoot();
            }
            
            return flag;
        }
	}
}
