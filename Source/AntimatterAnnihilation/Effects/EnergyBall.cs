using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AntimatterAnnihilation.Effects
{
    public class EnergyBall : IDisposable
    {
        private const float LERP_TIME = 0.5f;
        private const float MAX_SCALE = 4.1181f;

        public float SizeScale = 1f;
        public bool Visible;

        private Transform ball;
        private float lerp;
        private AnimationCurve lerpCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.8f, 1.2f), new Keyframe(1f, 1f));

        public EnergyBall(Vector3 pos, float rot)
        {
            var spawned = Object.Instantiate(Content.EnergyBallPrefab);
            ball = spawned.transform;
            ball.position = pos;
            ball.localEulerAngles = new Vector3(0f, rot, 90f);
            ball.localScale = Vector3.zero;
        }

        public void Tick()
        {
            if (ball == null)
                return;

            const float DT = 1f / 60f;

            lerp += DT / LERP_TIME * (Visible ? 1f : -1f);
            lerp = Mathf.Clamp01(lerp);
            float t = lerpCurve.Evaluate(lerp);
            float scale = MAX_SCALE * t * SizeScale;

            ball.localScale = new Vector3(scale, scale, scale);
        }

        public void Dispose()
        {
            if (ball == null)
                return;

            Object.Destroy(ball.gameObject);
            ball = null;
        }
    }
}
