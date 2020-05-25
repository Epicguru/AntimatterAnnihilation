using RimWorld;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_AlloyFusionMachine : Building_Storage
    {
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
    }
}
