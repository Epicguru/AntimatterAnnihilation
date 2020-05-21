using RimWorld;
using Verse;
using Verse.Sound;

namespace AntimatterAnnihilation.ThingComps
{
    /// <summary>
    /// Used to set a building's target charge (power consumption) rate.
    /// </summary>
    [StaticConstructorOnStartup]
    public class CompExplosiveCustom : CompExplosive
    {
        private bool hasPlayedAudio;

        public CompProperties_ExplosiveCustom RealProps
        {
            get
            {
                return (CompProperties_ExplosiveCustom)props;
            }
        }

        public override void CompTick()
        {
            if (base.wickStarted && wickTicksLeft <= RealProps.wickTicksToPlayAudio && !hasPlayedAudio)
            {
                hasPlayedAudio = true;
                PlayAudio();
            }
            base.CompTick();
        }

        public void PlayAudio()
        {
            RealProps.customExplodeAudio?.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref hasPlayedAudio, "hasPlayedAudio");
        }

        /// <summary>
        /// Sometimes the mine detonates 'unexpectedly', such as when it is instantly destroyed by a weapon or other explosion.
        /// This sets off the mine but doesn't do the wick timer, so the custom audio is not played.
        /// To fix this, this method is called when detonation occurs (through Harmony patch), and if the audio has not already been played then it is played.
        /// This often ruins the 'perfect' wick-audio sync, but playing a sound out-of-syc is better than not playing it at all.
        /// </summary>
        internal void OnPostDetonated()
        {
            if (hasPlayedAudio)
                return;

            hasPlayedAudio = true;
            PlayAudio();
        }
    }
}