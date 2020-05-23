using AntimatterAnnihilation.Effects;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_AntimatterRailgun : Building_AATurret
    {
        public static int CHARGE_TICKS = 60 * 3; // Around 3 seconds.

        public int CurrentChargeTicks = 0; // Needs to reach CHARGE_TICKS before it can fire. While charging, has particle effects.
        public RailgunEffectComp Effect { get; private set; }

        private int cooldownTicks;

        public Building_AntimatterRailgun()
        {
            this.ScreenShakeOnShoot = 0.8f;
        }

        public override AATurretTop CreateTop()
        {
            return new RailgunTop(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (Effect != null)
                return;

            Effect = RailgunEffectSpawner.Spawn(base.Map);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);

            if (Effect == null)
                return;

            Effect.Hide(true);
            Object.Destroy(Effect.gameObject);
            Effect = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref CurrentChargeTicks, "railgunChargeTicks");
            Scribe_Values.Look(ref cooldownTicks, "railgunCooldownTicks");
        }

        public override void Tick()
        {
            base.Tick();

            var rt = (top as RailgunTop);
            if (rt == null)
                return;

            // Check if should be charging.
            bool shouldBeCharging = TargetCurrentlyAimingAt.IsValid && base.HasLOS && rt.BaseCanShootNow() && base.burstCooldownTicksLeft <= 0;

            if (shouldBeCharging)
                CurrentChargeTicks++;
            else
                CurrentChargeTicks = -1;

            cooldownTicks--;
            if (cooldownTicks < 0)
                cooldownTicks = 0;

            if (Effect == null)
                return;

            // Effect is only visible when in the charging mode and with valid target.
            bool vis = CurrentChargeTicks > 0 && TargetCurrentlyAimingAt.IsValid;
            bool shouldFire = rt.IsCharged() && cooldownTicks == 0;

            if (vis)
            {
                Vector3 start = rt.GetMuzzlePos();
                Vector3 end = TargetCurrentlyAimingAt.CenterVector3;
                const float Y = -2;
                start.y = Y;
                end.y = Y;

                if(cooldownTicks == 0)
                    Effect.Show(start, end);
                Effect.Tick();

                if (shouldFire)
                {
                    float radius = base.AttackVerb?.GetProjectile()?.projectile?.explosionRadius ?? 4f;
                    radius += 4;
                    Effect.Fire(radius);
                    cooldownTicks = 120;
                }
            }
            else
            {
                Effect.Hide(false);
            }

        }

        public override string GetInspectString()
        {
            return base.GetInspectString() + $"\nChargeTicks: {CurrentChargeTicks}";
        }
    }
}
