using System;
using Verse;

namespace AntimatterAnnihilation.Effects
{
    public abstract class SpawnedEffect : IDisposable
    {
        public Map Map { get; }
        public bool Visible { get; private set; }

        protected SpawnedEffect(Map map)
        {
            this.Map = map;
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
        }

        public abstract void Show();
        public abstract void Hide();
        public abstract void Dispose();
    }
}
