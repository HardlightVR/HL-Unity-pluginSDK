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
	public class PackageViewer : MonoBehaviour
	{
		public Text Folder;
		public PopulateContainer fileContainer;
		public Button commander;
		public string path;
		public string myName = "";
		public string myNameSpace = "";
		private bool initialized = false;
		private AssetTool _assetTool;

		private void FindRequiredElements()
		{
			try
			{
				Folder = transform.GetChild(0).GetChild(1).GetComponent<Text>();
				fileContainer = GetComponent<PopulateContainer>();
				fileContainer.container = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>();
				fileContainer.prefab = Resources.Load<GameObject>("UI/Library Element");
			}
			catch (System.Exception e)
			{
				Debug.LogError("PackageViewer's FindRequiredElements has thrown an exception.\nThis was likely caused by some of its required elements being modified in the prefab or a missing component\n" + e.Message + "\n");
			}
		}


		//When a directory is 'opened'
		public void Init(AssetTool tool, AssetTool.PackageInfo package)
		{
			if (!initialized)
			{
				FindRequiredElements();
				initialized = true;
			}
			_assetTool = tool;
			myName = Path.GetFileName(package.path);
			myNameSpace = package.@namespace;
			Folder.text = myName + " Contents";

			path = package.path;

			PopulateMyDirectory(path);
		}
		private List<string> RetrieveFilesInFolder(string folderPath)
		{
			string[] unfilteredFiles = Directory.GetFiles(folderPath);

			return unfilteredFiles.Where((string filename) =>
			{
				string ext = Path.GetExtension(filename);
				return ext == ".pattern"
					|| ext == ".sequence"
					|| ext == ".experience";

			}).ToList();
		}
		//Fill the directory with library elements based on the haptics found
		void PopulateMyDirectory(string path)
		{
			var validSequences = RetrieveFilesInFolder(path + "/sequences");
			var validPatterns = RetrieveFilesInFolder(path + "/patterns");
			var validExperiences = RetrieveFilesInFolder(path + "/experiences");

			var allFiles = validExperiences.Concat(validPatterns).Concat(validSequences);

			//A natural result of the haptics being loaded by order of folder means they'll be pre-sorted.
			foreach (string element in allFiles)
			{
				CreateRepresentations(element);
			}

			//Make sure we scroll to the top of the ScrollRect
			ScrollRect sRect = GetComponentInChildren<ScrollRect>();
			sRect.verticalNormalizedPosition = 1;
		}

		//This returns a bool to prep for future failure possibilities, such as initialize/validation failure.
		public bool CreateRepresentations(string element)
		{
			//Debug.Log(s + "\n");
			LibraryElement libEle = fileContainer.AddPrefabToContainerReturn().GetComponent<LibraryElement>();
			//Elements need to be initialized so they get the proper name/icon/color
			libEle.Init(_assetTool, element, myNameSpace);

			return true;
		}

		public bool SortElements()
		{
			//Delete all of them
			fileContainer.Clear();

			//Repopulate them
			PopulateMyDirectory(path);

			//This is because sorting using Unity hierarchy is gross.
			//Ideal solution would be the elements keep track of their intended index and name, which makes sorting/rearranging easier.

			return true;
		}
	}
}