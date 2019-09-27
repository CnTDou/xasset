using Plugins.XAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Collections;
using Object = UnityEngine.Object;
using LoadType = Plugins.XAsset.LoadType;
using UtilText = GameFramework.Utility.Text;

public class AssetCheck
{
    private AssetsManifest _assetsManifest;
    private Dictionary<string, FileInfo> dicCheck;

    public bool IsCheckBundle
    {
        get
        {
#if UNITY_EDITOR
            return ResMgr.Instance.isCheckBundle;
#else
            return false;
#endif
        }
    }

    public void Init()
    {
        dicCheck = new Dictionary<string, FileInfo>();
        _assetsManifest = Util.EditorGetAsset<AssetsManifest>(Utility.AssetsManifestAsset);
    }

    public bool Check(string path)
    {
        if (IsCheckBundle && _assetsManifest)
        {
            FileInfo file = null;
            if (!dicCheck.TryGetValue(path, out file))
            {
                file = new FileInfo(path);
                dicCheck.Add(path, file);
            }

            if (file.Exists)
            {
                var assets = Array.FindAll(_assetsManifest.assets, (p) => { return p.name == file.Name; });
                if (assets != null && Array.Exists(assets, (p) => { return p.dir + p.name == file.FullName; }))
                {
                    return true;
                }

                // 没有在bundle 里面 并且文件存在
                Debug.LogWarningFormat("manifest check , path {0} no exists manifest.", path);
            }

            // 文件不存在
        }

        return false;
    }
}

/// <summary>
/// 资源总管理器
///     user :使用者 
///         例如 
///             创建prefab user传入预制件父级 当父级销毁引用自动-1
///             创建sprite user传入 image GameObject或者image自身 这样当 被销毁引用--
///     path : 传入 从 Assets/Demo/ 之后的路径 Prefabs/UIRoot 不携带后缀
///     pathAndSuffix : 传入 从 Assets/Demo/ 之后的路径 Prefabs/UIRoot.prefab 携带后缀
///     fullPath : 传入完整的路径 Assets/Demo/Prefabs/UIRoot.prefab 
/// </summary>
public class ResMgr : MonoBehaviour
{
    #region ... 单例

    /// <summary>
    /// The instance.
    /// </summary>
    private static ResMgr instance;

    /// <summary>
    /// Gets the singleton.
    /// </summary>
    /// <returns>The singleton.</returns>
    public static ResMgr Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<ResMgr>();
                if (!instance)
                {
                    GameObject singleton = new GameObject(typeof(ResMgr).Name);
                    if (!singleton)
                        throw new System.NullReferenceException();

                    instance = singleton.AddComponent<ResMgr>();

                    if (Application.isPlaying)
                        GameObject.DontDestroyOnLoad(singleton);
                }

                var assets = FindObjectOfType<Assets>();
                if (!assets)
                {
                    assets = instance.gameObject.AddComponent<Assets>();
                }
            }

            return instance;
        }
    }

    #endregion

    public enum State
    {
        Wait,
        Initing,
        InitializeError,
        Checking,
        CheckError,
        WaitUpdate,
        Updateing,
        UpdateError,
        Completed,
    }

    public bool isCheckBundle = false;
    public bool isWindow = false;
    [SerializeField] private State state = State.Wait;
    public UpdateControl updateControl = new UpdateControl();
    private CacheControl cacheControl = new CacheControl();


    public State GetState
    {
        get { return state; }
    }

    private void Awake()
    {
        if (isWindow)
        {
            ResDebug resDebug = gameObject.GetComponent<ResDebug>();
            if (!resDebug)
            {
                resDebug = gameObject.AddComponent<ResDebug>();
            }
        }
    }

    private void Update()
    {
        switch (state)
        {
            case State.Wait:
            case State.InitializeError:
            case State.CheckError:
            case State.UpdateError:
                // 当前状态错误
                return;
            case State.Initing:
            case State.Checking:
            case State.WaitUpdate:
            case State.Updateing:
            case State.Completed:
            default:
                break;
        }

        updateControl.Update();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="onCompleted"></param>
    /// <param name="onFailed"></param>
    public void Init(Action onCompleted, Action<string> onFailed)
    {
        if (state != State.Wait)
        {
            Util.Log("res error RES.Init, state {0}", state);
            return;
        }

        Versions.Load();
        state = State.Initing;
        Assets.Initialize(() =>
        {
            state = State.Completed;
            if (onCompleted != null)
            {
                onCompleted.Invoke();
            }
        }, (err) =>
        {
            state = State.InitializeError;
            Util.Log("res error RES.Init, error {0}", state);
            if (onFailed != null)
            {
                onFailed.Invoke(err);
            }
        });
    }

    /// <summary>
    /// 检查版本信息
    /// </summary>
    /// <param name="onSucceed">为null 则不需要进行下载</param>
    /// <param name="onFailed"></param>
    public void CheckVersion(Action<VersionInfo> onSucceed, Action<string> onFailed)
    {
        switch (state)
        {
            case State.Completed:
                break;
            default:
                Util.Log("res error RES.CheckVersion, state: {0}", state);
                return;
        }

        state = State.Checking;
        updateControl.CheckVersion((versionInfo) =>
        {
            state = versionInfo.IsUpdate ? State.WaitUpdate : State.Completed;
            if (onSucceed != null)
            {
                onSucceed.Invoke(versionInfo);
            }
        }, (err) =>
        {
            state = State.CheckError;
            Util.Log("res error RES.CheckVersion, error: {0}", state);
            if (onFailed != null)
            {
                onFailed.Invoke(err);
            }
        });
    }

    /// <summary>
    /// 开始更新资源
    /// </summary>
    /// <param name="onSucceed"></param>
    /// <param name="onFailed"></param>
    /// <param name="onProgress"></param>
    public void StartUpdateRes(Action onSucceed, Action<string> onFailed, Action<UpdatingInfo> onProgress)
    {
        if (state != State.WaitUpdate)
        {
            Util.Log("res error RES.UpdateRes, state {0}", state);
            return;
        }

        state = State.Updateing;
        onProgress += OnUpdateingRes;
        updateControl.StartUpdateRes(() =>
        {
            state = State.Completed;
            if (onSucceed != null)
            {
                onSucceed.Invoke();
            }
        }, (err) =>
        {
            state = State.UpdateError;
            Util.Log("res error RES.UpdateRes, error {0}", state);
            if (onFailed != null)
            {
                onFailed.Invoke(err);
            }
        }, onProgress);
    }

    public void Clear()
    {
        state = State.Wait;
        updateControl.Clear();
    }

    #region ... Asset

    public void LoadAsync<T>(string fullPath, Object user, Action<T> callback)
        where T : Object
    {
        LoadAsync(fullPath, Util.GetType(LoadType.NONE), (res) =>
        {
            if (string.IsNullOrEmpty(res.error) && res.Asset)
            {
                res.Require(user);
                if (callback != null)
                {
                    callback(res.Asset as T);
                }
            }
        });
    }

    /// <summary>
    /// 异步加载
    ///     路径.后缀
    /// </summary>
    /// <param name="_path">路径.后缀</param>
    /// <param name="type"></param>
    /// <param name="onCompleted"></param> 
    /// <param name="user"></param>
    public void LoadAsync<T>(string fullPath, Action<IResInfo> onSucceed) where T : Object
    {
        LoadAsync(fullPath, typeof(T), (res) =>
        {
            if (onSucceed != null)
            {
                onSucceed.Invoke(res);
            }
        });
    }

    /// <summary>
    /// 异步加载 [注意:使用]
    ///     注意此方法需要完成路径  路径.后缀
    /// </summary>
    /// <param name="fullPath"></param>
    /// <param name="type"></param>
    /// <param name="onCompleted"></param> 
    /// <param name="user"></param>
    public void LoadAsync(string fullPath, Type loadType, Action<IResInfo> onCompleted)
    {
        if (state != State.Completed)
        {
            Util.Log("res error RES.LoadAsync, state: {0} , no init", state);
            return;
        }

        Asset asset = Assets.LoadAsync(fullPath, loadType);
        asset.completed += (_asset) => { OnLoadComplete(fullPath, onCompleted, _asset); };
    }

    private void OnLoadComplete(string fullPath, Action<IResInfo> onCompleted, Asset _asset)
    {
        if (string.IsNullOrEmpty(_asset.error) && _asset.asset)
        {
            if (onCompleted != null)
            {
                IResInfo res = new ResInfo(_asset);
                onCompleted.Invoke(res);
            }
        }
        else
        {
            string err = string.Format(
                "res error RES.LoadAsync, fullPath: {0} , type: {1} , asset: {2} , error message: {3}.",
                fullPath, _asset.assetType, _asset.asset, _asset.error);
            if (onCompleted != null)
            {
                onCompleted.Invoke(new ResInfo(err));
            }

            Util.Log(err);
        }
    }

    #endregion

    #region ... Scene 

    public SceneAssetAsync LoadSceneAsync(string sceneName)
    {
        string _path = UtilText.Format("{0}{1}.{2}", Constnat.RES_SCENES_PATH, sceneName,
            Util.GetSuffix(LoadType.SCENE));
        SceneAssetAsync sceneAsset = Assets.LoadScene(_path, true, false) as SceneAssetAsync;
        return sceneAsset;
    }

    public void UnloadScene(string sceneName)
    {
        string _path = UtilText.Format("{0}{1}.{2}", Constnat.RES_SCENES_PATH, sceneName,
            Util.GetSuffix(LoadType.SCENE));
        Assets.UnloadScene(_path);
    }

    #endregion

    #region ... Cache 

    /// <summary>
    /// 加载并添加缓存
    /// </summary>
    /// <param name="loadParams"></param>
    /// <param name="user">当传入空包对象,加载成功/失败回调null ,当不为空时 加载成功返回对象</param>
    /// <param name="onComplete"></param>
    /// <param name="groupName"></param>
    public void LoadCache(LoadParam loadParams, Object user, Action<Object> onComplete,
        string groupName = Constnat.DEFAULT_CACHE)
    {
        cacheControl.LoadCache(loadParams, user, onComplete, groupName);
    }

    public void LoadCache(LoadParam[] loadParams, Action onComplete, LoadingCache onLoading,
        string groupName = Constnat.DEFAULT_CACHE)
    {
        cacheControl.LoadCache(loadParams, onComplete, onLoading, groupName);
    }

    public void UnloadCache(string groupName = Constnat.DEFAULT_CACHE)
    {
        cacheControl.UnloadCache(groupName);
    }

    public T GetCache<T>(string fullPath, Object user, string groupName = Constnat.DEFAULT_CACHE)
        where T : Object
    {
        return cacheControl.GetCache<T>(fullPath, user, groupName);
    }

    #endregion

    #region ... Event

    private void OnUpdateingRes(UpdatingInfo info)
    {
        string message = string.Format("Count: {0}/{1} \r\n", info.TotalUpdateSuccessCount, info.TotalUpdateCount);
        message += string.Format("Length: {0}/{1} \r\n", info.TotalUpdateSuccessLength, info.TotalUpdateLength);
        message += string.Format("Speed: {0} \r\n", info.NetworkSpeed);
        message += string.Format("Current Length: {0}/{1} \r\n", info.CurrentSuccessLength, info.CurrentTotalLength);
        message += string.Format("Current Progress: {0} ", info.CurrentProgress);
    }

    #endregion
}


public interface IResCache
{
    /// <summary>
    /// 是否所有已完成
    /// </summary>
    bool IsDone { get; }

    /// <summary>
    /// 当前加载进度
    /// </summary>
    float Progress { get; }

    /// <summary>
    /// 准备就绪数量
    /// </summary>
    int ReadyCount { get; }

    /// <summary>
    /// 加载中数量
    /// </summary>
    int LoadingCount { get; }

    void AddCache(string path, Type assetType);

    Object Get<T>(string path, Object user) where T : Object;

    void Clear();
}

public class ResCache : MonoBehaviour, IResCache
{
    public int ReadyCount
    {
        get { return cacheDic.Count; }
    }

    public int LoadingCount
    {
        get { return loadingCount; }
    }

    public bool IsDone
    {
        get { return LoadingCount == 0; }
    }

    public float Progress
    {
        get { return (100f / (ReadyCount + LoadingCount)) * ReadyCount; }
    }

    int loadingCount;
    Dictionary<string, IResInfo> cacheDic = new Dictionary<string, IResInfo>();

    public void AddCache(string path, Type assetType)
    {
        if (cacheDic.ContainsKey(path))
        {
            return;
        }

        loadingCount++;
        ResMgr.Instance.LoadAsync(path, assetType, (res) =>
        {
            loadingCount--;
            if (string.IsNullOrEmpty(res.error) && res.Asset)
            {
                // ... obj
                res.Require(this);
                cacheDic.Add(path, res);
            }
        });
    }

    public Object Get<T>(string path, Object user) where T : Object
    {
        IResInfo res;
        if (cacheDic.TryGetValue(path, out res))
        {
            res.Require(user);
            return res.Asset as T;
        }

        return null;
    }

    public void Clear()
    {
        foreach (var item in cacheDic)
        {
            item.Value.Dequire(this);
        }

        cacheDic.Clear();
    }
}