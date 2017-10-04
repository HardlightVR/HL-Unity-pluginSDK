using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Initial class for adding support of AssetBundles to Hardlight's asset pipeline.
/// Better support will come in .26 (the Mark III suit support and kickstarter SDK release)
/// </summary>
public class CreateAssetBundles
{
	[MenuItem("Assets/Build Hardlight AssetBundles")]
	static void BuildAllAssetBundles()
	{
		string assetBundleDirectory = "/Hardlight SDK/AssetBundles";
		var path = Application.dataPath + assetBundleDirectory;
		Debug.Log(path + "\n");

		if (!Directory.Exists(Application.dataPath + assetBundleDirectory))
		{
			Directory.CreateDirectory(Application.dataPath + assetBundleDirectory);
		}
		var manifest = BuildPipeline.BuildAssetBundles(Application.dataPath + assetBundleDirectory, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.AppendHashToAssetBundleName, BuildTarget.StandaloneWindows);
		Debug.Log(manifest.ToString() + "  " + manifest.GetAssetBundleHash("haptics").ToString() + "  \n");
	}
}