using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.XAsset
{
    public class VersionInfo
    {

        /// <summary>
        /// 更新数量
        /// </summary>
        public int updateCount;

        /// <summary>
        /// 总更新大小 [暂时无作用]
        /// </summary>
        public long totalUpdateLength;

        /// <summary>
        /// 是否需要更新
        /// </summary>
        public bool IsUpdate
        {
            get
            {
                return updateCount > 0;
            }
        }
    }
}