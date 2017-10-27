using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Hardlight.SDK.UEditor
{
	public class HapticAssetEditor : Editor
	{
		private bool DrawDefault = false;
		private Vector2 scrollPosition;
		#region Plugin Init/Dispose
		public void OnEnable()
		{
			EnsurePluginIsValid();
		}

		public void OnDisable()
		{
			HardlightManager.Instance.Shutdown();
		}

		public override void OnInspectorGUI()
		{
			bool change = HLEditorStyles.DrawButton(!DrawDefault ? "Draw Default Inspector" : "Dont Draw Default Inspector");
			if (change)
				DrawDefault = !DrawDefault;

			if (DrawDefault)
			{
				DrawDefaultInspector();
			}
			//scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, true, false);
			EditorGUILayout.BeginHorizontal("Box");
			EditorGUILayout.BeginVertical();

			DrawLabel();

			DrawElements();

			EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);

			DrawPreview();

			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			//if (GUILayout.Button("Save"))
			//{
			//	Undo.RecordObject(target, "Save");
			//	AssetDatabase.SaveAssets();
			//	AssetDatabase.Refresh();
			//	Debug.Log("TEST14\n");
			//}

			//EditorUtility.SetDirty(target);
			//EditorGUILayout.EndScrollView();
		}

		protected virtual void DrawLabel()
		{

		}
		protected virtual void DrawElements()
		{

		}
		protected virtual void DrawPreview()
		{

		}

		protected void EnsurePluginIsValid()
		{
			HardlightManager.Instance.InitPluginIfNull();
		}
		#endregion

	}
}