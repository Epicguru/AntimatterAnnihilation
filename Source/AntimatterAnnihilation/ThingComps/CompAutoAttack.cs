using System.Collections.Generic;
using Verse;

namespace AntimatterAnnihilation.ThingComps
{
    /// <summary>
    /// Used to set a building's target charge (power consumption) rate.
    /// </summary>
    [StaticConstructorOnStartup]
    public class CompAutoAttack : ThingComp
    {
        public CompProperties_AutoAttack Props
        {
            get
            {
                return (CompProperties_AutoAttack)props;
            }
        }

        public bool DefaultAutoAttack
        {
            get
            {
                return Props.defaultAutoAttack;
            }
        }

        public bool AutoAttackEnabled
        {
            get
            {
                return doAutoAttack;
            }
            set
            {
                doAutoAttack = value;
            }
        }

        private bool doAutoAttack;

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref doAutoAttack, "doAutoAttack");
        }

        public override void Initialize(CompProperties p)
        {
            base.Initialize(p);

            this.doAutoAttack = DefaultAutoAttack;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var cmd = new Command_Toggle();
            cmd.toggleAction = () =>
            {
                doAutoAttack = !doAutoAttack;
            };
            cmd.isActive = () => doAutoAttack;
            cmd.defaultLabel = "Toggle Auto Attack";
            cmd.defaultDesc = $"Toggle whether this {parent.LabelShort} will automatically attack enemies";
            cmd.icon = Content.AutoAttackIcon;

            yield return cmd;
        }
    }
}