using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NullSpace.SDK.Demos
{
	public class LastSequenceDisplay : MonoBehaviour
	{
		private Text MyText;

		private float textValue;
		public float TextValue
		{
			get { return textValue; }
			set
			{
				MyText.text = LibraryManager.Inst.LastSequenceName;
			}
		}

		private void Start()
		{
			MyText = GetComponent<Text>();
		}

		private void Update()
		{
			MyText.text = LibraryManager.Inst.LastSequenceName;
		}
	}
}