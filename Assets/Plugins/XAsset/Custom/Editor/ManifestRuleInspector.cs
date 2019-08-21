using Plugins.XAsset.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Plugins.XAsset.Editor
{
    using Editor = UnityEditor.Editor;

    [CustomEditor(typeof(ManifestRule))]
    public class ManifestRuleInspector : Editor
    {
        private SerializedObject _target;
        private string _path;

        private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

        const string AssetRoot = Constnat.AssetRoot;
        string fullAssetRoot;
        string addIgnorePath = AssetRoot;
        string addRulePath = AssetRoot;
        bool isSelect = false;
        BuildType selectBuildType = BuildType.Package;
        List<string> unAddFolder = new List<string>();
        ManifestRule t;

        void OnEnable()
        {
            _target = new SerializedObject(target);
            fullAssetRoot = AssetRoot.Replace("Assets", Application.dataPath);
        }

        void RefreshUnAdd()
        { 
            unAddFolder.Clear();
            GetSubFolder(fullAssetRoot, ref unAddFolder);
        }

        private void GetSubFolder(string folder, ref List<string> list)
        {
            UtilIO.GetSubFolder(folder, ref list, (p) =>
            {
                if (t.ignorePaths.Exists((o) => { return o == p; }))
                {
                    return false;
                }
                if (t.ruleInfos.Exists((o) => { return o.path == p; }))
                {
                    return false;
                }
                return true;
            });
        }

        public override void OnInspectorGUI()
        {
            t = (ManifestRule)target;

            //base.OnInspectorGUI();
            _target.Update();

            EditorGUILayout.BeginVertical();
            {
                //manifestRule.ignorePaths.Sort();
                //manifestRule.ruleInfos.Sort();

                EditorGUILayout.LabelField("Current Variant", t.version ?? "<Unknwon>");
                EditorGUILayout.LabelField("Resource Version", t.resourceVersion.ToString());

                if (Selection.activeObject)
                {
                    string selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    EditorGUILayout.LabelField("Selection Object Path", selectionPath);
                }
                DrawAllFolder("Un Add Folder");
                DrawIgnoreList("Ignore Paths");
                DrawRuleInfo("Rule Infos");
            }
            EditorGUILayout.EndVertical();

            _target.ApplyModifiedProperties();

            Repaint();

        }

        private void DrawAllFolder(string fullName)
        {
            bool currentState = GetState(fullName);

            if (currentState)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUI.indentLevel++; 

                    EditorGUILayout.LabelField("Count", unAddFolder.Count.ToString());
                    if (GUILayout.Button("刷新"))
                    {
                        RefreshUnAdd();
                    }

                    if (unAddFolder.Count > 0)
                    {
                        for (int i = 0; i < unAddFolder.Count; i++)
                        {
                            string folder = unAddFolder[i];
                            currentState = GetState("Folder:" + folder);
                            if (currentState)
                            {
                                if (GUILayout.Button("Add Rule"))
                                {
                                    t.AddRule(folder);
                                    RefreshUnAdd();
                                    break;
                                }
                                if (GUILayout.Button("Add Ignore"))
                                {
                                    t.AddIgnore(folder);
                                    RefreshUnAdd();
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("List is Empty ...");
                    }

                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();
            }
        }

        private void DrawIgnoreList(string fullName)
        {
            bool currentState = GetState(fullName);

            if (currentState)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUI.indentLevel++;

                    GUILayout.Label("请输入忽略路径:   [!注意 Assets/Resource/开头]");
                    addIgnorePath = GUILayout.TextField(addIgnorePath);
                    if (addIgnorePath != AssetRoot)
                    {
                        if (GUILayout.Button("Add Ignore Path"))
                        {
                            if (!t.ignorePaths.Exists((p) => { return p == addIgnorePath; }))
                            {
                                t.ignorePaths.Add(addIgnorePath);
                            }
                            addIgnorePath = AssetRoot;
                        }
                    }

                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.LabelField("Count", t.ignorePaths.Count.ToString());
                    if (GUILayout.Button("Clean All"))
                    {
                        t.ignorePaths.Clear();
                    }
                    if (t.ignorePaths.Count > 0)
                    {
                        for (int i = 0; i < t.ignorePaths.Count; i++)
                        {
                            var value = t.ignorePaths[i];
                            currentState = GetState("Path:" + value);
                            if (currentState)
                            {
                                if (GUILayout.Button("Remove"))
                                {
                                    t.ignorePaths.Remove(value);
                                    break;
                                }
                                if (GUILayout.Button("Add Rule"))
                                {
                                    t.AddRule(value);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("List is Empty ...");
                    }

                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();
            }
        }

        private void DrawRuleInfo(string fullName)
        {
            bool currentState = GetState(fullName);

            if (currentState)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUI.indentLevel++;

                    GUILayout.Label("请输入规则路径:   [!注意 Assets/Resource/开头]");
                    addRulePath = GUILayout.TextField(addRulePath);
                    if (string.IsNullOrEmpty(addRulePath))
                    {
                        addRulePath = AssetRoot;
                    }
                    if (addRulePath != AssetRoot)
                    {
                        if (GUILayout.Button("Add Rule Path"))
                        {
                            if (!t.ruleInfos.Exists((p) => { return p.path == addRulePath; }))
                            {
                                t.ruleInfos.Add(new RuleInfo() { path = addRulePath });
                            }
                            addRulePath = AssetRoot;
                        }
                    }

                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.LabelField("Count", t.ruleInfos.Count.ToString());
                    if (GUILayout.Button("Clean All"))
                    {
                        t.ruleInfos.Clear();
                    }
                    if (t.ruleInfos.Count > 0)
                    {
                        isSelect = EditorGUILayout.Foldout(isSelect, "开启筛选");
                        if (isSelect)
                        {
                            selectBuildType = (BuildType)EditorGUILayout.EnumPopup("查看 BuildType = ", selectBuildType);
                        }

                        EditorGUILayout.Separator();

                        for (int i = 0; i < t.ruleInfos.Count; i++)
                        {
                            var value = t.ruleInfos[i];
                            if (isSelect && value.buildType != selectBuildType)
                            {
                                continue;
                            }
                            currentState = GetState("Path:" + value.path);
                            if (currentState)
                            {
                                EditorGUILayout.BeginVertical("box");
                                {
                                    value.ruleType = (RuleType)EditorGUILayout.EnumPopup("RuleType: ", value.ruleType);
                                    value.buildType = (BuildType)EditorGUILayout.EnumPopup("BuildType: ", value.buildType);

                                    if (GUILayout.Button("Remove"))
                                    {
                                        t.RemoveRule(value.path);
                                        break;
                                    }
                                    if (new DirectoryInfo(value.path).GetDirectories().Length > 0)
                                    {
                                        if (GUILayout.Button("拆分子目录并添加"))
                                        {
                                            List<string> list = new List<string>();
                                            GetSubFolder(value.path, ref list);
                                            list.ForEach((_path) =>
                                            {
                                                t.AddRule(_path);
                                            });
                                            break;
                                        }

                                        if (GUILayout.Button("删除所有子目录"))
                                        {
                                            t.ruleInfos.RemoveAll((p) => { return p.path.Contains(value.path) && p.path != value.path; });
                                            break;
                                        }
                                    }
                                }
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.Separator();
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("List is Empty ...");
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();
            }
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
