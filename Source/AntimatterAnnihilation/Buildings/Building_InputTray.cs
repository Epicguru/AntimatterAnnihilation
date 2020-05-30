using System;
using RimWorld;
using System.Collections.Generic;
using Verse;
using AntimatterAnnihilation.Utils;

namespace AntimatterAnnihilation.Buildings
{
    public class Building_InputTray : Building_Storage
    {
        private static List<Thing> bin = new List<Thing>();

        public bool HasItem(string thingDefName, int minCount)
        {
            if (thingDefName == null)
                return false;

            foreach (var thing in base.slotGroup.HeldThings)
            {
                if (thing.def.defName == thingDefName && !thing.Destroyed)
                {
                    if (thing.stackCount >= minCount)
                        return true;
                }
            }
            return false;
        }

        public int TryPull(string thingDefName, int count)
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

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            var s = GetStoreSettings();
            s.filter.allowedHitPointsConfigurable = false;
            s.filter.allowedQualitiesConfigurable = false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
                yield return g;

            // For composite refiner
            yield return GizmoFor(AADefOf.Plasteel);
            yield return GizmoFor(AADefOf.AntimatterCanister_AA);

            // For alloy machine.
            yield return GizmoFor(AADefOf.AntimatterComposite_AA);
            yield return GizmoFor(AADefOf.Gold);
            yield return GizmoFor(AADefOf.Uranium);
            yield return GizmoFor(AADefOf.HyperAlloy_AA, StoragePriority.Low);
        }

        private Command_Action GizmoFor(ThingDef item, StoragePriority p = StoragePriority.Important)
        {
            var cmd = new Command_Action();
            cmd.icon = item.uiIcon;
            cmd.defaultLabel = "AA.SetTrayFor".Translate(item.LabelCap);
            cmd.defaultDesc = "AA.SetTrayForDesc".Translate(item.LabelCap);
            cmd.action = () =>
            {
                SetSettingsTo(item, p);
            };

            return cmd;
        }

        public void SetSettingsTo(ThingDef item, StoragePriority p = StoragePriority.Important)
        {
            var s = this.GetStoreSettings();
            s.filter.SetDisallowAll();
            s.filter.SetAllow(item, true);
            s.Priority = p;
        }

        public override string GetInspectString()
        {
            var s = GetStoreSettings();
            int count = s.filter.AllowedDefCount;

            string stored = count == 0 ? (string)"AA.TrayStoredNothing".Translate() : count == 1 ? s.filter.Summary.CapitalizeFirst() : (string)"AA.TrayStoredMultiple".Translate();
            return "AA.TrayCurrentlyStored".Translate(stored);
        }
    }
}
