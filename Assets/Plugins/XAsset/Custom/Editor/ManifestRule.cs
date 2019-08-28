using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Plugins.XAsset.Editor
{
    public class ManifestRule : ScriptableObject
    {
        public string version;
        public int resourceVersion = 0;
        public List<string> ignorePaths = new List<string>();
        public List<RuleInfo> ruleInfos = new List<RuleInfo>();
        private void OnEnable()
        {
            ruleInfos.RemoveAll((info) =>
            {
                string path = info.path;
                return !CheckExists(path);
            });

            ignorePaths.RemoveAll((path) => !CheckExists(path));
        }

        private bool CheckExists(string path)
        {
            if (path.EndsWith("/") || path.EndsWith(@"\"))
            {
                if (Directory.Exists(path))
                {
                    Debug.LogWarning("rule path 不存在自动清理: " + path);
                    return false;
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    Debug.LogWarning("rule path 不存在自动清理: " + path);
                    return false;
                }
            }
            return true;
        }

        public void AddRule(string folder)
        {
            if (!ruleInfos.Exists((p) => { return p.path == folder; }))
            {
                int index = ignorePaths.IndexOf(folder);
                if (index != -1)
                {
                    ignorePaths.RemoveAt(index);
                }
                ruleInfos.Add(new RuleInfo() { path = folder });
            }
        }

        public void RemoveRule(string path)
        {
            int index = ruleInfos.FindIndex((p) => { return p.path == path; });
            if (index != -1)
            {
                ruleInfos.RemoveAt(index);
            }
        }

        public void AddIgnore(string folder)
        {
            if (!ignorePaths.Exists((p) => { return p == folder; }))
            {
                int index = ruleInfos.FindIndex((p) => { return p.path == folder; });
                if (index != -1)
                {
                    ruleInfos.RemoveAt(index);
                }
                ignorePaths.Add(folder);

            }
        }
    }

    [Serializable]
    public class RuleInfo
    {
        public string path;
        public RuleType ruleType = RuleType.Dir;
        public BuildType buildType = BuildType.Package;
    }

    public enum RuleType
    {
        RootDir,
        Dir,
        File,
        FileName,
    }

    public enum BuildType
    {
        Package,
        Network,
    }

}
