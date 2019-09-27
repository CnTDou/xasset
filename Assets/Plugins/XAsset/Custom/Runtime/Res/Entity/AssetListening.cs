using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.XAsset
{
    public class AssetListening : MonoBehaviour
    {
        Dictionary<string, IResInfo> assetDic = new Dictionary<string, IResInfo>();
        private void OnDestroy()
        {
            foreach (var item in assetDic)
            {
                item.Value.Dequire(gameObject);
            }
            assetDic.Clear();
        }

        public static void Add(GameObject go, IResInfo res)
        {
            string key=res.Name;
            AssetListening resListening = go.GetComponent<AssetListening>();
            if (!resListening)
            {
                resListening = go.AddComponent<AssetListening>(); 
            } 
            if (resListening.assetDic.ContainsKey(key))
            { // todo: 单个对象存在多个 asset
                if (resListening.assetDic[key] != res)
                {
                    Debug.LogError("Add 资源监听错误 当前 key一致但是实例对象不一致.");
                }
                return;
            }
            resListening.assetDic.Add(key,res); 
            res.Require(go);
        }
    }
}
