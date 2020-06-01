using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AntimatterAnnihilation.ThingComps
{
    /// <summary>
    /// Version of conditional refuelable that is designed to be shuffled within the component list to trick rimworld into accepting multiple refuelable components.
    /// </summary>
    [StaticConstructorOnStartup]
    public class CompRefuelableMulti : CompRefuelableConditional
    {
        public int Id
        {
            get
            {
                return (Props as CompProperties_RefuelableMulti).id;
            }
        }

        public string FriendlyName
        {
            get
            {
                return Props.fuelFilter.Summary.CapitalizeFirst();
            }
        }

        public ThingDef FuelDef
        {
            get
            {
                if (cachedFuelDef == null)
                {
                    var h = (HashSet<ThingDef>)Props.fuelFilter.AllowedThingDefs;
                    cachedFuelDef = h.First(); // Yeah this is clunky.
                }
                return cachedFuelDef;
                
            }
        }

        public int FuelPriority
        {
            get
            {
                return (Props as CompProperties_RefuelableMulti).fuelPriority;
            }
        }

        private ThingDef cachedFuelDef;

        public override string CompInspectStringExtra()
        {
            return FriendlyName + ": " + Fuel.ToStringDecimalIfSmall() + " / " + Props.fuelCapacity.ToStringDecimalIfSmall();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield break;
        }

        public override void PostExposeData()
        {
            float fuel = this.Fuel;
            Scribe_Values.Look(ref fuel, $"fuel_{Id}");
            base.SetFuelLevel(fuel);

            // Not supported:
            //Scribe_Values.Look<float>(ref this.configuredTargetFuelLevel, "configuredTargetFuelLevel", -1f, false);

            Scribe_Values.Look(ref allowAutoRefuel, $"allowAutoRefuel_{Id}");

            if (Scribe.mode == LoadSaveMode.PostLoadInit && !this.Props.showAllowAutoRefuelToggle)
            {
                this.allowAutoRefuel = this.Props.initialAllowAutoRefuel;
            }
        }
    }
}