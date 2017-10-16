/* This code is licensed under the NullSpace Developer Agreement, available here:
** ***********************
** http://www.hardlightvr.com/wp-content/uploads/2017/01/NullSpace-SDK-License-Rev-3-Jan-2016-2.pdf
** ***********************
** Make sure that you have read, understood, and agreed to the Agreement before using the SDK
*/

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using IOHelper;
using Hardlight.SDK.FileUtilities;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// Base class for the library elements (folders & haptic elements)
	/// </summary>
	public class LibraryElement : ContainerItem
	{
		protected AssetTool _assetTool;
		protected string fullFilePath = "";
		//For the visual representation of the haptic effect.
		public enum LibraryElementType { Sequence, Pattern, Experience, Folder }

		[Header("Library Element Attributes")]
		public LibraryElementType typeOfLibraryElement = LibraryElementType.Sequence;

		[Space(10)]
		[Header("These are found by index search", order = 1)]
		[Space(-10, order = 2)]
		[Header("If you reorder LibEle's children, adjust  ", order = 3)]
		[Space(-10, order = 4)]
		[Header("LibraryElement.FindRequiredElements()'s contents  ", order = 5)]
		public Image visual;
		public Image myIcon;
		public Image processIcon;
		public Image copyIcon;
		public Text displayName;
		public Button playButton;
		public Button openLocationButton;
		public Button copyButton;
		public Button processButton;
		[Space(10)]
		public string myNamespace;
		public string fileAndExt;
		public string fileName;
		public DateTime lastModified;
		public AssetTool.PackageInfo myPackage;

		protected bool ToMarkAsBroken = false;
		//protected bool ToMarkAsChanged = false;
		protected bool initialized = false;
		protected string validationFailureReasons = string.Empty;

		protected void FindRequiredElements()
		{
			try
			{
				visual = transform.GetChild(0).GetComponent<Image>();
				myIcon = visual.transform.GetChild(0).GetChild(1).GetComponent<Image>();
				processIcon = visual.transform.GetChild(3).GetChild(1).GetComponent<Image>();
				copyIcon = visual.transform.GetChild(2).GetChild(1).GetComponent<Image>();

				displayName = visual.transform.GetChild(1).GetChild(0).GetComponent<Text>();

				playButton = visual.transform.GetChild(1).GetComponent<Button>();
				openLocationButton = myIcon.transform.GetChild(0).GetComponent<Button>();
				copyButton = copyIcon.transform.GetChild(0).GetComponent<Button>();
				processButton = processIcon.transform.GetChild(0).GetComponent<Button>();
			}
			catch (Exception e)
			{
				Debug.LogError("FindRequiredElements has thrown an exception.\nThis was likely caused by adjusting the prefab for Library Element and adjusting the order of it's children or removing children. Please investigate\n" + e.Message + "\n");
			}
		}

		protected virtual void ConfigureAsSpecificTypeOfLibraryElement()
		{
			
		}

		protected virtual void Start()
		{
			if (openLocationButton != null)
			{
				openLocationButton.onClick.AddListener(() =>
				{
					bool playResult = OpenFile();
					if (!playResult)
					{
						MarkElementBroken();
					}
				});
			}
			if (playButton != null)
			{
				playButton.onClick.AddListener(() =>
				{
					if (FileHasBeenModified())
					{
						MarkElementChanged();
					}

					bool playResult = ExecuteLibraryElement();
					if (!playResult)
					{
						MarkElementBroken();
					}
				});
			}
			if (copyButton != null)
			{
				copyButton.onClick.AddListener(() =>
				{
					bool playResult = CopyFile(true);
					if (!playResult)
					{
						MarkElementBroken();
					}
				});
			}
			if (processButton != null)
			{
				processButton.onClick.AddListener(() =>
				{
					bool playResult = ConvertElement();
					if (!playResult)
					{
						MarkElementBroken();
					}

					//string targetDirectory = myPackage.path + " - Converted";

					//playResult = OpenFile(targetDirectory);
					//if (!playResult)
					//{
					//	MarkElementBroken();
					//}
				});
			}

			CheckInitialized();
		}

		protected void MarkElementBroken()
		{
			ToMarkAsBroken = false;
			Debug.LogError("This element [" + fileAndExt + "] is broken\n");
			//This doesn't prevent the action of the element, but it indicates that the element is broken.

			//Only update the color for now.
			//myIcon.sprite = LibraryManager.Inst.errorIcon;
			myIcon.sprite = LibraryManager.Inst.errorIcon;
			myIcon.color = LibraryManager.Inst.errorColor;
		}

		protected void MarkElementChanged()
		{
			//Debug.Log("This element [" + fileAndExt + "] is changed\n");

			//This is disabled because the reason for a visual change isn't significant enough yet.
			//myIcon.color = LibraryManager.Inst.changedColor;
		}

		protected bool FileHasBeenModified()
		{
			DateTime dt = FileModifiedHelper.GetLastModified(fullFilePath);
			//Debug.Log(dt.ToString() + "\n");

			return dt != lastModified;
		}

		public bool CopyFile(bool editImmediately = false)
		{
			try
			{
				string copyResult = CopyHelper.SafeFileDuplicate(fullFilePath);

				if (copyResult.Length > 0)
				{
					//Debug.Log("Successful copy: [" + copyResult + "]\n");

					//Ask PackageViewer to create a representation of that file.
					LibraryManager.Inst.Selection.CreateRepresentations(copyResult);
					LibraryManager.Inst.Selection.SortElements();

					if (editImmediately)
					{
						//If we want to edit it immediately, open it.
						OpenFile(copyResult);
					}
				}
			}

			catch (Exception e)
			{
				Debug.LogError("Failure to duplicate file \n\t[" + fullFilePath + "]\n" + e.Message);
				return false;
			}

			return true;
		}

		protected delegate void SuccessCallback();
		protected delegate void ExceptionCallback(Exception exceptionHit);

		protected delegate string AsyncHDFConversionCaller(AssetTool.PackageInfo info, string outDir);

		//Make this return a bool and it'll mark the LibraryElement as broken.
		protected virtual bool ExecuteLibraryElement()
		{
			return true;
		}
		protected virtual bool ConvertElement()
		{
			return true;
		}

		public bool OpenFile(string requestedFilePath = "")
		{
			//Test case to check the broken marking is working.
			//return UnityEngine.Random.Range(0, 40) > 35 ? false : true;

			try
			{
				requestedFilePath = requestedFilePath.Length > 0 ? requestedFilePath : fullFilePath;
				OpenPathHelper.Open(requestedFilePath);
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to open file path [" + fullFilePath + "]" + "\n" + e.Message);
				return false;
			}
			return true;
		}

		protected virtual void CheckInitialized()
		{

		}
		protected void Update()
		{
			CheckInitialized();

			if (ToMarkAsBroken)
				MarkElementBroken();

			MidwayUpdate();


			//Ideally changed indication would be shown by an asterisk instead of color information?
			//if (FileHasBeenModified())
			//	MarkElementChanged();
		}

		protected virtual void MidwayUpdate()
		{

		}
	}
}