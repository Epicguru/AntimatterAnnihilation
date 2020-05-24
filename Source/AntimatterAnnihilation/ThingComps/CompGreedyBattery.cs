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
                return this.realMax;
            }
            set
            {
                if (value < 0)
                    value = 0;

                this.realMax = value;
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
        public int PullBatteriesCount { get; private set; }
        public string ReasonNoPull { get; private set; }
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

        internal float realMax;

        public override void Initialize(CompProperties p)
        {
            base.Initialize(p);

            MaxStoredEnergy = RealProps.initialEnergyMax;
        }

        public override void CompTick()
        {
            if (DoSelfDischarge)
                base.CompTick(); // Only does the discharge.

            IsPulling = false;
            PullBatteriesCount = 0;
            ReasonNoPull = null;

            if (PowerNet?.batteryComps == null)
            {
                ReasonNoPull = PowerNet == null ? "Null power net" : "Null battery comps in power net.";
                return;
            }

            if (StoredEnergyPct == 1f || MaxStoredEnergy <= 0f)
            {
                ReasonNoPull = $"Full on energy {StoredEnergyPct}, {MaxStoredEnergy}";
                return; // No need to pull anything.
            }

            var bats = GetPullableBatteries();

            if (bats.Count == 0)
            {
                ReasonNoPull = "No valid batteries in net.";
                return;
            }

            PullBatteriesCount = bats.Count;
            float toPull = MaxPull * CompPower.WattsToWattDaysPerTick;
            if (toPull > AmountCanAccept)
                toPull = AmountCanAccept;
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
                if (pulled > toPull)
                {
                    //Log.Warning($"GreedyBattery error: pulled more than was calculated! Wanted {toPull}, ended with {pulled}.");
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

            ReasonNoPull = $"Pulling because {StoredEnergy} < {MaxStoredEnergy}";

            bats.Clear();
        }

        public void SetStoredEnergy(float wattDays)
        {
            // Uses reflection so it's quite slow. Maybe use Harmony? Is that faster?

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

                if (!bat.Props.transmitsPower)
                    continue;

                batteries.Add(bat);
            }

            return batteries;
        }

        public override void PostExposeData()
        {
            // Have to do this hacky shit so that the max energy value doesn't limit the stored energy.
            Props.storedEnergyMax = float.MaxValue;
            base.PostExposeData();
            Props.storedEnergyMax = 600;

            // Store 'real max' value.
            Scribe_Values.Look(ref this.realMax, "realMaxEnergy");
        }

        public override string CompInspectStringExtra()
        {
            string devString = $"Stored: {StoredEnergy:F1} / {MaxStoredEnergy:F0}, pulling from {PullBatteriesCount} ({ReasonNoPull})";
            // Don't display the extra inspector info that the battery gives. It just clutters the inspector.
            // Optional, from props.
            if (!DoInspectorInfo)
            {
                if (Prefs.DevMode)
                    return devString;
                return null;
            }

            CompProperties_Battery p = this.Props;
            string text = devString + '\n' + "PowerBatteryStored".Translate() + ": " + StoredEnergy.ToString("F0") + " / " + MaxStoredEnergy.ToString("F0") + " Wd";
            text += "\n" + "PowerBatteryEfficiency".Translate() + ": " + (p.efficiency * 100f).ToString("F0") + "%";
            if (this.StoredEnergy > 0f)
            {
                text += "\n" + "SelfDischarging".Translate() + ": " + 5f.ToString("F0") + " W";
            }
            return text + "\n" + GetBaseInspectString();
        }

        private string GetBaseInspectString()
        {
            if (this.PowerNet == null)
            {
                return "PowerNotConnected".Translate();
            }
            string value = (this.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick).ToString("F0");
            string value2 = this.PowerNet.CurrentStoredEnergy().ToString("F0");
            return "PowerConnectedRateStored".Translate(value, value2);
        }
    }
}