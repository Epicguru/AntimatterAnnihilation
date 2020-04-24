using UnityEngine;
using Verse;

namespace AntimatterAnnihilation
{
    public static class BundleFinder
    {
        public static AssetBundle GetByName(this ModAssetBundlesHandler h, string name)
        {
            foreach (var bundle in h.loadedAssetBundles)
            {
                if (bundle.name == name)
                    return bundle;
            }

            return null;
        }
    }
}
