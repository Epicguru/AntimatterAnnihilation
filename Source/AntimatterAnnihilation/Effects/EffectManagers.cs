﻿using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Effects
{
    [StaticConstructorOnStartup]
    static class ExplosionEffectManager
    {
        public static GameObject Prefab;
        private static List<ExplosionEffect> list = new List<ExplosionEffect>();

        public static ExplosionEffect Spawn(Map map, Vector3 pos, float size)
        {
            var spawned = EffectPool<ExplosionEffect>.Get(() => Object.Instantiate(Prefab).AddComponent<ExplosionEffect>());
            Object.DontDestroyOnLoad(spawned.gameObject);
            spawned.Time = 0.7f;
            spawned.Despawn += (s) =>
            {
                list.Remove(s);
            };

            list.Add(spawned);

            spawned.Play(map, pos, size);

            return spawned;
        }

        public static void Tick()
        {
            for (int i = 0; i < list.Count; i++)
            {
                var thing = list[i];
                thing.Tick();
            }
        }
    }

    [StaticConstructorOnStartup]
    static class RailgunEffectSpawner
    {
        public static GameObject Prefab;

        public static RailgunEffectComp Spawn(Map map)
        {
            var spawned = EffectPool<RailgunEffectComp>.Get(() => Object.Instantiate(Prefab).AddComponent<RailgunEffectComp>());
            Object.DontDestroyOnLoad(spawned.gameObject);
            spawned.Setup(map);

            return spawned;
        }
    }
}
