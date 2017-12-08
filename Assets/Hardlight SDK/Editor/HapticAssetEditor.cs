using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Hardlight.SDK.UEditor
{
	public class HapticAssetEditor : Editor
	{
		private bool DrawDefault = false;
		private Vector2 scrollPosition;
		protected HapticHandle handle;
		protected float thingy;
		protected bool Dirty = false;

		#region Plugin Init/Dispose
		public void OnEnable()
		{
		}

		public void OnDisable()
		{
			if (handle != null)
				handle.Stop();
			//HardlightManager.Instance.Shutdown();
		}
		#endregion

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
			DrawHaltAll();

			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			
			if (Dirty)
			{
				EditorUtility.SetDirty(target);
				Dirty = false;
			}
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

		protected virtual void DrawHaltAll()
		{
			#region Preview
			if (GUILayout.Button("Stop All"))
			{
				EnsurePluginIsValid();

				try
				{
					HardlightManager.Instance.ClearAllEffects();
				}
				catch (System.Exception e)
				{
					Debug.LogError("An exception was caught while clearing all effects \n" + e.Message);
				}
			}
			#endregion
		}

		protected void EnsurePluginIsValid()
		{
			var pokeManager = HardlightManager.Instance;
		}
		
		//protected void DirtyThing()
		//{
		//	System.Func<string> f = () => { return "hello"; };
		//	string test = f();

		//	System.Func<float> field = (x) => { return EditorGUILayout.FloatField(x);};

		//	CheckDirty<float>((x) => { return EditorGUILayout.FloatField(x); }, thingy);
		//}

		//protected void CheckDirty<T>(System.Action<T> makeField, ref T oldOutput) where T : System.IComparable
		//{
		//	T newOutput = makeField(oldOutput);

		//	if (newOutput.CompareTo(oldOutput) != 0)
		//	{
		//		Dirty = true;
		//	}

		//	oldOutput = newOutput;
		//}
	}
}