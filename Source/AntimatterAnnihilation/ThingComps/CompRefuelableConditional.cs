using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace AntimatterAnnihilation.ThingComps
{
    /// <summary>
    /// This component (property) is a refuelable object that only consumes fuel while turned on and also powered.
    /// </summary>
    [StaticConstructorOnStartup]
    public class CompRefuelableConditional : CompRefuelable
    {
        private static FieldInfo fInfo;

        public Func<CompRefuelableConditional, bool> FuelConsumeCondition;
        public event Action<CompRefuelableConditional> OnRefueled;
        public event Action<CompRefuelableConditional> OnRunOutOfFuel;

        public bool IsConditionPassed { get; private set; }
        public float CustomFuelBurnRate { get; set; } = 0f;
        public float CustomFuelBurnMultiplier { get; set; } = 1f;

        public float RealFuelConsumeRate
        {
            get
            {
                return (CustomFuelBurnRate > 0f ? CustomFuelBurnRate : Props.fuelConsumptionRate) * CustomFuelBurnMultiplier;
            }
        }

        public override void CompTick()
        {
            bool consume = FuelConsumeCondition?.Invoke(this) ?? false;
            IsConditionPassed = consume;

            if (consume)
            {
                // Base tick consumes fuel using default conditions (must be switched on).
                if (CustomFuelBurnRate <= 0f && CustomFuelBurnMultiplier == 1f)
                {
                    base.CompTick();
                }
                else
                {
                    base.ConsumeFuel((CustomFuelBurnRate * CustomFuelBurnMultiplier) / 60000f);
                }
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

        public void SetFuelLevel(float fuelLevel)
        {
            if (fuelLevel == this.Fuel)
                return;

            if (fuelLevel <= 0)
                fuelLevel = 0f;

            if (fInfo == null)
                fInfo = typeof(CompRefuelable).GetField("fuel", BindingFlags.Instance | BindingFlags.NonPublic);

            fInfo.SetValue(this, fuelLevel);
        }

        public override string CompInspectStringExtra()
        {
            string str = Props.FuelLabel + ": " + Fuel.ToStringDecimalIfSmall() + " / " + Props.fuelCapacity.ToStringDecimalIfSmall();
            if (!Props.consumeFuelOnlyWhenUsed && HasFuel)
            {
                float daysRemaining = Fuel / RealFuelConsumeRate;
                int ticksRemaining = (int)(daysRemaining * 60000);
                str = str + " (" + ticksRemaining.ToStringTicksToPeriod() + ")";
            }
            if (!HasFuel && !Props.outOfFuelMessage.NullOrEmpty())
                str += $"\n{Props.outOfFuelMessage} ({GetFuelCountToFullyRefuel()}x {Props.fuelFilter.AnyAllowedDef.label})";
            if (Props.targetFuelLevelConfigurable)
                str = str + ("\n" + "ConfiguredTargetFuelLevel".Translate(TargetFuelLevel.ToStringDecimalIfSmall()));
            return str;
        }
    }
}