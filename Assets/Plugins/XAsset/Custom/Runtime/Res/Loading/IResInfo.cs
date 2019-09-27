using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Plugins.XAsset
{ 
    public interface IResInfo : IQuote
    {
        /// <summary>
        /// 直接获取资源
        /// </summary>
        Object Obj { get; }

        string Name { get; }
        
        string Error { get; }

        bool IsDone { get; }
 
        ResInfoState State { get; }
 
        event Action<Asset> completed;
    }
 
    public interface IQuote
    {
        /// <summary>
        /// 引用数量
        /// </summary>
        int RefCount { get; }

        /// <summary>
        /// 引用对象
        ///     计数++
        /// </summary>
        /// <param name="user"></param>
        void Require(Object user);

        /// <summary>
        /// 归还对象
        ///     计数--
        /// </summary>
        /// <param name="user"></param>
        void Dequire(Object user);

        /// <summary>
        /// 释放一个引用
        /// </summary>
        void Release();
    }
}