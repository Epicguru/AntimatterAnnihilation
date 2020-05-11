namespace AntimatterAnnihilation.Utils
{
    public class RecoilManager
    {
        public enum AddMode
        {
            Add,
            Set
        }

        public float CurrentRecoil { get; private set; }
        public float RecoveryMultiplier { get; set; } = 0.86f;
        public float VelocityMultiplier { get; set; } = 0.8f;

        public float CurrentVelocity { get; private set; }

        public void AddRecoil(float amount, AddMode mode = AddMode.Add)
        {
            switch (mode)
            {
                case AddMode.Add:
                    CurrentVelocity += amount;
                    break;
                case AddMode.Set:
                    CurrentVelocity = amount;
                    break;
            }
        }

        public void Tick()
        {
            const float DT = 1f / 60f;

            CurrentVelocity *= VelocityMultiplier;
            CurrentRecoil += CurrentVelocity * DT;
            CurrentRecoil *= RecoveryMultiplier;
        }
    }
}
