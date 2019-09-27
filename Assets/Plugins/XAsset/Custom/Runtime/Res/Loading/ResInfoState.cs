using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.XAsset
{
    public enum ResInfoState
    {
        Init,
        LoadAssetBundle,
        LoadAsset,
        Loaded,
        Unload,

        Error,
    }
}