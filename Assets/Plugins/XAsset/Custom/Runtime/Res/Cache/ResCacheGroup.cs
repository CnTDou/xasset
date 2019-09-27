using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Plugins.XAsset
{
    public class ResCacheGroup : MonoBehaviour
    {
        private Action _onLoadComplete;
        private LoadingCache _onLoadingCache;

        private int loadingCount;
        private bool isDone;
        Dictionary<string, IResInfo> readyDic = new Dictionary<string, IResInfo>();
        Queue<LoadParam> waitLoadAsset = new Queue<LoadParam>();

        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsDone
        {
            get { return isDone; }
        }

        /// <summary>
        /// 已经加载完成数量
        /// </summary>
        public int ReadyCount
        {
            get { return readyDic.Count; }
        }

        /// <summary>
        /// 等待加载数量
        /// </summary>
        public int WaitLoadCount
        {
            get { return waitLoadAsset.Count; }
        }

        /// <summary>
        /// 当前正在加载的数量
        /// </summary>
        public int LoadingCount
        {
            get { return loadingCount; }
        }

        /// <summary>
        /// 最大同时加载数量 =0则不作限制
        /// </summary>
        public int MaxLoadingCount { set; get; }

        /// <summary>
        /// 添加加载
        /// </summary>
        /// <param name="loadParam"></param> 
        /// <param name="onComplete"></param>
        public void Add(LoadParam loadParam, Action onComplete)
        {
            Load(loadParam.fullPath, loadParam.assetType, loadParam.isInstant,
                () =>
                {
                    if (onComplete != null)
                    {
                        onComplete.Invoke();
                    }
                });
        }

        public void Add(LoadParam[] loadParams, Action onComplete, LoadingCache onLoadingCache = null)
        {
            for (int i = 0; i < loadParams.Length; i++)
            {
                waitLoadAsset.Enqueue(loadParams[i]);
            }

            _onLoadComplete += onComplete;
            if (onLoadingCache != null)
                _onLoadingCache += onLoadingCache;
        }

        public void UnloadAll()
        {
            foreach (var item in readyDic)
            {
                item.Value.Dequire(gameObject);
                item.Value.Release();
            }

            readyDic.Clear();
        }

        private void Update()
        {
            isDone = waitLoadAsset.Count == 0 && loadingCount == 0;

            if (waitLoadAsset.Count > 0)
            {
                if (MaxLoadingCount == 0 || loadingCount < MaxLoadingCount)
                {
                    var loadParam = waitLoadAsset.Dequeue();
                    Load(loadParam.fullPath, loadParam.assetType, loadParam.isInstant, () =>
                    {
                        if (_onLoadingCache != null)
                        {
                            _onLoadingCache.Invoke(ReadyCount, ReadyCount + WaitLoadCount + LoadingCount);
                        }
                    });
                }
            }

            if (isDone)
            {
                _onLoadingCache = null;
                if (_onLoadComplete != null)
                {
                    _onLoadComplete.Invoke();
                    _onLoadComplete = null;
                }
            }
        }

        private void Load(string fullPath, Type assetType, bool isInstant, Action onColplete)
        {
            loadingCount++;
            ResMgr.Instance.LoadAsync(fullPath, assetType, (res) =>
            {
                loadingCount--;
                if (res != null && res.Asset)
                {
                    AddReady(res);
                    if (isInstant)
                    {
                        if (res.Asset is GameObject)
                        {
                            var obj = GameObject.Instantiate(
                                res.Asset as GameObject); // 实例化一下,防止除此显示延迟 卡顿等等 去老远老远地方 自个玩去 别影响显示 
                            obj.transform.localScale = Vector3.zero;
                            res.Require(obj);
                            GameObject.Destroy(obj, Time.deltaTime);
                        }

                        // other ...
                    }
                }
 
                if (onColplete != null)
                {
                    onColplete();
                }
            });
        }

        public void AddReady(IResInfo res)
        {
            if (!readyDic.ContainsKey(res.Name))
            {
                readyDic.Add(res.Name, res);
                res.Require(gameObject);
            }
        }

        public Object Get(string fullPath, Object user)
        {
            IResInfo res;
            if (readyDic.TryGetValue(fullPath, out res))
            {
                res.Require(user);
                return res.Asset;
            }

//            Util.Log("res warn ResCacheGroup.Get, path: {0} , user: {1} , groupName: {2} .", fullPath, user, name);
            return null;
        }
    }

    /// <summary>
    /// 加载缓存
    /// </summary>
    /// <param name="readyCount">已经就绪的数量</param>
    /// <param name="allCount">总数量</param>
    public delegate void LoadingCache(int readyCount, int allCount);
}