using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using LoadType = Plugins.XAsset.LoadType;
using UtilText = GameFramework.Utility.Text;
using System;


namespace Plugins.XAsset
{
    public class LoadParam
    {
        /// <summary>
        /// 完整路径
        /// </summary>
        public string fullPath;

        /// <summary>
        /// 加载类型
        /// </summary>
        public Type assetType = typeof(Object);

        /// <summary>
        /// 是否实例化
        /// </summary>
        public bool isInstant = false;

        public LoadParam()
        {
        }

        public LoadParam(string path, LoadType loadType)
        {
            fullPath = ResExtend.GetFullPath(path, loadType);
        }
    }
}