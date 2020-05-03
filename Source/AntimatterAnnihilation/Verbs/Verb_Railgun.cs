using AntimatterAnnihilation.Buildings;
using Verse;

namespace AntimatterAnnihilation.Verbs
{
    public class Verb_Railgun : Verb_LaunchProjectile
    {
        protected override bool TryCastShot()
        {
            Building_AATurret building_Railgun = this.caster as Building_AATurret;

            bool flag = base.TryCastShot();
            if (flag)
            {
                // Gun has fired.
                Find.CameraDriver.shaker.SetMinShake(0.3f);
            }
            
            return flag;
        }
	}
}
