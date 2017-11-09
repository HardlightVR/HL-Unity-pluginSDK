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
using System.ComponentModel;

namespace Hardlight.SDK.Demos
{
	public class FolderLibraryElement : LibraryElement
	{
		protected override void CheckInitialized()
		{
			if (!initialized)
				Init(null, null);
			base.CheckInitialized();
		}
		public void Init(AssetTool.PackageInfo package, AssetTool tool)
		{
			if (package == null || tool == null)
			{
				Debug.Log("Cleaning up: " + name + " from editor modification\n");
				Destroy(gameObject);
			}
			else
			if (!initialized)
			{
				FindRequiredElements();
				_assetTool = tool;

				myPackage = package;

				ConfigureAsSpecificTypeOfLibraryElement();

				initialized = true;
			}
		}
		protected override void ConfigureAsSpecificTypeOfLibraryElement()
		{
			ConfigureAsFolderLibraryElement();
		}
		protected void ConfigureAsFolderLibraryElement()
		{
			name = myPackage.@namespace + " [LibEle]";
			fullFilePath = myPackage.path;
			typeOfLibraryElement = LibraryElementType.Folder;
			fileAndExt = Path.GetFileName(myPackage.path);
			myIcon.sprite = LibraryManager.Inst.folderIcon;

			playButton.transform.localScale = Vector3.one;
			processIcon.sprite = LibraryManager.Inst.processIcon;

			visual.color = LibraryManager.Inst.folderColor;
			copyButton.transform.parent.parent.gameObject.SetActive(false);
			processButton.transform.parent.parent.gameObject.SetActive(true);
			displayName.text = Path.GetFileName(myPackage.path);
			myNamespace = myPackage.@namespace;
			fileName = myPackage.path;
			TooltipDescriptor.AddDescriptor(gameObject, myPackage.@namespace, "Haptic Package: A collection of sequences, patterns and experiences\nDefined by its config.json");
			TooltipDescriptor.AddDescriptor(openLocationButton.gameObject, "<color=#FF4500>Open Folder</color>", "View directories of " + fileName, new Color32(135, 206, 255, 225));

			TooltipDescriptor.AddDescriptor(processButton.gameObject, "<color=#FF4500>Convert Package to HDF</color>", "<b>Used for Unreal Engine Assets</b>\nConverts all elements within the package [" + myPackage.@namespace + "] to standalone HDFs", new Color32(135, 206, 255, 225));
		}

		protected string OutputMessage = string.Empty;
		protected string PathMessage = string.Empty;
		protected override bool ConvertElement()
		{
			//If we are a folder element
			//	Run an async element handling.
			//
			if (myPackage != null)
			{
				ConvertPackageToHDF(myPackage,
							//Success Delegate
							delegate ()
							{
								//This success delegate never occurs for some reason. However, this isn't a very necessary element it was not fixed

								OutputMessage = " No detected errors. ";
								PathMessage = myPackage.path + " - Converted";
							},
							//Failure Delegate
							delegate (Exception except)
							{
								ToMarkAsBroken = true;
								OutputMessage = except.Message;
								PathMessage = myPackage.path + " - Converted";
								//Make the file red to show it's broken.
								//Highlight the edit button.
								//Some sort of error reporting.
							});

				return true;
			}
			return false;
		}
		protected void ConvertPackageToHDF(AssetTool.PackageInfo package, SuccessCallback successCallback, ExceptionCallback failCallback)
		{
			AsyncHDFConversionCaller caller = new AsyncHDFConversionCaller(_assetTool.ConvertPackageToHDFs);

			//Using this is easier than splitting on last occurrence of forward slash and going up one directory.
			string targetDirectory = package.path + " - Converted";

			/*IAsyncResult r = */
			caller.BeginInvoke(package, targetDirectory, delegate (IAsyncResult iar)
			{
				AsyncResult result = (AsyncResult)iar;
				AsyncHDFConversionCaller caller2 = (AsyncHDFConversionCaller)result.AsyncDelegate;

				try
				{
					string errors = caller2.EndInvoke(iar);
					if (errors.Length > 0)
					{
						//Todo: make better broken case
						Debug.LogError("Encountered errors when converting package [" + package.@namespace + "] to HDFs:\n\t" + errors);
					}
				}
				catch (Exception whatTheHellMicrosoft)
				{
					Debug.LogError("Async Delegate Error converting package directory to HDFs - path [" + package.path + "]\n" + whatTheHellMicrosoft.Message);

					failCallback(whatTheHellMicrosoft);
				}
			}, successCallback);
		}

		protected override void MidwayUpdate()
		{
			if (OutputMessage.Length > 0)
			{
				Debug.Log(OutputMessage + "  " + PathMessage + "\n");
				LibraryManager.Inst.ResultDisplay.Display(OutputMessage, PathMessage);
				OutputMessage = string.Empty;
				PathMessage = string.Empty;
			}
			base.MidwayUpdate();
		}
	}
}