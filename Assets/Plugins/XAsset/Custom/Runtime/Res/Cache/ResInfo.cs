using Plugins.XAsset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.XAsset
{
    public class ResInfo : IResInfo
    {
        Asset asset;

        public ResInfo(string _err)
        {
            error = _err;
        }

        public ResInfo(Asset _asset)
        {
            asset = _asset;
        }

        public Object Asset
        {
            get { return asset != null ? asset.asset : null; }
        }

        public string Name
        {
            get { return asset != null ? asset.name : string.Empty; }
        }

        public int RefCount
        {
            get { return asset != null ? asset.refCount : 0; }
        }

        /// <summary>
        /// 引用对象
        /// </summary>
        /// <param name="user"></param>
        public void Require(Object user)
        {
            if (asset != null)
                asset.Require(user);
        }

        /// <summary>
        /// 归还对象
        /// </summary>
        /// <param name="user"></param>
        public void Dequire(Object user)
        {
            if (asset != null)
                asset.Dequire(user);
        }

        public void Release()
        {
            if (asset != null)
                asset.Release();
        }

        public string error { get; private set; }
    }
}