using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NullSpace.SDK.Demos
{
	public class SliderValueAccess : MonoBehaviour
	{
		private Text MyText;

		public enum ForceType { Integer, TwoDecimals, Effect, String };
		public ForceType DisplayType = ForceType.Integer;
		public bool DisplayStringOnZero = false;
		public string ReplaceZeroWith = "Natural Duration";

		private float textValue;
		public float TextValue
		{
			get { return textValue; }
			set
			{
				textValue = value;
				if (DisplayType == ForceType.Integer)
				{
					MyText.text = TextValue.ToString();
				}
				else if (DisplayType == ForceType.TwoDecimals)
				{
					float val = ((float)((int)(TextValue * 100)) / 100);
					MyText.text = val == 0 && DisplayStringOnZero ? ReplaceZeroWith: val.ToString();
				}
				else if (DisplayType == ForceType.Effect)
				{
					//Prevent array index out of bounds errors.
					int index = Mathf.Clamp((Mathf.RoundToInt(textValue)), 0, SuitImpulseDemo.effectOptions.Length - 1);
					MyText.text = SuitImpulseDemo.effectOptions[index].ToString();
				}
				else if (DisplayType == ForceType.String)
				{
					//Prevent array index out of bounds errors.
					int index = Mathf.Clamp((Mathf.RoundToInt(textValue)), 0, SuitImpulseDemo.SampleHapticSequence.Length - 1);
					MyText.text = SuitImpulseDemo.SampleHapticSequence[index].ToString();
				}
			}
		}

		private void Start()
		{
			MyText = GetComponent<Text>();
		}
	}
}