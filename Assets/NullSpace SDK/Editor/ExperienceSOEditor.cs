using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Hardlight.SDK.UEditor
{
	[CustomEditor(typeof(ExperienceSO), true)]
	public class ExperienceSOEditor : HapticAssetEditor
	{
		protected override void DrawLabel()
		{
			var exp = (ExperienceSO)target;
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
			}
			if (HLEditorStyles.DrawButton("Sort", 25, 65))
			{
				EditorGUIUtility.keyboardControl = 0;
				exp.Sort();
			}
			EditorGUILayout.EndHorizontal();
			#endregion

		}

		protected override void DrawElements()
		{
			var exp = (ExperienceSO)target;
			#region Element Display
			for (int i = 0; i < exp.Patterns.Count; i++)
			{
				EditorGUILayout.BeginHorizontal("Box");
				HLEditorStyles.DrawMinLabel(i + ":", 25, 45);
				exp.Patterns[i].Time = HLEditorStyles.FloatField(exp.Patterns[i].Time, 40, 45);
				//exp.Patterns[i].Area = (AreaFlag)HLEditorStyles.DrawEnumPopup(exp.Patterns[i].Area, 40, 90);
				exp.Patterns[i].Pattern = (PatternSO)HLEditorStyles.ObjectField("", exp.Patterns[i].Pattern, typeof(PatternSO), 40, 90);
				exp.Patterns[i].Strength = HLEditorStyles.FloatField(exp.Patterns[i].Strength, 40, 60);

				if (HLEditorStyles.DrawButton("Copy", 25, 65))
				{
					exp.Patterns.Insert(i, exp.Patterns[i].Clone());
				}
				if (HLEditorStyles.DrawButton("Delete", 25, 65))
				{
					exp.Patterns.RemoveAt(i);
				}
				EditorGUILayout.EndHorizontal();
			}
			#endregion

		}

		protected override void DrawPreview()
		{
			var exp = (ExperienceSO)target;
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