using System;
using System.Collections;
using System.Collections.Generic;
using Plugins.XAsset;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using LoadType = Plugins.XAsset.LoadType;
using UtilText = GameFramework.Utility.Text;

public class CustomTest : MonoBehaviour
{
    public bool isUpdate = true;
    private void Start()
    {
        if (isUpdate)
        {
            ResMgr.Instance.Init(() =>
            {
                ResMgr.Instance.CheckVersion(OnCheckVersionSucceed, OnErr);
            }, OnErr);
        }
        else
        {
            ResMgr.Instance.Init(EnterMain, OnErr);
        }
    }

    private void OnCheckVersionSucceed(VersionInfo versionInfo)
    {
        if (versionInfo.IsUpdate)
        {
            Debug.LogFormat("需要更新:  Count: {0}, Size:{1}.", versionInfo.updateCount, versionInfo.totalUpdateLength);
            ResMgr.Instance.StartUpdateRes(EnterMain, OnErr, OnUpdating);
        }
        else
        {
            Debug.Log("不需要更新. 即可开始");
            EnterMain();
        }
    }

    private void OnUpdating(UpdatingInfo info)
    {
        string message = "更新中 : \r\n";
        message += string.Format("Count: {0}/{1} ", info.TotalUpdateSuccessCount, info.TotalUpdateCount);
        message += string.Format("Length: {0}/{1} ", info.TotalUpdateSuccessLength, info.TotalUpdateLength);
        message += string.Format("Speed: {0} ", info.NetworkSpeed);
        message += string.Format("Current Length: {0}/{1} ", info.CurrentSuccessLength, info.CurrentTotalLength);
        message += string.Format("Current Progress: {0} ", info.CurrentProgress);
        Debug.Log(message);
    }

    private void OnErr(string err)
    {
        Debug.LogError(err);
    }


    private void EnterMain()
    {
        Debug.Log("资源流程完成. 进入主场景");

        // ResMgr.Instance.LoadPrefab("UIRoot", (obj) =>
        // {
        //     Debug.Log("加载完成 "+obj);  
        // }, this);

        ResMgr.Instance.LoadCache("common", new LoadParam[] {
            new LoadParam("Prefabs/UIRoot",LoadType.PREFAB){isInstant=true },
            new LoadParam("Prefabs/UIRoot2",LoadType.PREFAB){isInstant=true },
            new LoadParam("Prefabs/UIRoot3",LoadType.PREFAB){isInstant=true },
            new LoadParam("Prefabs/UIRoot4",LoadType.PREFAB){isInstant=true },
            new LoadParam("Prefabs/UIRoot5",LoadType.PREFAB){isInstant=true },
        }, () =>
        {
            Debug.Log("加载完成");
            Instant("Prefabs/UIRoot", LoadType.PREFAB);

        });
    }
    public Transform parent;
    private void Instant(string path, LoadType loadType)
    {
        GameObject go = ResMgr.Instance.GetCache<GameObject>(path, loadType, parent, "common");
        if (go)
        {
            GameObject.Instantiate(go, parent);
        }
    }
}
