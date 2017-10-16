using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Hardlight.SDK.Demos
{
	public class LastSequenceDisplay : MonoBehaviour
	{
		private Text MyText;

		public string TextValue
		{
			get { return LibraryManager.Inst.LastSequenceName; }
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