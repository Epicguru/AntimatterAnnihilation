using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public abstract class Building_TrayPuller : Building
    {
        public int TryPullFromTray(Building_InputTray tray, string defName, int amount)
        {
            if (amount <= 0)
                return 0;
            if (tray == null)
                return 0;
            if (defName == null)
                return 0;

            var removed = tray.TryPull(defName, amount);
            if (removed > 0)
            {
                return removed;
            }
            return 0;
        }

        public bool TrayHasItem(Building_InputTray tray, string defName, int minAmount)
        {
            if (tray == null)
                return false;
            if (defName == null)
                return false;

            return tray.HasItem(defName, minAmount);
        }

        public virtual Building_InputTray GetTray(IntVec3 offset)
        {
            var thing = Map?.thingGrid.ThingAt(base.Position + offset, ThingCategory.Building);

            return thing as Building_InputTray;
        }
    }
}
