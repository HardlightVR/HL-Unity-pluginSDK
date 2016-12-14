﻿using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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


		
		//When a directory is 'opened'
		public void Init(string filePath, string newNamespace)
		{
			string[] split = filePath.Split(new string[] { "StreamingAssets\\" }, System.StringSplitOptions.None);
			//Debug.Log(split[split.Length - 1] + "\n");
			myName = split[split.Length - 1];
			myNameSpace = DirectoryToNamespace.GetNameSpace(filePath);
			//Debug.Log(myName + "\n");
			Folder.text = myName + " Contents";
			path = filePath;

			PopulateMyDirectory(path);
		}

		//Fill the directory with library elements based on the haptics found
		void PopulateMyDirectory(string path)
		{
			List<FileInfo> hapticFiles = new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories).ToList();

			var validFiles = (from validFile in hapticFiles
							  where ((validFile.Extension.Contains(".seq") || validFile.Extension.Contains(".pat") || validFile.Extension.Contains(".exp")) && !validFile.Extension.Contains(".meta"))
							  select validFile.FullName).ToList();

			//A natural result of the haptics being loaded by order of folder means they'll be pre-sorted.
			foreach (string s in validFiles)
			{
				//Debug.Log(s + "\n");
				LibraryElement libEle = fileContainer.AddPrefabToContainerReturn().GetComponent<LibraryElement>();
				libEle.myButton.transform.localScale = Vector3.one;
				libEle.myButton.name = s;

				//Elements need to be initialized so they get the proper name/icon/color
				libEle.Init(s, myNameSpace);
			}

			//Make sure we scroll to the top of the ScrollRect
			ScrollRect sRect = GetComponentInChildren<ScrollRect>();
			sRect.verticalNormalizedPosition = 1;
		}
	}
}