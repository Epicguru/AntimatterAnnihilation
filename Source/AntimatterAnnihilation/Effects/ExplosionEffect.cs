using System;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Effects
{
    public class ExplosionEffect : MonoBehaviour
    {
        public ParticleSystem ParticleSystem;
        public Animator Animator;
        public Transform Scale;
        public float Time = 0.7f;
        public float TickRate = 1f;
        public event Action<ExplosionEffect> Despawn;

        private float timer;
        private int mapID;

        public void Play(Map map, Vector3 position, float scale)
        {
            // Store map as ID to avoid keeping a reference to it once world is closed to main menu.
            this.mapID = map?.uniqueID ?? -1;

            if (ParticleSystem == null)
            {
                ParticleSystem = GetComponentInChildren<ParticleSystem>();
                Animator = GetComponentInChildren<Animator>();
                Scale = transform.Find("Scale");
            }

            gameObject.SetActive(true);
            transform.position = position;
            transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            Scale.localScale = new Vector3(scale, scale, scale);
            ParticleSystem.Play();
            Animator.SetTrigger("Start");
            timer = Time;
        }

        public void Tick()
        {
            const float DT = 1f / 60f;
            timer -= DT;
            if (timer <= 0)
            {
                // End.
                Stop();
                return;
            }

            Animator.speed = TickRate;
            var main = ParticleSystem.main;
            main.simulationSpeed = TickRate;

            bool vis = Find.CurrentMap != null && Find.CurrentMap.uniqueID == this.mapID;
            if (gameObject.activeSelf != vis)
                gameObject.SetActive(vis);
        }

        public void Stop()
        {
            ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            gameObject.SetActive(false);

            Despawn?.Invoke(this);
            Despawn = null;
            EffectPool<ExplosionEffect>.Return(this);
        }
    }
}
