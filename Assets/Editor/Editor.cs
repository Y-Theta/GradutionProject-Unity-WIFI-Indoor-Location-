using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.UI;

#if UNITY_EDITOR_WIN
public class Editor {
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles() {
        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", BuildAssetBundleOptions.None, BuildTarget.Android);
    }

}
#endif