using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoading
{
    float Progress { get; }
    
    bool IsDone { get; }
    
    string Error { get; }
}

/// <summary>
/// 进度控制器 
///     多个任务同时加载时获取 0-1的进度
///         支持 AsyncOperation 方式
///         支持 手动赋值方式
///         支持 IProgress接口方式
/// </summary>
public class ProgressControl : ILoading, IDisposable
{
    /// <summary>
    /// 设置 Progress
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public float this[string key]
    {
        set { SetProgress(key, value); }
    }

    /// <summary>
    /// 0-1 进度
    /// </summary>
    public float Progress
    {
        get
        {
            float _progress = 0;
            taskList.ForEach((p) => { _progress += p.Progress; });
            foreach (var item in taskDic)
            {
                _progress += item.Value.Progress;
            }

            _progress *= 1f / (taskList.Count + taskDic.Count);
            return _progress;
        }
    }

    /// <summary>
    /// 是否已完成
    /// </summary>
    public bool IsDone
    {
        get
        {
            if (taskList.Count > 0)
            {
                for (int i = 0; i < taskList.Count; i++)
                {
                    if (!taskList[i].IsDone)
                    {
                        return false;
                    }
                }
            }

            if (taskDic.Count > 0)
            {
                foreach (var item in taskDic)
                {
                    if (!item.Value.IsDone)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public string Error { get; set; }

    /// <summary>
    /// 完成数量
    /// </summary>
    public int DoneCount
    {
        get
        {
            int count = 0;
            taskList.ForEach((p) =>
            {
                if (p.IsDone)
                {
                    count++;
                }
            });
            foreach (var item in taskDic)
            {
                if (item.Value.IsDone)
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount
    {
        get { return taskList.Count + taskDic.Count; }
    }

    List<ILoading> taskList = new List<ILoading>();
    Dictionary<string, ProgressItme> taskDic = new Dictionary<string, ProgressItme>();

    /// <summary>
    /// 添加IProgress任务
    /// </summary>
    /// <param name="progress"></param>
    public void AddTask(ILoading progress)
    {
        taskList.Add(progress);
    }

    /// <summary>
    /// 添加AsyncOperation任务
    /// </summary>
    /// <param name="key"></param>
    /// <param name="async"></param>
    public void AddTask(string key, AsyncOperation async)
    {
        Get(key, new ProgressItme(async));
    }

    /// <summary>
    /// 添加手动赋值任务
    /// </summary>
    /// <param name="key"></param>
    public void AddTask(string key)
    {
        Get(key);
    }

    /// <summary>
    /// 设置进度
    /// </summary>
    /// <param name="key"></param>
    /// <param name="progress"></param>
    public void SetProgress(string key, float progress)
    {
        Get(key).SetProgress(progress);
    }

    /// <summary>
    /// 设置已完成
    /// </summary>
    /// <param name="key"></param>
    public void SetDone(string key)
    {
        Get(key).SetProgress(1f);
        Get(key).Done();
    }

    private ProgressItme Get(string key, ProgressItme defaultValue = null)
    {
        ProgressItme item;
        if (!taskDic.TryGetValue(key, out item))
        {
            item = defaultValue ?? new ProgressItme();
            taskDic.Add(key, item);
        }

        return item;
    }

    public class ProgressItme : ILoading
    {
        public ProgressItme()
        {
        }

        public ProgressItme(AsyncOperation async)
        {
            this.async = async;
        }

        private readonly AsyncOperation async;

        private float _progress;
        private bool _isDone;

        public float Progress
        {
            get { return async != null ? async.progress : _progress; }
            private set { _progress = value; }
        }

        public bool IsDone
        {
            get { return async != null ? async.isDone : _isDone; }
            private set { _isDone = value; }
        }
        public string Error { get; set; }

        public void SetProgress(float progress)
        {
            Progress = progress;
        }

        public void Done()
        {
            IsDone = true;
        }
    }

    public void Clean()
    {
        taskDic.Clear();
        taskList.Clear();
    }

    public void Dispose()
    {
        Clean();
    }
}