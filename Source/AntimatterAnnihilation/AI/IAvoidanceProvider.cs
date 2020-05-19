using System.Collections.Generic;
using Verse;

namespace AntimatterAnnihilation.AI
{
    public interface IAvoidanceProvider
    {
        IEnumerable<(IntVec3 cell, byte weight)> GetAvoidance(Map map);
    }
}
