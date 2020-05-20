using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Effects
{
    public abstract class SpawnedEffect : IDisposable
    {
        public Map Map { get; }
        public bool Visible { get; private set; }

        private List<Material> mats = new List<Material>();

        protected SpawnedEffect(Map map)
        {
            this.Map = map;
        }

        protected void AddRenderers(Transform trs)
        {
            if (trs == null)
                return;

            var renderers = trs.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                AddRenderer(r);
            }
        }

        protected void AddRenderer(Renderer renderer)
        {
            if (renderer == null)
                return;

            this.AddMat(renderer.material);
        }

        protected void AddMat(Material m)
        {
            if (m == null)
                return;

            this.mats.Add(m);
        }

        public virtual void Tick()
        {
            bool shouldBeVisible = Find.CurrentMap == this.Map;
            if (shouldBeVisible != Visible)
            {
                if (Visible)
                {
                    Hide();
                    Visible = false;
                }
                else
                {
                    Show();
                    Visible = true;
                }
            }

            foreach (var thing in mats)
            {
                if(thing != null)
                {
                    thing.SetFloat("_SpeedScale", Find.TickManager.TickRateMultiplier);
                }
            }
        }

        public abstract void Show();
        public abstract void Hide();
        public abstract void Dispose();
    }
}
