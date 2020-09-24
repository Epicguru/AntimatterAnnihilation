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
            Building_Megumin meg = this.caster as Building_Megumin;
            if (meg == null)
                Log.Error("Verb_Megumin used by a caster that is not a Building_Megumin?");

            Map map = meg.TargetMap;
            if (map == null)
            {
                Log.Error("Null map to cast megumin verb. Yikes.");
                return false;
            }

            var targetCell = this.currentTarget.Cell;

            Log.Message($"MegVerb: Firing on map: {map}, target: {targetCell}");

            CustomOrbitalStrike beam = (CustomOrbitalStrike)GenSpawn.Spawn(AADefOf.CustomOrbitalStrike_AA, targetCell, map, WipeMode.Vanish);
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

        // ALWAYS HIT! It's a fucking sky laser, you can't hide from it.
        public override bool CanHitTarget(LocalTargetInfo targ) { return true; }
        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ) { return true; }
    }
    
}
