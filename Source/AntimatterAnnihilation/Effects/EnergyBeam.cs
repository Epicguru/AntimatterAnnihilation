using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AntimatterAnnihilation.Effects
{
    public class EnergyBeam : IDisposable
    {
        public Vector3 Position;
        public bool Visible;
        public float Length = 1f;
        public float Angle;
        public float RadiusScale = 1f;
        public float BeamExtendPerSecond = 60;
        public float RadiusChangeSpeed = 1f;
        public float LengthOffset;

        private float currentRadiusScale;
        private float currentLength;
        private Transform beam;

        public EnergyBeam(Vector3 pos, float rot)
        {
            beam = Object.Instantiate(Content.EnergyBeamPrefab).transform;
            beam.position = pos;
            beam.localScale = Vector3.zero;
            beam.eulerAngles = new Vector3(90f, rot, 0f);

            this.Angle = rot;
            this.Position = pos;
        }

        public void Tick()
        {
            if (beam == null)
                return;

            const float DT = 1f / 60f;

            if (Visible)
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

            beam.localScale = new Vector3(currentRadiusScale, currentLength, currentRadiusScale);
        }

        public void Dispose()
        {
            if (beam == null)
                return;

            Object.Destroy(beam.gameObject);
            beam = null;
        }
    }
}
