using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using NullSpace.SDK.FileUtilities;
using UnityEngine;
using System.Collections;
using System.Threading;

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
		Queue<KeyValuePair<string, string>> _workQueue = new Queue<KeyValuePair<string, string>>();
		Queue<string> _fetchQueue = new Queue<string>();
		static bool _importing = false;
		static bool _fetching = false;
		float timeElapsed = 0;

		float totalProgress = 0f;
		float currentProgress = -1f;
		private class ImportStatus
		{
			public int TotalSucceeded;
			public int Total;

			public ImportStatus(int totalS, int total)
			{
				TotalSucceeded = totalS;
				Total = total;
			}
		}

		private ImportStatus _lastImport = new ImportStatus(0,0);
		public void init()
		{
			Debug.Log("Initting");
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
			Debug.Log("Using root folder " + _path);
			_assetTool.SetRootHapticsFolder(_path);

			try
			{
				if (_assetTool == null)
				{
					Debug.Log("It's null");
				}
				Debug.Assert(_assetTool != null);
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

				_status = string.Format("Found {0} packages.", _packages.Count);

			}
			catch (System.ComponentModel.Win32Exception e)
			{
				Debug.LogError("[NSVR] Problem communicating with HapticAssetTools.exe");
				_status = "Problem communicating with HapticAssetTools.exe";
			}
			catch (InvalidOperationException e)
			{
				//The filename was not set. This could be if the registry key was not found
				Debug.LogError("[NSVR] Could not locate the HapticAssetTools.exe program, make sure the NSVR Service was installed. Try reinstalling if the problem persists.");
				_status = "Could not locate the HapticAssetTools.exe program, make sure the NSVR Service was installed. Try reinstalling if the problem persists.";
				return;
			}
		}


		[MenuItem("Window/Haptic Packages")]
		public static void ShowPackageWindow()
		{

			var window = EditorWindow.GetWindow<PackageWindow>("Packages") as PackageWindow;
			window.init();
		}

		KeyValuePair<string, string> getJsonFromPath(string path)
		{
			
			//If they clicked away from the file dialog, we won't have a valid path
			if (path == "")
			{
				return new KeyValuePair<string, string>("NSVR_EMPTY_PATH", "NSVR_FAILED");
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
				return new KeyValuePair<string, string>("NSVR_NO_HAT", "NSVR_FAILED");
			}
			catch (System.ComponentModel.Win32Exception e)
			{
				Debug.LogError("[NSVR] Could not open the HapticAssetTools.exe program (was it renamed? Does it exist within the service install directory?): " + e.Message);
				return new KeyValuePair<string, string>("NSVR_NO_OPEN", "NSVR_FAILED");
			}


			//If the asset tool succeeded in running, but returned nothing, it's an error
			if (json == "")
			{
				Debug.LogWarning("[NSVR] Unable to load " + path + " it's probably malformed");
				return new KeyValuePair<string, string>("NSVR_EMPTY_RESPONSE", "NSVR_FAILED");

			}

			return new KeyValuePair<string, string>(path, json);
		}
		private List<KeyValuePair<string, string>> getAllJsonFromPaths(List<string> paths)
		{
			var results = new List<KeyValuePair<string, string>>();
			foreach (var path in paths)
			{
				//If they clicked away from the file dialog, we won't have a valid path
				if (path == "")
				{
					continue;
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
					continue;
				}
				catch (System.ComponentModel.Win32Exception e)
				{
					Debug.LogError("[NSVR] Could not open the HapticAssetTools.exe program (was it renamed? Does it exist within the service install directory?): " + e.Message);
					continue;
				}


				//If the asset tool succeeded in running, but returned nothing, it's an error
				if (json == "")
				{
					Debug.LogWarning("[NSVR] Unable to load " + path + " it's probably malformed");
					continue;
				}

				results.Add(new KeyValuePair<string, string>(path, json));
			}

			return results;
		}

		private void CreateHapticAsset(string oldPath, string json)
		{

			//Create our simple json holder. Later, this could be a complex object
			var asset = CreateInstance<JsonAsset>();
			asset.SetJson(json);

			var fileName = System.IO.Path.GetFileNameWithoutExtension(oldPath);

			//If we don't replace . with _, then Unity has serious trouble locating the file
			var newAssetName = fileName.Replace('.', '_') + ".asset";


			//This is where we'd want to change the default location of new haptic assets
			createAssetFolderIfNotExists();

			var newAssetPath = "Assets/Resources/Haptics/" + newAssetName;
			asset.name = newAssetName;

			AssetDatabase.CreateAsset(asset, newAssetPath);
			//Undo.RegisterCreatedObjectUndo(asset, "Create " + asset.name);
		
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

			if (_importing)
			{
			
				EditorUtility.DisplayProgressBar("Hang in there!",string.Format( "Importing haptics.. ({0}/{1})", currentProgress, totalProgress), currentProgress / totalProgress);
			}

			if (_fetching)
			{
				EditorUtility.DisplayProgressBar("Look around the room!", string.Format("Fetching haptics.. ({0}/{1})", currentProgress, totalProgress), currentProgress / totalProgress);
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
				var selectedPackage = pList.Value[_packageSelectionIndices[pList.Key]];

				EditorGUI.BeginDisabledGroup(_importing || _fetching);
				if (GUILayout.Button("All", GUILayout.MaxWidth(60)))
				{
					_importing = true;
					
					importAll(selectedPackage);
				
				}
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.LabelField("Individual..", EditorStyles.miniBoldLabel, GUILayout.Width(65));
				EditorGUI.BeginDisabledGroup(_importing || _fetching);

				if (GUILayout.Button("Sequence", GUILayout.MaxWidth(80)))
				{

					openFileDialogAndMakeAsset(selectedPackage.path, "sequence");
				}


				if (GUILayout.Button("Pattern", GUILayout.MaxWidth(80)))
				{
					openFileDialogAndMakeAsset(selectedPackage.path, "pattern");
				}
				if (GUILayout.Button("Experience", GUILayout.MaxWidth(80)))
				{
					openFileDialogAndMakeAsset(selectedPackage.path, "experience");
				}
				EditorGUI.EndDisabledGroup();

				GUILayout.EndHorizontal();
				EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

			}

			EditorGUILayout.LabelField(_status);
			EditorGUILayout.EndScrollView();


			if (GUILayout.Button("Select root haptics package directory"))
			{
				_path = EditorUtility.OpenFolderPanel("Select root haptics package directory", "", "Haptics");
				_status = "Set root package directory to " + _path;
				//RescanPackages();
			}
			if (GUILayout.Button("Rescan for Packages"))
			{
				//RescanPackages();
			}

	

		}
		private void openFileDialogAndMakeAsset(string path, string hapticType)
		{
			var asd = string.Format("{0}/{1}s/", path, hapticType);
			string newPath = EditorUtility.OpenFilePanel("Import " + hapticType, asd, hapticType);
			var json = getJsonFromPath(newPath);
			if (json.Value != "NSVR_FAILED")
			{
				this.CreateHapticAsset(json.Key, json.Value);
			}
		}

		void Update()
		{
			
			if (_workQueue.Count > 0 && !_fetching)
			{
				int importRate = Mathf.Min(20, Mathf.Max(1, _workQueue.Count / 5));
				timeElapsed += .005f;
				if (timeElapsed > 0.005f)
				{
					timeElapsed = 0;
					for (int i = 0; i <importRate; i++)
					{
						var item = _workQueue.Dequeue();
						CreateHapticAsset(item.Key, item.Value);
						
					}
					currentProgress += importRate;
					this.Repaint();
				}
				
				
			} else if (_workQueue.Count == 0 && _importing)
			{
				
					_status = string.Format("Imported {0}/{1} files successfully", _lastImport.TotalSucceeded, _lastImport.Total);
					_importing = false;
					EditorUtility.ClearProgressBar();

					this.Repaint();
				
			}

			if (_fetchQueue.Count > 0)
			{
				int importRate = Mathf.Min(20, Mathf.Max(1, _fetchQueue.Count / 5));
				timeElapsed += .005f;
				if (timeElapsed > 0.005f)
				{
					timeElapsed = 0;
					for (int i = 0; i < importRate; i++)
					{
						var item = _fetchQueue.Dequeue();
						var result = getJsonFromPath(item);
						if (result.Value == "NSVR_FAILED")
						{
							continue;
						} else
						{
							_workQueue.Enqueue(result);
						}

					}
					currentProgress += importRate;
					this.Repaint();
				}


			} else
			{
				if (_fetching)
				{

					_lastImport.TotalSucceeded = _workQueue.Count;
					_status = string.Format("Fetched {0}/{1} files, preparing for import..", _lastImport.TotalSucceeded, _lastImport.Total);
					_fetching = false;
					_importing = true;
					currentProgress = 0;
					totalProgress = _lastImport.TotalSucceeded;
					EditorUtility.ClearProgressBar();

					this.Repaint();
				}
			}
		}
		private void importAll(object state)
		{
			AssetTool.PackageInfo package = (AssetTool.PackageInfo)(state);
			
			var allSequences = getFilesWithExtension(package.path + "/sequences/", ".sequence");
			currentProgress = 0f;
			totalProgress = allSequences.Count;
			_lastImport.Total = allSequences.Count;
			_fetchQueue = new Queue<string>(allSequences);
			_fetching = true;

			//	var results = getAllJsonFromPaths(allSequences);
			//_lastImport.TotalSucceeded = results.Count;
			//	_lastImport.Total = allSequences.Count;
			//	_workQueue = new Queue<KeyValuePair<string, string>>(results);



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
