using System;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Effects
{
    public class RailgunEffectComp : MonoBehaviour
    {
        public Animator Anim;
        public Transform BeamScale;
        public Transform ParticlesScale;
        public ParticleSystem[] LongParticles;
        public ParticleSystem InParticles;

        public float TickRate = 1f;
        public event Action<RailgunEffectComp> Despawn;

        private float timeToDespawnAfterFire = 3f;
        private float timer = 0f;
        private Vector3 endPos;
        private int mapID;

        public void Setup(Map map, Vector3 start, Vector3 end)
        {
            // Store map as ID to avoid keeping a reference to it once world is closed to main menu.
            this.mapID = map?.uniqueID ?? -1;

            if (Anim == null)
            {
                Anim = GetComponent<Animator>();
                BeamScale = transform.Find("Beam");
                ParticlesScale = transform.Find("Particles");
                LongParticles = transform.Find("Particles").GetComponentsInChildren<ParticleSystem>();
                InParticles = transform.Find("InParticles").GetComponent<ParticleSystem>();
            }

            this.endPos = end;
            gameObject.SetActive(true);

            transform.localPosition = start;
            transform.rotation = Quaternion.LookRotation(end - start, Vector3.up);
            transform.Rotate(new Vector3(-90f, 0f, 0f), Space.Self);

            float length = (start - end).magnitude;
            BeamScale.localScale = new Vector3(1, length + 3, 1);
            ParticlesScale.localPosition = new Vector3(0, (length + 3) * -0.5f + 3, 0f);
            foreach (var part in LongParticles)
            {
                var shape = part.shape;
                var scale = shape.scale;
                scale.y = length + 3;
                shape.scale = scale;

                part.Clear(true);
                part.Play(true);
            }

            Anim.SetTrigger("Reset");
            InParticles.Clear(true);
            InParticles.Play(true);
            timer = -1f;
        }

        public void Fire(float explosionSize)
        {
            Anim.SetTrigger("Shoot");
            InParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            foreach (var part in LongParticles)
            {
                part.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            timer = timeToDespawnAfterFire;

            if (explosionSize > 0)
            {
                Vector3 pos = endPos;
                var map = TryGetThisMap();
                if(map != null)
                    ExplosionEffectManager.Spawn(map, pos, explosionSize);
            }
        }

        private Map TryGetThisMap()
        {
            if (Find.Maps == null || Find.Maps.Count == 0)
                return null;

            foreach (var map in Find.Maps)
            {
                if (map.uniqueID == this.mapID)
                    return map;
            }

            return null;
        }

        public void Tick()
        {
            const float DT = 1f / 60f;
            if (timer > 0)
            {
                timer -= DT;
                if (timer <= 0f)
                {
                    Stop();
                    return;
                }
            }

            Anim.speed = TickRate;
            ChangeSpeed(InParticles);
            foreach (var part in LongParticles)
            {
                ChangeSpeed(part);
            }

            bool vis = Find.CurrentMap != null && Find.CurrentMap.uniqueID == this.mapID;
            if (gameObject.activeSelf != vis)
                gameObject.SetActive(vis);

            void ChangeSpeed(ParticleSystem s)
            {
                var main = s.main;
                main.simulationSpeed = TickRate;
            }
        }

        public void Stop()
        {
            InParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            foreach (var part in LongParticles)
            {
                part.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            gameObject.SetActive(false);
            Despawn?.Invoke(this);
            Despawn = null;
            EffectPool<RailgunEffectComp>.Return(this);
        }
    }
}
