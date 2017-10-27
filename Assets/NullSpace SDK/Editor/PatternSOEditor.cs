using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Hardlight.SDK.UEditor
{
	[CustomEditor(typeof(PatternSO), true)]
	public class PatternSOEditor : HapticAssetEditor
	{
		protected override void DrawLabel()
		{
			var pat = (PatternSO)target;
			#region Label Section
			EditorGUILayout.BeginHorizontal("Box");

			HLEditorStyles.DrawMinLabel("Index", 25, 45);
			HLEditorStyles.DrawMinLabel("Time", 40, 45);
			HLEditorStyles.DrawMinLabel("Area", 40, 90);
			HLEditorStyles.DrawMinLabel("Sequence", 40, 90);
			HLEditorStyles.DrawMinLabel("Strength", 40, 60);

			if (HLEditorStyles.DrawButton("Add", 25, 65))
			{
				pat.Sequences.Add(new ParameterizedSequence(null, AreaFlag.None));
			}
			if (HLEditorStyles.DrawButton("Sort", 25, 65))
			{
				EditorGUIUtility.keyboardControl = 0;
				pat.Sort();
			}
			EditorGUILayout.EndHorizontal();
			#endregion
		}

		protected override void DrawElements()
		{
			var pat = (PatternSO)target;
			#region Element Display
			for (int i = 0; i < pat.Sequences.Count; i++)
			{
				EditorGUILayout.BeginHorizontal("Box");
				HLEditorStyles.DrawMinLabel(i + ":", 25, 45);
				pat.Sequences[i].Time = HLEditorStyles.FloatField(pat.Sequences[i].Time, 40, 45);
				pat.Sequences[i].Area = (AreaFlag)HLEditorStyles.DrawEnumPopup(pat.Sequences[i].Area, 40, 90);
				pat.Sequences[i].Sequence = (SequenceSO)HLEditorStyles.ObjectField("", pat.Sequences[i].Sequence, typeof(SequenceSO), 40, 90);
				pat.Sequences[i].Strength = HLEditorStyles.FloatField(pat.Sequences[i].Strength, 40, 60);

				if (HLEditorStyles.DrawButton("Copy", 25, 65))
				{
					pat.Sequences.Insert(i, pat.Sequences[i].Clone());
				}
				if (HLEditorStyles.DrawButton("Delete", 25, 65))
				{
					pat.Sequences.RemoveAt(i);
				}
				EditorGUILayout.EndHorizontal();
			}
			#endregion
		}

		protected override void DrawPreview()
		{
			var pat = (PatternSO)target;
			if (GUILayout.Button("Preview"))
			{
				EnsurePluginIsValid();
				pat.Play();
			}
		}
	}
}