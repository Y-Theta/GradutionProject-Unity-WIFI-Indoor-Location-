///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

public class ResourceManager {
    public static readonly object _lock = new object();

    #region Methods
    /// <summary>
    /// 从StreamingAssets中获取相应资源 异步
    /// </summary>
    public static async Task<T> GetResourceAsync<T>(string key, string name) where T : Object {
        return await Task.Run<T>(() => {
            lock (_lock) {
                AssetBundle bundle = null;
                if (Application.platform == RuntimePlatform.Android) {
                    bundle = AssetBundle.LoadFromFile(Application.dataPath + "!assets" + key);
                }
                else if (Application.platform == RuntimePlatform.WindowsEditor) {
                    bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + key);
                }
                T x = bundle.LoadAsset<T>(name);
                bundle.Unload(false);
                bundle = null;
                return x;
            }
        });
    }

    /// <summary>
    /// 从StreamingAssets中获取相应资源 同步 
    /// 无法在短时间连续获取同一路径下的资源
    /// </summary>
    public static T GetResource<T>(string key, string name) where T : Object {
        AssetBundle bundle = null;
        if (Application.platform == RuntimePlatform.Android) {
            bundle = AssetBundle.LoadFromFile(Application.dataPath + "!assets" + key);
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor) {
            bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + key);
        }
        T x = bundle.LoadAsset<T>(name);
        bundle.Unload(false);
        bundle = null;
        return x;
    }

    /// <summary>
    /// 从StreamingAssets中获取相应资源 同步
    /// 用于获取同一路径下的一系列资源
    /// </summary>
    public static List<T> GetResourceList<T>(string key, string[] names) where T : Object {
        AssetBundle bundle = null;
        if (Application.platform == RuntimePlatform.Android) {
            bundle = AssetBundle.LoadFromFile(Application.dataPath + "!assets" + key);
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor) {
            bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + key);
        }
        List<T> res = new List<T>();
        foreach(var name in names) {
            res.Add(bundle.LoadAssetAsync<T>(name).asset as T);
        }
        bundle.Unload(false);
        bundle = null;
        return res;
    }
    #endregion
}
