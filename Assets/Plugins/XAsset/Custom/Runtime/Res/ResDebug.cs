using Plugins.XAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using State = ResMgr.State;

public class ResDebug : MonoBehaviour
{

    private ResMgr Res
    {
        get
        {
            return ResMgr.Instance;
        }
    }
    string message, assetPath;

    int curIndex;
    int current, _max = 8;
    bool isCheck;

    private void OnGUI()
    {
        if (Res.isWindow)
        {
            using (var v = new GUILayout.VerticalScope("RES", "window"))
            {
                switch (Res.GetState)
                {
                    case State.Wait:
                        if (GUILayout.Button("Init"))
                        {
                            Res.Init(() => { message = " ready ."; }, (err) => { message = err; });
                        }
                        isCheck = true;
                        break;
                    case State.Completed:
                        isCheck = true;
                        if (GUILayout.Button("Clear"))
                        {
                            Res.Clear();
                        }
                        break;
                    default:
                        isCheck = false;
                        break;
                }

                if (isCheck && GUILayout.Button("Check"))
                {
                    Res.CheckVersion((vinfo) =>
                    {
                        if (vinfo.IsUpdate)
                        {
                            Res.StartUpdateRes(null, (err) => { message = err; }, null);
                        }
                    }, (err) => { message = err; });
                }

                GUILayout.Label(string.Format("{0}:{1}", Res.GetState, message));
                if (Res.GetState == State.Completed)
                {
                    int maxIndex = current + _max;
                    maxIndex = maxIndex >= Assets.bundleAssets.Count ? Assets.bundleAssets.Count : maxIndex;
                    GUILayout.Label(string.Format("AllBundleAssets , allCount : {0} , minIndex: {1} , maxIndex: {2}",
                        Assets.bundleAssets.Count, current, maxIndex));

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("<<", GUILayout.MaxWidth(80)))
                        {
                            current -= _max;
                            if (current <= 0)
                            {
                                current = 0;
                            }
                        }
                        if (GUILayout.Button(">>", GUILayout.MaxWidth(80)))
                        {
                            current += _max;
                            if (current >= Assets.bundleAssets.Count)
                            {
                                current -= _max;
                            }
                            if (current < 0)
                            {
                                current = 0;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();

                    curIndex = 0;
                    foreach (var item in Assets.bundleAssets)
                    {
                        curIndex++;
                        if (curIndex > current && curIndex < (current + _max))
                        {
                            if (GUILayout.Button(item.Key))
                            {
                                assetPath = item.Key;
                            }
                        }
                    }

                    using (var h = new GUILayout.HorizontalScope())
                    {
                        assetPath = GUILayout.TextField(assetPath, GUILayout.Width(256));
                        if (GUILayout.Button("Load"))
                        {
                            var asset = Assets.Load(assetPath, typeof(UnityEngine.Object));
                            asset.completed += OnAssetLoaded;
                        }

                        if (GUILayout.Button("LoadAsync"))
                        {
                            var asset = Assets.LoadAsync(assetPath, typeof(UnityEngine.Object));
                            asset.completed += OnAssetLoaded;
                        }

                        if (GUILayout.Button("LoadScene"))
                        {
                            var asset = Assets.LoadScene(assetPath, true, true);
                            asset.completed += OnAssetLoaded;
                        }
                    }

                    if (loadedAssets.Count > 0)
                    {
                        if (GUILayout.Button("UnloadAll"))
                        {
                            for (int i = 0; i < loadedAssets.Count; i++)
                            {
                                var item = loadedAssets[i];
                                item.Release();
                            }

                            loadedAssets.Clear();
                        }

                        for (int i = 0; i < loadedAssets.Count; i++)
                        {
                            var item = loadedAssets[i];
                            using (var h = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label(item.name);
                                if (GUILayout.Button("Unload"))
                                {
                                    item.Release();
                                    loadedAssets.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    List<Asset> loadedAssets = new List<Asset>();

    void OnAssetLoaded(Asset asset)
    {
        if (asset.name.EndsWith(".prefab", StringComparison.CurrentCulture))
        {
            var go = Instantiate(asset.asset);
            go.name = asset.asset.name;
            asset.Require(go);
            Destroy(go, 3);
        }
        loadedAssets.Add(asset);
    }

}
