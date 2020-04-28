using AntimatterAnnihilation.UI;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    [StaticConstructorOnStartup]
    public class Building_PowerNetConsole : Building
    {
        private static Graphic normal, running;

        public event Action OnDestroyed;

        public CompPowerTrader PowerTraderComp
        {
            get
            {
                if (_powerTraderComp == null)
                    this._powerTraderComp = base.GetComp<CompPowerTrader>();
                return _powerTraderComp;
            }
        }
        private CompPowerTrader _powerTraderComp;

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);

            OnDestroyed?.Invoke();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(var thing in base.GetGizmos())
            {
                yield return thing;
            }

            yield return new Command_Action()
            {
                action = () =>
                {
                    UI_PowerNetConsole.Open(this);
                }
            };
        }
    }
}
