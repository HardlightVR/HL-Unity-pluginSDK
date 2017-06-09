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
using NullSpace.SDK.FileUtilities;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace NullSpace.SDK.Demos
{
	public class LibraryElement : MonoBehaviour
	{
		private AssetTool _assetTool;
		string fullFilePath = "";
		//For the visual representation of the haptic effect.
		public enum LibraryElementType { Sequence, Pattern, Experience, Folder }
		public LibraryElementType myType = LibraryElementType.Sequence;

		[Header("These are found by index search", order = 0)]
		[Space(-10, order = 1)]
		[Header("If you reorder LibEle's children, adjust  ", order = 2)]
		[Space(-10, order = 3)]
		[Header("LibraryElement.FindRequiredElements()'s contents  ", order = 4)]
		public Image visual;
		public Image myIcon;
		public Image processIcon;
		public Image copyIcon;
		public Text displayName;
		public Button playButton;
		public Button openLocationButton;
		public Button copyButton;
		public Button processButton;
		[Space(10, order = 5)]
		public string myNamespace;
		public string fileAndExt;
		public string fileName;
		public DateTime lastModified;
		public AssetTool.PackageInfo myPackage;

		private bool ToMarkAsBroken = false;
		//private bool ToMarkAsChanged = false;
		private bool initialized = false;
		private string validationFailureReasons = string.Empty;

		private void FindRequiredElements()
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

		public void Init(AssetTool.PackageInfo package, AssetTool tool)
		{
			if (!initialized)
			{
				FindRequiredElements();
				_assetTool = tool;
				name = package.@namespace + " [LibEle]";
				fullFilePath = package.path;
				myType = LibraryElementType.Folder;
				fileAndExt = Path.GetFileName(package.path);
				myIcon.sprite = LibraryManager.Inst.folderIcon;

				playButton.transform.localScale = Vector3.one;
				processIcon.sprite = LibraryManager.Inst.processIcon;

				visual.color = LibraryManager.Inst.folderColor;
				copyButton.transform.parent.parent.gameObject.SetActive(false);
				processButton.transform.parent.parent.gameObject.SetActive(true);
				displayName.text = Path.GetFileName(package.path);
				myNamespace = package.@namespace;
				fileName = package.path;
				TooltipDescriptor.AddDescriptor(gameObject, package.@namespace, "Haptic Package: A collection of sequences, patterns and experiences\nDefined by its config.json");
				TooltipDescriptor.AddDescriptor(openLocationButton.gameObject, "<color=#FF4500>Open Folder</color>", "View directories of " + fileName, new Color32(135, 206, 255, 225));

				TooltipDescriptor.AddDescriptor(processButton.gameObject, "<color=#FF4500>Convert Package to HDF</color>", "<b>Used for Unreal Engine Assets</b>\nConverts all elements within the package [" + package.@namespace + "] to standalone HDFs", new Color32(135, 206, 255, 225));

				myPackage = package;

				initialized = true;
			}
		}
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
				fullFilePath = fullPath;
				fileAndExt = Path.GetFileName(fullPath);
				fileName = Path.GetFileNameWithoutExtension(fullPath);
				displayName.text = fileName;
				myNamespace = packageName;
				name = Path.GetFileName(fullPath) + " [LibEle]";

				playButton.transform.localScale = Vector3.one;
				processButton.transform.parent.parent.gameObject.SetActive(false);

				if (fullFilePath.Contains(".seq"))
				{
					myType = LibraryElementType.Sequence;
					myIcon.sprite = LibraryManager.Inst.seqIcon;
					visual.color = LibraryManager.Inst.seqColor;
					TooltipDescriptor.AddDescriptor(gameObject, fileName + " - Sequence", "Plays on all selected pads\nOr when the green haptic trigger touches a pad");
					TooltipDescriptor.AddDescriptor(openLocationButton.gameObject, "<color=#FF4500>Edit File</color>", "View Source of [" + fileName + "]\nWe recommend a text editor", new Color32(135, 206, 255, 225));

					TooltipDescriptor.AddDescriptor(copyButton.gameObject, "<color=#FF4500>Copy and Edit File</color>", "Creates a duplicate of - [" + fileName + "]\nThen open and edit the new file.", new Color32(135, 206, 255, 225));
				}
				else if (fullFilePath.Contains(".pat"))
				{
					myType = LibraryElementType.Pattern;
					myIcon.sprite = LibraryManager.Inst.patIcon;
					visual.color = LibraryManager.Inst.patColor;
					TooltipDescriptor.AddDescriptor(gameObject, fileName + " - Pattern", "Plays pattern which is composed of sequences on specified areas");
					TooltipDescriptor.AddDescriptor(openLocationButton.gameObject, "<color=#FF4500>Edit File</color>", "View Source of [" + fileName + "]\nWe recommend a text editor", new Color32(135, 206, 255, 225));

					TooltipDescriptor.AddDescriptor(copyButton.gameObject, "<color=#FF4500>Copy and Edit File</color>", "Creates a duplicate of - [" + fileName + "]\nThen open and edit the new file.", new Color32(135, 206, 255, 225));
				}
				else if (fullFilePath.Contains(".exp"))
				{
					myType = LibraryElementType.Experience;
					myIcon.sprite = LibraryManager.Inst.expIcon;
					visual.color = LibraryManager.Inst.expColor;
					TooltipDescriptor.AddDescriptor(gameObject, fileName + " - Experience", "Plays experience which is composed of multiple Patterns.");
					TooltipDescriptor.AddDescriptor(openLocationButton.gameObject, "<color=#FF4500>Edit File</color>", "View Source of [" + fileName + "]\nWe recommend a text editor", new Color32(135, 206, 255, 225));

					TooltipDescriptor.AddDescriptor(copyButton.gameObject, "<color=#FF4500>Copy and Edit File</color>", "Creates a duplicate of [" + fileName + "]\nThen open and edit the new file.", new Color32(135, 206, 255, 225));
				}
				else
				{
					processButton.transform.parent.parent.gameObject.SetActive(true);
				}

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

		void Start()
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
		}

		private void MarkElementBroken()
		{
			ToMarkAsBroken = false;
			Debug.LogError("This element [" + fileAndExt + "] is broken\n");
			//This doesn't prevent the action of the element, but it indicates that the element is broken.

			//Only update the color for now.
			//myIcon.sprite = LibraryManager.Inst.errorIcon;
			myIcon.sprite = LibraryManager.Inst.errorIcon;
			myIcon.color = LibraryManager.Inst.errorColor;
		}

		private void MarkElementChanged()
		{
			//Debug.Log("This element [" + fileAndExt + "] is changed\n");

			//This is disabled because the reason for a visual change isn't significant enough yet.
			//myIcon.color = LibraryManager.Inst.changedColor;
		}

		private bool FileHasBeenModified()
		{
			DateTime dt = FileModifiedHelper.GetLastModified(fullFilePath);
			//Debug.Log(dt.ToString() + "\n");

			return dt != lastModified;
		}

		private bool ValidateFile()
		{
			//Open the file

			//Check each of the validation conditions
			//Keep track of the returned reasons.
			validationFailureReasons += ValidateFileName();

			//Then return true or false.
			if (validationFailureReasons.Length > 0)
			{
				return false;
			}
			return true;
		}

		private string ValidateFileName()
		{
			//Debug.Log("[" + myType.ToString() + "]  [" + fileName + "]  [" + fileAndExt + "]  [" + myNamespace + "]\n", this);

			return string.Empty;

			//return "Failed due to comma after the last element\n";
		}

		private string EvaluateName(string packageName)
		{
			string fileName = fullFilePath;
			string[] split = fileName.Split(new char[] { '\\', '/' });
			fileName = split[split.Length - 1];
			return fileName;
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

		private delegate void HapticDefinitionCallback(HapticDefinitionFile file);
		private delegate void SuccessCallback();
		private delegate void ExceptionCallback(Exception exceptionHit);
		private delegate HapticDefinitionFile AsyncMethodCaller(string path);
		private delegate string AsyncHDFConversionCaller(AssetTool.PackageInfo info, string outDir);

		/// <summary>
		/// Retrieve a haptic definition file from a given path asynchronously
		/// </summary>
		/// <param name="path">The path to the raw asset</param>
		/// <param name="HDFSuccessCallback">The callback to be executed upon receiving the file</param>
		/// <param name="failCallback">The callback to be executed if we encounter an exception along the way - invalid files, junk content, etc</param>
		private void GetHapticDefinitionAsync(string path, HapticDefinitionCallback HDFSuccessCallback, ExceptionCallback failCallback)
		{
			AsyncMethodCaller caller = new AsyncMethodCaller(_assetTool.GetHapticDefinitionFile);

			/*IAsyncResult r = */
			caller.BeginInvoke(path, delegate (IAsyncResult iar)
			{
				AsyncResult result = (AsyncResult)iar;
				AsyncMethodCaller caller2 = (AsyncMethodCaller)result.AsyncDelegate;
				HapticDefinitionCallback hdfCallback = (HapticDefinitionCallback)iar.AsyncState;

				try
				{
					HapticDefinitionFile hdf = caller2.EndInvoke(iar);
					hdfCallback(hdf);
				}
				catch (Exception whatTheHellMicrosoft)
				{
					Debug.LogError("Async Delegate Error getting haptic definition at path [" + path + "]\n" + whatTheHellMicrosoft.Message);

					failCallback(whatTheHellMicrosoft);
				}
			}, HDFSuccessCallback);
		}

		//Make this return a bool and it'll mark the LibraryElement as broken.
		public bool ExecuteLibraryElement()
		{
			if (FileHasBeenModified())
			{
				MarkElementChanged();
			}

			//Debug.Log("File has been modified since load: [" + FileHasBeenModified() + "]\n");
			try
			{
				if (LibraryManager.Inst.LastPlayed != null && LibraryManager.Inst.StopLastPlaying)
				{
					LibraryManager.Inst.LastPlayed.Stop();
					//Todo: implement dispose again
					//	LibraryManager.Inst.LastPlayed
				}

				Action<HapticHandle> playHandleAndSetLastPlayed = delegate (HapticHandle h)
				{
					LibraryManager.Inst.LastPlayed = h;

					if (LibraryManager.Inst.LastPlayed != null)
					{
						LibraryManager.Inst.LastPlayed.Play();
					}
				};

				//Get the file path
				if (myType == LibraryElementType.Sequence)
				{
					//Debug.Log(fileAndExt + "  " + fullFilePath + "\n");
					GetHapticDefinitionAsync(fullFilePath,
						//Success Delegate
						delegate (HapticDefinitionFile hdf)
						{
							var seq = CodeHapticFactory.CreateSequence(hdf.rootEffect.name, hdf);
							//If sequence, use the specific pads selected (unsupported atm)
							AreaFlag flag = LibraryManager.Inst.GetActiveAreas();
							LibraryManager.Inst.SetTriggerSequence(seq, hdf.rootEffect.name);

							playHandleAndSetLastPlayed(seq.CreateHandle(flag));
						},
						//Failure delegate
						delegate (Exception except)
						{
							ToMarkAsBroken = true;
							//Make the file red to show it's broken.
							//Highlight the edit button.
							//Some sort of error reporting.
						});

				}
				if (myType == LibraryElementType.Pattern)
				{
					GetHapticDefinitionAsync(fullFilePath,
						//Success Delegate
						delegate (HapticDefinitionFile hdf)
						{
							var pat = CodeHapticFactory.CreatePattern(hdf.rootEffect.name, hdf);

							playHandleAndSetLastPlayed(pat.CreateHandle());
						},
						//Failure Delegate
						delegate (Exception except)
						{
							ToMarkAsBroken = true;
							//Make the file red to show it's broken.
							//Highlight the edit button.
							//Some sort of error reporting.
						});
				}
				if (myType == LibraryElementType.Experience)
				{
					GetHapticDefinitionAsync(fullFilePath,
						//Success Delegate
						delegate (HapticDefinitionFile hdf)
						{
							var exp = CodeHapticFactory.CreateExperience(hdf.rootEffect.name, hdf);

							playHandleAndSetLastPlayed(exp.CreateHandle());

						},
						//Failure delegate
						delegate (Exception except)
						{
							ToMarkAsBroken = true;
							//Make the file red to show it's broken.
							//Highlight the edit button.
							//Some sort of error reporting.
						});
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to play haptic [" + fullFilePath + "]" + "\n" + e.Message);
				return false;
			}
			return true;
		}

		private void ConvertPackageToHDF(AssetTool.PackageInfo package, SuccessCallback successCallback, ExceptionCallback failCallback)
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

		private string OutputMessage = string.Empty;
		private string PathMessage = string.Empty;
		public bool ConvertElement()
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

		public HapticHandle CreateCodeHaptic()
		{
			//Debug.Log("Hit\n");
			HapticSequence seq = new HapticSequence();
			seq.AddEffect(0.0f, new HapticEffect(Effect.Buzz, .2f));
			seq.AddEffect(0.3f, new HapticEffect(Effect.Click, 0.0f));
			//seq.Play(AreaFlag.All_Areas);

			HapticPattern pat = new HapticPattern();
			pat.AddSequence(0.5f, AreaFlag.Lower_Ab_Both, seq);
			pat.AddSequence(1.0f, AreaFlag.Mid_Ab_Both, seq);
			pat.AddSequence(1.5f, AreaFlag.Upper_Ab_Both, seq);
			pat.AddSequence(2.0f, AreaFlag.Chest_Both, seq);
			pat.AddSequence(2.5f, AreaFlag.Shoulder_Both, seq);
			pat.AddSequence(2.5f, AreaFlag.Back_Both, seq);
			pat.AddSequence(3.0f, AreaFlag.Upper_Arm_Both, seq);
			pat.AddSequence(3.5f, AreaFlag.Forearm_Both, seq);
			return pat.CreateHandle().Play();
		}

		void Update()
		{
			if (!initialized)
				Init(_assetTool, "");

			if (ToMarkAsBroken)
				MarkElementBroken();

			if (OutputMessage.Length > 0)
			{
				Debug.Log(OutputMessage + "  " + PathMessage + "\n");
				LibraryManager.Inst.ResultDisplay.Display(OutputMessage, PathMessage);
				OutputMessage = string.Empty;
				PathMessage = string.Empty;
			}

			//Ideally changed indication would be shown by an asterisk instead of color information?
			//if (FileHasBeenModified())
			//	MarkElementChanged();
		}
	}
}