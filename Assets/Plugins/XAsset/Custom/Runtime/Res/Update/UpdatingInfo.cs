using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using System;

namespace Plugins.XAsset
{
    public class UpdatingInfo : IReference
    {

        /// <summary>
        /// 总更新数量
        /// </summary>
        public int TotalUpdateCount { get; private set; }

        /// <summary>
        /// 总更新成功数量
        /// </summary>
        public int TotalUpdateSuccessCount { get; private set; }


        /// <summary>
        /// 总更新大小
        /// </summary>
        public long TotalUpdateLength { get; private set; }

        /// <summary>
        /// 总更新成功大小
        /// </summary>
        public long TotalUpdateSuccessLength { get; private set; }


        /// <summary>
        /// 网络速度
        /// </summary>
        public float NetworkSpeed { get; private set; }


        /// <summary>
        /// 当前成功大小
        /// </summary>
        public long CurrentSuccessLength { get; private set; }

        /// <summary>
        /// 当前总大小
        /// </summary>
        public long CurrentTotalLength { get; private set; }

        /// <summary>
        /// 当前进度
        /// </summary>
        public float CurrentProgress { get; private set; }


        public void Clear()
        {
            TotalUpdateSuccessLength = 0;
            TotalUpdateSuccessCount = 0;
            NetworkSpeed = 0f;
        }

        public UpdatingInfo Fill(VersionInfo _versionInfo)
        {
            TotalUpdateCount = _versionInfo.updateCount;
            TotalUpdateLength = _versionInfo.totalUpdateLength;
            return this;
        }

        public void Fill(long len, long maxlen, float progress)
        {
            CurrentSuccessLength = len;
            CurrentTotalLength = maxlen;
            CurrentProgress = progress;
        }

        public void Fill(long successLength, int successCount)
        {
            TotalUpdateSuccessLength = successLength;
            TotalUpdateSuccessCount = successCount;
        }

    } 
}