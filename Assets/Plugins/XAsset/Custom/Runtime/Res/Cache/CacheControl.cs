using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilText = GameFramework.Utility.Text;
using Object = UnityEngine.Object;

namespace Plugins.XAsset
{
    public class CacheControl
    {
        private Dictionary<string, ResCacheGroup> groupDic = new Dictionary<string, ResCacheGroup>();

        private ResCacheGroup TryGetGroup(string groupName)
        {
            ResCacheGroup group;
            if (!groupDic.TryGetValue(groupName, out group))
            {
                GameObject go = new GameObject(groupName);
                go.transform.SetParent(ResMgr.Instance.transform);
                group = go.AddComponent<ResCacheGroup>();
                groupDic.Add(groupName, group);
            }

            return group;
        }

        private void AddCache(string groupName, IResInfo res)
        {
            ResCacheGroup group = TryGetGroup(groupName);
            group.AddReady(res);
        }

        /// <summary>
        /// 加载缓存
        /// </summary>
        /// <param name="loadParams"></param>
        /// <param name="user">当传入空包对象,加载成功/失败回调null ,当不为空时 加载成功返回对象</param>
        /// <param name="onComplete"></param>
        /// <param name="groupName"></param>
        public void LoadCache(LoadParam loadParams, Object user, Action<Object> onComplete, string groupName)
        {
            ResCacheGroup group = TryGetGroup(groupName);
            group.Add(loadParams, () =>
            {
                if (onComplete != null)
                {
                    onComplete(user ? group.Get(loadParams.fullPath, user) : null);
                }
            });
        }

        public void LoadCache(LoadParam[] loadParams, Action onComplete, LoadingCache onLoading, string groupName)
        {
            if (loadParams == null || loadParams.Length == 0)
            {
                if (onComplete != null)
                {
                    onComplete.Invoke();
                } 
                return;
            }

            ResCacheGroup group = TryGetGroup(groupName);
            group.Add(loadParams, onComplete, onLoading);
        }

        public void UnloadCache(string groupName)
        {
            ResCacheGroup group = TryGetGroup(groupName);
            group.UnloadAll();
        }

        public T GetCache<T>(string fullPath, Object user, string groupName) where T : Object
        {
            ResCacheGroup group = TryGetGroup(groupName);
            return group.Get(fullPath, user) as T;
        }
    }
}