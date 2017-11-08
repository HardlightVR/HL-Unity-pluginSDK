using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Hardlight.SDK.UEditor
{
	[CustomEditor(typeof(HapticExperience), true)]
	public class HapticExperienceEditor : HapticAssetEditor
	{
		private int[] widths = { 24, 26, 145, 40, 125, 80, 65, 65 };
		protected override void DrawLabel()
		{
			var exp = (HapticExperience)target;
			#region Label Section
			EditorGUILayout.BeginHorizontal("Box");

			HLEditorStyles.DrawMinLabel("#", 15, widths[0]);
			HLEditorStyles.DrawMinLabel("Time", 40, widths[1] + 8);
			//HLEditorStyles.DrawMinLabel("Area", 40, 90);
			HLEditorStyles.DrawMinLabel("Pattern", 40, widths[2]);
			HLEditorStyles.DrawMinLabel("Strength", 40, widths[5]);

			GUILayout.Space(4);
			if (HLEditorStyles.DrawButton("Add", 25, widths[6]))
			{
				exp.Patterns.Add(new ParameterizedPattern(null));
				Dirty = true;
			}
			if (HLEditorStyles.DrawButton("Sort", 25, widths[7]))
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
				bool showError = exp.Patterns[i].Pattern == null;
				HLEditorStyles.HighlightBox(showError, () =>
				{
					EditorGUILayout.BeginHorizontal("Box");
					HLEditorStyles.DrawMinLabel(i + ":", 20, widths[0]);

					var time = HLEditorStyles.FloatField(exp.Patterns[i].Time, 40, widths[1] + 8);
					time = Mathf.Clamp(time, 0, float.MaxValue);
					if (time != exp.Patterns[i].Time)
						Dirty = true;
					exp.Patterns[i].Time = time;

					var patt = (HapticPattern)HLEditorStyles.ObjectField("", exp.Patterns[i].Pattern, typeof(HapticPattern), 40, widths[2]);
					if (patt != exp.Patterns[i].Pattern)
						Dirty = true;
					exp.Patterns[i].Pattern = patt;

					var str = HLEditorStyles.RangeField(exp.Patterns[i].Strength, new Vector2(0.0f, 1.0f), 20, widths[5] * 1 / 2);
					str = Mathf.Clamp(str, 0, 1.0f);
					if (str != exp.Patterns[i].Strength)
						Dirty = true;

					exp.Patterns[i].Strength = str;

					str = HLEditorStyles.FloatField(exp.Patterns[i].Strength, 20, widths[5] / 2);
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
				});
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