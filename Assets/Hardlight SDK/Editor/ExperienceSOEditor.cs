using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Hardlight.SDK.UEditor
{
	[CustomEditor(typeof(HapticExperience), true)]
	public class HapticExperienceEditor : HapticAssetEditor
	{
		protected override void DrawLabel()
		{
			var exp = (HapticExperience)target;
			#region Label Section
			EditorGUILayout.BeginHorizontal("Box");

			HLEditorStyles.DrawMinLabel("Index", 25, 45);
			HLEditorStyles.DrawMinLabel("Time", 40, 45);
			//HLEditorStyles.DrawMinLabel("Area", 40, 90);
			HLEditorStyles.DrawMinLabel("Pattern", 40, 90);
			HLEditorStyles.DrawMinLabel("Strength", 40, 60);

			if (HLEditorStyles.DrawButton("Add", 25, 65))
			{
				exp.Patterns.Add(new ParameterizedPattern(null));
				Dirty = true;
			}
			if (HLEditorStyles.DrawButton("Sort", 25, 65))
			{
				EditorGUIUtility.keyboardControl = 0;
				Dirty = true;
				exp.Sort();
			}
			EditorGUILayout.EndHorizontal();
			#endregion
		}

		protected override void DrawElements()
		{
			var exp = (HapticExperience)target;
			#region Element Display
			for (int i = 0; i < exp.Patterns.Count; i++)
			{
				EditorGUILayout.BeginHorizontal("Box");
				HLEditorStyles.DrawMinLabel(i + ":", 25, 45);

				var time = HLEditorStyles.FloatField(exp.Patterns[i].Time, 40, 45);
				time = Mathf.Clamp(time, 0, float.MaxValue);
				if (time != exp.Patterns[i].Time)
					Dirty = true;
				exp.Patterns[i].Time = time;

				var patt = (HapticPattern)HLEditorStyles.ObjectField("", exp.Patterns[i].Pattern, typeof(HapticPattern), 40, 90);
				if (patt != exp.Patterns[i].Pattern)
					Dirty = true;
				exp.Patterns[i].Pattern = patt;

				var str = HLEditorStyles.FloatField(exp.Patterns[i].Strength, 40, 60);
				str = Mathf.Clamp(str, 0, 1.0f);
				if (str != exp.Patterns[i].Strength)
					Dirty = true;
				exp.Patterns[i].Strength = str;

				if (HLEditorStyles.DrawButton("Copy", 25, 65))
				{
					exp.Patterns.Insert(i, exp.Patterns[i].Clone());
					Dirty = true;
				}
				if (HLEditorStyles.DrawButton("Delete", 25, 65))
				{
					exp.Patterns.RemoveAt(i);
					Dirty = true;
				}
				EditorGUILayout.EndHorizontal();
			}
			#endregion
		}

		protected override void DrawPreview()
		{
			var exp = (HapticExperience)target;
			#region Preview
			if (GUILayout.Button("Preview"))
			{
				EnsurePluginIsValid();
				exp.Play();
			}
			#endregion
		}
	}
}