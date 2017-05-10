using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NullSpace.SDK.Demos;
using NullSpace.SDK;

namespace NullSpace.SDK.Editor
{
	public class EmulationPane : EditorPane
	{
		public Rect windowRect = new Rect(100, 100, 200, 200);

		Vector2 scrollPos;

		#region IsValid & Setup
		public override bool IsValid()
		{
			bool isValid = base.IsValid();
			if (!Initialized || !isValid)
			{
				isValid = false;
				Initialized = false;
			}

			return isValid;
		}

		public override void Setup()
		{
			PaneTitle = "Emulation";
			ShortPaneTitle = "Emulation";

			base.Setup();
		}
		#endregion

		//Unsure if this is necessary - remove?
		void OnInspectorUpdate()
		{
			Repaint();
		}

		public override void DrawTitleContent()
		{
			base.DrawTitleContent();
		}
		public override void DrawPaneContent()
		{
			NSEditorStyles.OpenHorizontal(ColorBoxType.Warning);
			NSEditorStyles.DrawTitle("This tool is coming soon!\nIt will let you watch in real time what is going on in each pad.");
			NSEditorStyles.CloseHorizontal();
		}

		#region Editor Saving
		private void OnEnable()
		{
			//QuickButtonFoldout = ImprovedEditorPrefs.GetBool("NS-QuickButton", QuickButtonFoldout);
			//Debug.Log("Looking for NS-QuickButton - " + EditorPrefs.HasKey("NS-QuickButton") + "   \n" + EditorPrefs.GetBool("NS-QuickButton"));
		}

		private void OnDisable()
		{
			//EditorPrefs.SetBool("NS-QuickButton", QuickButtonFoldout);
		}
		#endregion
	}
}