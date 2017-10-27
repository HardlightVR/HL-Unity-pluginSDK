using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace Hardlight.SDK.UEditor
{
	[CustomEditor(typeof(PatternSO), true)]
	public class PatternSOEditor : HapticAssetEditor
	{
		private bool showKeys = true;
		private List<string> Keys = new List<string>();

		protected override void DrawLabel()
		{
			var pat = (PatternSO)target;

			#region Sequence Key Elements
			bool change = HLEditorStyles.DrawButton(!showKeys ? "Show Sequence Keys" : "Hide Sequence Keys");
			if (change)
				showKeys = !showKeys;

			if (showKeys)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical("Box");

				if (HLEditorStyles.DrawButton("Add", 25, 65))
				{
					pat.SequenceKeys.Add(null);
				}

				EditorGUILayout.BeginHorizontal();
				for (int i = 0; i < pat.SequenceKeys.Count; i++)
				{
					HLEditorStyles.DrawMinLabel("Key [" + i + "]", 25, 55);
					pat.SequenceKeys[i] = (SequenceSO)HLEditorStyles.ObjectField("", pat.SequenceKeys[i], typeof(SequenceSO), 40, 90);
					GUILayout.Space(18);

					if (i > 0 && (i + 1) % 3 == 0)
					{
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(24);
			}
			#endregion

			#region Label Section
			EditorGUILayout.BeginHorizontal("Box");

			HLEditorStyles.DrawMinLabel("Index", 25, 45);
			HLEditorStyles.DrawMinLabel("Time", 40, 45);
			HLEditorStyles.DrawMinLabel("Area", 40, 90);
			HLEditorStyles.DrawMinLabel("Sequence", 40, 90);
			HLEditorStyles.DrawMinLabel("Strength", 40, 60);

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

				//EditorGUILayout.EndHorizontal();
				//EditorGUILayout.BeginHorizontal();
				//CreateKeyArray();
				//pat.Sequences[i].SequenceKey = EditorGUILayout.Popup(pat.Sequences[i].SequenceKey, Keys.ToArray());
				//EditorGUILayout.EndHorizontal();
				//EditorGUILayout.BeginHorizontal();

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
				GUILayout.Space(18);
			}
			#endregion
		}

		private void CreateKeyArray()
		{
			var pat = (PatternSO)target;
			Keys.Clear();
			for (int i = 0; i < pat.Sequences.Count; i++)
			{
				if (pat.SequenceKeys[i] != null)
				{
					Keys.Add(pat.SequenceKeys[i].name);
				}
			}
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