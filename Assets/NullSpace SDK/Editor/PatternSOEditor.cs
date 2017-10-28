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
		private bool useRanges = false;
		private List<string> Keys = new List<string>();

		private int[] widths = { 45, 45, 105, 55, 90, 80, 65, 65 };

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

			HLEditorStyles.DrawMinLabel("Index", 25, widths[0]);
			HLEditorStyles.DrawMinLabel("Time", 40, widths[1]);
			HLEditorStyles.DrawMinLabel("Area", 40, widths[2]);
			HLEditorStyles.DrawMinLabel("Mode", 40, widths[3]);
			HLEditorStyles.DrawMinLabel("Sequence", 40, widths[4]);
			HLEditorStyles.DrawMinLabel("Strength", 40, widths[5]);

			GUILayout.Space(2);
			if (HLEditorStyles.DrawButton("Add", 25, widths[6]))
			{
				pat.Sequences.Add(new ParameterizedSequence(null, AreaFlag.None));
			}

			if (HLEditorStyles.DrawButton("Sort", 25, widths[7]))
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
				if (pat.Sequences[i] != null)
				{
					EditorGUILayout.BeginHorizontal("Box");
					GUILayout.Space(10);
					HLEditorStyles.DrawMinLabel(i + ":", 15, widths[0] - 20);
					pat.Sequences[i].Time = HLEditorStyles.FloatField(pat.Sequences[i].Time, 35, widths[1] + 10);
					//pat.Sequences[i].Area = (AreaFlag)HLEditorStyles.DrawEnumPopup(pat.Sequences[i].Area, 40, widths[2]);

					//EditorGUILayout.EndHorizontal();
					//EditorGUILayout.BeginHorizontal();
					//CreateKeyArray();
					//pat.Sequences[i].SequenceKey = EditorGUILayout.Popup(pat.Sequences[i].SequenceKey, Keys.ToArray());
					//EditorGUILayout.EndHorizontal();
					//EditorGUILayout.BeginHorizontal();
					DrawLocationInfo(pat.Sequences[i]);

					pat.Sequences[i].Sequence = (SequenceSO)HLEditorStyles.ObjectField("", pat.Sequences[i].Sequence, typeof(SequenceSO), 10, widths[4]);
					pat.Sequences[i].Strength = HLEditorStyles.RangeField(pat.Sequences[i].Strength, new Vector2(0.0f, 1.0f), 20, widths[5] * 1 / 2);
					pat.Sequences[i].Strength = HLEditorStyles.FloatField(pat.Sequences[i].Strength, 20, widths[5] / 2);

					if (HLEditorStyles.DrawButton("Copy", 32, widths[6]))
					{
						pat.Sequences.Insert(i, pat.Sequences[i].Clone());
					}
					if (HLEditorStyles.DrawButton("Delete", 42, widths[7]))
					{
						pat.Sequences.RemoveAt(i);
					}
					EditorGUILayout.EndHorizontal();
					//GUILayout.Space(18);
				}
			}
			#endregion
		}

		private void DrawLocationInfo(ParameterizedSequence seq)
		{
			if (!seq.UsingGenerator)
			{
				//if (HLEditorStyles.DrawButton("Use Area", 25, widths[2] / 2))
				//{
				//	seq.info = new AreaFlagLocation();
				//}
				//if (HLEditorStyles.DrawButton("Use Rng", 25, widths[2] / 2))
				//{
				//	seq.info = new GeneratorLocation();
				//}
				//}
				//else if (seq.info.GetType() == typeof(AreaFlagLocation))
				//{
				//var areaLocation = (AreaFlagLocation)seq.info;
				seq.Area = (AreaFlag)HLEditorStyles.DrawEnumPopup(seq.Area, 40, widths[2]);

				GUILayout.Space(4);

				if (HLEditorStyles.DrawButton("Area", 25, widths[3]))
				{
					seq.UsingGenerator = true;
				}
			}
			else/* if (seq.info.GetType() == typeof(GeneratorLocation))*/
			{
				HLEditorStyles.DrawMinLabel("derp", 40, widths[2]);
				//var areaLocation = (AreaFlagLocation)info;
				//areaLocation.Area = (AreaFlag)HLEditorStyles.DrawEnumPopup(areaLocation.Area, 40, 90);

				if (HLEditorStyles.DrawButton("Rng", 25, widths[3]))
				{
					seq.UsingGenerator = false;
				}
			}
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