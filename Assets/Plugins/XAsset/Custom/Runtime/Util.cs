using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Plugins.XAsset.Editor
{
    public static class Util
    {
        private const string BackupFileSuffixName = ".bak";
        private static string UNITY_PATH = Util.GetRegularPath(Application.dataPath);
         
        #region ... Field

        public static T GetStaticField<T>(string typeName, string fieldName)
        {
            try
            {
                Type t = Type.GetType(typeName);
                if (t != null)
                {
                    var field = t.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                    if (field != null)
                        return (T)field.GetValue(t);
                }
                return default(T);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("{0} get static field '{1}' fail , error {2} ", typeName, fieldName, e.Message);
                return default(T);
            }
        }

        public static T GetPublicField<T>(object instance, string fieldName)
        {
            return GetField<T>(instance, fieldName, BindingFlags.Instance | BindingFlags.Public);
        }

        public static T GetPrivateField<T>(object instance, string fieldName)
        {
            return GetField<T>(instance, fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        }


        public static Dictionary<string, object> GetPublicFields(object instance)
        {
            return GetFieIds(instance, BindingFlags.Instance | BindingFlags.Public);
        }

        public static Dictionary<string, object> GetPrivateFields(object instance)
        {
            return GetFieIds(instance, BindingFlags.Instance | BindingFlags.NonPublic);
        }


        public static T GetField<T>(object instance, string fieldName, BindingFlags flag = BindingFlags.Default)
        {
            try
            {
                Type type = instance.GetType();
                FieldInfo field = flag == BindingFlags.Default ? type.GetField(fieldName) : type.GetField(fieldName, flag);
                if (field != null)
                    return (T)field.GetValue(instance);
                return default(T);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("{0} get field '{1}' fail , flag:{2} , error {3} ", instance, fieldName, flag, e.Message);
                return default(T);
            }
        }

        public static Dictionary<string, object> GetFieIds(object instance, BindingFlags flag = BindingFlags.Default)
        {
            try
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                Type type = instance.GetType();
                FieldInfo[] fields = flag == BindingFlags.Default ? type.GetFields() : type.GetFields(flag);
                if (fields != null && fields.Length > 0)
                {
                    foreach (var item in fields)
                    {
                        dic[item.Name] = item.GetValue(instance);
                    }
                }
                return dic;
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("{0} get fields '{1}' fail , flag:{2} , error {3} ", instance, flag, e.Message);
                return null;
            }
        }

        #endregion

        #region ... IO.File

        /// <summary> 在指定目录下创建文本文件</summary>
        public static void CreateTextFile(string filePath, string contents)
        {
            CheckFolder(filePath);
            File.WriteAllText(filePath, contents);
        }

        /// <summary> 在指定目录下创建二进制文件</summary>
        public static void CreateBytesFile(string filePath, byte[] bytes)
        {
            CheckFolder(filePath);
            File.WriteAllBytes(filePath, bytes);
        }

        /// <summary>获取指定路径的文件名，不包含后缀</summary>
        public static string GetFileName(string path)
        {
            path = path.Replace("\\", "/");
            if (path.IndexOf('/') >= 0)
            {
                if (path.IndexOf('.') > 0)
                    return path.Substring(path.LastIndexOf('/') + 1, path.LastIndexOf('.'));
                else
                    return path.Substring(path.LastIndexOf('/') + 1);
            }
            else
            {
                if (path.IndexOf('.') > 0)
                    return path.Substring(0, path.LastIndexOf('.'));
                else
                    return path;
            }
        }

        /// <summary>
        /// 删除删除文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool TryDeleteFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        #endregion

        #region ... IO.Folder

        /// <summary>
        /// 检查文件路径是否存储
        /// </summary>
        /// <param name="fileName">文件名称</param>
        public static void CheckFolder(string fileName)
        {
            string newDir = System.IO.Path.GetDirectoryName(fileName);
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);
        }

        /// <summary>
        /// 删除文件夹里面部分内容
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="ignore">忽略数组 注意 路径 为\  </param>
        public static void DeleteFolder(string folderPath, string[] ignore = null)
        {
            if (!Directory.Exists(folderPath))
            {
                return;
            }
            try
            {
                string[] fileNames = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                if (fileNames != null && fileNames.Length > 0)
                {
                    bool isIgnore = false;
                    foreach (string fileName in fileNames)
                    {
                        isIgnore = false;
                        if ((ignore != null && ignore.Length > 0))
                        {
                            if (Array.FindIndex(ignore, (p) => { return fileName.Contains(p); }) != -1)
                            {
                                isIgnore = true;
                            }
                        }
                        if (!isIgnore)
                            File.Delete(fileName);
                    }
                    RemoveEmptyFolder(folderPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("DeleteFolder error , directoryName : '{0}' ,  message: '{1}'  , StackTrace: '{2}'.", folderPath, e.Message, e.StackTrace);
            }
        }

        /// <summary>
        /// 拷贝文件夹
        /// </summary>
        /// <param name="sourceDirectoryName"></param>
        /// <param name="targetDirectoryName"></param>
        /// <param name="isForce"></param>
        public static void CopyFolder(string sourceDirectoryName, string targetDirectoryName, bool isForce = true)
        {
            try
            {
                string[] fileNames = Directory.GetFiles(sourceDirectoryName, "*", SearchOption.AllDirectories);
                foreach (string fileName in fileNames)
                {
                    string destFileName = Path.Combine(targetDirectoryName, fileName.Substring(sourceDirectoryName.Length));
                    FileInfo destFileInfo = new FileInfo(destFileName);
                    if (!destFileInfo.Directory.Exists)
                    {
                        destFileInfo.Directory.Create();
                    }

                    if (isForce && File.Exists(destFileName))
                    {
                        File.Delete(destFileName);
                    }

                    File.Copy(fileName, destFileName);
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("CopyFolder error , sourceDirectoryName : '{0}' , targetDirectoryName : '{1}' , message: '{2}'  , StackTrace: '{3}'.",
                    sourceDirectoryName, targetDirectoryName, e.Message, e.StackTrace);
            }

        }

        /// <summary>获取指定文件夹下文件信息</summary>
        public static List<FileInfo> GetFileInfoByFolder(string folderPath, SearchOption option, string searchPattern = "*")
        {
            List<FileInfo> fileInfos = new List<FileInfo>();
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            if (dirInfo.Exists)
            {
                FileInfo[] fis = dirInfo.GetFiles(searchPattern, option);
                if (fis.Length > 0)
                {
                    for (int i = 0; i < fis.Length; i++)
                    {
                        if (!fis[i].Name.EndsWith(".DS_Store") && !fis[i].Name.EndsWith(".meta"))
                        {
                            fileInfos.Add(fis[i]);
                        }
                    }
                }
            }
            return fileInfos;
        }

        public static void GetSubFolder(string folderPath, ref List<string> folder, Predicate<string> match)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            if (dirInfo.Exists)
            {
                foreach (var item in dirInfo.GetDirectories())
                {
                    if (item.FullName.EndsWith(".DS_Store") || item.FullName.EndsWith(".meta"))
                    {
                        continue;
                    }
                    string dir = GetUnityAssetPath(item.FullName);
                    if (match(dir) && !folder.Exists((p) => { return p == dir; }))
                        folder.Add(dir);
                }
            }
        }

        public static void GetSubFile(string folderPath, ref List<string> files, Predicate<string> match)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            if (dirInfo.Exists)
            {
                foreach (var item in dirInfo.GetFiles())
                {
                    if (item.FullName.EndsWith(".DS_Store") || item.FullName.EndsWith(".meta"))
                    {
                        continue;
                    }
                    string dir = GetUnityAssetPath(item.FullName);
                    if (match(dir) && !files.Exists((p) => { return p == dir; }))
                        files.Add(dir);
                }
            }
        }

        /// <summary>
        /// 移除空文件夹。
        /// </summary>
        /// <param name="directoryName">要处理的文件夹名称。</param>
        /// <returns>是否移除空文件夹成功。</returns>
        public static bool RemoveEmptyFolder(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
            {
                Debug.LogError("Directory name is invalid.");
                return false;
            }

            try
            {
                if (!Directory.Exists(directoryName))
                {
                    return false;
                }

                // 不使用 SearchOption.AllDirectories，以便于在可能产生异常的环境下删除尽可能多的目录
                string[] subDirectoryNames = Directory.GetDirectories(directoryName, "*");
                int subDirectoryCount = subDirectoryNames.Length;
                foreach (string subDirectoryName in subDirectoryNames)
                {
                    if (RemoveEmptyFolder(subDirectoryName))
                    {
                        subDirectoryCount--;
                    }
                }

                if (subDirectoryCount > 0)
                {
                    return false;
                }

                if (Directory.GetFiles(directoryName, "*").Length > 0)
                {
                    return false;
                }

                Directory.Delete(directoryName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ... IO.Path

        public static string GetUnityAssetPath(string path)
        {
            return Util.GetRegularPath(path).Replace(UNITY_PATH, "Assets");
        }

        /// <summary>
        /// 获取规范的路径。
        /// </summary>
        /// <param name="path">要规范的路径。</param>
        /// <returns>规范的路径。</returns>
        public static string GetRegularPath(string path)
        {
            if (path == null)
            {
                return null;
            }

            return path.Replace('\\', '/');
        }

        /// <summary>
        /// 获取连接后的路径。
        /// </summary>
        /// <param name="path">路径片段。</param>
        /// <returns>连接后的路径。</returns>
        public static string GetCombinePath(params string[] path)
        {
            if (path == null || path.Length < 1)
            {
                return null;
            }

            string combinePath = path[0];
            for (int i = 1; i < path.Length; i++)
            {
                combinePath = System.IO.Path.Combine(combinePath, path[i]);
            }

            return GetRegularPath(combinePath);
        }

        /// <summary>
        /// 获取远程格式的路径（带有file:// 或 http:// 前缀）。
        /// </summary>
        /// <param name="path">原始路径。</param>
        /// <returns>远程格式路径。</returns>
        public static string GetRemotePath(params string[] path)
        {
            string combinePath = GetCombinePath(path);
            if (combinePath == null)
            {
                return null;
            }

            return combinePath.Contains("://") ? combinePath : ("file:///" + combinePath).Replace("file:////", "file:///");
        }

        /// <summary>
        /// 获取带有后缀的资源名。
        /// </summary>
        /// <param name="resourceName">原始资源名。</param>
        /// <returns>带有后缀的资源名。</returns>
        public static string GetResourceNameWithSuffix(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new Exception("Resource name is invalid.");
            }

            return string.Format("{0}.dat", resourceName);
        }

        /// <summary>
        /// 获取带有 CRC32 和后缀的资源名。
        /// </summary>
        /// <param name="resourceName">原始资源名。</param>
        /// <param name="hashCode">CRC32 哈希值。</param>
        /// <returns>带有 CRC32 和后缀的资源名。</returns>
        public static string GetResourceNameWithCrc32AndSuffix(string resourceName, int hashCode)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new Exception("Resource name is invalid.");
            }

            return string.Format("{0}.{1:x8}.dat", resourceName, hashCode);
        }

        #endregion

        #region ... Unity.IO

        /// <summary> 复制文件夹到指定目录</summary>
        public static void CopyDirectory(string sourceDirectoryPath, string targetDirectoryPath, string searchPattern = "*.*", bool isDeleteExist = false)
        {
            string[] files = Directory.GetFiles(sourceDirectoryPath, searchPattern, SearchOption.AllDirectories);
            string file, newPath, newDir;
            for (int i = 0; i < files.Length; i++)
            {
                file = files[i];
                file = file.Replace("\\", "/");
                if (!file.EndsWith(".meta") && !file.EndsWith(".DS_Store"))
                {
                    newPath = file.Replace(sourceDirectoryPath, targetDirectoryPath);
                    newDir = System.IO.Path.GetDirectoryName(newPath);
                    if (!Directory.Exists(newDir))
                        Directory.CreateDirectory(newDir);
                    if (File.Exists(newPath))
                        if (isDeleteExist)
                            File.Delete(newPath);
                        else
                            continue;
                    if (Application.platform == RuntimePlatform.Android)
                        AndroidCopyFile(file, newPath);
                    else
                        File.Copy(file, newPath);
                }
            }
        }

        private static IEnumerator AndroidCopyFile(string sourceFilePath, string targetFilePath)
        {
            WWW www = new WWW("file://" + sourceFilePath);
            yield return www;
            File.WriteAllBytes(targetFilePath, UnicodeEncoding.UTF8.GetBytes(www.text));
        }

        #endregion

    }
}
