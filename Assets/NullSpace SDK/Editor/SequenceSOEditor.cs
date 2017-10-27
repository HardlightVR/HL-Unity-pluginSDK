using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Hardlight.SDK.UEditor
{
	[CustomEditor(typeof(SequenceSO), true)]
	public class SequenceSOEditor : HapticAssetEditor
	{
		protected override void DrawLabel()
		{
			var seq = (SequenceSO)target;
			#region Label Section
			EditorGUILayout.BeginHorizontal("Box");

			HLEditorStyles.DrawMinLabel("Index", 25, 45);
			HLEditorStyles.DrawMinLabel("Time", 40, 45);
			HLEditorStyles.DrawMinLabel("Duration", 40, 60);
			HLEditorStyles.DrawMinLabel("Effect", 40, 90);
			HLEditorStyles.DrawMinLabel("Strength", 40, 60);

			if (HLEditorStyles.DrawButton("Add", 25, 65))
			{
				seq.Effects.Add(new HapticEffect(Effect.Click));
			}
			if (HLEditorStyles.DrawButton("Sort", 25, 65))
			{
				EditorGUIUtility.keyboardControl = 0;
				seq.Sort();
			}
			EditorGUILayout.EndHorizontal();
			#endregion
		}

		protected override void DrawElements()
		{
			var seq = (SequenceSO)target;
			#region Element Display
			for (int i = 0; i < seq.Effects.Count; i++)
			{
				EditorGUILayout.BeginHorizontal("Box");
				HLEditorStyles.DrawMinLabel(i + ":", 25, 45);
				seq.Effects[i].Time = HLEditorStyles.FloatField(seq.Effects[i].Time, 40, 45);
				seq.Effects[i].Duration = HLEditorStyles.FloatField((float)seq.Effects[i].Duration, 40, 60);
				seq.Effects[i].Effect = (Effect)HLEditorStyles.DrawEnumPopup(seq.Effects[i].Effect, 40, 90);
				seq.Effects[i].Strength = HLEditorStyles.FloatField(seq.Effects[i].Strength, 40, 60);

				if (HLEditorStyles.DrawButton("Copy", 25, 65))
				{
					seq.Effects.Insert(i, seq.Effects[i].Clone());
				}
				if (HLEditorStyles.DrawButton("Delete", 25, 65))
				{
					seq.Effects.RemoveAt(i);
				}
				EditorGUILayout.EndHorizontal();
			}
			#endregion
		}

		protected override void DrawPreview()
		{
			var seq = (SequenceSO)target;
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