using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundles
{
    private static readonly string assetBundleDirectory = Application.streamingAssetsPath;

    [MenuItem("MyExpansion/Build AssetBundles Android")]
    static void BuildAssetBundleAndroid()
    {
        BuildAssetBundles(BuildTarget.Android, "Android");
    }

    [MenuItem("MyExpansion/Build AssetBundles WebGl")]
    static void BuildAssetBundleWebgl()
    {
        BuildAssetBundles(BuildTarget.WebGL, "Webgl");
    }

    private static void BuildAssetBundles(BuildTarget target, string directory)
    {
        Debug.Log("Begin Build AssetBundles!");

        string outDirectory = Path.Combine(assetBundleDirectory, directory);
        if (!Directory.Exists(outDirectory))
        {
            Directory.CreateDirectory(outDirectory);
        }
        BuildPipeline.BuildAssetBundles(outDirectory, BuildAssetBundleOptions.None, target);

        Debug.Log("Complete Build AssetBundles!");
    }
}