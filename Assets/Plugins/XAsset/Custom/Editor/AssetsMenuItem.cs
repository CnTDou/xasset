using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Plugins.XAsset.Editor
{
    public static partial class AssetsMenuItem
    {
        public static void _BuildManifest()
        {
            _BuildManifest(BuildType.Package);
        }

        [MenuItem("Assets/AssetBundles/生成 Network 配置", true, 1000)]
        public static void _BuildNetworkManifest()
        {
            _BuildManifest(BuildType.Network);
        }

        private static void _BuildManifest(BuildType buildType)
        {
            EditorUtility.DisplayProgressBar("Build Manifest", "Start", 0f);

            Settings settings = BuildScript.GetSettings();
            string assetRootPath = settings.assetRootPath;
            AssetsManifest assetsManifest = BuildScript.GetManifest();
            EditorUtility.DisplayProgressBar("Build Manifest", "Read ManifestRule", 0f);
            ManifestRule manifestRule = BuildScript.GetAsset<ManifestRule>(Constnat.ManifestRulePath);
            EditorUtility.DisplayProgressBar("Build Manifest", "Read ManifestRule", 1f);

            assetsManifest.downloadURL = string.Empty;
            assetsManifest.assets = new AssetData[0];
            assetsManifest.dirs = new string[0];
            assetsManifest.bundles = new string[0];

            int number = 0;
            for (int i = 0; i < manifestRule.ruleInfos.Count; i++)
            {
                var ruleInfo = manifestRule.ruleInfos[i];
                var path = ruleInfo.path;

                EditorUtility.DisplayCancelableProgressBar("Build Manifest", path, i * 1f / manifestRule.ruleInfos.Count);

                if (ruleInfo.buildType != buildType)
                {
                    continue;
                }
                number++;

                switch (ruleInfo.ruleType)
                {
                    case RuleType.Dir:
                        SetAssetsWithDir(path, assetsManifest);
                        break;
                    case RuleType.File:
                        SetAssetsWithFile(path, assetsManifest);
                        break;
                    case RuleType.FileName:
                        SetAssetsWithName(path, assetsManifest);
                        break;
                    default:
                        break;
                }
            }

            EditorUtility.ClearProgressBar();

            EditorUtility.SetDirty(assetsManifest);
            AssetDatabase.SaveAssets();

            Debug.LogFormat("build manifest complete , assets count : {0} , bundles count : {1} .",
                assetsManifest.assets.Length, assetsManifest.bundles.Length);
        }

        private static void SetAssetsWithDir(string path, AssetsManifest assetsManifest)
        {
            List<FileInfo> fileInfos = UtilIO.GetFileInfoByFolder(path, SearchOption.AllDirectories);
            var assetBundleName = TrimedAssetBundleName(Path.GetDirectoryName(path).Replace("\\", "/")) + "_g";

            for (int i = 0; i < fileInfos.Count; i++)
            {
                path = UtilIO.GetUnityAssetPath(fileInfos[i].FullName);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null, assetsManifest);
            }
        }

        private static void SetAssetsWithFile(string path, AssetsManifest assetsManifest)
        {
            List<FileInfo> fileInfos = UtilIO.GetFileInfoByFolder(path, SearchOption.AllDirectories);
            var assetBundleName = TrimedAssetBundleName(Path.GetDirectoryName(path).Replace("\\", "/"));

            for (int i = 0; i < fileInfos.Count; i++)
            {
                path = UtilIO.GetUnityAssetPath(fileInfos[i].FullName);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;

                var dir = Path.GetDirectoryName(path);
                var name = Path.GetFileNameWithoutExtension(path);
                if (dir == null)
                    continue;
                dir = dir.Replace("\\", "/") + "/";
                if (name == null)
                    continue;

                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null, assetsManifest);
            }
        }

        private static void SetAssetsWithName(string path, AssetsManifest assetsManifest)
        {
            List<FileInfo> fileInfos = UtilIO.GetFileInfoByFolder(path, SearchOption.AllDirectories);
            var assetBundleName = Path.GetFileNameWithoutExtension(path);

            for (int i = 0; i < fileInfos.Count; i++)
            {
                path = UtilIO.GetUnityAssetPath(fileInfos[i].FullName);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null, assetsManifest);
            }
        }


        [MenuItem("Tools/AssetBundles/重置 Manifest Rule")]
        private static void RefreshManifest()
        {
            var settings = BuildScript.GetSettings();
            string assetRootPath = settings.assetRootPath;
            ManifestRule manifestRule = BuildScript.GetAsset<ManifestRule>(Constnat.ManifestRulePath);

            DirectoryInfo dirInfo = new DirectoryInfo(assetRootPath);
            DirectoryInfo[] dirs = dirInfo.GetDirectories();

            manifestRule.version = Application.version;
            manifestRule.resourceVersion = 1;

            for (int i = 0; i < dirs.Length; i++)
            {
                string path = UtilIO.GetUnityAssetPath(dirs[i].FullName) + "/";

                if (manifestRule.ruleInfos.Exists((p) => { return p.path == path && p.buildType == BuildType.Network; }) ||
                    manifestRule.ignorePaths.Exists((p) => { return p == path; }))
                {
                    continue;
                }

                if (!manifestRule.ruleInfos.Exists((p) => { return p.path == path; }))
                {
                    manifestRule.ruleInfos.Add(new RuleInfo() { path = path, ruleType = RuleType.Dir });
                }
            }
            EditorUtility.SetDirty(manifestRule);
            AssetDatabase.SaveAssets();
        }

        [MenuItem(@"Tools/AssetBundles/生成 Package 资源包 并 Copy")]
        public static void BuildPackageAssetBundle()
        {
            _BuildManifest();
            BuildAssetBundles();
            CopyAssetBundles();
        }

        [MenuItem("Tools/AssetBundles/生成 Network 资源包 并 上传")]
        public static void BuildNetworkAssetBundle()
        {
            _BuildNetworkManifest();
            BuildAssetBundles();
            UploadAssetBundles();
        }

        private static void UploadAssetBundles()
        {

        }
    }
}
