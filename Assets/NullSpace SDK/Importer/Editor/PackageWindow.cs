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
		Dictionary<string, List<AssetTool.PackageInfo>> _uniqueCompanies = new Dictionary<string, List<AssetTool.PackageInfo>>();
		Dictionary<string, int> _packageSelectionIndices = new Dictionary<string, int>();
		string _status = "";
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
				_uniqueCompanies.Clear();
				foreach (var p in _packages)
				{
					if (!_uniqueCompanies.ContainsKey(p.studio))
					{
						_uniqueCompanies[p.studio] = new List<AssetTool.PackageInfo>();
					}
					_uniqueCompanies[p.studio].Add(p);


				}

				foreach (var company in _uniqueCompanies)
				{
					_packageSelectionIndices[company.Key] = 0;
				}

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
			}
			catch (InvalidOperationException e)
			{
				//The filename was not set. This could be if the registry key was not found
				Debug.LogError("[NSVR] Could not locate the HapticAssetTools.exe program, make sure the NSVR Service was installed. Try reinstalling if the problem persists.");
				return;
			}
			catch (System.ComponentModel.Win32Exception e)
			{
				Debug.LogError("[NSVR] Could not open the HapticAssetTools.exe program (was it renamed? Does it exist within the service install directory?): " + e.Message);
				return;
			}


			//If the asset tool succeeded in running, but returned nothing, it's an error
			if (json == "")
			{
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
			createAssetFolderIfNotExists();

			var newAssetPath = "Assets/Resources/Haptics/" + newAssetName;
			asset.name = newAssetName;

			AssetDatabase.CreateAsset(asset, newAssetPath);
			Undo.RegisterCreatedObjectUndo(asset, "Create " + asset.name);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Selection.activeObject = asset;
		}

		private static void createAssetFolderIfNotExists()
		{
			if (!AssetDatabase.IsValidFolder("Assets/Resources/Haptics"))
			{
				if (!AssetDatabase.IsValidFolder("Assets/Resources"))
				{
					AssetDatabase.CreateFolder("Assets", "Resources");
				}
				AssetDatabase.CreateFolder("Assets/Resources", "Haptics");
			}
		}

		void OnGUI()
		{
			if (!_created)
			{
				this.init();
			}

		
		
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,
												  false,
												  false);
			foreach (var pList in _uniqueCompanies)
			{
				EditorGUILayout.LabelField(pList.Key, EditorStyles.boldLabel);
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("package ", EditorStyles.miniBoldLabel, GUILayout.Width(45));

				var options = pList.Value.Select(package => package.@namespace).ToArray();
				_packageSelectionIndices[pList.Key] = 
				EditorGUILayout.Popup(_packageSelectionIndices[pList.Key], options, GUILayout.MaxWidth(110));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Import..", EditorStyles.miniBoldLabel, GUILayout.Width(45));

				if (GUILayout.Button("All", GUILayout.MaxWidth(60)))
				{
					var selectedPackage = pList.Value[_packageSelectionIndices[pList.Key]];
					importAll(selectedPackage);
				}
				EditorGUILayout.LabelField("Individual..", EditorStyles.miniBoldLabel, GUILayout.Width(65));


				GUILayout.Button("Sequence", GUILayout.MaxWidth(80));
				GUILayout.Button("Pattern", GUILayout.MaxWidth(80));
				GUILayout.Button("Experience", GUILayout.MaxWidth(80));

				GUILayout.EndHorizontal();
				EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

			}

			EditorGUILayout.LabelField(_status);


			EditorGUILayout.EndScrollView();
			if (GUILayout.Button("Rescan for Packages"))
			{
				RescanPackages();
			}

		}

		private void importAll(AssetTool.PackageInfo package)
		{
			_status = "Importing.. hold tight..";
			this.Repaint();
			
			var allSequences = getFilesWithExtension(package.path + "/sequences/", ".sequence");
			foreach (var sequence in allSequences)
			{
				Debug.Log("Trying to import " + sequence);
				CreateHapticAsset(sequence);
			}
			_status = "";
		}

		List<string> getFilesWithExtension(string directory, string extension)
		{
			List<string> outPaths = new List<string>();
			var allFiles = System.IO.Directory.GetFiles(directory);
			foreach (var potentialFile in allFiles)
			{
				if (System.IO.Path.GetExtension(potentialFile) == extension)
				{
					outPaths.Add(potentialFile);
				}
			}
			return outPaths;
		}
	}
}
