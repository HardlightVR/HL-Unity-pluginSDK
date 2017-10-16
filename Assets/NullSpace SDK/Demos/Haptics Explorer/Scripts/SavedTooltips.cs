using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Hardlight.SDK.Demos;

[System.Serializable]
public class SavedTooltips : ScriptableObject
{
	public void Init()
	{
		tooltips = new List<SavedTooltip>();
		Debug.Log("Initialized Saved Tooltips Object\n");
	}
	public void Add(TooltipDescriptor tooltip)
	{
		SavedTooltip tt = new SavedTooltip();
		tt.objectName = tooltip.name;
		tt.tooltipTitle = tooltip.TooltipName;
		tt.tooltipContent = tooltip.DetailedTooltip;
		tt.backgroundColor = tooltip.BackgroundColor;
		if (tooltip.transform.parent != null)
		{
			tt.parentName = tooltip.transform.parent.name;
		}

		Debug.Log("Adding: " + tt.objectName + " - " + tt.tooltipTitle + " \n");

		tooltips.Add(tt);
	}
	public List<SavedTooltip> tooltips;

	[System.Serializable]
	public class SavedTooltip
	{
		[SerializeField]
		public string objectName;
		[SerializeField]
		public string tooltipTitle;
		[SerializeField]
		public string tooltipContent;
		[SerializeField]
		public string parentName;
		[SerializeField]
		public Color backgroundColor;
	}
}

