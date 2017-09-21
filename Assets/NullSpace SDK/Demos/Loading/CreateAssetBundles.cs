using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateAssetBundles
{
	[MenuItem("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles()
	{
		string assetBundleDirectory = "/NullSpace SDK/AssetBundles";
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