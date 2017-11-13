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
	public class HapticLibraryElement : LibraryElement
	{
		public ScriptableObjectHaptic OnPlay;
		public void Init(AssetTool tool, string fullPath, string packageName = "")
		{
			if (fullPath.Length == 0)
			{
				Debug.Log("Cleaning up: " + name + " from editor modification\n");
				Destroy(gameObject);
			}
			else
			if (!initialized)
			{
				FindRequiredElements();
				_assetTool = tool;
				_caller = new AsyncMethodCaller(_assetTool.GetHapticDefinitionFile);

				fullFilePath = fullPath;
				fileAndExt = Path.GetFileName(fullPath);
				fileName = Path.GetFileNameWithoutExtension(fullPath);
				displayName.text = fileName;
				myNamespace = packageName;
				name = Path.GetFileName(fullPath) + " [LibEle]";

				playButton.transform.localScale = Vector3.one;
				processButton.transform.parent.parent.gameObject.SetActive(false);

				ConfigureAsSpecificTypeOfLibraryElement();

				copyIcon.sprite = LibraryManager.Inst.copyIcon;


				//Temporary disabling of the copy-me feature.
				//copyButton.transform.parent.parent.gameObject.SetActive(false);

				//casey removed: because the asset tool does it now
				//	if (!ValidateFile())
				//{
				//		MarkElementBroken();
				//	}

				lastModified = FileModifiedHelper.GetLastModified(fullFilePath);

				initialized = true;
			}
		}

		protected IAsyncResult _asyncResult;
		protected delegate void HapticDefinitionCallback(HapticDefinitionFile file);
		protected delegate HapticDefinitionFile AsyncMethodCaller(string path);
		protected AsyncMethodCaller _caller;

		protected override void ConfigureAsSpecificTypeOfLibraryElement()
		{
			if (fullFilePath.Contains(".seq"))
			{
				ConfigureAsSequenceLibraryElement();
			}
			else if (fullFilePath.Contains(".pat"))
			{
				ConfigureAsPatternLibraryElement();
			}
			else if (fullFilePath.Contains(".exp"))
			{
				ConfigureAsExperienceLibraryElement();
			}
		}

		protected void ConfigureAsExperienceLibraryElement()
		{
			typeOfLibraryElement = LibraryElementType.Experience;
			myIcon.sprite = LibraryManager.Inst.expIcon;
			visual.color = LibraryManager.Inst.expColor;
			TooltipDescriptor.AddDescriptor(gameObject, fileName + " - Experience", "Plays experience which is composed of multiple Patterns.");
			TooltipDescriptor.AddDescriptor(openLocationButton.gameObject, "<color=#FF4500>Edit File</color>", "View Source of [" + fileName + "]\nWe recommend a text editor", new Color32(135, 206, 255, 225));

			TooltipDescriptor.AddDescriptor(copyButton.gameObject, "<color=#FF4500>Copy and Edit File</color>", "Creates a duplicate of [" + fileName + "]\nThen open and edit the new file.", new Color32(135, 206, 255, 225));
			processButton.transform.parent.parent.gameObject.SetActive(false);

		}

		protected void ConfigureAsPatternLibraryElement()
		{
			typeOfLibraryElement = LibraryElementType.Pattern;
			myIcon.sprite = LibraryManager.Inst.patIcon;
			visual.color = LibraryManager.Inst.patColor;
			TooltipDescriptor.AddDescriptor(gameObject, fileName + " - Pattern", "Plays pattern which is composed of sequences on specified areas");
			TooltipDescriptor.AddDescriptor(openLocationButton.gameObject, "<color=#FF4500>Edit File</color>", "View Source of [" + fileName + "]\nWe recommend a text editor", new Color32(135, 206, 255, 225));

			TooltipDescriptor.AddDescriptor(copyButton.gameObject, "<color=#FF4500>Copy and Edit File</color>", "Creates a duplicate of - [" + fileName + "]\nThen open and edit the new file.", new Color32(135, 206, 255, 225));
			processButton.transform.parent.parent.gameObject.SetActive(false);
		}

		protected void ConfigureAsSequenceLibraryElement()
		{
			typeOfLibraryElement = LibraryElementType.Sequence;
			myIcon.sprite = LibraryManager.Inst.seqIcon;
			visual.color = LibraryManager.Inst.seqColor;
			TooltipDescriptor.AddDescriptor(gameObject, fileName + " - Sequence", "Plays on all selected pads\nOr when the green haptic trigger touches a pad");
			TooltipDescriptor.AddDescriptor(openLocationButton.gameObject, "<color=#FF4500>Edit File</color>", "View Source of [" + fileName + "]\nWe recommend a text editor", new Color32(135, 206, 255, 225));

			TooltipDescriptor.AddDescriptor(copyButton.gameObject, "<color=#FF4500>Copy and Edit File</color>", "Creates a duplicate of - [" + fileName + "]\nThen open and edit the new file.", new Color32(135, 206, 255, 225));
			processButton.transform.parent.parent.gameObject.SetActive(false);
		}

		protected override bool ExecuteLibraryElement()
		{
			//Debug.Log("File has been modified since load: [" + FileHasBeenModified() + "]\n");
			try
			{
				if (LibraryManager.Inst.LastPlayed != null && LibraryManager.Inst.StopLastPlaying)
				{
					LibraryManager.Inst.LastPlayed.Stop();
				}

				_asyncResult = _caller.BeginInvoke(fullFilePath, null, null);
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to play haptic [" + fullFilePath + "]" + "\n" + e.Message);
				return false;
			}
			return base.ExecuteLibraryElement();
		}

		protected override void MidwayUpdate()
		{
			ResolveAfterGettingHDF();

			base.MidwayUpdate();
		}
		private void ResolveAfterGettingHDF()
		{
			if (_asyncResult != null && _asyncResult.IsCompleted)
			{
				bool successfullyGotHdf = false;
				HapticDefinitionFile hdf = new HapticDefinitionFile();
				try
				{
					hdf = _caller.EndInvoke(_asyncResult);
					successfullyGotHdf = true;
				}
				catch (Exception e)
				{
					successfullyGotHdf = false;
					Debug.LogException(e);
				}
				_asyncResult = null;

				if (successfullyGotHdf)
				{
					if (hdf.root_effect.type == "sequence")
					{
						OnPlay = CodeHapticFactory.CreateSequenceFromHDF(hdf.root_effect.name, hdf);

						LibraryManager.Inst.SetTriggerSequence((OnPlay as HapticSequence), hdf.root_effect.name);
						AreaFlag flag = LibraryManager.Inst.GetActiveAreas();

						var handle = (OnPlay as HapticSequence).CreateHandle(flag);

						PlayHandleAndSetLastPlayed(handle);
					}
					else if (hdf.root_effect.type == "pattern")
					{
						OnPlay = CodeHapticFactory.CreatePatternFromHDF(hdf.root_effect.name, hdf);
						PlayHandleAndSetLastPlayed((OnPlay as HapticPattern).CreateHandle());
					}
					else if (hdf.root_effect.type == "experience")
					{
						OnPlay = CodeHapticFactory.CreateExperienceFromHDF(hdf.root_effect.name, hdf);
						PlayHandleAndSetLastPlayed((OnPlay as HapticExperience).CreateHandle());
					}
				}
			}
		}

		protected void PlayHandleAndSetLastPlayed(HapticHandle h)
		{
			LibraryManager.Inst.LastPlayed = h;
			if (LibraryManager.Inst.LastPlayed != null)
			{
				LibraryManager.Inst.LastPlayed.Play();
			}
		}

		//protected bool ValidateFile()
		//{
		//	//Open the file

		//	//Check each of the validation conditions
		//	//Keep track of the returned reasons.
		//	validationFailureReasons += ValidateFileName();

		//	//Then return true or false.
		//	if (validationFailureReasons.Length > 0)
		//	{
		//		return false;
		//	}
		//	return true;
		//}

		//protected string ValidateFileName()
		//{
		//	//Debug.Log("[" + myType.ToString() + "]  [" + fileName + "]  [" + fileAndExt + "]  [" + myNamespace + "]\n", this);

		//	return string.Empty;

		//	//return "Failed due to comma after the last element\n";
		//}

		protected override void CheckInitialized()
		{
			if (!initialized)
				Init(_assetTool, "");
		}

		protected string EvaluateName(string packageName)
		{
			string fileName = fullFilePath;
			string[] split = fileName.Split(new char[] { '\\', '/' });
			fileName = split[split.Length - 1];
			return fileName;
		}
	}
}