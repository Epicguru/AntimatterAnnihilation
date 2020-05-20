using UnityEngine;
using Verse;
using Object = UnityEngine.Object;

namespace AntimatterAnnihilation.Effects
{
    public class EnergyBeam : SpawnedEffect
    {
        public Vector3 Position;
        public bool BeamVisible;
        public float Length = 1f;
        public float Rotation;
        public float RadiusScale = 1f;
        public float BeamExtendPerSecond = 60;
        public float RadiusChangeSpeed = 1f;
        public float LengthOffset;

        private float currentRadiusScale;
        private float currentLength;
        private Transform beam;

        public EnergyBeam(Map map, Vector3 pos, float rot, bool isInBeam) : base(map)
        {
            beam = Object.Instantiate(isInBeam ? Content.EnergyBeamInPrefab : Content.EnergyBeamOutPrefab).transform;
            beam.position = pos;
            beam.localScale = Vector3.zero;
            beam.eulerAngles = new Vector3(90f, rot, 0f);

            this.Rotation = rot;
            this.Position = pos;

            base.AddRenderers(beam);
        }

        public override void Tick()
        {
            base.Tick();

            if (beam == null)
                return;

            const float DT = 1f / 60f;

            if (BeamVisible)
            {
                float tl = Length + LengthOffset;
                if (currentLength != tl)
                {
                    bool smaller = currentLength < tl;
                    currentLength += BeamExtendPerSecond * DT * (smaller ? 1f : -1f);
                    if (!smaller)
                        currentLength = tl;

                    if (smaller && currentLength > tl)
                        currentLength = tl;
                    if (!smaller && currentLength < tl)
                        currentLength = tl;
                }
                if(currentRadiusScale != RadiusScale)
                {
                    bool smaller = currentRadiusScale < RadiusScale;
                    currentRadiusScale += RadiusChangeSpeed * DT * (smaller ? 1f : -1f);
                    if (smaller && currentRadiusScale > RadiusScale)
                        currentRadiusScale = RadiusScale;
                    if (!smaller && currentRadiusScale < RadiusScale)
                        currentRadiusScale = RadiusScale;
                }
            }
            else
            {
                if (currentRadiusScale > 0)
                {
                    currentRadiusScale -= DT * RadiusChangeSpeed;
                    if (currentRadiusScale < 0)
                    {
                        currentRadiusScale = 0f;
                        currentLength = 0;
                    }
                }
            }

            beam.position = Position;
            beam.eulerAngles = new Vector3(90f, Rotation, 0f);
            beam.localScale = new Vector3(currentRadiusScale, currentLength, currentRadiusScale);
        }

        public override void Show()
        {
            beam.gameObject.SetActive(true);
        }

        public override void Hide()
        {
            beam.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            if (beam == null)
                return;

            Object.Destroy(beam.gameObject);
            beam = null;
        }
    }
}
