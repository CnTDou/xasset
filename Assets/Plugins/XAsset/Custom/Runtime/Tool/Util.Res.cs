using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plugins.XAsset
{
    public static partial class Util
    {
        public static string GetSuffix(LoadType loadType)
        {
            switch (loadType)
            {
                case LoadType.AUDIO_CLIP_OGG: return ".ogg";
                case LoadType.AUDIO_CLIP_WAV: return ".wav";
                case LoadType.AUDIO_CLIP_MP3: return ".mp3";
                case LoadType.ANIMATION_CLIP: return ".anim";
                case LoadType.SCENE: return ".unity";
                case LoadType.NONE:
                    return string.Empty;
            }

            return GameFramework.Utility.Text.Format(".{0}", loadType.ToString().ToLower());
        }

        public static Type GetType(LoadType loadType)
        {
            Type _type = typeof(UnityEngine.Object);
            switch (loadType)
            {
                case LoadType.PREFAB:
                    _type = typeof(GameObject);
                    break;
                case LoadType.JPG:
                case LoadType.JPEG:
                case LoadType.PNG:
                    _type = typeof(Texture2D);
                    break;
                case LoadType.AUDIO_CLIP_OGG:
                case LoadType.AUDIO_CLIP_WAV:
                case LoadType.AUDIO_CLIP_MP3:
                    _type = typeof(AudioClip);
                    break;
                case LoadType.ANIMATION_CLIP:
                    _type = typeof(AnimationClip);
                    break;
                case LoadType.NONE:
                case LoadType.SCENE:
                default:
                    _type = typeof(UnityEngine.Object);
                    break;
            }

            return _type;
        }

        public static void Log(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
        }

        public static T EditorGetAsset<T>(string path) where T : ScriptableObject
        {
#if UNITY_EDITOR
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                UnityEditor.AssetDatabase.CreateAsset(asset, path);
                UnityEditor.AssetDatabase.SaveAssets();
            } 
            return asset;
#else
            return  default(T)
#endif
        }
    }
}