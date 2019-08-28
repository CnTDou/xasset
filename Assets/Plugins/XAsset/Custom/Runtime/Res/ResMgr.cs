using Plugins.XAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Collections;
using Object = UnityEngine.Object;
using LoadType = Plugins.XAsset.LoadType;
using UtilText = GameFramework.Utility.Text;


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
    public const string RES_ROOT_PATH = Constnat.AssetRoot;
    public const string RES_PREFABS_PATH = "Prefabs/";
    public const string RES_SCENES_PATH = RES_ROOT_PATH + "Scenes/";

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

    enum State
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
    public bool isWindow = false;
    [SerializeField]
    private State state = State.Wait;
    public UpdateControl updateControl = new UpdateControl();
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
            ResMgr.OutLog("res error RES.Init, state {0}", state);
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
            ResMgr.OutLog("res error RES.Init, error {0}", state);
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
                ResMgr.OutLog("res error RES.CheckVersion, state: {0}", state);
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
            ResMgr.OutLog("res error RES.CheckVersion, error: {0}", state);
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
            ResMgr.OutLog("res error RES.UpdateRes, state {0}", state);
            return;
        }
        state = State.Updateing;
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
            ResMgr.OutLog("res error RES.UpdateRes, error {0}", state);
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

    /// <summary>
    /// 预制件名称 
    ///     例: 预制件路径 @{AssetRoot}/Prefabs/xxx.prefab
    /// </summary>
    /// <param name="prefabName">预制件名称 xxx </param>
    /// <param name="onCompleted">加载完成</param>
    /// <param name="user">使用者 [必填] 引用计数作用</param>
    public void LoadPrefab(string prefabName, Action<IRes> onCompleted)
    {
        LoadAsync(UtilText.Format("{0}{1}", RES_PREFABS_PATH, prefabName),
            LoadType.PREFAB, onCompleted);
    }

    /// <summary>
    /// 加载资源
    ///    例: 
    ///         @{AssetRoot}/Prefabs/xxx.prefab
    ///         @{AssetRoot}/Sprites/xxx.png
    ///         @{AssetRoot}/Textures/xxx.jpg
    /// </summary>
    /// <param name="_path"> 
    ///         Prefabs/xxx
    ///         Sprites/xxx
    ///         Textures/xxx
    /// </param>
    /// <param name="_loadType"> 
    ///         LoadType.PREFAB 
    ///         LoadType.TEXTURE2D_PNG 
    ///         LoadType.TEXTURE2D_JPG 
    /// </param>
    /// <param name="onCompleted"></param>
    /// <param name="user">使用者 [必填] 引用计数作用</param>
    public void LoadAsync(string _path, LoadType _loadType, Action<IRes> onCompleted)
    {
        string suffix = GetSuffix(_loadType);
        if (string.IsNullOrEmpty(suffix))
        {
            ResMgr.OutLog("res warn RES.LoadAsync, Cannot get suffix by load type {0}.", _loadType);
        }
        _path = UtilText.Format("{0}{1}.{2}", RES_ROOT_PATH, _path, suffix);
        LoadAsync(_path, GetType(_loadType), onCompleted);
    }

    /// <summary>
    /// 异步加载
    ///     路径.后缀
    /// </summary>
    /// <param name="_path">路径.后缀</param>
    /// <param name="type"></param>
    /// <param name="onCompleted"></param> 
    /// <param name="user"></param>
    public void LoadAsync<T>(string _pathAndSuffix, Action<IRes> onSucceed) where T : Object
    {
        string fullPath = UtilText.Format("{0}{1}", RES_ROOT_PATH, _pathAndSuffix);
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
    public void LoadAsync(string fullPath, Type loadType, Action<IRes> onCompleted)
    {
        if (state != State.Completed)
        {
            ResMgr.OutLog("res error RES.LoadAsync, state: {0} , no init", state);
            return;
        }
        Asset asset = Assets.LoadAsync(fullPath, loadType);
        asset.completed += (_asset) =>
        {
            OnLoadComplete(fullPath, onCompleted, _asset);
        };
    }

    private void OnLoadComplete(string fullPath, Action<IRes> onCompleted, Asset _asset)
    {
        if (string.IsNullOrEmpty(_asset.error) && _asset.asset)
        {
            if (onCompleted != null)
            {
                IRes res = new ResInfo(_asset);
                onCompleted.Invoke(res); 
            }
        }
        else
        {
            if (onCompleted != null)
            {
                onCompleted.Invoke(null);
            }
            ResMgr.OutLog("res error RES.LoadAsync, fullPath: {0} , type: {1} , asset: {2} , error message: {3}.",
                fullPath, _asset.assetType, _asset.asset, _asset.error);
        }
    }

    #endregion

    #region ... Scene 

    public SceneAssetAsync LoadSceneAsync(string sceneName)
    {
        string _path = UtilText.Format("{0}{1}.{2}", RES_SCENES_PATH, sceneName, GetSuffix(LoadType.SCENE));
        SceneAssetAsync sceneAsset = Assets.LoadScene(_path, true, false) as SceneAssetAsync;
        return sceneAsset;
    }

    public void UnloadScene(string sceneName)
    {
        string _path = UtilText.Format("{0}{1}.{2}", RES_SCENES_PATH, sceneName, GetSuffix(LoadType.SCENE));
        Assets.UnloadScene(_path);
    }

    #endregion

    #region ... Cache 
    private const string DEFAULT_CACHE = "default"; 
    private Dictionary<string, ResCacheGroup> groupDic = new Dictionary<string, ResCacheGroup>();

    private ResCacheGroup TryGetGroup(string groupName)
    {
        ResCacheGroup group;
        if (!groupDic.TryGetValue(groupName, out group))
        {
            GameObject go = new GameObject(groupName);
            go.transform.SetParent(transform);
            group = go.AddComponent<ResCacheGroup>();
            groupDic.Add(groupName, group);
        }
        return group;
    }

    private void AddCache(string groupName, IRes res)
    {
        ResCacheGroup group = TryGetGroup(groupName);
        group.AddReady(res);
    }

    public void LoadCache(string groupName, LoadParam[] loadParams, Action onComplete, LoadingCache onLoading = null)
    {
        ResCacheGroup group = TryGetGroup(groupName);
        group.Add(loadParams, onComplete, onLoading);
    }

    public void UnloadCache(string groupName)
    {
        ResCacheGroup group = TryGetGroup(groupName);
        group.UnloadAll();
    }

    public T GetCache<T>(string _path, LoadType _loadType, Object user, string groupName) where T : Object
    {
        ResCacheGroup group = TryGetGroup(groupName);

        string suffix = GetSuffix(_loadType);
        if (string.IsNullOrEmpty(suffix))
        {
            ResMgr.OutLog("res warn RES.GetCache<T:{0}>, path: {1} , Cannot get suffix by load type: {2} , user: {3} , groupName: {4} .",
                typeof(T), _path, _loadType, user, groupName);
        }
        _path = UtilText.Format("{0}{1}.{2}", RES_ROOT_PATH, _path, suffix);
        return group.Get(_path, user) as T;
    }

    #endregion

    #region ... Util

    public static string GetSuffix(LoadType loadType)
    {
        switch (loadType)
        {
            case LoadType.AUDIO_CLIP_OGG: return "ogg";
            case LoadType.AUDIO_CLIP_WAV: return "wav";
            case LoadType.AUDIO_CLIP_MP3: return "mp3";
            case LoadType.ANIMATION_CLIP: return "anim";
            case LoadType.SCENE: return "unity";
            case LoadType.NONE:
                return string.Empty;
        }
        return loadType.ToString().ToLower();
    }

    public static Type GetType(LoadType loadType)
    {
        Type _type = typeof(UnityEngine.Object);
        switch (loadType)
        {
            case LoadType.PREFAB:
                _type = typeof(GameObject);
                break;
            case LoadType.JPG:
            case LoadType.JPEG:
            case LoadType.PNG:
                _type = typeof(Texture2D);
                break;
            case LoadType.AUDIO_CLIP_OGG:
            case LoadType.AUDIO_CLIP_WAV:
            case LoadType.AUDIO_CLIP_MP3:
                _type = typeof(AudioClip);
                break;
            case LoadType.ANIMATION_CLIP:
                _type = typeof(AnimationClip);
                break;
            case LoadType.NONE:
            case LoadType.SCENE:
            default:
                _type = typeof(UnityEngine.Object);
                break;
        }
        return _type;
    }

    public static void OutLog(string format, params object[] args)
    {
        Debug.LogFormat(format, args);
    }

    #endregion

    #region ... Test
    string message, assetPath;

    int curIndex;
    int current, _max = 8;
    bool isCheck;
    private void OnGUI()
    {
        if (isWindow)
        {
            using (var v = new GUILayout.VerticalScope("RES", "window"))
            {
                switch (state)
                {
                    case State.Wait:
                        if (GUILayout.Button("Init"))
                        {
                            Init(() => { message = " ready ."; }, (err) => { message = err; });
                        }
                        isCheck = true;
                        break;
                    case State.Completed:
                        isCheck = true;
                        if (GUILayout.Button("Clear"))
                        {
                            Clear();
                        }
                        break;
                    default:
                        isCheck = false;
                        break;
                }

                if (isCheck && GUILayout.Button("Check"))
                {
                    CheckVersion((vinfo) =>
                    {
                        if (vinfo.IsUpdate)
                        {
                            StartUpdateRes(null, (err) => { message = err; }, (uinfo) =>
                            {
                                message = "更新中 : \r\n";
                                message += string.Format("Count: {0}/{1} -\r\n", uinfo.TotalUpdateSuccessCount, uinfo.TotalUpdateCount);
                                message += string.Format("Length: {0}/{1} -\r\n", uinfo.TotalUpdateSuccessLength, uinfo.TotalUpdateLength);
                                message += string.Format("Speed: {0} -\r\n", uinfo.NetworkSpeed);
                                message += string.Format("Current Length: {0}/{1} -\r\n", uinfo.CurrentSuccessLength, uinfo.CurrentTotalLength);
                                message += string.Format("Current Progress: {0} ", uinfo.CurrentProgress);
                            });
                        }
                    }, (err) => { message = err; });
                }

                GUILayout.Label(string.Format("{0}:{1}", state, message));
                if (state == State.Completed)
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
    #endregion

}
