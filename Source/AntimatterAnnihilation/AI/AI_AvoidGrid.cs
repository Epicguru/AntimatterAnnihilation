using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AntimatterAnnihilation.AI
{
    /// <summary>
    /// Note that this class doesn't write any data to avoid breaking game saves if the mod is removed.
    /// This results in slower grid generation times.
    /// </summary>
    public class AI_AvoidGrid : MapComponent
    {
        public const byte WEIGHT_INJECTOR_BEAM = 250;
        public const byte WEIGHT_ENERGY_BEAM = 255;

        [TweakValue("AntimatterAnnihilation")]
        public static bool DoNonDestructiveAvoidance = true;

        public static void DoAvoidGrid(Pawn pawn, ref ByteGrid grid)
        {
            bool shouldApply = pawn.Spawned && !pawn.Dead && pawn.Faction != null && pawn.Faction.def.canUseAvoidGrid && ShouldFactionAvoid(pawn.Faction);
            if (!shouldApply)
                return;

            var mapGrid = Ensure(pawn.Map);
            if (grid == null)
            {
                grid = mapGrid.AvoidGrid;
            }
            else
            {
                // This pawn already has an avoid grid - this normally doesn't happen for player-controlled pawns (except if a mod adds this functionality).
                // Also for special job/lord cases, I think.
                // It's fairly safe to assume that if a player pawn has an avoid grid, it was caused by another mod. Therefore, I want to enable maximum compatibility
                // by adding an option to have 'additive' avoidance grid creation, but it's also very slow.

                if (DoNonDestructiveAvoidance)
                {
                    // This is slow...
                    mapGrid.AddNonDestructive(grid);
                }
                else
                {
                    Log.ErrorOnce($"Pawn {pawn} from {pawn.Faction} has an avoidance grid, but {nameof(DoNonDestructiveAvoidance)} is false. This is probably caused by another mod adding custom avoidance. Enable {nameof(DoNonDestructiveAvoidance)} to fix this, but it will affect performace quite heavily.", 891723879);
                    grid = mapGrid.AvoidGrid;
                }
            }
        }

        private static bool ShouldFactionAvoid(Faction f)
        {
            // All factions currently avoid the laser beams.
            // Because they are very obvious and glow and shit. So everyone can see them.
            return true;
        }

        public static AI_AvoidGrid Ensure(Map m)
        {
            var current = m.GetComponent<AI_AvoidGrid>();
            if(current != null)
            {
                return current;
            }
            else
            {
                var newComp = new AI_AvoidGrid(m);
                m.components.Add(newComp);
                return newComp;
            }
        }

        public int RebuildInterval = 30; // Twice per second at normal speed;
        public ByteGrid AvoidGrid;
        public List<IAvoidanceProvider> AvoidanceProviders = new List<IAvoidanceProvider>();

        private ulong tick;

        public AI_AvoidGrid(Map map) : base(map)
        {
            AvoidGrid = new ByteGrid(map);
        }

        public override void MapComponentTick()
        {
            tick++;
            if(RebuildInterval <= 0 || tick % (ulong)RebuildInterval == 0)
            {
                Rebuild();
            }
        }

        public void Rebuild()
        {
            AvoidGrid.Clear(0);

            foreach (var thing in AvoidanceProviders)
            {
                if (thing == null)
                    continue;

                foreach (var data in thing.GetAvoidance(this.map))
                {
                    byte current = AvoidGrid[data.cell];
                    byte proposed = data.weight;
                    if(proposed > current)
                    {
                        AvoidGrid[data.cell] = proposed;
                    }
                }
            }
        }

        public void AddNonDestructive(ByteGrid existing)
        {
            // Take the existing grid and add on our grid if our values are higher.
            // This is very slow if done for every pawn on the map, considering that it runs through every single cell on the map.

            // Therefore it will be a compatibility option, enabled by default.

            if (!existing.MapSizeMatches(this.map))
            {
                Log.ErrorOnce("AvoidGridNonDestructive error: existing and map grid size do not match. Probably caused by another mod doing fucky stuff.", 8712316);
                return;
            }

            for(int i = 0; i < this.AvoidGrid.CellsCount; i++)
            {
                var own = this.AvoidGrid[i];
                var other = existing[i];

                if (own > other)
                    existing[i] = own;
            }
        }
    }
}
