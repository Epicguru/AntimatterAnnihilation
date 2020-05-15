using System;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_InputTray : Building_Storage
    {
        private static List<Thing> bin = new List<Thing>();

        public int TryRemove(string thingDefName, int count)
        {
            if (count <= 0)
                return 0;

            bin.Clear();
            int found = 0;
            foreach (var thing in base.slotGroup.HeldThings)
            {
                if(thing.def.defName == thingDefName && !thing.Destroyed)
                {
                    bin.Add(thing);
                    found += thing.stackCount;
                    if (found >= count)
                        break;
                }
            }

            int amountRemoved = 0;
            int toDelete = count;

            foreach (var thing in bin)
            {
                try
                {
                    if (thing.stackCount <= toDelete)
                    {
                        toDelete -= thing.stackCount;
                        amountRemoved += thing.stackCount;
                        thing.Destroy(DestroyMode.Vanish);
                    }
                    else
                    {
                        thing.stackCount -= toDelete;
                        amountRemoved += toDelete;
                    }
                    
                }
                catch(Exception e)
                {
                    Log.ErrorOnce($"Failed to destroy item stored in InputTray: {e}", 908123);
                }
            }
            bin.Clear();

            return amountRemoved;
        }
    }
}
