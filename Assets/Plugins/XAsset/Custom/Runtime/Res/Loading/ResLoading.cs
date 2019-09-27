using System;
using System.Collections;
using System.Collections.Generic;
using Plugins.XAsset;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Plugins.XAsset
{
    /// <summary>
    /// 资源加载器
    ///     依赖对象的 资源加载器 
    /// </summary>
    public class ResLoading : System.IDisposable, ILoading
    {
        private Dictionary<string, Asset> cacheDic = new Dictionary<string, Asset>();
        private Object target;

        private ResLoading()
        {
        }

        private ResLoading(Object _target)
        {
            if (cacheResLoadingDic.ContainsKey(_target))
            {
                Error = string.Format(
                    "ResLoading 一个Object对象只能绑定一个 ResLoading , 当前对象 '{0}' 已经存在绑定了.请尝试使用 ResLoading.Get(object) . ",
                    _target);
                return;
            }

            target = _target;
            cacheResLoadingDic.Add(target, this);
        }

        #region ... API

        public int AllCount
        {
            get { return cacheDic.Count; }
        }

        public int DoneCount
        {
            get
            {
                int count = 0;
                foreach (var item in cacheDic)
                {
                    if (item.Value.IsDone)
                        count++;
                }

                return count;
            }
        }

        public int LoadingCount
        {
            get
            {
                int count = 0;
                foreach (var item in cacheDic)
                {
                    if (item.Value.State == ResInfoState.Loaded)
                        count++;
                }

                return count;
            }
        }

        public float Progress
        {
            get { return 100f / AllCount * DoneCount; }
        }

        public bool IsDone
        {
            get { return LoadingCount == 0; }
        }

        public string Error { get; private set; }

        #endregion

        private Asset TryGet(string path)
        {
            Asset res;
            if (cacheDic.TryGetValue(path, out res))
            {
                res = ResMgr.Instance.TryGet(path);
                res.Require(target);
            }

            return res;
        }

        public void Dispose()
        {
            if (target)
            {
                if (cacheDic != null && cacheDic.Count > 0)
                {
                    foreach (var item in cacheDic)
                    {
                        item.Value.Dequire(target);
                    }

                    cacheDic.Clear();
                }

                cacheResLoadingDic.Remove(target);
            }

            target = null;
        }

        /// <summary>
        /// 获取当前缓存的对象
        /// </summary>
        /// <param name="path"></param>
        /// <param name="user"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Object Get(string path, Object user)
        {
            IResInfo res = TryGet(path);
            if (res != null)
            {
                if (user == target)
                {
                    Debug.LogWarningFormat("不能使用和ResLoading传入一样的Object对象,这样会造成引用计数错误异常.");
                }

                if (res.IsDone && string.IsNullOrEmpty(res.Error))
                {
                    res.Require(user);
                    return res.Obj;
                }
            }

            Debug.LogErrorFormat("ResLoading.Get failed, path:{0} user:{1} state:{2} error:{3}", path, user,
                res.State, res.Error);
            return null;
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="user"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public void LoadAsync(string path, Object user, Action<Object> callback)
        {
            if (!string.IsNullOrEmpty(Error))
            {
                Debug.LogErrorFormat("load error : {0} , path:{1}  user:{2}", Error, path, user);
            }

            LoadAsync(path, typeof(Object), (_asset) =>
            {
                _asset.Require(user);
                if (callback != null)
                    callback.Invoke(_asset.Obj);
            }, (_asset) =>
            {
                if (callback != null)
                    callback.Invoke(null);
            });
        }

        /// <summary>
        /// 异步加载 [注意:]
        ///     1. fullPath = Assets/...路径.后缀
        ///     2. 回调中 Asset获取对象是 asset.Require(Object);
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="type"></param>
        /// <param name="onCompleted"></param> 
        /// <param name="user"></param>
        public void LoadAsync(string fullPath, Type loadType, Action<IResInfo> onSucceed, Action<IResInfo> onFailed)
        {
            Asset asset = TryGet(fullPath);
            asset.completed += (p) =>
            {
                if (string.IsNullOrEmpty(asset.Error) && asset.Obj)
                {
                    if (onSucceed != null)
                    {
                        onSucceed.Invoke(asset);
                    }
                }
                else
                {
                    string error = string.Format(
                        "res error RES.LoadAsync, fullPath: {0} , asset: {1} , loadState: {2} , error message: {3}.",
                        fullPath, asset.Obj, asset.State, asset.Error);
                    if (onFailed != null)
                    {
                        onFailed.Invoke(asset);
                    }
                }
            };
            asset.Load();
        }

// ************************************************************************

        static Dictionary<Object, ResLoading> cacheResLoadingDic = new Dictionary<Object, ResLoading>();

        public static ResLoading Get(Object target)
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("You cannot use this method 'ResMgr.GetResLoading' at non-runtime " + target);
                return null;
            }

            ResLoading resLoading;
            if (!cacheResLoadingDic.TryGetValue(target, out resLoading))
            {
                resLoading = new ResLoading(target);
            }

            return resLoading;
        }
    }
}