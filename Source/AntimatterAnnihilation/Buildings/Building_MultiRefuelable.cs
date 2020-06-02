using AntimatterAnnihilation.ThingComps;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public abstract class Building_MultiRefuelable : Building
    {
        private static List<CompRefuelableMulti> tempComps = new List<CompRefuelableMulti>();

        public int ShuffleInterval = 5;
        public bool AllowAutoRefuelToggle = true;
        public CompRefuelableMulti[] RefuelComps { get; protected set; }
        public bool MissingComps { get; private set; }

        private long tick;

        public Building_MultiRefuelable()
        {
            
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            if (RefuelComps != null)
                return;

            tempComps.Clear();

            foreach (var comp in base.AllComps)
            {
                if (comp is CompRefuelableMulti s)
                    tempComps.Add(s);
            }

            RefuelComps = new CompRefuelableMulti[tempComps.Count];
            for (int i = 0; i < RefuelComps.Length; i++)
            {
                RefuelComps[i] = tempComps[i];
                if (RefuelComps[i].Id == 0)
                {
                    Log.Error($"A CompRefuelableSpecial on this {LabelCap} has default 'id' value of 0. Please change it to a unique ID.");
                }
            }

            tempComps.Clear();

            Log.Message($"RefuelComps: {RefuelComps.Length}");
            if (RefuelComps == null || RefuelComps.Length == 0)
            {
                Log.Error($"Missing refuel comps on this {this.LabelCap}.");
                MissingComps = true;
            }

            base.SpawnSetup(map, respawningAfterLoad);
        }

        public virtual bool MeetsRefuelCondition(CompRefuelableMulti s)
        {
            return s.FuelPercentOfMax < 0.99999f;
        }

        public virtual float GetFuelPriority(CompRefuelableMulti s)
        {
            return s.FuelPriority;
        }

        public CompRefuelableMulti GetFuelComp(int id)
        {
            foreach (var comp in RefuelComps)
            {
                if (comp.Id == id)
                    return comp;
            }
            return null;
        }

        public override void Tick()
        {
            base.Tick();

            tick++;

            // Re-shuffle the components to trick rimworld into accepting multiple refuelable comps.
            bool shouldShuffle = ShuffleInterval <= 0 || tick % ShuffleInterval == 0;
            if (!shouldShuffle)
                return;
            if (RefuelComps == null)
                return;

            CompRefuelableMulti toPutFirst = null;
            float lowest = float.MaxValue;
            foreach (var s in RefuelComps)
            {
                float priority = GetFuelPriority(s);

                if (priority >= lowest)
                    continue;

                if (MeetsRefuelCondition(s))
                {
                    lowest = priority;
                    toPutFirst = s;
                }
            }

            if (toPutFirst == null)
                return;

            int targetPos = 0;
            for (int i = 0; i < AllComps.Count; i++)
            {
                var c = AllComps[i];

                if (c == toPutFirst)
                    return; // It is already in first place.

                if (c is CompRefuelableConditional)
                {
                    // Needs to go before other refuel comps.
                    targetPos = i;
                    break;
                }
            }

            base.AllComps.Remove(toPutFirst);
            base.AllComps.Insert(targetPos, toPutFirst); // This makes rimworld treat this as 'THE' refuelable comp, even though there are others.
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
                yield return g;

            if (RefuelComps == null || RefuelComps.Length == 0)
                yield break;

            if (AllowAutoRefuelToggle)
            {
                bool areEnabled = RefuelComps[0].allowAutoRefuel;
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandToggleAllowAutoRefuel".Translate(),
                    defaultDesc = "CommandToggleAllowAutoRefuelDesc".Translate(),
                    hotKey = KeyBindingDefOf.Command_ItemForbid,
                    icon = areEnabled ? TexCommand.ForbidOff : TexCommand.ForbidOn,
                    isActive = () => areEnabled,
                    toggleAction = () =>
                    {
                        foreach (var comp in RefuelComps)
                        {
                            comp.allowAutoRefuel = !comp.allowAutoRefuel;
                        }
                    }
                };
            }
        }
    }
}
