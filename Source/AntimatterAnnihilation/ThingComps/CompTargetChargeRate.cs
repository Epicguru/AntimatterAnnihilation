using AntimatterAnnihilation.UI;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.ThingComps
{
    /// <summary>
    /// Used to set a building's target charge (power consumption) rate.
    /// </summary>
    [StaticConstructorOnStartup]
    public class CompTargetChargeRate : ThingComp
    {
        public float Watts
        {
            get
            {
                return Mathf.Clamp(_watts, Props.rateRange.TrueMin, Props.rateRange.TrueMax);
            }
            set
            {
                _watts = Mathf.Clamp(value, Props.rateRange.TrueMin, Props.rateRange.TrueMax); ;
            }
        }
        public CompProperties_TargetChargeRate Props
        {
            get
            {
                return base.props as CompProperties_TargetChargeRate;
            }
        }
        public float MinRate
        {
            get
            {
                return Props.rateRange.TrueMin;
            }
        }
        public float MaxRate
        {
            get
            {
                return Props.rateRange.TrueMax;
            }
        }
        public float Interval
        {
            get
            {
                return Props.interval;
            }
        }

        private float _watts;

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref _watts, "rate");
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            this._watts = this.Props.defaultRate;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Command_Action cmd = new Command_Action();
            cmd.defaultLabel = "Change charge speed";
            cmd.defaultDesc = $"Changes the power consumption when the M3G_UMIN is charging. This allows you to charge faster, if you have enough power.\nCurrent rate: {Watts:F0} watts.";
            cmd.icon = Content.PowerLevel;
            cmd.hotKey = KeyBindingDefOf.Misc5;
            cmd.action = () =>
            {
                Find.WindowStack?.Add(new UI_CompTargetChargeRate(this));
            };
            yield return cmd;
        }
    }
}