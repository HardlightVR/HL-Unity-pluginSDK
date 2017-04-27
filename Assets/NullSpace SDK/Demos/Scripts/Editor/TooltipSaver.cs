using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using NullSpace.SDK.Demos;

public class TooltipSaver
{
	[MenuItem("Tools/Haptics Explorer/Find And Save Tooltips")]
	private static void SaveTooltips()
	{
		TooltipDescriptor[] tooltips = GameObject.FindObjectsOfType<TooltipDescriptor>();
		Debug.Log("Found: " + tooltips.Length + "\n");
		SavedTooltips saved = ScriptableObject.CreateInstance<SavedTooltips>();
		saved.Init();
		for (int i = 0; i < tooltips.Length; i++)
		{
			saved.Add(tooltips[i]);
		}

		string target = Application.streamingAssetsPath + "\\" + "Tooltips.txt";

		Debug.Log("Saving: " + saved.tooltips.Count + "\n");

		bool prettyRecords = false;

		string json = JsonUtility.ToJson(saved, prettyRecords);
		Debug.Log("JSON:" + json + "\n");
		File.WriteAllText(target, json);
	}

	[MenuItem("Tools/Haptics Explorer/Load Saved Tooltips")]
	private static void LoadTooltips()
	{
		string target = Application.streamingAssetsPath + "\\" + "Tooltips.txt";

		if (File.Exists(target))
		{
			//Read the file
			var json = File.ReadAllText(target);

			SavedTooltips saved = ScriptableObject.CreateInstance<SavedTooltips>();
			saved.Init();
			Debug.Log("Loading: " + saved.tooltips.Count + "\n");

			//Replace the playing one. (we don't want to overwrite a null object)
			JsonUtility.FromJsonOverwrite(json, saved);

			TooltipDescriptor[] allTooltips = GameObject.FindObjectsOfType<TooltipDescriptor>();

			for (int i = 0; i < saved.tooltips.Count; i++)
			{
				var picked = from tooltip in allTooltips
							 where (tooltip.name == saved.tooltips[i].objectName) && (tooltip.transform.parent != null && tooltip.transform.parent.name == saved.tooltips[i].parentName)
							 select tooltip;

				if (picked != null && picked.First())
				{
					picked.First().TooltipName = saved.tooltips[i].tooltipTitle;
					picked.First().DetailedTooltip = saved.tooltips[i].tooltipContent;
					picked.First().BackgroundColor = saved.tooltips[i].backgroundColor;
				}
			}
		}
		else
		{
			Debug.LogError("No tooltip file exists\n");
		}
	}
}
