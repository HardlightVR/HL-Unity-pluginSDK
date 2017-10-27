using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Hardlight.SDK.UEditor
{
	public class HapticAssetEditor : Editor
	{
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