using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace Hardlight.SDK.UEditor
{
	[CustomEditor(typeof(HapticPattern), true)]
	public class HapticPatternEditor : HapticAssetEditor
	{
		private bool showKeys = true;
		private bool useRanges = false;
		private List<string> Keys = new List<string>();

		private int[] widths = { 45, 35, 105, 55, 90, 80, 65, 65 };

		protected override void DrawLabel()
		{
			var pat = (HapticPattern)target;

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
					var received = (HapticSequence)HLEditorStyles.ObjectField("", pat.SequenceKeys[i], typeof(HapticSequence), 40, 90);
					if (received != pat.SequenceKeys[i])
						Dirty = true;
					pat.SequenceKeys[i] = received;
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
			HLEditorStyles.DrawMinLabel("Time", 40, widths[1] + 8);
			HLEditorStyles.DrawMinLabel("Area", 40, widths[2]);
			HLEditorStyles.DrawMinLabel("Mode", 40, widths[3]);
			HLEditorStyles.DrawMinLabel("Sequence", 40, widths[4]);
			HLEditorStyles.DrawMinLabel("Strength", 40, widths[5]);

			GUILayout.Space(2);
			if (HLEditorStyles.DrawButton("Add", 25, widths[6]))
			{
				pat.Sequences.Add(new ParameterizedSequence(null, AreaFlag.None));
				Dirty = true;
			}

			if (HLEditorStyles.DrawButton("Sort", 25, widths[7]))
			{
				EditorGUIUtility.keyboardControl = 0;
				pat.Sort();
				Dirty = true;
			}
			EditorGUILayout.EndHorizontal();
			#endregion
		}

		protected override void DrawElements()
		{
			var pat = (HapticPattern)target;

			#region Element Display
			for (int i = 0; i < pat.Sequences.Count; i++)
			{
				if (pat.Sequences[i] != null)
				{
					EditorGUILayout.BeginHorizontal("Box");
					GUILayout.Space(10);
					HLEditorStyles.DrawMinLabel(i + ":", 15, widths[0] - 12);
					var time = HLEditorStyles.FloatField(pat.Sequences[i].Time, 35, widths[1] + 8);

					if (pat.Sequences[i].Time != time)
						Dirty = true;

					pat.Sequences[i].Time = time;

					//EditorGUILayout.EndHorizontal();
					//EditorGUILayout.BeginHorizontal();
					//CreateKeyArray();
					//pat.Sequences[i].SequenceKey = EditorGUILayout.Popup(pat.Sequences[i].SequenceKey, Keys.ToArray());
					//EditorGUILayout.EndHorizontal();
					//EditorGUILayout.BeginHorizontal();
					DrawLocationInfo(pat.Sequences[i]);

					var newSeq = (HapticSequence)HLEditorStyles.ObjectField("", pat.Sequences[i].Sequence, typeof(HapticSequence), 10, widths[4]);
					if (newSeq != pat.Sequences[i].Sequence)
						Dirty = true;
					pat.Sequences[i].Sequence = newSeq;

					var str = HLEditorStyles.RangeField(pat.Sequences[i].Strength, new Vector2(0.0f, 1.0f), 20, widths[5] * 1 / 2);
					str = Mathf.Clamp(str, 0, 1.0f);
					if (str != pat.Sequences[i].Strength)
						Dirty = true;

					pat.Sequences[i].Strength = str;

					str = HLEditorStyles.FloatField(pat.Sequences[i].Strength, 20, widths[5] / 2);
					str = Mathf.Clamp(str, 0, 1.0f);
					if (str != pat.Sequences[i].Strength)
						Dirty = true;

					pat.Sequences[i].Strength = str;

					if (HLEditorStyles.DrawButton("Copy", 32, widths[6]))
					{
						Dirty = true;
						pat.Sequences.Insert(i, pat.Sequences[i].Clone());
					}
					if (HLEditorStyles.DrawButton("Delete", 42, widths[7]))
					{
						Dirty = true;
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

				var area = seq.Area = (AreaFlag)HLEditorStyles.DrawEnumPopup(seq.Area, 40, widths[2]);
				if (area != seq.Area)
					Dirty = true;
				seq.Area = area;

				GUILayout.Space(4);

				if (HLEditorStyles.DrawButton("Area", 25, widths[3]))
				{
					Dirty = true;
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
					Dirty = true;
					seq.UsingGenerator = false;
				}
			}
		}

		private void CreateKeyArray()
		{
			var pat = (HapticPattern)target;
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
			var pat = (HapticPattern)target;
			if (GUILayout.Button("Preview"))
			{
				EnsurePluginIsValid();
				pat.Play();
			}
		}
	}
}