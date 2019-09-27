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
        private string _path;

        private readonly HashSet<string> m_OpenedItems = new HashSet<string>();

        const string AssetRoot = Constnat.AssetRoot;
        string fullAssetRoot;
        string addIgnorePath = AssetRoot;
        string addRulePath = AssetRoot;
        bool isSelect = false;
        BuildType selectBuildType = BuildType.Package;
        List<string> unAddFolderOrFiles = new List<string>();
        ManifestRule t;

        bool isChanged = false;

        void OnEnable()
        {
            t = (ManifestRule)target;
            fullAssetRoot = AssetRoot.Replace("Assets", Application.dataPath);
        }

        void RefreshUnAdd()
        {
            unAddFolderOrFiles.Clear();
            GetSubFolder(fullAssetRoot, ref unAddFolderOrFiles);

            Util.GetSubPath(fullAssetRoot, ref unAddFolderOrFiles, (p) =>
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

        private void GetSubFolder(string folder, ref List<string> list)
        {
            Util.GetSubPath(folder, ref list, (p) =>
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
            //base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            {
                if (string.IsNullOrEmpty(t.version))
                    t.version = Application.version;

                t.version = EditorGUILayout.TextField("Current Variant", t.version);
                t.resourceVersion = EditorGUILayout.IntField("Resource Version", t.resourceVersion);

                if (Selection.activeObject)
                {
                    string selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    EditorGUILayout.LabelField("Selection Object Path", selectionPath);
                }
                if (isChanged)
                {
                    GUILayout.Box("请按Ctrl+S or Commod+s 保存.");
                }
                DrawAllFolder("Un Add Folder");
                DrawIgnoreList("Ignore Paths");
                DrawRuleInfo("Rule Infos");
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed || isChanged)
            {
                isChanged = false;
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawAllFolder(string fullName)
        {
            bool currentState = GetState(fullName);

            if (currentState)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.LabelField("Count", unAddFolderOrFiles.Count.ToString());
                    if (GUILayout.Button("刷新"))
                    {
                        RefreshUnAdd();
                    }

                    if (unAddFolderOrFiles.Count > 0)
                    {
                        for (int i = 0; i < unAddFolderOrFiles.Count; i++)
                        {
                            string item = unAddFolderOrFiles[i];
                            currentState = GetState("Un Folder Or File List:" + item);
                            if (currentState)
                            {
                                if (GUILayout.Button("Add Rule"))
                                {
                                    isChanged = true;
                                    t.AddRule(item);
                                    RefreshUnAdd();
                                    break;
                                }
                                if (GUILayout.Button("Add Ignore"))
                                {
                                    isChanged = true;
                                    t.AddIgnore(item);
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
                            isChanged = true;
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
                        isChanged = true;
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
                                    isChanged = true;
                                    t.ignorePaths.Remove(value);
                                    break;
                                }
                                if (GUILayout.Button("Add Rule"))
                                {
                                    isChanged = true;
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
                    if (GUILayout.Button("Add Rule Path"))
                    {
                        isChanged = true;
                        if (!t.ruleInfos.Exists((p) => { return p.path == addRulePath; }))
                        {
                            t.ruleInfos.Add(new RuleInfo() { path = addRulePath });
                        }
                        addRulePath = AssetRoot;
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
                        isChanged = true;
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
                                        isChanged = true;
                                        t.RemoveRule(value.path);
                                        break;
                                    }
                                    if (GUILayout.Button("Add Ignore"))
                                    {
                                        isChanged = true;
                                        t.RemoveRule(value.path);
                                        t.AddIgnore(value.path);
                                        break;
                                    }
                                    DrawSubDir(value.path);

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

        private void DrawSubDir(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                DirectoryInfo[] sub = dir.GetDirectories();
                if (sub.Length == 0 || (sub.Length == 1 && Util.GetUnityAssetPath(sub[0].FullName) == path))
                {
                    return;
                }

                bool currentState = GetState("Sub Directory:" + path);
                if (currentState)
                {
                    EditorGUI.indentLevel++;
                    if (dir.Exists)
                    {
                        if (sub.Length > 0)
                        {
                            if (GUILayout.Button("删除所有子目录"))
                            {
                                isChanged = true;
                                t.ruleInfos.RemoveAll((p) => { return p.path.Contains(path) && p.path != path; });
                            }

                            List<string> list = new List<string>();
                            GetSubFolder(path, ref list);
                            if (list.Count > 0)
                            {
                                if (GUILayout.Button("拆分子目录并添加"))
                                {
                                    isChanged = true;
                                    if (list.Count > 0)
                                    {
                                        list.ForEach((_path) =>
                                        {
                                            t.AddRule(_path);
                                        });
                                    }
                                }

                                list.ForEach((_path) =>
                                {

                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        EditorGUILayout.LabelField(_path);
                                        if (GUILayout.Button("Add Ignore"))
                                        {
                                            isChanged = true;
                                            t.AddIgnore(_path);
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
                                
                                });
                            }
                        }
                    }
                    EditorGUILayout.Separator();

                    EditorGUI.indentLevel--;
                }
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
