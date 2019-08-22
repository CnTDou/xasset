using Plugins.XAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Collections;
using Object = UnityEngine.Object;
using LoadType = Plugins.XAsset.LoadType;


/// <summary>
/// 资源总管理器
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
                GameObject singleton = new GameObject(typeof(ResMgr).Name);
                if (!singleton)
                    throw new System.NullReferenceException();

                instance = singleton.AddComponent<ResMgr>();
                if (singleton.GetComponent<Assets>() == null)
                    singleton.AddComponent<Assets>();
                if (Application.isPlaying)
                    GameObject.DontDestroyOnLoad(singleton);
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
            case State.Wait:
            case State.Completed:
                break;
            default:
                ResMgr.OutLog("res error RES.CheckVersion, state: {0}", state);
                return;
        }
        state = State.Checking;
        Versions.Load();
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
    public void LoadPrefab(string prefabName, Action<Object> onCompleted, Object user)
    {
        LoadAsync(RES_PREFABS_PATH + prefabName, LoadType.PREFAB, onCompleted, user);
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
    public void LoadAsync(string _path, LoadType _loadType, Action<Object> onCompleted, Object user)
    {
        string suffix = GetSuffix(_loadType);
        if (string.IsNullOrEmpty(suffix))
        {
            Debug.LogWarningFormat("load async fail, Cannot get suffix by load type {0}.", _loadType);
        }
        _path = string.Format("{0}{1}.{2}", RES_ROOT_PATH, _path, suffix);
        LoadAsync(_path, GetType(_loadType), onCompleted, user);
    }

    /// <summary>
    /// 异步加载 [不推荐使用,建议封装LoadType方式使用] 
    ///     注意此方法需要完成路径  路径.后缀
    /// </summary>
    /// <param name="fullPath"></param>
    /// <param name="type"></param>
    /// <param name="onCompleted"></param> 
    /// <param name="user"></param>
    public void LoadAsync<T>(string fullPath, Action<T> onSucceed, Object user) where T : Object
    {
        LoadAsync(fullPath, typeof(T), (asset) =>
        {
            if (onSucceed != null)
            {
                onSucceed.Invoke(asset as T);
            }
        }, user);
    }

    /// <summary>
    /// 异步加载 [不推荐使用,建议封装LoadType方式使用] 
    ///     注意此方法需要完成路径  路径.后缀
    /// </summary>
    /// <param name="fullPath"></param>
    /// <param name="type"></param>
    /// <param name="onCompleted"></param> 
    /// <param name="user"></param>
    public void LoadAsync(string fullPath, Type loadType, Action<Object> onCompleted, Object user)
    {
        if (state != State.Completed)
        {
            ResMgr.OutLog("res error RES.LoadAsync, state: {0} , no init", state);
            return;
        }

        Asset asset = Assets.LoadAsync(fullPath, loadType);
        asset.completed += (_asset) =>
        {
            if (string.IsNullOrEmpty(_asset.error))
            {
                if (onCompleted != null)
                {
                    _asset.Require(user);
                    onCompleted.Invoke(_asset.asset);
                }
            }
            else
            {
                if (onCompleted != null)
                {
                    onCompleted.Invoke(null);
                }
                ResMgr.OutLog("res error RES.LoadAsync, fullPath: {0} , loadType: {1} , error message: {2}.",
                    fullPath, loadType, _asset.error);
            }
        };
    }

    #endregion

    #region ... Scene 

    public SceneAssetAsync LoadSceneAsync(string sceneName)
    {
        SceneAssetAsync sceneAsset = Assets.LoadScene(RES_SCENES_PATH + sceneName, true, false) as SceneAssetAsync;
        return sceneAsset;
    }

    public void UnloadScene(string sceneName)
    {
        Assets.UnloadScene(RES_SCENES_PATH + sceneName);
    }

    #endregion

    #region ... Cache

    public T GetCache<T>(string path, Object user) where T : Object
    {
        return null;
    }

    #endregion

    public string GetSuffix(LoadType loadType)
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

    public Type GetType(LoadType loadType)
    {
        Type _type = typeof(Object);
        //switch (loadType)
        //{ 
        //    case LoadType.PREFAB:
        //        _type = typeof(GameObject);
        //        break;
        //    case LoadType.JPG: 
        //    case LoadType.JPEG: 
        //    case LoadType.PNG:
        //        _type = typeof(Texture2D); 
        //        break;
        //    case LoadType.AUDIO_CLIP_OGG: 
        //    case LoadType.AUDIO_CLIP_WAV: 
        //    case LoadType.AUDIO_CLIP_MP3:
        //        _type = typeof(AudioClip); 
        //        break;
        //    case LoadType.ANIMATION_CLIP:
        //        _type = typeof(AnimationClip); 
        //        break;
        //    case LoadType.NONE:
        //    case LoadType.SCENE: 
        //    default:
        //        _type = typeof(Object);
        //        break;
        //}

        return _type;
    }

    public static void OutLog(string format, params object[] args)
    {
        Debug.LogFormat(format, args);
    }

}
