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
        public string fullPath;
        public Type assetType;
        public bool isInstant;

        public LoadParam(string _path, LoadType loadType)
        {
            fullPath = UtilText.Format("{0}{1}.{2}", ResMgr.RES_ROOT_PATH, _path, ResMgr.GetSuffix(loadType));
            assetType = ResMgr.GetType(loadType);
        }

        public LoadParam(string _pathAndSuffix)
        {
            fullPath = UtilText.Format("{0}{1}", ResMgr.RES_ROOT_PATH, _pathAndSuffix);
            assetType = ResMgr.GetType(LoadType.NONE);
        }

    }
}