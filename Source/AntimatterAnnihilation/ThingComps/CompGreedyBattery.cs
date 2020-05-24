using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.ThingComps
{
    /// <summary>
    /// A greedy battery is a battery that will pull energy from other batteries in order to be filled first.
    /// Greedy batteries will not pull energy from other greedy batteries.
    /// </summary>
    [StaticConstructorOnStartup]
    public class CompGreedyBattery : CompPowerBattery
    {
        private static List<CompPowerBattery> batteries = new List<CompPowerBattery>();
        private static FieldInfo storedEnergyInfo;

        public CompProperties_GreedyBattery RealProps
        {
            get
            {
                return Props as CompProperties_GreedyBattery;
            }
        }
        public float MaxStoredEnergy
        {
            get
            {
                return Props.storedEnergyMax;
            }
            set
            {
                if (value < 0)
                    value = 0;

                Props.storedEnergyMax = value;
                if(StoredEnergy > value)
                {
                    SetStoredEnergyPct(1f);
                }
            }
        }

        public float MaxPull
        {
            get
            {
                return RealProps.maxPull;
            }
        }

        public bool IsPulling { get; private set; }

        public bool DoInspectorInfo
        {
            get
            {
                return RealProps.doInspectorInfo;
            }
        }

        public bool DoSelfDischarge
        {
            get
            {
                return RealProps.doSelfDischarge;
            }
        }

        public float WattDaysUntilFull
        {
            get
            {
                return MaxStoredEnergy - StoredEnergy;
            }
        }

        public override void CompTick()
        {
            if (DoSelfDischarge)
                base.CompTick(); // Only does the discharge.

            IsPulling = false;

            if (PowerNet?.batteryComps == null)
                return;

            var bats = GetPullableBatteries();

            if (bats.Count == 0)
                return;

            float toPull = MaxPull * CompPower.WattsToWattDaysPerTick;
            float pulled = 0f;

            // Try to pull equally by splitting total pull between number of batteries.
            float pullFromEach = toPull / bats.Count;

            foreach (var bat in bats)
            {
                float maxDraw = bat.StoredEnergy;
                float toDraw = Mathf.Min(pullFromEach, maxDraw);
                bat.DrawPower(toDraw);

                pulled += toDraw;

                // In theory this should not be possible, but I'll check anyway for debugging purposes.
                if (pulled >= toPull)
                {
                    Log.Warning("GreedyBattery error: pulled more than was calculated!");
                    break;
                }
            }

            if (pulled < toPull)
            {
                // The plan to pull equally from all didn't work, almost certainly because the batteries have different power levels.
                // I could continue running the 'equal pull' algorithm more times and I would eventually reach the desired pull amount, but that could get very slow.
                // Instead I will just pull as much as possible from each battery in sequence, in a last ditch attempt to get the desired power.

                foreach (var bat in bats)
                {
                    float maxDraw = bat.StoredEnergy;
                    float tryToDraw = (toPull - pulled);
                    float toDraw = Mathf.Min(tryToDraw, maxDraw);
                    bat.DrawPower(toDraw);

                    pulled += toDraw;

                    if (pulled >= toPull)
                        break;
                }
            }

            // Done, now all the toPull power will have been pulled, or failed if there was just not enough energy in any batteries.
            if (pulled > 0)
            {
                IsPulling = true;
                SetStoredEnergy(this.StoredEnergy + pulled);
            }
        }

        public void SetStoredEnergy(float wattDays)
        {
            if (storedEnergyInfo == null)
            {
                storedEnergyInfo = typeof(CompPowerBattery).GetField("storedEnergy", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if (wattDays == base.StoredEnergy)
                return;

            if (wattDays < 0)
                wattDays = 0;
            if (wattDays > MaxStoredEnergy)
                wattDays = MaxStoredEnergy;

            storedEnergyInfo.SetValue(this, wattDays);
        }

        private List<CompPowerBattery> GetPullableBatteries()
        {
            batteries.Clear();

            foreach (var bat in PowerNet.batteryComps)
            {
                if (bat == null || bat is CompGreedyBattery)
                    continue;

                if (bat.StoredEnergy <= 0f)
                    continue;

                batteries.Add(bat);
            }

            return batteries;
        }

        public override string CompInspectStringExtra()
        {
            // Don't display the extra inspector info that the battery gives. It just clutters the inspector.
            // Optional, from props.
            if(!DoInspectorInfo)
                return null;

            return base.CompInspectStringExtra();
        }
    }
}