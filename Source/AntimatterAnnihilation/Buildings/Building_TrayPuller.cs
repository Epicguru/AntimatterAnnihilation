using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public abstract class Building_TrayPuller : Building
    {
        public void TryGet(Building_InputTray tray, string defName, int amount, ref int counter)
        {
            if (amount <= 0)
                return;
            if (tray == null)
                return;
            if (defName == null)
                return;

            var removed = tray.TryRemove(defName, amount);
            if (removed > 0)
            {
                counter += removed;
            }
        }

        public virtual Building_InputTray GetTray(IntVec3 offset)
        {
            var thing = Map?.thingGrid.ThingAt(base.Position + offset, ThingCategory.Building);

            return thing as Building_InputTray;
        }
    }
}
