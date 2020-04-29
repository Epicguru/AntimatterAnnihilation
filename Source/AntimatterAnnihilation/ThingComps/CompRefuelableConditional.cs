using RimWorld;
using System;
using Verse;

namespace AntimatterAnnihilation.ThingComps
{
    /// <summary>
    /// This component (property) is a refuelable object that only consumes fuel while turned on and also powered.
    /// </summary>
    [StaticConstructorOnStartup]
    public class CompRefuelableConditional : CompRefuelable
    {
        public Func<CompRefuelableConditional, bool> FuelConsumeCondition;
        public event Action<CompRefuelableConditional> OnRefueled;
        public event Action<CompRefuelableConditional> OnRunOutOfFuel;

        public bool IsConditionPassed { get; private set; }

        public override void CompTick()
        {
            bool consume = FuelConsumeCondition?.Invoke(this) ?? false;
            IsConditionPassed = consume;

            if (consume)
            {
                // Base tick consumes fuel using default conditions (must be switched on).
                base.CompTick();
            }
        }

        public override void ReceiveCompSignal(string signal)
        {
            switch (signal)
            {
                case "RanOutOfFuel":
                    OnRunOutOfFuel?.Invoke(this);
                    break;

                case "Refueled":
                    OnRefueled?.Invoke(this);
                    break;
            }

            base.ReceiveCompSignal(signal);
        }
    }
}