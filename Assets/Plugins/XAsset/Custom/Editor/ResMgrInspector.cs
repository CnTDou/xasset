using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Object = UnityEngine.Object;

namespace Plugins.XAsset.Editor
{
    [CustomEditor(typeof(ResMgr))]
    public class ResMgrInspector : UnityEditor.Editor
    {
        private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

        private void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var t = (ResMgr)target;

            if (!Application.isPlaying)
                return;

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("init", EditorStyles.toolbarButton, GUILayout.MaxWidth(100)))
                    {
                        t.Init(null, null);
                    }
                    if (GUILayout.Button("check", EditorStyles.toolbarButton, GUILayout.MaxWidth(100)))
                    {
                        ResMgr.Instance.CheckVersion((v) =>
                        {
                            ResMgr.Instance.StartUpdateRes(null, null, null);
                        }, null);
                    }
                    if (GUILayout.Button("clear", EditorStyles.toolbarButton, GUILayout.MaxWidth(100)))
                    {
                        if (EditorUtility.DisplayDialog("Clear", "Do you really want to  clear the all?", "OK", "Cancel"))
                            t.Clear();
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

                DrawDictionary("Update.Version ", t.updateControl._versions);

                DrawDictionary("Update.ServerVersion ", t.updateControl._serverVersions);

                DrawDictionary("Versions.data ", Versions.data); 

                DrawDictionary("Assets.bundleAssets ", Assets.bundleAssets);


                DrawList<List<Asset>, Asset>("Assets._assets ", typeof(Assets).ToString(), "_assets");

                DrawList<List<Asset>, Asset>("Assets._unusedAssets ", typeof(Assets).ToString(), "_unusedAssets"); 

                DrawList<List<Bundle>, Bundle>("Bundles._bundles  ", typeof(Bundles).ToString(), "_bundles");

                DrawList<List<Bundle>, Bundle>("Bundles._loading  ", typeof(Bundles).ToString(), "_loading");

                DrawList<List<Bundle>, Bundle>("Bundles._ready2Load  ", typeof(Bundles).ToString(), "_ready2Load");

                DrawList<List<Bundle>, Bundle>("Bundles._unusedBundles  ", typeof(Bundles).ToString(), "_unusedBundles");

            }
            EditorGUILayout.EndVertical();

        }

        private void DrawDictionary<TKey, TValue>(string fullName, Dictionary<TKey, TValue> dic)
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField(fullName + " Count:" + dic.Count);
                bool currentState = GetState(fullName + " Collections");
                if (currentState)
                {
                    if (dic.Count > 0)
                    {
                        foreach (var item in dic)
                        {
                            EditorGUILayout.LabelField(item.Key + " : " + item.Value);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Collections is Empty ...");
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }


        private void DrawList<T, TValue>(string fullName, string type, string fieidName) where T : IList<TValue>
        {
            bool currentState = GetState(" " + fullName + " Collections [注:请在不使用时关闭,展开时会有一定性能损耗.]");
            if (currentState)
            {
                var list = Util.GetStaticField<T>(type, fieidName); 
                DrawList<T, TValue>(fullName, list); 
            }
        }

        private void DrawList<T, TValue>(string fullName, T list) where T : IList<TValue>
        {
            EditorGUILayout.BeginVertical("box");
            {
                if (list == null)
                {
                    EditorGUILayout.LabelField(fullName + " Null.");
                }
                else
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.LabelField(fullName + " Count:" + list.Count);
                    bool currentState = GetState(fullName + " Collections");
                    if (currentState)
                    {
                        if (list.Count > 0)
                        {
                            EditorGUI.indentLevel++;

                            foreach (var item in list)
                            {
                                currentState = DrawItem(item);
                            }

                            EditorGUI.indentLevel--;

                        }
                        else
                        {
                            EditorGUILayout.LabelField("Collections is Empty ...");
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private bool DrawItem<TValue>(TValue item)
        {
            bool currentState = GetState("Item:" + item);
            if (currentState)
            {
                EditorGUI.indentLevel++;
                foreach (var _item in Util.GetFieIds(item))
                {
                    EditorGUILayout.LabelField(_item.Key + " : " + _item.Value);
                }
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            return currentState;
        }

        private bool GetState(string fullName, int indexLevel = -1)
        {
            int oldLevel = EditorGUI.indentLevel;
            if (indexLevel != -1)
            {
                EditorGUI.indentLevel = indexLevel;
            }
            bool lastState = m_OpenedItems.Contains(fullName);
            bool currentState = EditorGUILayout.Foldout(lastState, fullName);
            if (currentState != lastState)
            {
                if (currentState)
                {
                    m_OpenedItems.Add(fullName);
                }
                else
                {
                    m_OpenedItems.Remove(fullName);
                }
            }

            if (indexLevel != -1)
                EditorGUI.indentLevel = oldLevel;

            return currentState;
        }

    }
}
