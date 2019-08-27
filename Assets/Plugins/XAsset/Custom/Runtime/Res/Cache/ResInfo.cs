using Plugins.XAsset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRes
{
    /// <summary>
    /// 直接获取资源
    /// </summary>
    Object Asset { get; }
    string Name { get; }

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

public class ResInfo : IRes
{
    Asset asset;

    public ResInfo(Asset _asset)
    {
        asset = _asset;
    }

    public Object Asset
    {
        get
        {
            return asset != null ? asset.asset : null;
        }
    }

    public string Name
    {
        get
        {
            return asset != null ? asset.name : string.Empty;
        }
    }

    public int RefCount
    {
        get
        {
            return asset != null ? asset.refCount : 0;
        }
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
}
