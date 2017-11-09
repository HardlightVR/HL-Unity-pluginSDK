using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using Hardlight.SDK.FileUtilities;
using UnityEngine;
using System.Collections;
using System.Threading;

namespace Hardlight.SDK.UEditor
{
	public class PackagingPane : EditorPane
	{
		static bool _created = false;
		static bool _pathError = false;
		static string _path;
		static string SavedPath
		{
			set
			{
				//Debug.Log("Setting last accessed to : " + value + "\n");
				PlayerPrefs.SetString("SavedPath", value);
			}
			get
			{
				if (PlayerPrefs.HasKey("SavedPath"))
				{
					//Debug.Log("Last Package Accessed [" + PlayerPrefs.GetString("LastPackageAccessed") + "]\n");

					return PlayerPrefs.GetString("SavedPath");
				}
				return "";
			}
		}
		static Vector2 _scrollPos;
		static AssetTool _assetTool = new AssetTool();
		List<AssetTool.PackageInfo> _packages = new List<AssetTool.PackageInfo>();
		Dictionary<string, List<AssetTool.PackageInfo>> _uniqueCompanies = new Dictionary<string, List<AssetTool.PackageInfo>>();
		Dictionary<string, int> _packageSelectionIndices = new Dictionary<string, int>();
		public HelpMessage _status = new HelpMessage("", MessageType.None);

		private enum ImportState { Idle, Fetching, Importing }
		private PackageImport CurrentImport;

		public ScriptableObjectHaptic JustCreated;

		private class PackageImport
		{
			private class ImportStatus
			{
				private int totalSucceeded;
				public int Total;

				public int TotalSucceeded
				{
					get
					{
						return totalSucceeded;
					}

					set
					{
						totalSucceeded = value;
					}
				}

				public ImportStatus(int totalS, int total)
				{
					TotalSucceeded = totalS;
					Total = total;
				}
			}
			private ImportStatus _lastImport = new ImportStatus(0, 0);

			public PackagingPane MyPane;
			private ImportStatus CurrentStatus;
			private enum ImportState { Idle, Fetching, Waiting, Importing, Finished }
			private ImportState activeState = ImportState.Idle;
			float timeElapsed = 0;
			float updateRate = .03f;
			int maxImportedPerStep = 5;
			string directory;

			public bool Idle
			{
				get
				{
					return activeState == ImportState.Idle;
				}
			}
			public bool Fetching
			{
				get
				{
					return activeState == ImportState.Fetching;
				}
			}
			public bool Waiting
			{
				get
				{
					return activeState == ImportState.Waiting;
				}
			}
			public bool Importing
			{
				get
				{
					return activeState == ImportState.Importing;
				}
			}
			public bool Finished
			{
				get
				{
					return activeState == ImportState.Finished;
				}
			}

			float totalProgress = 0f;
			float currentProgress = -1f;
			bool ShouldRepaint = false;
			public ScriptableObjectHaptic lastCreatedAsset;

			Queue<KeyValuePair<string, string>> _workQueue = new Queue<KeyValuePair<string, string>>();
			Queue<string> _fetchQueue = new Queue<string>();
			Queue<object> _packageQueue = new Queue<object>();

			public PackageImport(PackagingPane pane)
			{
				MyPane = pane;
				_workQueue = new Queue<KeyValuePair<string, string>>();
				_fetchQueue = new Queue<string>();
				_packageQueue = new Queue<object>();
			}
			//TODO: Turn this into one function with parameter intake.

			private void DrawProgressBar(float percentProgress, string content, float width = 300, float height = 45)
			{
				Rect r = GUILayoutUtility.GetRect(width, height);
				EditorGUI.ProgressBar(r, percentProgress, content);
			}
			public void DrawCurrentState()
			{
				HLEditorStyles.DrawLabel("Current State: [" + activeState.ToString() + "]");
			}
			public void DrawFetchingHaptics()
			{
				if (Fetching)
				{
					float prog = currentProgress / (2 * totalProgress);
					DrawProgressBar(prog, string.Format("Fetching haptics.. ({0}/~{1})", currentProgress, 2 * totalProgress));
					ShouldRepaint = true;
				}
				//else
				//{
				//	NSEditorStyles.DrawLabel("[Not Fetching]");
				//}
			}
			public void DrawImportingHaptics()
			{
				if (Waiting)
				{
					int result = EditorUtility.DisplayDialogComplex("Overwrite Existing Haptics", "There are [N] existing haptics", "Overwrite", "Cancel Import", "Rename Old");
					Debug.Log(result + " \n");
					if (result == 0)
					{
						ResumeImport();
						//EditorApplication.delayCall += ResumeImport;
					}
					if (result == 1)
					{
						QuitImport();
						//EditorApplication.delayCall += QuitImport;
					}
					if (result == 2)
					{
						ResumeImport();
						//EditorApplication.delayCall += ResumeImport;
					}
				}
				else if (Importing)
				{
					float prog = currentProgress / (2 * totalProgress);
					DrawProgressBar(prog, string.Format("Importing haptics.. ({0}/{1})", currentProgress, 2 * totalProgress));

					ShouldRepaint = true;
					//EditorGUILayout.EndVertical();
				}
				//else
				//{
				//	NSEditorStyles.DrawLabel("[Not Importing]");
				//}
			}
			public void DrawFinishedImport()
			{
				if (Finished)
				{
					float prog = currentProgress / (2 * totalProgress);
					DrawProgressBar(prog, string.Format("Import Complete: ({0}) fetched & imported", totalProgress));
					//Draw a box to click and go to the target directory.
				}
			}
			private void FinishImport()
			{
				HardlightEditor.myWindow.ActivateSpecificEditorPane(typeof(PackagingPane));
				HardlightEditor.myWindow.Focus();
				MyPane._status = new HelpMessage(string.Format("Imported {0}/{1} files successfully", _lastImport.TotalSucceeded, _lastImport.Total), MessageType.Info);

				if (_packageQueue.Count <= 0)
				{
					ChangeState(ImportState.Finished);
				}
				else
				{
					ImportPackage(_packageQueue.Dequeue());
				}
				AssetDatabase.SaveAssets();

				MyPane.Repaint();
			}
			private void QuitImport()
			{
				_fetchQueue.Clear();
				_workQueue.Clear();
				ChangeState(ImportState.Idle);
			}
			private void ResumeImport()
			{
				ChangeState(ImportState.Importing);
			}

			private void ChangeState(ImportState targetState)
			{
				#region Idle
				if (Idle)
				{
					activeState = targetState;
				}
				#endregion
				#region Fetching
				else if (Fetching)
				{
					activeState = targetState;
				}
				#endregion
				#region Waiting
				else if (Waiting)
				{
					activeState = targetState;
				}
				#endregion
				#region Importing
				else if (Importing)
				{
					activeState = targetState;
				}
				#endregion
				#region Finished
				else if (Finished)
				{
					activeState = targetState;
				}
				#endregion

				//Debug.Log("New State: " + activeState.ToString() + "\n");
			}
			public void Update()
			{
				UpdateFetch();
				UpdateImport();

				if (ShouldRepaint)
				{
					ShouldRepaint = false;
					MyPane.Repaint();
				}
			}

			private void UpdateFetch()
			{
				#region Fetch Queue populates work queue
				if (_fetchQueue.Count > 0 && Fetching)
				{
					//Debug.Log("Wut, moar werk? Fetching " + _fetchQueue.Count + "\n");

					int importRate = Mathf.Min(maxImportedPerStep, Mathf.Max(1, _fetchQueue.Count / 5));
					timeElapsed += Time.fixedDeltaTime;
					if (timeElapsed > updateRate)
					{
						timeElapsed = 0;
						for (int i = 0; i < importRate; i++)
						{
							var item = _fetchQueue.Dequeue();
							if (item.Length > 0)
							{
								var result = GetJsonFromPath(item);
								if (result.Value == "NSVR_FAILED")
								{
									continue;
								}
								else
								{
									_workQueue.Enqueue(result);
								}
							}
						}
						currentProgress += importRate;

						ShouldRepaint = true;
					}
				}
				#endregion
				#region Finish Fetching
				else
				{
					//If we're still fetching
					if (Fetching)
					{
						//Debug.Log("Fetching complete " + _fetchQueue.Count + "\n");

						//We're ready to import.
						_lastImport.TotalSucceeded = _workQueue.Count;
						MyPane._status = new HelpMessage(string.Format("Fetched {0}/{1} files, preparing for import..", _lastImport.TotalSucceeded, _lastImport.Total), MessageType.Info);
						ChangeState(ImportState.Importing);
						totalProgress = _lastImport.TotalSucceeded;

						bool WillOverwrite = CheckForWorkQueueForOverwrite();

						if (WillOverwrite)
						{
							ChangeState(ImportState.Waiting);
						}
						else
						{
							ChangeState(ImportState.Importing);
							//Open confirmation dialog
						}

						ShouldRepaint = true;
					}
				}
				#endregion
			}
			private void UpdateImport()
			{
				#region Handle Importing
				//If we have a work queue and we're importing
				if (_workQueue.Count > 0 && Importing)
				{
					//Debug.Log("Importing : " + _workQueue.Count + "\n");
					int importRate = Mathf.Min(maxImportedPerStep, Mathf.Max(1, _workQueue.Count / 5));
					timeElapsed += Time.fixedDeltaTime;
					if (timeElapsed > updateRate)
					{
						timeElapsed = 0;
						for (int i = 0; i < importRate; i++)
						{
							var item = _workQueue.Dequeue();
							lastCreatedAsset = MyPane.CreateHapticAsset(item.Key, item.Value);
						}
						currentProgress += importRate;
						MyPane.Repaint();
					}
				}
				#endregion
				#region Finish Import
				//We're done!
				else if (_fetchQueue.Count == 0 && _workQueue.Count == 0 && !Idle && !Finished)
				{
					FinishImport();
				}
				//Debug.Log(_workQueue.Count + "  " + _fetchQueue.Count + "   " + activeState + "\n");
				#endregion
			}

			public void ImportPackage(object state)
			{
				AssetTool.PackageInfo package = (AssetTool.PackageInfo)(state);

				var allHapticFiles = GetFilesWithExtension(package.path + "/experiences/", ".experience");
				allHapticFiles.AddRange(GetFilesWithExtension(package.path + "/patterns/", ".pattern"));
				allHapticFiles.AddRange(GetFilesWithExtension(package.path + "/sequences/", ".sequence"));
				currentProgress = 0f;
				totalProgress = allHapticFiles.Count;
				_lastImport.Total = allHapticFiles.Count;

				#region Debug found haptic files
				if (HardlightEditor.myWindow.DebugHardlightEditor)
				{
					string li = string.Empty;
					for (int i = 0; i < allHapticFiles.Count; i++)
					{
						li += allHapticFiles[i] + "\n";
					}
					Debug.Log("All Haptic Files: Count [" + allHapticFiles.Count + "]\n" + li);
				}
				#endregion

				if (_fetchQueue == null)
				{
					_fetchQueue = new Queue<string>();
				}
				//Add all the new haptic files.
				for (int i = 0; i < allHapticFiles.Count; i++)
				{
					_fetchQueue.Enqueue(allHapticFiles[i]);
				}

				ChangeState(ImportState.Fetching);
				//Debug.Log("Created Fetch Queue: " + _fetchQueue.Count + "\n");

				var results = GetAllJsonFromPaths(allHapticFiles);
				_lastImport.TotalSucceeded = results.Count;
				_lastImport.Total = allHapticFiles.Count;
				//Debug.Log("Last Import: " + _lastImport.Total + "   " + _lastImport.Total + "\n");

				if (_workQueue == null)
				{
					_workQueue = new Queue<KeyValuePair<string, string>>();
				}
			}
			public void QueueImportPackage(object nextPackage)
			{
				Debug.Log(nextPackage.GetType().ToString() + "\n");
				//Don't enqueue the same object multiple times
				if (!_packageQueue.Contains(nextPackage))
				{
					//Save the JSON for later to fetch then import.
					_packageQueue.Enqueue(nextPackage);
				}

				Debug.Log("Package Queue Count: " + _packageQueue.Count + "\n");
			}

			public KeyValuePair<string, string> GetJsonFromPath(string path)
			{
				//It is likely very difficult to provide an empty path here.
				if (path == "")
				{
					MyPane.OutputMessage("Invalid path provided to getJsonFromPath.", MessageType.Error);
					return new KeyValuePair<string, string>("NSVR_EMPTY_PATH", "NSVR_FAILED");
				}

				bool continueInstead;
				//Attempt to get json of the haptic definition file from the tool
				var json = TryGetJsonHapticDefinition(path, out continueInstead);

				#region Old Block (before making TryGetJSonHapticDefinition)
				//try
				//{
				//	json = _assetTool.GetHapticDefinitionFileJson(path);
				//}
				//catch (InvalidOperationException e)
				//{
				//	_pathError = true;
				//	//The filename was not set. This could be if the registry key was not found
				//	OutputMessage("Invalid Operation Exception - Could not locate the HapticAssetTools.exe program, make sure the HLVR Service was installed. Try reinstalling if the problem persists.", MessageType.Error);

				//	Debug.LogError("[HLVR] Could not locate the HapticAssetTools.exe program, make sure the HLVR Service was installed. Try reinstalling if the problem persists." + e.Message);
				//	return new KeyValuePair<string, string>("HLVR_NO_HAT", "HLVR_FAILED");
				//}
				//catch (System.ComponentModel.Win32Exception e)
				//{
				//	_pathError = true;
				//	OutputMessage("Win32Exception - Could not open the HapticAssetTools.exe program (was it renamed? Does it exist within the service install directory?)", MessageType.Error);

				//	Debug.LogError("[HLVR] Could not open the HapticAssetTools.exe program (was it renamed? Does it exist within the service install directory?): " + e.Message);
				//	return new KeyValuePair<string, string>("HLVR_NO_OPEN", "HLVR_FAILED");
				//} 
				#endregion

				//If the asset tool succeeded in running, but returned nothing, it's an error
				if (json.Length < 1)
				{
					//_pathError = true;
					Debug.LogWarning("[HLVR] Unable to load path [" + path + "] it's probably malformed\n");

					MyPane.OutputMessage("Empty json result - likely a failure to load from\n\tpath [" + path + "]", MessageType.Error);

					return new KeyValuePair<string, string>("NSVR_EMPTY_RESPONSE", "NSVR_FAILED");
				}
				else
				{
					_pathError = false;
				}

				return new KeyValuePair<string, string>(path, json);
			}

			public List<KeyValuePair<string, string>> GetAllJsonFromPaths(List<string> paths)
			{
				var results = new List<KeyValuePair<string, string>>();
				foreach (var path in paths)
				{
					//If they clicked away from the file dialog, we won't have a valid path
					if (path.Length < 1)
					{
						continue;
					}

					//Attempt to get json of the haptic definition file from the tool
					bool continueInstead;
					var json = TryGetJsonHapticDefinition(path, out continueInstead);

					//If the asset tool succeeded in running, but returned nothing, it's an error
					if (json.Length < 1)
					{
						Debug.LogWarning("[HLVR] Unable to load " + path + " it's probably malformed");
						MyPane.OutputMessage("Unable to load path [" + path + "] - it is probably malformed", MessageType.Warning);
						continue;
					}

					results.Add(new KeyValuePair<string, string>(path, json));
				}
				MyPane.OutputMessage("Getting all JSON from paths.\n\tAttempted: [" + paths.Count + "]\n\tSuccesses: [" + results.Count + "]", MessageType.None);
				//Debug.Log("Get all JSON from paths\n" + paths.Count + "   " + results.Count);

				return results;
			}

			private string TryGetJsonHapticDefinition(string path, out bool continueInstead)
			{
				continueInstead = false;
				string json = "";
				try
				{
					if (HLEditor != null && HLEditor.DebugHardlightEditor)
					{
						Debug.Log("\tGet haptic definition file from path\n\t\t[" + path + "]\n");
					}
					json = _assetTool.GetHapticDefinitionFileJson(path);
				}
				catch (InvalidOperationException e)
				{
					//The filename was not set. This could be if the registry key was not found
					Debug.LogError("[HLVR] Could not locate the HapticAssetTools.exe program, make sure the HLVR Service was installed. Try reinstalling if the problem persists." + e.Message);
					continueInstead = true;
				}
				catch (System.ComponentModel.Win32Exception e)
				{
					Debug.LogError("[HLVR] Could not open the HapticAssetTools.exe program (was it renamed? Does it exist within the service install directory?): " + e.Message);
					continueInstead = true;
				}
				catch (HapticsAssetException e)
				{
					//This would be great if we could hand in a context reference.
					//var thing = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
					//if (thing != null)
					//{
					Debug.LogError("[HLVR] Haptics Asset Exception loading item at path [" + path + "]: " + e.Message);
					//}

					continueInstead = true;
				}
				return json;
			}

			List<string> GetFilesWithExtension(string directory, string extension)
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

			bool CheckForWorkQueueForOverwrite()
			{
				bool willOverwrite = false;
				var workList = _workQueue.ToList();
				for (int i = 0; i < workList.Count; i++)
				{
					//This does not correctly detect if an asset already exists.

					//string old = workList[i].Key.Replace('\\', '/');
					//var str = new string[] { "/Assets/" };
					//old = old.Split(str, StringSplitOptions.None)[1];
					//Debug.Log(old + "\n");
					//var asset = AssetDatabase.LoadAllAssetRepresentationsAtPath("Assets/" + old);
					//if (asset != null && asset.Length > 0)
					//{
					//	Debug.Log("ASSET EXISTS\n");
					//	willOverwrite = true;
					//}
					//Debug.Log(workList[i].Key + "  " + workList[i].Value + "\n");
				}
				return willOverwrite;
			}
		}

		//Keep delegate reference for holding repaint?

		public override void Setup()
		{
			PaneTitle = "Package Importer";
			ShortPaneTitle = "Package Importer";
			if (_assetTool == null)
			{
				_assetTool = new AssetTool();
			}
			if (SavedPath.Length <= 0)
			{
				_path = Application.streamingAssetsPath + "/Haptics";
			}
			else
			{
				_path = SavedPath;
			}

			_pathError = false;
			RescanPackages();
			_created = true;

			SetupImport();

			base.Setup();
			//Debug.Log("We have found: " + _uniqueCompanies.Count + " companies\n");
		}
		private void SetupImport()
		{
			CurrentImport = new PackageImport(this);
		}

		public override bool IsValid()
		{
			bool isValid = base.IsValid();
			if (_assetTool == null || !_created || CurrentImport == null || !isValid)
			{
				isValid = false;
				Initialized = false;
			}

			return isValid;
		}

		private void RescanPackages()
		{
			//Debug.Log("Using root folder " + _path + "\n");
			_assetTool.SetRootHapticsFolder(_path);
			try
			{
				if (_assetTool == null)
				{
					//TODO: AssetTool is Null
					OutputMessage("Asset Tool is Null\n\tThis is likely a service version problem.", MessageType.Error);
					Debug.LogError("Asset Tool is Null\n\tThis is likely a service version problem.");
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


				_status = new HelpMessage(string.Format("Found {0} packages.", _packages.Count), MessageType.Info);

			}
			catch (System.ComponentModel.Win32Exception)
			{
				//TODO: Win32Exception communicating with HapticAssetTools.
				Debug.LogError("[HLVR] Problem communicating with HapticAssetTools.exe\n");
				_status = new HelpMessage("Problem communicating with HapticAssetTools.exe", MessageType.Error);
			}
			catch (InvalidOperationException)
			{
				//TODO: Invalid Operation Exception finding HapticAssetTools.exe (service version problem?
				//The filename was not set. This could be if the registry key was not found
				Debug.LogError("[HLVR] Could not locate the HapticAssetTools.exe program, make sure the HLVR Service was installed. Try reinstalling if the problem persists.\n");
				_status = new HelpMessage("Could not locate the HapticAssetTools.exe program, make sure the HLVR Service was installed. Try reinstalling if the problem persists.", MessageType.Error);
				return;
			}
			catch (HapticsAssetException e)
			{
				//TODO: Invalid Operation Exception finding HapticAssetTools.exe (service version problem?
				//The filename was not set. This could be if the registry key was not found
				Debug.LogError("[HLVR] Could not locate the HapticAssetTools.exe program, make sure the HLVR Service was installed. Try reinstalling if the problem persists.\n" + e.Message);
				_status = new HelpMessage("Could not locate the HapticAssetTools.exe program, make sure the HLVR Service was installed.\nTry reinstalling if the problem persists.", MessageType.Error);
				return;
			}
		}
		private void NavigateToNewDirectory()
		{

		}

		protected ScriptableObjectHaptic CreateHapticAsset(string oldPath, string json, int undoGroup = 0)
		{
			//Create our simple json holder. Later, this could be a complex object
			var assetPath = "Assets/Resources/Haptics/";
			//var asset = CreateInstance<JsonAsset>();
			//asset.SetJson(json);

			var fileName = System.IO.Path.GetFileNameWithoutExtension(oldPath);

			////If we don't replace . with _, then Unity has serious trouble locating the file
			//var newAssetName = fileName.Replace('.', '_') + ".asset";
			//var newAssetName = fileName.Replace('.', '_') + ".asset";
			//ScriptableObjectHaptic Scrob = null;

			bool isSeq = oldPath.Contains(".sequence");
			bool isPat = oldPath.Contains(".pattern");
			bool isExp = oldPath.Contains(".experience");
			//Debug.Log("Attemtping haptic asset import: " + oldPath + " " + isSeq + "\n" + newAssetName + "\n\n" + json + "\n", this);

			if (isSeq)
			{
				JustCreated = HapticSequence.LoadFromJson(oldPath);
				HapticSequence.SaveAsset(fileName, (HapticSequence)JustCreated);
			}
			else if (isPat)
			{
				JustCreated = HapticPattern.LoadFromJson(oldPath);
				HapticPattern.SaveAsset(fileName, (HapticPattern)JustCreated);
			}
			else if (isExp)
			{
				JustCreated = HapticExperience.LoadFromJson(oldPath);
				HapticExperience.SaveAsset(fileName, (HapticExperience)JustCreated);
			}

			////This is where we'd want to change the default location of new haptic assets
			//CreateAssetFolderIfNotExists();

			//var newAssetPath = "Assets/Resources/Haptics/" + newAssetName;
			//asset.name = newAssetName;
			//EditorUtility.SetDirty(asset);

			//var old = AssetDatabase.LoadAssetAtPath(newAssetPath, typeof(JsonAsset));
			//if (old != null)
			//{
			//	//Previous file exists
			//	//Debug.LogError("Overwriting " + newAssetPath + "\n");
			//	Undo.RecordObject(old, "Reimport " + asset.name);
			//}

			//AssetDatabase.CreateAsset(asset, newAssetPath);
			//if (old == null)
			//{
			//	//Previous file did not exist
			//	Undo.RegisterCreatedObjectUndo(asset, "Import " + asset.name);
			//}

			return JustCreated;

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
				Debug.LogError("[HLVR] Could not locate the HapticAssetTools.exe program, make sure the HLVR Service was installed. Try reinstalling if the problem persists." + e.Message);
				return;
			}
			catch (System.ComponentModel.Win32Exception e)
			{
				Debug.LogError("[HLVR] Could not open the HapticAssetTools.exe program (was it renamed? Does it exist within the service install directory?): " + e.Message);
				return;
			}


			//If the asset tool succeeded in running, but returned nothing, it's an error
			if (json == "")
			{
				Debug.LogError("[HLVR] Unable to communicate with HapticAssetTools.exe");
				return;
			}

			//Create our simple json holder. Later, this could be a complex object
			var asset = CreateInstance<JsonAsset>();
			asset.SetJson(json);

			var fileName = System.IO.Path.GetFileNameWithoutExtension(path);

			//If we don't replace . with _, then Unity has serious trouble locating the file
			var newAssetName = fileName.Replace('.', '_') + ".asset";


			//This is where we'd want to change the default location of new haptic assets
			CreateAssetFolderIfNotExists();

			var newAssetPath = "Assets/Resources/Haptics/" + newAssetName;
			asset.name = newAssetName;

			AssetDatabase.CreateAsset(asset, newAssetPath);
			Undo.RegisterCreatedObjectUndo(asset, "Create " + asset.name);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Selection.activeObject = asset;
		}

		private static void CreateAssetFolderIfNotExists()
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

		public override void DrawTitleContent()
		{
			base.DrawTitleContent();
		}
		public override void DrawPaneContent()
		{
			#region [OLD] Migrated into PackageImport class object
			#region Importing Progress Bar
			//if (_importing)
			//{
			//	//EditorUtility.DisplayProgressBar("Hang in there!", string.Format("Importing haptics.. ({0}/{1})", currentProgress, totalProgress), currentProgress / totalProgress);

			//	Rect r = GUILayoutUtility.GetRect(300, 25);
			//	EditorGUI.ProgressBar(r, currentProgress / totalProgress, string.Format("Importing haptics.. ({0}/{1})", currentProgress, totalProgress));
			//}
			#endregion

			#region Fetching Progress Bar
			//if (_fetching)
			//{
			//	//TODO: Adjust the text for fetching haptics
			//	//TODO: Ensure the display progress bars ALWAYS close.
			//	//EditorUtility.DisplayProgressBar("Looking around the room!", string.Format("Fetching haptics.. ({0}/{1})", currentProgress, totalProgress), currentProgress / totalProgress);

			//	//Rect r = GUILayoutUtility.GetRect(300, 25);
			//	//EditorGUI.ProgressBar(r, currentProgress / totalProgress, string.Format("Fetching haptics.. ({0}/{1})", currentProgress, totalProgress));
			//}
			#endregion
			#endregion

			DrawHapticDirectory();

			DrawSelectHapticRootButton();

			if (GUILayout.Button("Rescan for Packages"))
			{
				RescanPackages();
			}

			#region Current Import
			if (CurrentImport != null)
			{
				//Don't draw a divider if we're idle.
				if (!CurrentImport.Idle)
				{
					HLEditorStyles.DrawSliderDivider();
				}
				if (HardlightEditor.myWindow.DebugHardlightEditor)
				{
					CurrentImport.DrawCurrentState();
				}

				CurrentImport.DrawFetchingHaptics();
				CurrentImport.DrawImportingHaptics();
				CurrentImport.DrawFinishedImport();

				if (CurrentImport.Finished && CurrentImport.lastCreatedAsset != null)
				{
					if (GUILayout.Button("Navigate to new assets!"))
					{
						Selection.activeObject = CurrentImport.lastCreatedAsset;
					}
				}
			}
			#endregion

			#region Draw Each Package
			HLEditorStyles.DrawSliderDivider();
			DrawPackages();
			#endregion

			#region Draw Status
			HLEditorStyles.OpenHorizontal(_status.messageType, true);
			HLEditorStyles.DrawLabel(_status.message);
			HLEditorStyles.CloseHorizontal();
			#endregion
			EditorGUILayout.EndScrollView();
		}

		private void DrawHapticDirectory()
		{
			IsTutorialStep(0, () =>
			{
				HLEditorStyles.DrawLabel("Welcome to the Haptic Package Tool-torial!\n" +
					"We will go over how to import JSON Haptic Assets into Unity Scriptable Object assets\n" +
					"As well as the benefits for doing so."
					, 105, 14);
			});

			TutorialHighlight(1, () =>
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical();
				GUILayout.Space(8);
				HLEditorStyles.DrawLabel("Haptic Package Directory");
				EditorGUILayout.EndVertical();
				HLEditorStyles.OpenHorizontal(_pathError ? ColorBoxType.Error : ColorBoxType.Normal);
				HLEditorStyles.DrawLabel(_path);
				HLEditorStyles.CloseHorizontal();
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(2);
			});


			IsTutorialStep(1, () =>
			{
				HLEditorStyles.DrawLabel("This is the selected haptic directory\n" +
					"This can be located anywhere on your computer."
					, 105, 14);
			});
		}

		private void DrawSelectHapticRootButton()
		{
			//TODO: Make a folder icon (to indicate directory picking)
			if (GUILayout.Button("Select root haptics package directory"))
			{
				string _newPath = EditorUtility.OpenFolderPanel("Select root haptics package directory", "", "Haptics");
				_status = new HelpMessage("Root package directory set to path [" + _path + "]", MessageType.Info);

				//If they provided an incorrect path by OpenFolderPanel, it will be of length 0.
				if (_newPath.Length > 0)
				{
					//Only assign when greater than 0.
					_path = _newPath;
					SavedPath = _path;
					_pathError = false;
				}
				//else
				//{
				//	OutputMessage("Invalid Directory Selected. Did not apply new selected directory.", MessageType.Error);
				//}
				RescanPackages();
			}
		}

		private void DrawPackages()
		{
			//Begin the scrollview
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,
												  false,
												  false);

			int packageCounter = 0;
			#region Draw Individual Companies
			foreach (var pList in _uniqueCompanies)
			{
				//We only display the tutorial highlight for the first package (so we don't have 30 tutorials boxes with repeated content)
				TutorialHighlight(1, packageCounter == 0, () =>
				{
					DrawPackageInfoElement(pList.Key, pList.Value, packageCounter);
				});

				IsTutorialStep(1, packageCounter == 0, () =>
				{
					HLEditorStyles.DrawLabel("This is a detected haptic package."
						, 105, 14);

					using (new EditorGUI.DisabledGroupScope(true))
					{
						HLEditorStyles.TextField("Haptic Package Location", pList.Value[0].path);
					}
					//Ideally we'd want a button that opens directly to that directory. Unity does not make this easy.
					//if (NSEditorStyles.OperationToolbarButton(false, new GUIContent("Open Directory")))
					//{
					//	string path = pList.Value[0].path;
					//	Debug.Log(path + "\n");

					//	var dummypath = System.IO.Path.Combine(path, "fake.asset");
					//	Debug.Log(dummypath + "\n");
					//	dummypath = dummypath.Replace('\\', '/');
					//	var assetpath = AssetDatabase.GenerateUniqueAssetPath(dummypath);
					//	//if (assetpath == "")
					//	//{
					//	//	// couldn't generate a path, current asset must be a file
					//	//	Debug.Log("File: " + item.name);
					//	//}
					//	//else
					//	//{
					//	//	Debug.Log("Directory: " + item.name);
					//	//}
					//	Selection.activeObject = AssetDatabase.LoadAssetAtPath(assetpath, typeof(DefaultAsset));


					//}
				});

				packageCounter++;
			}
			#endregion
			//If we have 0 companies, or 0 packages amongst those companies (unsure how that could happen)
			if (_uniqueCompanies.Count < 1 || packageCounter < 1)
			{
				HLEditorStyles.OpenVertical(MessageType.Error);

				//Draw the helper content for finding no packages.
				HLEditorStyles.DrawLabel("Detected 0 haptic packages\n" +
					"Did you select the wrong directory? The default directory is \"StreamingAssets/Haptics\""
					, 105, 14);

				HLEditorStyles.DrawLabel("It's possible the haptic content is missing."
									, 105, 0);

				if (HLEditorStyles.DrawButton("Click here to revert to the default haptic directory."))
				{
					_path = Application.streamingAssetsPath + "/Haptics";
					SavedPath = _path;
					RescanPackages();
				}

				if (HLEditorStyles.DrawButton("Click here to download a zipped archive of haptics."))
				{
					Application.OpenURL(HardlightEditor.myWindow.HapticZipLink);
				}

				GUILayout.Space(14);
				HLEditorStyles.DrawLabel("It is also possible you have a malformed config.json file.\nCheck the Console window to see if there is an HLVR error and then follow the path to the malformed file (and fix the json structural problems with it.)", 105, 4);


				HLEditorStyles.CloseVertical();

			}
		}
		private void DrawPackageInfoElement(string companyName, List<AssetTool.PackageInfo> packages, int packageIndex = 0)
		{
			EditorGUILayout.LabelField(companyName, EditorStyles.boldLabel);
			float labelOptionWidth = 110;
			float inputOptionWidth = 230;
			var selectedPackage = packages[_packageSelectionIndices[companyName]];
			var packagesByThisCompany = new string[0];
			packagesByThisCompany = packages.Select(package => package.@namespace).ToArray();

			#region Package Selection
			TutorialHighlight(2, packageIndex == 0, () =>
			{
				GUILayout.BeginHorizontal();

				EditorGUILayout.LabelField("Package ", EditorStyles.miniBoldLabel, GUILayout.Width(labelOptionWidth));

				_packageSelectionIndices[companyName] = EditorGUILayout.Popup(_packageSelectionIndices[companyName], packagesByThisCompany, GUILayout.MaxWidth(inputOptionWidth));
				GUILayout.EndHorizontal();
			});

			IsTutorialStep(2, packageIndex == 0, () =>
			{
				HLEditorStyles.DrawLabel("This dropdown lets you select which of this company's haptic packages you want to import.\n" +
					"NullSpace provides several packages with different objectives"
						, 105, 14);
			});
			#endregion

			#region Package Description
			HLEditorStyles.DrawLabel("Description: " + selectedPackage.description);

			#endregion

			#region Import All
			GUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("Import All Packages", EditorStyles.miniBoldLabel, GUILayout.Width(labelOptionWidth));

			//Do we have an import currently?
			bool HaveImport = CurrentImport != null;

			//Are we currently importing (or fetching)
			bool importing = HaveImport ? CurrentImport.Importing || CurrentImport.Fetching : false;

			if (!HaveImport)
			{
				SetupImport();
			}

			//TODO: Tutorial Highlights want a MaxWidth parameter?
			TutorialHighlight(3, packageIndex == 0, () =>
			{
				//Disable if we're importing (which is only if we have a current import in progress)
				using (new EditorGUI.DisabledGroupScope(importing))
				{
					string packageDesc = packagesByThisCompany.Length > 1 ? " packages by " : " package by ";
					if (GUILayout.Button(packagesByThisCompany.Length + packageDesc + companyName, GUILayout.MaxWidth(inputOptionWidth)))
					{
						Debug.LogError("This is not yet complete\n");
						CurrentImport.ImportPackage(selectedPackage);

						//Loop through all the packages
						for (int i = 0; i < packages.Count; i++)
						{
							if (packages[i].studio == companyName)
							{
								var nextPackage = packages[i];
								CurrentImport.QueueImportPackage(nextPackage);
							}
						}
					}
				}
			});
			GUILayout.EndHorizontal();

			GUILayout.Space(6);
			#endregion

			#region Import Package
			GUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("Import Package", EditorStyles.miniBoldLabel, GUILayout.Width(labelOptionWidth));

			selectedPackage = packages[_packageSelectionIndices[companyName]];

			//Do we have an import currently?
			HaveImport = CurrentImport != null;

			//Are we currently importing (or fetching)
			importing = HaveImport ? CurrentImport.Importing || CurrentImport.Fetching : false;

			if (!HaveImport)
			{
				SetupImport();
			}

			//TODO: Tutorial Highlights want a MaxWidth parameter?
			TutorialHighlight(3, packageIndex == 0, () =>
			{

				//Disable if we're importing (which is only if we have a current import in progress)
				using (new EditorGUI.DisabledGroupScope(importing))
				{
					if (GUILayout.Button("Entire [" + packagesByThisCompany[_packageSelectionIndices[companyName]] + "] Package", GUILayout.MaxWidth(inputOptionWidth)))
					{
						CurrentImport.ImportPackage(selectedPackage);
					}
				}
			});
			GUILayout.EndHorizontal();
			#endregion

			#region Import Individual
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Import Individual  ", EditorStyles.miniBoldLabel, GUILayout.Width(labelOptionWidth));

			using (new EditorGUI.DisabledGroupScope(importing))
			{
				TutorialHighlight(4, packageIndex == 0, () =>
				{
					if (GUILayout.Button("Sequence", GUILayout.MaxWidth(inputOptionWidth / 3 - 3)))
					{
						OpenFileDialogAndMakeAsset(selectedPackage.path, "sequence");
					}
				});
				TutorialHighlight(5, packageIndex == 0, () =>
				{
					if (GUILayout.Button("Pattern", GUILayout.MaxWidth(inputOptionWidth / 3 - 3)))
					{
						OpenFileDialogAndMakeAsset(selectedPackage.path, "pattern");
					}
				});
				TutorialHighlight(6, packageIndex == 0, () =>
				{
					if (GUILayout.Button("Experience", GUILayout.MaxWidth(inputOptionWidth / 3 - 3)))
					{
						OpenFileDialogAndMakeAsset(selectedPackage.path, "experience");
					}
				});
			}
			GUILayout.EndHorizontal();
			#endregion

			#region Import Tutorial Content
			IsTutorialStep(3, packageIndex == 0, () =>
				{
					HLEditorStyles.DrawLabel("These buttons will queue up importing multiple haptics at once.\n" +
						"It is recommended to choose this option when creating a new project or adding a new package to a project."
							, 105, 14);
				});
			IsTutorialStep(4, packageIndex == 0, () =>
			{
				HLEditorStyles.DrawLabel("Sequences are the smallest user component.\nThey contain no location information, merely time, duration, effect and strength.\nA sequence can be played if you give it Area information using AreaFlags.\n\n" +
					"This option is recommended if you have a single new asset or if you want to reimport a damaged haptic file."
						, 105, 14);
			});
			IsTutorialStep(5, packageIndex == 0, () =>
			{
				HLEditorStyles.DrawLabel("A pattern is the most usable user component.\nPatterns are made up of sequences with time offset and Area information. This means you can reuse common small components but give them novel location information.\n\nA pattern can represent a small to large haptic animation.\n\n" +
					"This option is recommended if you have a single new asset or if you want to reimport a damaged haptic file."
						, 105, 14);
			});
			IsTutorialStep(6, packageIndex == 0, () =>
			{
				HLEditorStyles.DrawLabel("Experiences are a complex haptic construct made up of multiple patterns with additional time offset information.\nExperiences are best described as cutscene haptics. Best used for when you have many patterns you want to execute with timing information.\n\n" +
					"This option is recommended if you have a single new asset or if you want to reimport a damaged haptic file."
						, 105, 14);
			});
			#endregion

			HLEditorStyles.DrawSliderDivider();
		}

		private void OpenFileDialogAndMakeAsset(string path, string hapticType)
		{
			var asd = string.Format("{0}/{1}s/", path, hapticType);
			string newPath = EditorUtility.OpenFilePanel("Import " + hapticType, asd, hapticType);
			if (newPath.Length > 0)
			{
				var json = CurrentImport.GetJsonFromPath(newPath);
				if (json.Value != "NSVR_FAILED")
				{
					this.CreateHapticAsset(json.Key, json.Value);
					AssetDatabase.SaveAssets();
				}
				else
				{
					OutputMessage("HLVR Failed to create the asset for [" + hapticType + "] at path\n\t[" + path + "].", MessageType.Error);
				}
			}
		}

		public override void Update()
		{
			if (CurrentImport != null)
			{
				CurrentImport.Update();
			}
			//UpdateFetch();
			//UpdateImport();
		}

		#region Moved into PackageImport class
		//private void UpdateFetch()
		//{
		//	#region Handle Fetching
		//	//If we have a work queue and aren't still fetching
		//	if (_workQueue.Count > 0 && !_fetching)
		//	{
		//		int importRate = Mathf.Min(20, Mathf.Max(1, _workQueue.Count / 5));
		//		timeElapsed += updateRate;
		//		if (timeElapsed > updateRate)
		//		{
		//			timeElapsed = 0;
		//			for (int i = 0; i < importRate; i++)
		//			{
		//				var item = _workQueue.Dequeue();
		//				CreateHapticAsset(item.Key, item.Value);

		//			}
		//			currentProgress += importRate;
		//			this.Repaint();
		//		}
		//	}
		//	else if (_workQueue.Count == 0 && !_fetching)
		//	{
		//		//Debug.Log("Work Queue fully dequeued: " + _workQueue.Count + "\n");
		//	}

		//	#endregion
		//	#region Finish Fetching
		//	//We're done!
		//	else if (_workQueue.Count == 0 && _importing)
		//	{
		//		//_status = new HelpMessage(string.Format("Imported {0}/{1} files successfully", _lastImport.TotalSucceeded, _lastImport.Total), MessageType.Info);
		//		_importing = false;
		//		EditorUtility.ClearProgressBar();

		//		this.Repaint();
		//	}
		//	#endregion
		//}
		//private void UpdateImport()
		//{
		//	#region Fetch Queue is populated
		//	if (_fetchQueue.Count > 0)
		//	{
		//		int importRate = Mathf.Min(20, Mathf.Max(1, _fetchQueue.Count / 5));
		//		timeElapsed += updateRate;
		//		if (timeElapsed > updateRate)
		//		{
		//			timeElapsed = 0;
		//			for (int i = 0; i < importRate; i++)
		//			{
		//				var item = _fetchQueue.Dequeue();
		//				if (item.Length > 0)
		//				{
		//					var result = getJsonFromPath(item);
		//					if (result.Value == "NSVR_FAILED")
		//					{
		//						continue;
		//					}
		//					else
		//					{
		//						_workQueue.Enqueue(result);
		//					}
		//				}
		//			}
		//			currentProgress += importRate;
		//			Repaint();
		//		}
		//	}
		//	#endregion
		//	#region Finish Fetching
		//	else
		//	{
		//		//If we're still fetching
		//		if (_fetching)
		//		{
		//			//We're ready to import.
		//			//_lastImport.TotalSucceeded = _workQueue.Count;
		//			//_status = new HelpMessage(string.Format("Fetched {0}/{1} files, preparing for import..", _lastImport.TotalSucceeded, _lastImport.Total), MessageType.Info);
		//			_fetching = false;
		//			_importing = true;
		//			//totalProgress = _lastImport.TotalSucceeded;
		//			EditorUtility.ClearProgressBar();

		//			this.Repaint();
		//		}
		//	}
		//	#endregion
		//}


		//private void ImportAll(object state)
		//{
		//	AssetTool.PackageInfo package = (AssetTool.PackageInfo)(state);

		//	var allHapticFiles = getFilesWithExtension(package.path + "/sequences/", ".sequence");
		//	allHapticFiles.AddRange(getFilesWithExtension(package.path + "/patterns/", ".pattern"));
		//	allHapticFiles.AddRange(getFilesWithExtension(package.path + "/experiences/", ".experience"));
		//	currentProgress = 0f;
		//	totalProgress = allHapticFiles.Count;
		//	//_lastImport.Total = allHapticFiles.Count;
		//	string li = string.Empty;
		//	for (int i = 0; i < allHapticFiles.Count; i++)
		//	{
		//		li += allHapticFiles[i] + "\n";
		//	}
		//	Debug.Log("All Haptic Files: Count [" + allHapticFiles.Count + "]" + li);
		//	_fetchQueue = new Queue<string>(allHapticFiles);
		//	_fetching = true;
		//	Debug.Log("Created Fetch Queue: " + _fetchQueue.Count + "\n");

		//	var results = getAllJsonFromPaths(allHapticFiles);
		//	//_lastImport.TotalSucceeded = results.Count;
		//	//_lastImport.Total = allHapticFiles.Count;
		//	//Debug.Log("Last Import: " + _lastImport.Total + "   " + _lastImport.Total);
		//	_workQueue = new Queue<KeyValuePair<string, string>>(results);
		//}

		//KeyValuePair<string, string> getJsonFromPath(string path)
		//{
		//	//It is likely very difficult to provide an empty path here.
		//	if (path == "")
		//	{
		//		OutputMessage("Invalid path provided to getJsonFromPath.", MessageType.Error);
		//		return new KeyValuePair<string, string>("NSVR_EMPTY_PATH", "NSVR_FAILED");
		//	}

		//	bool continueInstead;
		//	//Attempt to get json of the haptic definition file from the tool
		//	var json = TryGetJsonHapticDefinition(path, out continueInstead);

		//	#region Old Block (before making TryGetJSonHapticDefinition)
		//	//try
		//	//{
		//	//	json = _assetTool.GetHapticDefinitionFileJson(path);
		//	//}
		//	//catch (InvalidOperationException e)
		//	//{
		//	//	_pathError = true;
		//	//	//The filename was not set. This could be if the registry key was not found
		//	//	OutputMessage("Invalid Operation Exception - Could not locate the HapticAssetTools.exe program, make sure the HLVR Service was installed. Try reinstalling if the problem persists.", MessageType.Error);

		//	//	Debug.LogError("[HLVR] Could not locate the HapticAssetTools.exe program, make sure the HLVR Service was installed. Try reinstalling if the problem persists." + e.Message);
		//	//	return new KeyValuePair<string, string>("NSVR_NO_HAT", "NSVR_FAILED");
		//	//}
		//	//catch (System.ComponentModel.Win32Exception e)
		//	//{
		//	//	_pathError = true;
		//	//	OutputMessage("Win32Exception - Could not open the HapticAssetTools.exe program (was it renamed? Does it exist within the service install directory?)", MessageType.Error);

		//	//	Debug.LogError("[HLVR] Could not open the HapticAssetTools.exe program (was it renamed? Does it exist within the service install directory?): " + e.Message);
		//	//	return new KeyValuePair<string, string>("NSVR_NO_OPEN", "NSVR_FAILED");
		//	//} 
		//	#endregion

		//	//If the asset tool succeeded in running, but returned nothing, it's an error
		//	if (json.Length < 1)
		//	{
		//		_pathError = true;
		//		Debug.LogWarning("[HLVR] Unable to load path [" + path + "] it's probably malformed\n");

		//		OutputMessage("Empty json result - likely a failure to load from\n\tpath [" + path + "]", MessageType.Error);

		//		return new KeyValuePair<string, string>("NSVR_EMPTY_RESPONSE", "NSVR_FAILED");
		//	}
		//	else
		//	{
		//		_pathError = false;
		//	}

		//	return new KeyValuePair<string, string>(path, json);
		//}

		//private List<KeyValuePair<string, string>> getAllJsonFromPaths(List<string> paths)
		//{
		//	var results = new List<KeyValuePair<string, string>>();
		//	foreach (var path in paths)
		//	{
		//		//If they clicked away from the file dialog, we won't have a valid path
		//		if (path.Length < 1)
		//		{
		//			continue;
		//		}

		//		//Attempt to get json of the haptic definition file from the tool
		//		bool continueInstead;
		//		var json = TryGetJsonHapticDefinition(path, out continueInstead);

		//		//If the asset tool succeeded in running, but returned nothing, it's an error
		//		if (json.Length < 1)
		//		{
		//			Debug.LogWarning("[HLVR] Unable to load " + path + " it's probably malformed");
		//			OutputMessage("Unable to load path [" + path + "] - it is probably malformed", MessageType.Warning);
		//			continue;
		//		}

		//		results.Add(new KeyValuePair<string, string>(path, json));
		//	}
		//	OutputMessage("Getting all JSON from paths.\n\tAttempted: [" + paths.Count + "]\n\tSuccesses: [" + results.Count + "]", MessageType.None);
		//	//Debug.Log("Get all JSON from paths\n" + paths.Count + "   " + results.Count);

		//	return results;
		//}

		//private string TryGetJsonHapticDefinition(string path, out bool continueInstead)
		//{
		//	continueInstead = false;
		//	string json = "";
		//	try
		//	{
		//		if (HLEditor != null && HLEditor.DebugHardlightEditor)
		//		{
		//			Debug.Log("\tGet haptic definition file from path\n\t\t[" + path + "]\n");
		//		}
		//		json = _assetTool.GetHapticDefinitionFileJson(path);
		//	}
		//	catch (InvalidOperationException e)
		//	{
		//		//The filename was not set. This could be if the registry key was not found
		//		Debug.LogError("[HLVR] Could not locate the HapticAssetTools.exe program, make sure the HLVR Service was installed. Try reinstalling if the problem persists." + e.Message);
		//		continueInstead = true;
		//	}
		//	catch (System.ComponentModel.Win32Exception e)
		//	{
		//		Debug.LogError("[HLVR] Could not open the HapticAssetTools.exe program (was it renamed? Does it exist within the service install directory?): " + e.Message);
		//		continueInstead = true;
		//	}
		//	catch (HapticsAssetException e)
		//	{
		//		Debug.LogError("[HLVR] Haptics Asset Exception loading item at path [" + path + "]: " + e.Message);
		//		continueInstead = true;
		//	}
		//	return json;
		//}

		//List<string> getFilesWithExtension(string directory, string extension)
		//{
		//	List<string> outPaths = new List<string>();
		//	var allFiles = System.IO.Directory.GetFiles(directory);
		//	foreach (var potentialFile in allFiles)
		//	{
		//		if (System.IO.Path.GetExtension(potentialFile) == extension)
		//		{
		//			outPaths.Add(potentialFile);
		//		}
		//	}
		//	return outPaths;
		//}
		#endregion
	}
}