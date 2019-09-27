using Plugins.XAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using UtilText = GameFramework.Utility.Text;
using Object = UnityEngine.Object;


public static class ResExtend
{ 
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
    public static void LoadAsync<T>(this ResMgr resMgr, string _path, LoadType _loadType,
        Object user, Action<T> onCompleted)where T:Object
    { 
        _path = UtilText.Format("{0}{1}{2}", Constnat.RES_ROOT_PATH, _path, Util.GetSuffix(_loadType));
        resMgr.LoadAsync<T>(_path,user, onCompleted);
    }
  
    /// <summary>
    /// 预制件名称 
    ///     例: 预制件路径 @{AssetRoot}/Prefabs/xxx.prefab
    /// </summary>
    /// <param name="prefabName">预制件名称 xxx </param>
    /// <param name="onCompleted">加载完成</param>
    /// <param name="user">使用者 [必填] 引用计数作用</param>
    public static void LoadPrefab(this ResMgr resMgr, string path, Object user, Action<GameObject> callback)
    { 
        resMgr.LoadAsync<GameObject>(path, LoadType.PREFAB, user,callback);
    }

    /// <summary>
    /// 从 Resources.LoadAysnc<T> 加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resMgr"></param>
    /// <param name="path"></param>
    /// <param name="user"></param>
    /// <param name="callback"></param>
    public static void LoadAsync_Res<T>(this ResMgr resMgr, string path, Object user, Action<T> callback)
        where T : Object
    {
        ResourceRequest r = Resources.LoadAsync<T>(path);
        r.completed += (rq) =>
        {
            if (callback != null)
            {
                callback(r.asset as T);
            }
        };
    }

    #region ... Image.sprite

    public static void Load(this Image image, string path)
    {
        Asset asset = Assets.LoadAsync(path, typeof(Texture2D));
        asset.completed += (_asset) =>
        {
            if (string.IsNullOrEmpty(_asset.error) && _asset.asset)
            {
                Texture2D txt2d = _asset.asset as Texture2D;
                if (txt2d)
                {
                    image.sprite = Sprite.Create(txt2d, new Rect(0, 0, txt2d.width, txt2d.height), Vector2.zero);
                    AssetListening.Add(image.gameObject, asset);
                    return;
                }
            }

            Util.Log("res error ResExtend.Load, path: {0} .", path);
            _asset.Release();
        };
    }

    public static void LoadJPEG(this Image image, string path)
    {
        ResMgr.Instance.LoadAsync<Texture2D>(path, LoadType.JPEG, image,(asset) => { ExecuteComplete(image, asset); });
    }

    public static void LoadJPG(this Image image, string path)
    {
        ResMgr.Instance.LoadAsync<Texture2D>(path, LoadType.JPG,  image,(asset) => { ExecuteComplete(image, asset); });
    }

    public static void LoadPNG(this Image image, string path)
    {
        ResMgr.Instance.LoadAsync<Texture2D>(path, LoadType.PNG,  image,(asset) => { ExecuteComplete(image, asset); });
    }

    private static void ExecuteComplete(Image image, Object asset)
    {
        Texture2D txt2d=asset as Texture2D;
        if (txt2d)
        {
            image.sprite = Sprite.Create(txt2d, new Rect(0, 0, txt2d.width, txt2d.height), Vector2.zero);
        }
    }

    #endregion

    #region ... AudioSource.clip

    public static void LoadClipOGG(this ResMgr resMgr, string path, Object user, Action<AudioClip> onComplete)
    {
        resMgr.LoadAsync<AudioClip>(path, LoadType.AUDIO_CLIP_OGG,
            user,(res) => { ExecuteComplete(res  ,onComplete); });
    }

    public static void LoadClipWAV(this ResMgr resMgr, string path, Object user, Action<AudioClip> onComplete)
    {
        resMgr.LoadAsync<AudioClip>(path, LoadType.AUDIO_CLIP_WAV,
            user,(res) => { ExecuteComplete(res  ,onComplete); });
    }

    public static void LoadClipMP3(this ResMgr resMgr, string path, Object user, Action<AudioClip> onComplete)
    {
        resMgr.LoadAsync<AudioClip>(path, LoadType.AUDIO_CLIP_MP3,
            user,(res) => { ExecuteComplete(res  ,onComplete); });
    }

    private static void ExecuteComplete(Object res,  Action<AudioClip> onComplete)
    {
        AudioClip clip = res as AudioClip;
        if (clip && onComplete != null)
        { 
            onComplete.Invoke(clip);
        }
    }

    #endregion

    #region ... 自定义

    public static string worldPath = "";

    public static void LoadPrefab_World(this ResMgr resMgr, string path, Object user, Action<GameObject> callback)
    {
        path = GetWorldFullPath(path, LoadType.PREFAB);
        resMgr.LoadAsync<GameObject>(path, user, (asset) =>
        {
            if (callback != null)
            {
                callback(asset);
            }
        });
    }

    public static void LoadPrefab_Tiled2Unity(this ResMgr resMgr, string path, Object user, Action<GameObject> callback)
    {
        path = GetFullPath(Constnat.RES_TILED2UNITY_PREFAB_PATH, path, LoadType.PREFAB);
        resMgr.LoadAsync<GameObject>(path, user, (asset) =>
        {
            if (callback != null)
            {
                callback(asset);
            }
        });
    }

    public static void LoadPrefab_Effect(this ResMgr resMgr, string path, Object user, Action<GameObject> callback)
    {
        path = GetEffectFullPath(path, LoadType.NONE);
        resMgr.LoadAsync<GameObject>(path, user, (asset) =>
        {
            if (callback != null)
            {
                callback(asset);
            }
        });
    }

    #endregion

    public static string GetPath(string path, LoadType loadType)
    {
        return UtilText.Format("{0}{1}{2}", Constnat.RES_ROOT_PATH, path,
            Util.GetSuffix(loadType));
    }

    public static string GetEffectFullPath(string path, LoadType loadType)
    {
        return UtilText.Format("{0}Effect/{1}{2}",
            Constnat.RES_ROOT_PATH, path, Util.GetSuffix(loadType));
    }

    public static string GetWorldFullPath(string path, LoadType loadType)
    {
        path = UtilText.Format("{0}{1}{2}{3}", Constnat.RES_ROOT_PATH,
            worldPath, path, Util.GetSuffix(loadType));
        return path;
    }

    public static string GetFullPath(string path, LoadType loadType)
    {
        return UtilText.Format("{0}{1}{2}",
            Constnat.RES_ROOT_PATH, path, Util.GetSuffix(loadType));
    }

    public static string GetFullPath(string rootPath, string path, LoadType loadType)
    {
        return UtilText.Format("{0}{1}{2}", rootPath, path, Util.GetSuffix(loadType));
    }
}