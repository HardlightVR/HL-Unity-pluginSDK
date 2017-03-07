using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using NullSpace.SDK.FileUtilities;
using UnityEngine;
namespace NullSpace.SDK.Editor
{
	public class PackageWindow : EditorWindow
	{
		static bool _created = false;
		static string _path;
		static Vector2 _scrollPos;
		static AssetTool _assetTool = new AssetTool();
		List<AssetTool.PackageInfo> _packages = new List<AssetTool.PackageInfo>();
		public void init()
		{
			if (_assetTool == null)
			{
				_assetTool = new AssetTool();
			}
			_path = Application.streamingAssetsPath + "/Haptics";
			RescanPackages();
			_created = true;

		}
		
		private void RescanPackages()
		{
			_assetTool.SetRootHapticsFolder(_path);

			try
			{
				_packages = _assetTool.TryGetPackageInfo();
			} catch (System.ComponentModel.Win32Exception e)
			{
				Debug.LogError("[NSVR] Problem communicating with HapticAssetTools.exe");
			}
			catch (InvalidOperationException e)
			{
				//The filename was not set. This could be if the registry key was not found
				Debug.LogError("[NSVR] Could not locate the HapticAssetTools.exe program, make sure the NSVR Service was installed. Try reinstalling if the problem persists.");
				return;
			}
		}
		

		[MenuItem("Window/Haptic Packages")]
		public static void ShowPackageWindow()
		{

			var window = EditorWindow.GetWindow<PackageWindow>("Packages") as PackageWindow;
			window.init();
		}

		private void CreateHapticAsset(string path)
		{
			//If they clicked away from the file dialog, we won't have a valid path
			if (path == "")
			{
				return;
			}

			//Attempt to get json of the haptic definition file from the tool
			var json = "";

			try
			{
				json = _assetTool.GetHapticDefinitionFileJson(path);
			} catch (InvalidOperationException e)
			{
				//The filename was not set. This could be if the registry key was not found
				Debug.LogError("[NSVR] Could not locate the HapticAssetTools.exe program, make sure the NSVR Service was installed. Try reinstalling if the problem persists.");
				return; 
			} catch (System.ComponentModel.Win32Exception e)
			{
				Debug.LogError("[NSVR] Could not open the HapticAssetTools.exe program (was it renamed? Does it exist within the service install directory?): " + e.Message);
				return;
			} 


			//If the asset tool succeeded in running, but returned nothing, it's an error
			if (json == "") {
				Debug.LogError("[NSVR] Unable to communicate with HapticAssetTools.exe");
				return;
			}

			//Create our simple json holder. Later, this could be a complex object
			var asset = CreateInstance<JsonAsset>();
			asset.SetJson(json);

			var fileName = System.IO.Path.GetFileNameWithoutExtension(path);

			//If we don't replace . with _, then Unity has serious trouble locating the file
			var newAssetName = fileName.Replace('.', '_') + ".asset";


			//This is where we'd want to change the default location of new haptic assets
			if (!AssetDatabase.IsValidFolder("Assets/Resources/Haptics"))
			{
				if (!AssetDatabase.IsValidFolder("Assets/Resources")) {
					AssetDatabase.CreateFolder("Assets", "Resources");
				}
				AssetDatabase.CreateFolder("Assets/Resources", "Haptics");
			}

			var newAssetPath = "Assets/Resources/Haptics/" + newAssetName;
			asset.name = newAssetName;

			AssetDatabase.CreateAsset(asset, newAssetPath);
			Undo.RegisterCreatedObjectUndo(asset, "Create " + asset.name);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Selection.activeObject = asset;
		}
		void OnGUI()
		{
			if (!_created)
			{
				this.init();
			}

			Dictionary<string, List<AssetTool.PackageInfo>> uniqueCompanies = new Dictionary<string, List<AssetTool.PackageInfo>>();
			foreach (var p in _packages)
			{
				if (!uniqueCompanies.ContainsKey(p.studio))
				{
					uniqueCompanies[p.studio] = new List<AssetTool.PackageInfo>();
				}
				uniqueCompanies[p.studio].Add(p);

			}
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,
												  false,
												  false);
			foreach (var pList in uniqueCompanies)
			{
				EditorGUILayout.LabelField(pList.Key, EditorStyles.boldLabel);
			
				foreach (var p in pList.Value)
				{

					GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("> " + p.@namespace, EditorStyles.miniBoldLabel, GUILayout.Width(150));

					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					GUIStyle test = new GUIStyle(GUI.skin.button);
					test.margin = new RectOffset(10, 10, 0, 0);

					EditorGUILayout.LabelField("Import:", GUILayout.Width(60));
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("sequence", test, GUILayout.Width(100)))
					{
						var path = EditorUtility.OpenFilePanel("Import Haptic Sequence", p.path + "/sequences", "sequence");
						CreateHapticAsset(path);

					}
					if (GUILayout.Button("pattern", test, GUILayout.Width(100)))
					{
						var path = EditorUtility.OpenFilePanel("Import Haptic Pattern", p.path + "/patterns", "pattern");
						CreateHapticAsset(path);
					}

					GUILayout.EndHorizontal();

					EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
				}
			}

			EditorGUILayout.EndScrollView();
			if (GUILayout.Button("Rescan for Packages"))
			{
				RescanPackages();
			}

		}
	}
}
