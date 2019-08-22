using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Plugins.XAsset.Editor
{
    using Editor = UnityEditor.Editor;
    [CustomEditor(typeof(ResMgr))]
    public class ResMgrInspector : Editor
    {
        private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

        private void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var t = (ResMgr)target;
            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            {
                DrawBundleAssets();

            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBundleAssets()
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Bundle Assets  Count:" + Assets.bundleAssets.Count);
                bool currentState = GetState("Bundle Assets Collections");
                if (currentState)
                {
                    if (Assets.bundleAssets.Count > 0)
                    {
                        foreach (var item in Assets.bundleAssets)
                        {
                            EditorGUILayout.LabelField("    Asset Path : " + item.Key);
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
