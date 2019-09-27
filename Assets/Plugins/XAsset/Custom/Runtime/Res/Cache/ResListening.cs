using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.XAsset
{
    public class ResListening : MonoBehaviour
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
            if (string.IsNullOrEmpty(res.Name))
            {
                Util.Log("res error ResListening, res.Name: {0}, res.Asset: {1}.", res.Name, res.Asset);
                return;
            }
            ResListening resListening = go.GetComponent<ResListening>();
            if (!resListening)
            {
                resListening = go.AddComponent<ResListening>();
                resListening.assetDic[res.Name] = res;
                res.Require(go);
            }
        }
    }
}
