using AntimatterAnnihilation.ThingComps;
using System.Collections.Generic;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_AutoCrafter : Building
    {
        private static List<CompRefuelableSpecial> tempComps = new List<CompRefuelableSpecial>();

        public int ShuffleInterval = 0;
        public CompRefuelableSpecial[] RefuelComps { get; protected set; }

        private long tick;

        public Building_AutoCrafter()
        {
            
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (RefuelComps != null)
                return;

            tempComps.Clear();

            foreach (var comp in base.AllComps)
            {
                if (comp is CompRefuelableSpecial s)
                    tempComps.Add(s);
            }

            RefuelComps = new CompRefuelableSpecial[tempComps.Count];
            for (int i = 0; i < RefuelComps.Length; i++)
            {
                RefuelComps[i] = tempComps[i];
                if (RefuelComps[i].Id == 0)
                {
                    Log.Error($"A CompRefuelableSpecial on this {LabelCap} has default 'id' value of 0. Please change it to a unique ID.");
                }
            }

            tempComps.Clear();
        }

        public virtual bool MeetsRefuelCondition(CompRefuelableSpecial s)
        {
            return s.FuelPercentOfMax < 0.99999f;
        }

        public virtual float GetFuelPriority(CompRefuelableSpecial s)
        {
            return s.FuelPriority;
        }

        public CompRefuelableSpecial GetFuelComp(int id)
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

            CompRefuelableSpecial toPutFirst = null;
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
    }
}
