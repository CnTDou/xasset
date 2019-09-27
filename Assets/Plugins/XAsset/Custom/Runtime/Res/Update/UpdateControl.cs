using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Plugins.XAsset
{
    public class UpdateControl
    {
        private const string VERSION_NAME = "versions.txt"; // 版本名称

        public Dictionary<string, string> _versions = new Dictionary<string, string>();
        public Dictionary<string, string> _serverVersions = new Dictionary<string, string>();
        private List<Download> _downloadCompletes = new List<Download>();       //下载完成
        private Queue<Download> _waltDownloadQueue = new Queue<Download>();  // 等待下载的
        private Download _download = null;                                  // 当前正在下载的

        private bool _isDownload = false;
        private Action _onUpdateSucceed;
        private Action<string> _onUpdateFailed;
        private Action<UpdatingInfo> _onProgress;
        private UpdatingInfo _updatingInfo = null;
        private long _updateSuccessLength = 0;

        public UpdateControl()
        {
        }

        /// <summary>
        /// 检查版本
        /// </summary>
        /// <param name="onSucceed">检查完成 不需要更新 </param>
        /// <param name="onFailed">检查失败</param> 
        /// <param name="onWaitUpdate">等待更新</param>
        public void CheckVersion(Action<VersionInfo> onSucceed, Action<string> onFailed)
        {
            var path = Utility.GetRelativePath4Update(VERSION_NAME);
            if (!File.Exists(path))
            {
                var asset = Assets.LoadAsync(Utility.GetWebUrlFromDataPath(VERSION_NAME), typeof(TextAsset));
                asset.completed += delegate
                {
                    if (asset.error != null)
                    {
                        onFailed(asset.error);
                        return;
                    }

                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    File.WriteAllText(path, asset.text);
                    LoadVersions(asset.text, onSucceed, onFailed);
                    asset.Release();
                };
            }
            else
            {
                LoadVersions(File.ReadAllText(path), onSucceed, onFailed);
            }
        }

        public void Clear()
        {
            var dir = Path.GetDirectoryName(Utility.updatePath);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            _waltDownloadQueue.Clear();
            _downloadCompletes.Clear();
            _versions.Clear();
            _serverVersions.Clear();

            Versions.Clear();

            var path = Utility.updatePath + Versions.versionFile;
            if (File.Exists(path))
                File.Delete(path);
        }

        public void Update()
        {
            if (_isDownload)
            {
                if (_download == null && _waltDownloadQueue.Count > 0)
                {
                    var download = _waltDownloadQueue.Dequeue();
                    download.Start();
                    _download = download;
                }

                if (_download != null)
                {
                    _download.Update();

                    if (!string.IsNullOrEmpty(_download.error))
                    {
                        if (_onUpdateFailed != null)
                        {
                            _onUpdateFailed.Invoke(string.Format("download fail , url: {0}, error: {1}", _download.url, _download.error));
                            _onUpdateFailed = null;
                        }
                        _isDownload = false;
                        return;
                    }
                    _updatingInfo.Fill(_download.len, _download.maxlen, _download.progress);
                    Debug.LogFormat("{0} , {1} , {2}", _download.url, _download.state, _download.isDone);
                    if (_download.isDone)
                    {
                        _updateSuccessLength += _download.len;
                        _downloadCompletes.Add(_download);
                        _download = null;
                    }
                }

                _updatingInfo.Fill(_updateSuccessLength, _downloadCompletes.Count);

                if (_waltDownloadQueue.Count == 0 && _download == null)
                {
                    _isDownload = false;

                    Complete();
                }
                else
                {
                    if (_onProgress != null)
                        _onProgress.Invoke(_updatingInfo);
                }
            }
        }

        private void Complete()
        {
            Versions.Save();

            if (_downloadCompletes.Count > 0)
            {
                for (int i = 0; i < _downloadCompletes.Count; i++)
                {
                    var item = _downloadCompletes[i];
                    if (!item.isDone)
                    {
                        break;
                    }
                    else
                    {
                        if (_serverVersions.ContainsKey(item.path))
                        {
                            _versions[item.path] = _serverVersions[item.path];
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();
                foreach (var item in _versions)
                {
                    sb.AppendLine(string.Format("{0}:{1}", item.Key, item.Value));
                }

                var path = Utility.GetRelativePath4Update(VERSION_NAME);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.WriteAllText(path, sb.ToString());

                Assets.Initialize(delegate
                {
                    if (_onUpdateSucceed != null)
                    {
                        _onUpdateSucceed.Invoke();
                        _onUpdateSucceed = null;
                    }
                    _onUpdateFailed = null;
                    _onProgress = null;
                }, (err) =>
                {
                    if (_onUpdateFailed != null)
                    {
                        _onUpdateFailed.Invoke(err);
                        _onUpdateFailed = null;
                    }
                    _onUpdateSucceed = null;
                    _onProgress = null;
                });
                string message = string.Format("{0} files has update.", _downloadCompletes.Count);
                Util.Log(message);
                return;
            }

            if (_onUpdateSucceed != null)
            {
                _onUpdateSucceed.Invoke();
                _onUpdateSucceed = null;
            }
            _onUpdateFailed = null;
            _onProgress = null;
            Util.Log("nothing to update.");
        }

        public void StartUpdateRes(Action onSucceed, Action<string> onFailed, Action<UpdatingInfo> onProgress)
        {
            _onUpdateSucceed = onSucceed;
            _onUpdateFailed = onFailed;
            _onProgress = onProgress;
            _isDownload = true;
        }

        private void LoadVersions(string text, Action<VersionInfo> onSucceed, Action<string> onFailed)
        {
            LoadText2Map(text, ref _versions);
            var asset = Assets.LoadAsync(Utility.GetDownloadURL(VERSION_NAME), typeof(TextAsset));
            asset.completed += delegate
            {
                if (asset.error != null)
                {
                    onFailed(asset.error);
                    return;
                }

                LoadText2Map(asset.text, ref _serverVersions);
                foreach (var item in _serverVersions)
                {
                    string ver;
                    if (!_versions.TryGetValue(item.Key, out ver) || !ver.Equals(item.Value))
                    {
                        var downloader = new Download();
                        downloader.url = Utility.GetDownloadURL(item.Key);
                        downloader.path = item.Key;
                        downloader.version = item.Value;
                        downloader.savePath = Utility.GetRelativePath4Update(item.Key);
                        _waltDownloadQueue.Enqueue(downloader);
                    }
                }
                if (_waltDownloadQueue.Count == 0)
                {
                    onSucceed(new VersionInfo());
                }
                else
                {
                    var downloader = new Download();
                    downloader.url = Utility.GetDownloadURL(Utility.GetPlatform());
                    downloader.path = Utility.GetPlatform();
                    downloader.savePath = Utility.GetRelativePath4Update(Utility.GetPlatform());
                    _waltDownloadQueue.Enqueue(downloader);

                    VersionInfo versionInfo = new VersionInfo()
                    {
                        updateCount = _waltDownloadQueue.Count,
                    };
                    _updatingInfo = new UpdatingInfo().Fill(versionInfo);
                    onSucceed(versionInfo);
                }
            };
        }

        private void LoadText2Map(string text, ref Dictionary<string, string> map)
        {
            map.Clear();
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split(':');
                    if (fields.Length > 1)
                    {
                        map.Add(fields[0], fields[1]);
                    }
                }
            }
        }

    }
}