using System;
using System.Collections;
using System.Collections.Generic;
using Plugins.XAsset;
using UnityEngine;

public class CustomTest : MonoBehaviour
{
    private void Start()
    {
        ResMgr.Instance.Init(OnInitComplete, OnErr);
    }

    private void OnInitComplete()
    {
        // todo: 检查完成后 本地资源即可使用了 
        // 进入游戏 玩 在包内部的资源可正常使用
        //EnterMain();
        Debug.Log("初始化完毕. 检查更新");
         
        // 可后台更新 不影响当前逻辑
        ResMgr.Instance.CheckVersion(OnCheckVersionSucceed, OnErr);
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
        message += string.Format("Count: {0}/{1} -\r\n", info.TotalUpdateSuccessCount, info.TotalUpdateCount);
        message += string.Format("Length: {0}/{1} -\r\n", info.TotalUpdateSuccessLength, info.TotalUpdateLength);
        message += string.Format("Speed: {0} -\r\n", info.NetworkSpeed);
        message += string.Format("Current Length: {0}/{1} -\r\n", info.CurrentSuccessLength, info.CurrentTotalLength);
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
        ResMgr.Instance.LoadPrefab("UIRoot",(obj)=> {
            if (obj)
            {
                GameObject.Instantiate(obj);
            }
            else
            {
                Debug.LogError("加载失败");
            }
        },this);
    }
}
