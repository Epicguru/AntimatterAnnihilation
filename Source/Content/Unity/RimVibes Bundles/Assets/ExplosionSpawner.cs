using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class ExplosionSpawner : MonoBehaviour
    {
        public GameObject ExplosionPrefab, RailgunEffectPrefab;
        public Vector3 posA, posB;
        public RailgunEffectComp lastRailgun;

        public bool Visible = true;

        private void Start()
        {
            ExplosionEffectManager.Prefab = ExplosionPrefab;
            RailgunEffectManager.Prefab = RailgunEffectPrefab;
        }

        private void Update()
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.y = 0;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ExplosionEffectManager.Spawn(pos, 12);
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                posA = pos;
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                posB = pos;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                lastRailgun = RailgunEffectManager.Spawn(posA, posB);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                lastRailgun?.Fire(12);
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                Visible = !Visible;
            }

            ExplosionEffectManager.Tick(Visible);
            RailgunEffectManager.Tick(Visible);
        }
    }

    static class ExplosionEffectManager
    {
        public static GameObject Prefab;
        private static List<ExplosionEffect> list = new List<ExplosionEffect>();

        public static ExplosionEffect Spawn(Vector3 pos, float size)
        {
            var spawned = Pool<ExplosionEffect>.Get(() => Object.Instantiate(Prefab).AddComponent<ExplosionEffect>());
            Object.DontDestroyOnLoad(spawned.gameObject);
            spawned.Time = 0.7f;
            spawned.Despawn += (s) =>
            {
                list.Remove(s);
            };

            list.Add(spawned);

            spawned.Play(pos, size);

            return spawned;
        }

        public static void Tick(bool vis)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var thing = list[i];
                thing.Tick(vis);
            }
        }
    }

    static class RailgunEffectManager
    {
        public static GameObject Prefab;
        private static List<RailgunEffectComp> list = new List<RailgunEffectComp>();

        public static RailgunEffectComp Spawn(Vector3 start, Vector3 end)
        {
            var spawned = Pool<RailgunEffectComp>.Get(() => Object.Instantiate(Prefab).AddComponent<RailgunEffectComp>());
            Object.DontDestroyOnLoad(spawned.gameObject);
            spawned.Despawn += (s) =>
            {
                list.Remove(s);
            };

            list.Add(spawned);

            spawned.Setup(start, end);

            return spawned;
        }

        public static void Tick(bool vis)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var thing = list[i];
                thing.Tick(vis);
            }
        }
    }
}
