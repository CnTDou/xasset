
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Object = UnityEngine.Object;

namespace Plugins.XAsset.Editor
{
    [CustomEditor(typeof(ResCacheGroup))]
    public class ResCacheGroupInsppector : UnityEditor.Editor
    {
        private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

        private void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying)
                return;

            var t = (ResCacheGroup)target;

            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("全部折叠", EditorStyles.toolbarButton, GUILayout.MaxWidth(100)))
                    {
                        m_OpenedItems.Clear();
                    }
                    if (GUILayout.Button("UnloadAll", EditorStyles.toolbarButton, GUILayout.MaxWidth(100)))
                    {
                        t.UnloadAll();
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("IsDone : " + t.IsDone);
                EditorGUILayout.LabelField("Ready Count : " + t.ReadyCount);
                EditorGUILayout.LabelField("WaitLoad Count : " + t.WaitLoadCount);
                EditorGUILayout.LabelField("Loading Count : " + t.LoadingCount);
                EditorGUILayout.LabelField("Max Loading Count : " + t.MaxLoadingCount);


                DrawDictionary("ResMgr.groupDic ", Util.GetPrivateField<Dictionary<string, IResInfo>>(t, "readyDic"));

            }
            EditorGUILayout.EndVertical();

        }

        private void DrawDictionary<TKey, TValue>(string fullName, Dictionary<TKey, TValue> dic, bool isDrawValue = false)
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField(fullName + " Count:" + dic.Count);
                bool currentState = GetState(fullName + " Collections " + fullName.GetHashCode());
                if (currentState)
                {
                    if (dic.Count > 0)
                    {
                        foreach (var item in dic)
                        {
                            EditorGUILayout.LabelField(item.Key + " : " + item.Value);
                            if (isDrawValue)
                            {
                                DrawDictionary(item.Key + " " + item.Key.GetHashCode(), Util.GetFieIds(item.Value));
                            }
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
                    bool currentState = GetState(fullName + " Collections ");
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
            bool currentState = GetState(" Item : " + item + " hashCode:" + item.GetHashCode());
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
