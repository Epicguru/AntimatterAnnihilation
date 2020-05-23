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

        private Vector3 endPos;
        private int mapID;
        private bool hidden;

        public void Setup(Map map)
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

            Hide(true);
        }

        public void Fire(float explosionSize)
        {
            Anim.SetTrigger("Shoot");
            Log.Message("Shoot");
            InParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            foreach (var part in LongParticles)
            {
                part.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

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
            bool vis = Find.CurrentMap != null && Find.CurrentMap.uniqueID == this.mapID;
            if (gameObject.activeSelf != vis)
                gameObject.SetActive(vis);
        }

        public void Show(Vector3 start, Vector3 end)
        {
            if (hidden)
            {
                InParticles.Play(true);
                foreach (var part in LongParticles)
                {
                    part.Play(true);
                }
            }

            this.endPos = end;

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
            }

            Anim.SetTrigger("Reset"); // Sucks to trigger this every frame but oh well..
            Log.Message("Reset");
            hidden = false;
        }

        public void Hide(bool clear)
        {
            if (hidden)
                return;

            InParticles.Stop(true, clear ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
            foreach (var part in LongParticles)
            {
                part.Stop(true, clear ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
            }
            Anim.SetTrigger("Reset");
            hidden = true;
        }
    }
}
