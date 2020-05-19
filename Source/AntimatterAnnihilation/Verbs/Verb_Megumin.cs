using AntimatterAnnihilation.Buildings;
using RimWorld;
using Verse;

namespace AntimatterAnnihilation.Verbs
{
    public class Verb_Megumin : Verb
    {
        protected override bool TryCastShot()
        {
            Log.Message("Spawning power beam.");
            PowerBeam powerBeam = (PowerBeam)GenSpawn.Spawn(ThingDefOf.PowerBeam, this.currentTarget.Cell, this.caster.Map, WipeMode.Vanish);
            powerBeam.duration = Building_Megumin.DURATION_TICKS;
            powerBeam.instigator = this.caster;
            powerBeam.weaponDef = ((base.EquipmentSource != null) ? base.EquipmentSource.def : null);
            powerBeam.StartStrike();
            //if (base.EquipmentSource != null && !base.EquipmentSource.Destroyed)
            //{
            //    base.EquipmentSource.Destroy(DestroyMode.Vanish);
            //}
            Log.Message("Done.");
            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return Building_Megumin.RADIUS;
        }
    }
    
}
