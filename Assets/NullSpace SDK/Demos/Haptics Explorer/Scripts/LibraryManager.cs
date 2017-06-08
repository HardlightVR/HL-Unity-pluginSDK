/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using NullSpace.SDK.FileUtilities;
namespace NullSpace.SDK.Demos
{
	public class LibraryManager : MonoBehaviour
	{
		public static LibraryManager Inst;

		public GameObject tempRef;

		public Sprite seqIcon;
		public Color seqColor = new Color32(103, 255, 148, 255);
		public Sprite patIcon;
		public Color patColor = new Color32(82, 162, 255, 255);
		public Sprite expIcon;
		public Color expColor = new Color32(255, 239, 28, 255);
		public Sprite errorIcon;
		public Color errorColor = new Color32(176, 14, 14, 255);
		public Sprite folderIcon;
		public Color folderColor = new Color32(174, 174, 174, 255);
		public Color changedColor = new Color32(107, 171, 163, 255);
		public Sprite processIcon;
		public Sprite copyIcon;

		public Dictionary<string, PackageViewer> ContentsDict;
		public PopulateContainer ContentContainer;
		public PopulateContainer FolderContainer;
		public PackagingResults ResultDisplay;

		public HapticTrigger greenBox;
		public Text greenBoxText;
		public SuitRegionSelectorDemo selector;

		private ScrollRect DirectoryScroll;
		private PackageViewer currentSelected;

		private AssetTool assetTool;
		public PackageViewer Selection
		{
			set
			{
				if (value != null)
				{
					//Toggles off the old
					if (currentSelected != null)
					{
						currentSelected.gameObject.SetActive(false);
					}
					//Toggles on the new.
					currentSelected = value;
					currentSelected.gameObject.SetActive(true);
				}
			}
			get { return currentSelected; }
		}

		public string LastPackageAccessed
		{
			set
			{
				//Debug.Log("Setting last accessed to : " + value + "\n");
				PlayerPrefs.SetString("LastPackageAccessed", value);
			}
			get
			{
				if (PlayerPrefs.HasKey("LastPackageAccessed"))
				{
					//Debug.Log("Last Package Accessed [" + PlayerPrefs.GetString("LastPackageAccessed") + "]\n");

					return PlayerPrefs.GetString("LastPackageAccessed");
				}
				return "";
			}
		}

		public bool StopLastPlaying;
		public HapticHandle LastPlayed;
		public HapticSequence LastSequence;
		public string LastSequenceName;

		public string currentFolderSelected = "";
		public string lastFileSelected = "";

		//This is so the project will remember the last thing they had open
		//Not implemented just yet.
		public string LastOpened
		{
			get
			{
				if (PlayerPrefs.HasKey("LastHapticFolder"))
				{
					return PlayerPrefs.GetString("LastHapticFolder");
				}
				return "";
			}
			set
			{
				PlayerPrefs.SetString("LastHapticFolder", value);
			}
		}

		void FindRequiredElements()
		{
			seqIcon = Resources.Load<Sprite>("Button Icons/SequenceSprite");
			patIcon = Resources.Load<Sprite>("Button Icons/PatternSprite");
			expIcon = Resources.Load<Sprite>("Button Icons/ExperienceSprite");
			errorIcon = Resources.Load<Sprite>("Button Icons/cancel");
			folderIcon = Resources.Load<Sprite>("Button Icons/Open Folder");
			processIcon = Resources.Load<Sprite>("Button Icons/cardboard-box");
			copyIcon = Resources.Load<Sprite>("Button Icons/files");

			ContentContainer = GameObject.Find("Package Viewer Parent").GetComponent<PopulateContainer>();
			ContentContainer.prefab = Resources.Load<GameObject>("UI/Package Viewer");
			FolderContainer = GameObject.Find("Folder Elements").GetComponent<PopulateContainer>();
			FolderContainer.prefab = Resources.Load<GameObject>("UI/Library Element");

			selector = GameObject.Find("Suit Region Demo").GetComponent<SuitRegionSelectorDemo>();
			greenBox = GameObject.Find("Haptic Trigger - Green Box").GetComponent<HapticTrigger>();
			greenBoxText = greenBox.transform.GetChild(0).GetChild(1).GetComponent<Text>();
			ResultDisplay = GameObject.FindObjectOfType<PackagingResults>();
			ResultDisplay.SetVisibility(false);
		}

		void Awake()
		{
			//For easier referencing in this small scale tool.
			if (Inst == null)
			{
				Inst = this;
			}
			else
			{
				Debug.LogError("Multiple Library Managers.\nDuplicate will now self destructing.\n");
				Destroy(gameObject);
			}

			FindRequiredElements();

			//This is where we will keep reference to previously opened files/directories.
			ContentsDict = new Dictionary<string, PackageViewer>();
			assetTool = new AssetTool();
		}

		void Start()
		{
			//Populate the folders that contain packages.
			SetupLibraries();
			DirectoryScroll = transform.FindChild("Folder Viewer").FindChild("Sub Directory").FindChild("Scroll View").GetComponent<ScrollRect>();

			SetTriggerSequence("Haptics/pulse", "ns.pulse");

			//Minor tweak to get the scroll position to start at the top.
			DirectoryScroll.verticalNormalizedPosition = 1;
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.G))
			{
				ResultDisplay.Display("Hello World", "");
			}
			SetGUI();

			GetInput();
		}
		void SetGUI()
		{
			greenBoxText.text = LastSequenceName;
		}

		//This includes a quit condition
		void GetInput()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Application.Quit();
			}

		}

		public void SetupLibraries()
		{
			//Base for the path - has ALL the folders
			string path = Application.streamingAssetsPath;

			assetTool.SetRootHapticsFolder(path + "/Haptics/");
			var packages = assetTool.TryGetPackageInfo();



			for (int i = 0; i < packages.Count; i++)
			{
				//Debug.Log("Directory: " + folders[i] + "\n");
				//A library element represents either a folder or a haptic file. It will configure it's appearance based on its name (if it has .seq/.exp/.pat in its name, it'll adjust accordingly)
				LibraryElement libEle = FolderContainer.AddPrefabToContainerReturn().GetComponent<LibraryElement>();
				libEle.Init(packages[i], assetTool);
				libEle.playButton.transform.localScale = Vector3.one;
				libEle.playButton.name = Path.GetFileName(packages[i].path);
				string folderName = packages[i].path;
				AssetTool.PackageInfo tempP = packages[i];
				libEle.playButton.onClick.AddListener(
					() => { SelectDirectory(tempP, libEle.playButton); }
					);
				tempRef = libEle.gameObject;

				//Debug.Log(Selection == null);
				//string lastAccessed = LastOpened;

				//If we have something that we last accessed
				var found = packages.Find(item => item.path == LastPackageAccessed);
				if (LastPackageAccessed.Length > 0 && found != null)
				{
					if (folderName == LastPackageAccessed)
					{
						SelectDirectory(packages[i], libEle.playButton);
					}
				}
				else if (packages.Count > 0)
				{
					//Select the first folder
					SelectDirectory(packages[i], libEle.playButton);
				}
			}
		}

		public AreaFlag GetActiveAreas()
		{
			AreaFlag flag = AreaFlag.None;

			//Safely proceed to avoid broken refs.
			if (selector != null)
			{
				for (int i = 0; i < selector.suitObjects.Count; i++)
				{
					//If this selected element isn't null
					if (selector.suitObjects[i] != null)
					{
						//Add that flag
						flag = flag | selector.suitObjects[i].regionID;
					}
				}
			}
			else
			{
				return AreaFlag.All_Areas;
				//Debug.LogError("Selector is null. Check Library Manager in the inspector\n");
			}

			return flag;
		}

		public void SetTriggerSequence(HapticSequence sequence, string labelName)
		{
			LastSequence = sequence;
			greenBox.SetSequence(sequence);
			LastSequenceName = labelName;
			//Debug.Log("Set Last Sequence! \t" + (LastSequence != null) + "\n");
		}

		public void SetTriggerSequence(string sequenceName, string visibleName)
		{
			//HapticSequence newSeq = new HapticSequence();
			try
			{
				LastSequence = new HapticSequence();
				LastSequence.LoadFromAsset(sequenceName);
				greenBox.SetSequence(LastSequence);
			}
			catch (HapticsAssetException hExcept)
			{
				Debug.LogError("[Library Manager - Haptics Asset Exception]   Exception while loading sequence - " + sequenceName + "\n\t" + hExcept.Message);
			}
			catch (System.Exception e)
			{
				Debug.LogError("[Exception]   \n\tLoad failed and set was disallowed\n" + e.Message);
			}
			LastSequenceName = visibleName;
			greenBoxText.text = visibleName;
		}

		//Creates a viewer for the given folder.
		public PackageViewer SelectDirectory(AssetTool.PackageInfo packageInfo, Button button)
		{
			//If we haven't already opened one, make a new one
			if (!ContentsDict.ContainsKey(packageInfo.path))
			{
				//LastOpened = folderSelected;

				PackageViewer pv = ContentContainer.AddPrefabToContainerReturn().GetComponent<PackageViewer>();
				//So we could change the color of opened folder...
				pv.commander = button;

				//Ask the package viewer to do it's own setup.
				pv.Init(assetTool, packageInfo);

				//Hold onto the parent object so we can easily switch between opened ones.
				ContentsDict.Add(packageInfo.path, pv);
			}

			//Open the one we asked for (it was made if it didn't exist, or it already exists)
			//Debug.Log("Entry already exists for: " + folderSelected + "\n");
			Selection = ContentsDict[packageInfo.path];

			//Remember what they had open
			LastPackageAccessed = packageInfo.path;
			//Debug.Log("Opened Directory:" + LastPackageAccessed + "\n");

			return ContentsDict[packageInfo.path];
		}

		#region Static Helper Methods
		//These functions are temporary directory controls to get the appropriate subfolders.
		//Being as this is mostly an editor/helper application, these don't need to be runtime efficient.
		public static IEnumerable<string> GetSubdirectoriesContainingOnlyFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			//This doesn't find directories in other directories quite yet.
			return from subdirectory in Directory.GetDirectories(path, "*", searchOption)
				   where DirectoryContains(subdirectory, searchPattern)
				   select subdirectory;
		}
		public static bool DirectoryContains(string dir, string searchPattern = "*")
		{
			var file = Directory.GetFiles(dir, searchPattern, SearchOption.TopDirectoryOnly)
					.FirstOrDefault();

			return (file != null);
		}

		public static string CleanPathToFile(string fullFilePath)
		{
			string fileName = fullFilePath;
			string[] split = fileName.Split(new char[] { '\\', '/' });
			fileName = split[split.Length - 1];
			return fileName;
		}
		#endregion
	}
}
