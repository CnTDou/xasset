using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 实体信息 Res 加载并创建的对象体
/// </summary>
public class EntityInfo : MonoBehaviour
{
    [HideInInspector] public int id; // 实例化后唯一Id 
    [HideInInspector] public string url; // load path
    [HideInInspector] public string group; // load group
    [HideInInspector] public Object asset;
      
}