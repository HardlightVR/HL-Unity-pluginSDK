using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
namespace NullSpace.SDK.FileUtilities
{
	internal sealed class HapticImporter : AssetPostprocessor
	{
		static string hdfExtension = ".hdf";
		static string newExtension = ".asset";
		public static bool IsHDF(string asset)
		{
			return asset.EndsWith(hdfExtension, System.StringComparison.OrdinalIgnoreCase);
		}
	
		public static string ConvertToInternalPath(string asset)
		{
			var lastDot = asset.LastIndexOf(".");

			var name = asset.Substring(0, lastDot);
			return name + newExtension;
		}

	
		static void OnPostprocessAllAssets(string[] importedAssets,
			 string[] deletedAssets,
			 string[] movedAssets,
			 string[] movedFromAssetPaths
		 )
		{
			for (int i = 0; i < importedAssets.Length; i++)
			{
				string asset = importedAssets[i];
				if (IsHDF(asset))
				{
					using (var reader = new StreamReader(asset))
					{
						//todo: make sure this is a real haptic file
						ImportHapticAsset(asset);
					}
				}
			}
		}


		static void ImportHapticAsset(string asset)
		{

		//	var hdf = ParsingUtils.ParseHDF(asset);
			//Debug.Log(hdf.rootEffect.name);
		//	if (hdf.rootEffect.type == "pattern")
		//	{
		//		var HapticPattern = FileToCodeHaptic.CreatePattern(hdf.rootEffect.name, hdf);
			//}
		//	string newPath = ConvertToInternalPath(asset);
		//	converter.TryBake(asset, newPath + ".baked");

		//	HapticSequenceAsset newHaptic = AssetDatabase.LoadAssetAtPath(newPath, typeof(HapticSequenceAsset)) as HapticSequenceAsset;
		//	bool loaded = (newHaptic != null);
			//if (!loaded)
			//{
			//	newHaptic = ScriptableObject.CreateInstance<HapticSequenceAsset>(); 
			//} else
			//{
			//	return;
			//}

			//newHaptic.Load(asset);
		//	if (!loaded)
			//{
			//	AssetDatabase.CreateAsset(newHaptic, newPath);
		//	}

			//AssetDatabase.SaveAssets();


		}
	}
}
