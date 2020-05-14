using Verse;

namespace AntimatterAnnihilation.Damage
{
    public class DamageWorker_GalvaPain : DamageWorker_AddInjury
    {
        protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
        {
            return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside, null);
        }
    }
}
