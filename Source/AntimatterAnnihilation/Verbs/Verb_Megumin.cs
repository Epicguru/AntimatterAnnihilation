using AntimatterAnnihilation.Attacks;
using AntimatterAnnihilation.Buildings;
using AntimatterAnnihilation.Utils;
using Verse;

namespace AntimatterAnnihilation.Verbs
{
    public class Verb_Megumin : Verb
    {
        protected override bool TryCastShot()
        {
            CustomOrbitalStrike beam = (CustomOrbitalStrike)GenSpawn.Spawn(AADefOf.CustomOrbitalStrike_AA, this.currentTarget.Cell, this.caster.Map, WipeMode.Vanish);
            beam.duration = Building_Megumin.DURATION_TICKS;
            beam.instigator = this.caster;
            beam.weaponDef = ((base.EquipmentSource != null) ? base.EquipmentSource.def : null);
            beam.DamageDef = AADefOf.EnergyBurn_AA;
            beam.UpdatesPerTick = 18;
            beam.ArmorPen = 0.95f; // To make it viable against mechanoids, who have 200% heat armour.
            beam.DamageAmountRange = new IntRange(35, 39); // Applied up to 18 times per tick, that's a lot of damage.
            beam.CorpseDamageAmountRange = new IntRange(10, 15); // Does less damage to corpses, so that there is something left over after the devastation, although this may be OP (killing entire army and keeping their gear).
            beam.StartStrike();

            // Spawn huge explosion once the beam fires.
            GenExplosion.DoExplosion(currentTarget.Cell, caster.Map, Building_Megumin.EXPLOSION_RADIUS, AADefOf.EnergyBurn_AA, caster, Building_Megumin.EXPLOSION_DAMAGE, Building_Megumin.EXPLOSION_PEN, AADefOf.Explosion_Antimatter_Large_AA);

            if(caster != null)
                beam.OnStrikeOver += ((Building_Megumin)caster).OnStrikeEnd;

            return true;
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return Building_Megumin.RADIUS;
        }
    }
    
}
