﻿using AntimatterAnnihilation.Effects;
using AntimatterAnnihilation.Utils;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AntimatterAnnihilation.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_AntimatterRailgun : Building_AATurret
    {
        public static int CHARGE_TICKS = 280; // Made to match audio clip, around 4.7 seconds.
        public static AnimationCurve ParticlesAmountCurve;

        static Building_AntimatterRailgun()
        {
            var c = new AnimationCurve();

            c.AddKey(0f, 0f);
            c.AddKey(0.5f, 1f);
            c.AddKey(0.85f, 0f);
            c.AddKey(1f, 0f);

            ParticlesAmountCurve = c;
        }

        public int CurrentChargeTicks = 0; // Needs to reach CHARGE_TICKS before it can fire. While charging, has particle effects.
        public RailgunEffectComp Effect { get; private set; }

        private int cooldownTicks;
        private Sustainer soundSustainer;

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
            bool shouldBeCharging = CurrentOrForcedTarget.IsValid && base.HasLOS && rt.BaseCanShootNow() && base.burstCooldownTicksLeft <= 0;

            if (shouldBeCharging)
                CurrentChargeTicks++;
            else
                CurrentChargeTicks = -1;

            if (shouldBeCharging && CurrentChargeTicks == 0)
            {
                // Start playing audio.
                SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
                soundSustainer = AADefOf.RailgunShoot_AA.TrySpawnSustainer(info);
            }
            if (!shouldBeCharging && cooldownTicks == 0)
            {
                soundSustainer?.End();
            }

            if (soundSustainer != null)
            {
                if (soundSustainer.Ended)
                    soundSustainer = null;
                else
                    soundSustainer.Maintain();
            }

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

                Effect.SetParticleRate(GetCurrentParticleRate());
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

        public float DstToTarget()
        {
            if (!CurrentOrForcedTarget.IsValid)
                return 10f;

            Vector3 shooter = Position.ToVector3();
            shooter.y = 0;

            Vector3 target = CurrentOrForcedTarget.CenterVector3;
            target.y = 0;

            return (target - shooter).magnitude;
        }

        public float GetCurrentParticleRate()
        {
            const float BASE_PER_METER = 20f / 10f; // Values taken from in Unity test area, just eyeballing it.
            float dst = DstToTarget();

            float baseRate = BASE_PER_METER * dst;

            float chargeUpNormalizedTime = Mathf.Clamp01((float) CurrentChargeTicks / CHARGE_TICKS);
            float multiFromCurve = Mathf.Clamp01(ParticlesAmountCurve.Evaluate(chargeUpNormalizedTime));

            float final = baseRate * multiFromCurve;

            return final;
        }
    }
}
