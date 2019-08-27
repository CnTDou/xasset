using Plugins.XAsset;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UtilText = GameFramework.Utility.Text;
 
public static class ResExtend
{

    #region ... Image.sprite

    public static void Load(this Image image, string path)
    {
        Asset asset = Assets.LoadAsync(path, typeof(Texture2D));
        asset.completed += (_asset) =>
        {
            if (string.IsNullOrEmpty(_asset.error) && _asset.asset)
            {
                Texture2D txt2d = _asset.asset as Texture2D;
                if (txt2d)
                {
                    image.sprite = Sprite.Create(txt2d, new Rect(0, 0, txt2d.width, txt2d.height), Vector2.zero);
                    ResListening.Add(image.gameObject, new ResInfo(asset));
                    return;
                }
            }
            ResMgr.OutLog("res error ResExtend.Load, path: {0} .", path);
            _asset.Release();
        };

    }

    public static void LoadJPEG(this Image image, string path)
    {
        ResMgr.Instance.LoadAsync(path, LoadType.JPEG, (res) =>
        {
            ExecuteComplete(image, res);
        });
    }

    public static void LoadJPG(this Image image, string path)
    {
        ResMgr.Instance.LoadAsync(path, LoadType.JPG, (res) =>
        {
            ExecuteComplete(image, res);
        });
    }

    public static void LoadPNG(this Image image, string path)
    {
        ResMgr.Instance.LoadAsync(path, LoadType.PNG, (res) =>
       {
           ExecuteComplete(image, res);
       });
    }

    private static void ExecuteComplete(Image image, IRes res)
    {
        Texture2D txt2d = res.Asset as Texture2D;
        if (txt2d)
        {
            ResListening.Add(image.gameObject, res);
            image.sprite = Sprite.Create(txt2d, new Rect(0, 0, txt2d.width, txt2d.height), Vector2.zero);
        }
    }

    #endregion

    #region ... AudioSource.clip

    public static void PlayClipOGG(this AudioSource audioSource, string path, bool isPlayOneShot = false)
    {
        ResMgr.Instance.LoadAsync(path, LoadType.AUDIO_CLIP_OGG, (res) =>
        {
            ExecuteComplete(audioSource, res, isPlayOneShot);
        });
    }

    public static void PlayClipWAV(this AudioSource audioSource, string path, bool isPlayOneShot = false)
    {
        ResMgr.Instance.LoadAsync(path, LoadType.AUDIO_CLIP_WAV, (res) =>
        {
            ExecuteComplete(audioSource, res, isPlayOneShot);
        });
    }

    public static void PlayClipMP3(this AudioSource audioSource, string path, bool isPlayOneShot = false)
    {
        ResMgr.Instance.LoadAsync(path, LoadType.AUDIO_CLIP_MP3, (res) =>
        {
            ExecuteComplete(audioSource, res, isPlayOneShot);
        });
    }

    private static void ExecuteComplete(AudioSource audioSource, IRes res, bool isPlayOneShot = false)
    {
        AudioClip clip = res.Asset as AudioClip;
        if (clip)
        {
            ResListening.Add(audioSource.gameObject, res);
            if (isPlayOneShot)
            { // todo: 此处播放完毕后 无释放
                audioSource.PlayOneShot(clip);
            }
            else
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
    }

    #endregion

}
