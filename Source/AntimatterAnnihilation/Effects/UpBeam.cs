using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Effects
{
    public class UpBeam : SpawnedEffect
    {
        public int FadeTicks = 25;
        public bool IsActive;

        private Transform goTrs;
        private Transform beam;
        private Transform mote;
        private int tickTimer;

        public UpBeam(Map map, Vector3 pos) : base(map)
        {
            goTrs = Object.Instantiate(Content.UpBeamPrefab).transform;
            goTrs.position = pos;
            goTrs.localScale = Vector3.zero;
            goTrs.eulerAngles = new Vector3(90f, 0f, 0f);

            beam = goTrs.Find("Beam");
            mote = goTrs.Find("Mote");

            SetVisScale(0f);
            base.AddRenderers(beam);
        }

        public override void Tick()
        {
            base.Tick();

            if (IsActive)
                tickTimer++;
            else
                tickTimer--;
            tickTimer = Mathf.Clamp(tickTimer, 0, FadeTicks);

            SetVisScale(tickTimer / (float)FadeTicks);
        }

        private void SetVisScale(float p)
        {
            p = Mathf.Clamp01(p);

            const float BEAM_MAX_HEIGHT = 250f;
            const float BEAM_MAX_WIDTH = 0.25f;
            const float MOTE_MAX_SCALE = 3.520297f;

            float moteScale = MOTE_MAX_SCALE * p;
            float beamHeight = BEAM_MAX_HEIGHT * p;
            float beamWidth = BEAM_MAX_WIDTH * p;

            mote.localScale = Vector3.one * moteScale;
            beam.localScale = new Vector3(beamWidth, beamWidth, beamHeight);
        }

        public override void Show()
        {
            goTrs.gameObject.SetActive(true);
        }

        public override void Hide()
        {
            goTrs.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            if (goTrs == null)
                return;

            Object.Destroy(goTrs.gameObject);
            goTrs = null;
            mote = null;
            beam = null;
        }
    }
}
