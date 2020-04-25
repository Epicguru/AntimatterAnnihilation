using System;
using Verse;

namespace AntimatterAnnihilation.ThingComps
{
    public class CompSignalHook : ThingComp
    {
        public event Action<string> OnSignal;

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);

            OnSignal?.Invoke(signal);
        }
    }
}
