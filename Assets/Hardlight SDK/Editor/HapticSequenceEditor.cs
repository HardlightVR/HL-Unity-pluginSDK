using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Hardlight.SDK.UEditor
{
	[CustomEditor(typeof(HapticSequence), true)]
	public class HapticSequenceEditor : HapticAssetEditor
	{
		private int[] widths = { 24, 26, 60, 80, 125, 80, 65, 65 };
		protected override void DrawLabel()
		{
			var seq = (HapticSequence)target;
			#region Label Section
			EditorGUILayout.BeginHorizontal("Box");

			HLEditorStyles.DrawMinLabel("#", 15, widths[0]);
			HLEditorStyles.DrawMinLabel("Time", 40, widths[1] + 8);
			HLEditorStyles.DrawMinLabel("Duration", 40, widths[2]);
			HLEditorStyles.DrawMinLabel("Effect", 40, widths[3]);
			HLEditorStyles.DrawMinLabel("Strength", 40, widths[5]);

			GUILayout.Space(4);
			if (HLEditorStyles.DrawButton("Add", 25, widths[6]))
			{
				seq.Effects.Add(new HapticEffect(Effect.Click));
				Dirty = true;
			}
			if (HLEditorStyles.DrawButton("Sort", 25, widths[7]))
			{
				EditorGUIUtility.keyboardControl = 0;
				seq.Sort();
				Dirty = true;
			}
			EditorGUILayout.EndHorizontal();
			#endregion
		}

		protected override void DrawElements()
		{
			var seq = (HapticSequence)target;
			#region Element Display
			for (int i = 0; i < seq.Effects.Count; i++)
			{
				EditorGUILayout.BeginHorizontal("Box");
				HLEditorStyles.DrawMinLabel(i + ":", 20, widths[0]);

				var time = HLEditorStyles.FloatField(seq.Effects[i].Time, 40, widths[1] + 8);
				time = Mathf.Clamp(time, 0, float.MaxValue);
				if (time != seq.Effects[i].Time)
					Dirty = true;
				seq.Effects[i].Time = time;

				var dur = HLEditorStyles.FloatField((float)seq.Effects[i].Duration, 40, widths[2]);
				dur = Mathf.Clamp(dur, 0, float.MaxValue);
				if (dur != seq.Effects[i].Duration)
					Dirty = true;
				seq.Effects[i].Duration = dur;

				var eff = (Effect)HLEditorStyles.DrawEnumPopup(seq.Effects[i].Effect, 40, widths[3]);
				if (eff != seq.Effects[i].Effect)
					Dirty = true;
				seq.Effects[i].Effect = eff;

				var str = HLEditorStyles.RangeField(seq.Effects[i].Strength, new Vector2(0.0f, 1.0f), 20, widths[5] * 1 / 2);
				str = Mathf.Clamp(str, 0, 1.0f);
				if (str != seq.Effects[i].Strength)
					Dirty = true;

				seq.Effects[i].Strength = str;

				str = HLEditorStyles.FloatField(seq.Effects[i].Strength, 20, widths[5] / 2);
				str = Mathf.Clamp(str, 0, 1.0f);
				if (str != seq.Effects[i].Strength)
					Dirty = true;

				seq.Effects[i].Strength = str;

				if (HLEditorStyles.DrawButton("Copy", 25, 65))
				{
					seq.Effects.Insert(i, seq.Effects[i].Clone());
					Dirty = true;
				}
				if (HLEditorStyles.DrawButton("Delete", 25, 65))
				{
					seq.Effects.RemoveAt(i);
					Dirty = true;
				}
				EditorGUILayout.EndHorizontal();
			}
			#endregion
		}

		protected override void DrawPreview()
		{
			var seq = (HapticSequence)target;
			#region Preview
			if (GUILayout.Button("Preview"))
			{
				EnsurePluginIsValid();
				seq.Play(AreaFlag.All_Areas);
			}
			#endregion
		}
	}
}