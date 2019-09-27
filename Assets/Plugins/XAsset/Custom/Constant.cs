using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.XAsset
{
    public static class Constnat
    {
        public const string AssetRoot = "Assets/Resource/";
        public const string SettingPath = AssetRoot + "Settings.asset";
        public const string ManifestRulePath = AssetRoot + "ManifestRule.asset";


        public const string RES_ROOT_PATH = Constnat.AssetRoot;
        public const string RES_PREFABS_PATH = "Prefabs/";
        public const string RES_SCENES_PATH = RES_ROOT_PATH + "Scenes/";
        public const string RES_TILED2UNITY_PREFAB_PATH = "Assets/Tiled2Unity/Prefabs/";

        public const string DEFAULT_CACHE = "default";
    }
}

