using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class EntityControl
{
    private Dictionary<int, EntityInfo> entityDic = new Dictionary<int, EntityInfo>();
    static int sEntityId;

    private void LoadAsycn(string path, Object user, Action<GameObject> callback)
    {
        ResMgr.Instance.LoadPrefab(path,user,callback);
    }

    public int CreateEntity(string path, Transform parent, bool instantiateInWorldSpace,
        Action<GameObject> complete, string group)
    {
        int id = sEntityId++;
        LoadAsycn(path, parent, (asset) =>
        {
            if (asset && asset is GameObject)
            {
                GameObject entity = null;
                if (parent)
                {
                    entity = GameObject.Instantiate(asset as GameObject, parent, instantiateInWorldSpace);
                }
                else
                {
                    entity = GameObject.Instantiate(asset as GameObject);
                }

                EntityInfo entityInfo = entity.AddComponent<EntityInfo>();
                entityInfo.id = id;
                entityInfo.url = path;
                entityInfo.group = group;

                entityInfo.asset = asset;

                entityDic.Add(entityInfo.id, entityInfo);
                if (complete != null)
                    complete(entity);
            }
            else
            {
                Debug.LogErrorFormat("create entity fail '{0}'", path, parent);
                if (complete != null)
                    complete(null);
            }
        });
        return id;
    }

    public void DestroyEntity(GameObject _entity)
    {
        EntityInfo entity = _entity.GetComponent<EntityInfo>();
        if (entityDic.ContainsKey(entity.id))
        {
            if (entityDic[entity.id] == entity)
            {
                entityDic.Remove(entity.id);
            }
            else
            {
                Debug.LogWarningFormat(
                    "not destroy entity, entity info  disagree, current entityId '{0}' ,  cache entity '{0}' ", entity,
                    entityDic[entity.id]);
            }
        }

        Destroy(_entity);
    }

    public void DestroyEntityGroup(string group)
    {
        List<int> deleteList = new List<int>();

        foreach (var item in entityDic)
        {
            if (item.Value == null || item.Value.@group == group)
            {
                deleteList.Add(item.Key);
            }
        }

        EntityInfo info;
        for (int i = 0; i < deleteList.Count; i++)
        {
            info = entityDic[deleteList[i]];
            if (info)
            {
                info.asset = null;
                if (info.gameObject != null)
                    Destroy(info.gameObject);
            }

            entityDic.Remove(deleteList[i]);
        }

        deleteList.Clear();
        deleteList = null;
    }

    public void DestroyEntityAll()
    {
        List<int> deleteList = new List<int>(entityDic.Keys);

        EntityInfo info;
        for (int i = 0; i < deleteList.Count; i++)
        {
            info = entityDic[deleteList[i]];
            if (info)
            {
                info.asset = null;
                if (info.gameObject != null)
                    Destroy(info.gameObject);
            }

            entityDic.Remove(deleteList[i]);
        }

        entityDic.Clear();
        deleteList.Clear();
        deleteList = null;
    }

    private void Destroy(GameObject obj)
    {
        GameObject.Destroy(obj);
    }
}