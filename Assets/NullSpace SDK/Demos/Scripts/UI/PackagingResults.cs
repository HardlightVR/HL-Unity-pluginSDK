using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using IOHelper;

namespace NullSpace.SDK.Demos
{
	/// <summary>
	/// This class is a short term solution for displaying the text results of a HDF Packaging operation.
	/// It also displays a button to open the converted file.
	/// </summary>
	public class PackagingResults : MonoBehaviour
	{
		private RectTransform rect;
		private GameObject primaryChild;
		public Button OpenConverted;
		public Text ResultText;
		bool toEnable = false;
		bool toDisable = false;
		void Start()
		{
			if (rect == null)
				rect = GetComponent<RectTransform>();
			primaryChild = transform.GetChild(0).gameObject;
		}

		void Update()
		{
			if (toEnable)
			{
				primaryChild.SetActive(true);
				toEnable = false;
			}
			if (toDisable)
			{
				primaryChild.SetActive(false);
				toDisable = false;
			}
		}

		public void Display(string results, string convertedPath)
		{
			SetVisibility(true);
			ResultText.text = "<b>[The Operation Completed]</b>\n" + results + "\n" + "<i>(Better error reporting will come later)</i>";
			OpenConverted.onClick.RemoveAllListeners();
			OpenConverted.onClick.AddListener(() =>
			{
				try
				{
					OpenPathHelper.Open(convertedPath);
				}
				catch (System.Exception e)
				{
					Debug.LogError("Unable to open path " + convertedPath + "\n\t" + e.Message);
				}
				SetVisibility(false);
			});
		}

		public void SetVisibility(bool target)
		{
			if (target)
			{
				toEnable = true;
			}
			else
			{
				toDisable = true;
			}
		}
	}
}