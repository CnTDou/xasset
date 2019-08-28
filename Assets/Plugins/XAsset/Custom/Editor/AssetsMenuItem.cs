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
        [MenuItem("Tools/AssetBundles/创建所需要配置", false, 20)]
        public static void CreateConfig()
        {
            EditorUtility.SetDirty(BuildScript.GetSettings());
            EditorUtility.SetDirty(BuildScript.GetManifest());
            EditorUtility.SetDirty(BuildScript.GetAsset<ManifestRule>(Constnat.ManifestRulePath));
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/AssetBundles/生成 Package 配置 [根据Rule文件]", false, 21)]
        public static void _BuildManifestByRule()
        {
            _BuildManifest(BuildType.Package);
        }

        [MenuItem("Tools/AssetBundles/生成 Network 配置 [根据Rule文件]", false, 22)]
        public static void _BuildNetworkManifestByRule()
        {
            _BuildManifest(BuildType.Network);
        }

        [MenuItem(@"Tools/AssetBundles/生成 Package 资源包", false, 23)]
        public static void BuildPackage()
        {
            _BuildManifestByRule();
            BuildAssetBundles();
        }

        [MenuItem(@"Tools/AssetBundles/生成 Package 资源包 并 Copy to StreamingAssets", false, 24)]
        public static void BuildPackageAndCopy()
        {
            _BuildManifestByRule();
            BuildAssetBundles();
            CopyAssetBundles();
        }

        [MenuItem("Tools/AssetBundles/生成 Network 资源包 并 上传", false, 25)]
        public static void BuildNetwork()
        {
            _BuildNetworkManifestByRule();
            BuildAssetBundles();
            UploadAssetBundles();
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

            if (string.IsNullOrEmpty(assetsManifest.downloadURL))
                assetsManifest.downloadURL = BuildScript.GetServerURL();

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
                    case RuleType.RootDir:
                        SetAssetsWithDir(path, assetsManifest, true);
                        break;
                    case RuleType.Dir:
                        SetAssetsWithDir(path, assetsManifest, false);
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

        private static void SetAssetsWithDir(string path, AssetsManifest assetsManifest, bool isRootDir)
        {
            List<FileInfo> fileInfos = Util.GetFileInfoByFolder(path, SearchOption.AllDirectories);
            var assetBundleName = TrimedAssetBundleName(Path.GetDirectoryName(path).Replace("\\", "/")) + "_g";

            for (int i = 0; i < fileInfos.Count; i++)
            {
                path = Util.GetUnityAssetPath(fileInfos[i].FullName);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;
                if (!isRootDir)
                    assetBundleName = TrimedAssetBundleName(Path.GetDirectoryName(path).Replace("\\", "/")) + "_g";
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null, assetsManifest);
            }
        }

        private static void SetAssetsWithFile(string path, AssetsManifest assetsManifest)
        {
            List<FileInfo> fileInfos = Util.GetFileInfoByFolder(path, SearchOption.AllDirectories);

            for (int i = 0; i < fileInfos.Count; i++)
            {
                path = Util.GetUnityAssetPath(fileInfos[i].FullName);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;
                var dir = Path.GetDirectoryName(path);
                var name = Path.GetFileNameWithoutExtension(path);
                if (dir == null)
                    continue;
                dir = dir.Replace("\\", "/") + "/";
                if (name == null)
                    continue;

                var assetBundleName = TrimedAssetBundleName(Path.Combine(dir, name));
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null, assetsManifest);
            }
        }

        private static void SetAssetsWithName(string path, AssetsManifest assetsManifest)
        {
            List<FileInfo> fileInfos = Util.GetFileInfoByFolder(path, SearchOption.AllDirectories);

            for (int i = 0; i < fileInfos.Count; i++)
            {
                path = Util.GetUnityAssetPath(fileInfos[i].FullName);
                if (Directory.Exists(path) || path.EndsWith(".cs", System.StringComparison.CurrentCulture))
                    continue;
                var assetBundleName = Path.GetFileNameWithoutExtension(path);
                BuildScript.SetAssetBundleNameAndVariant(path, assetBundleName.ToLower(), null, assetsManifest);
            }
        }


        private static void UploadAssetBundles()
        {
            // todo: 上传操作
        }

    }
}
