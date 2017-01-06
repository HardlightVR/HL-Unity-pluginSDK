using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using NullSpace.SDK;

namespace NullSpace.SDK.Demos
{
	public class LibraryElement : MonoBehaviour
	{
		string fullFilePath = "";

		//For the visual representation of the haptic effect.
		public enum LibraryElementType { Sequence, Pattern, Experience, Folder }
		public LibraryElementType myType = LibraryElementType.Sequence;

		public Image visual;
		public Image myIcon;
		public Text displayName;
		public Button myButton;
		public string myNamespace;
		public string fileAndExt;
		public string fileName;

		public void Init(string fullPath, string packageName = "")
		{
			fullFilePath = fullPath;
			fileAndExt = LibraryManager.CleanPathToFile(fullFilePath);
			string[] fileParts = fileAndExt.Split('.');
			fileName = fileParts[0];
			displayName.text = fileParts[0];
			myNamespace = packageName;

			if (fullFilePath.Contains(".seq"))
			{
				myType = LibraryElementType.Sequence;
				myIcon.sprite = LibraryManager.Inst.seqIcon;
				visual.color = LibraryManager.Inst.seqColor;
			}
			else if (fullFilePath.Contains(".pat"))
			{
				myType = LibraryElementType.Pattern;
				myIcon.sprite = LibraryManager.Inst.patIcon;
				visual.color = LibraryManager.Inst.patColor;
			}
			else if (fullFilePath.Contains(".exp"))
			{
				myType = LibraryElementType.Experience;
				myIcon.sprite = LibraryManager.Inst.expIcon;
				visual.color = LibraryManager.Inst.expColor;
			}
			else
			{
				myType = LibraryElementType.Folder;
				myIcon.sprite = LibraryManager.Inst.folderIcon;
				visual.color = LibraryManager.Inst.folderColor;
			}

		}

		void Start()
		{
			myButton.onClick.AddListener(() => { PlayHaptic(); });
		}

		private string EvaluateName(string packageName)
		{
			string fileName = fullFilePath;
			string[] split = fileName.Split(new char[] { '\\', '/' });
			fileName = split[split.Length - 1];
			return fileName;
		}

		public void PlayHaptic()
		{
			//Get the file path
			//Debug.Log("[" + myNamespace + "] [" + fileName + "]\n" + myNamespace + "" + fileName);
			if (myType == LibraryElementType.Sequence)
			{
				//If sequence, use the specific pads selected (unsupported atm)
				new Sequence(myNamespace + fileName).CreateHandle(AreaFlag.All_Areas).Play().Dispose();
			}
			if (myType == LibraryElementType.Pattern)
			{
				new Pattern(myNamespace + fileName).CreateHandle().Play().Dispose();
			}
			if (myType == LibraryElementType.Experience)
			{
				new Experience(myNamespace + fileName).CreateHandle().Play().Dispose();
			}
		}

		void Update()
		{

		}
	}
}