﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace NullSpace.SDK.Editor
{
	public class HardlightEditor : EditorWindow
	{
		//Uses a Tab/EditorPane class.
		//Has a number. Presents them all as tabs.
		//Keeps a main scroll region over them all.
		//Has a Welcome/tutorial EditorPane

		public static HardlightEditor myWindow;
		public bool DebugHardlightEditor = false;
		bool compactMode = false;

		public List<EditorPane> HardlightPanes;
		public EditorPane ActiveTab;
		bool initialized = false;

		public Vector2 errorScrollPos = Vector2.zero;

		public Texture2D icon;
		public Material mat;
		private bool tutorial;
		public bool Tutorial
		{
			get { return tutorial; }
			set { tutorial = value; }
		}

		// Add menu item to show this demo.
		[MenuItem("Tools/Hardlight Window")]
		public static void OpenWindow()
		{
			myWindow = GetWindow(typeof(HardlightEditor)) as HardlightEditor;
			myWindow.Init();
			myWindow.titleContent = new GUIContent("Hardlight Ed");
			myWindow.name = "Hardlight Ed";
		}

		public void Init()
		{
			if (!initialized)
			{
				HardlightPanes = new List<EditorPane>();
				//Note: SuitSetupPane (inherited from EditorPane) is a ScriptableObject.
				SuitSetupPane setup = CreateInstance<SuitSetupPane>();
				setup.Setup();
				setup.ShouldDisplay = false;
				PackagingPane package = CreateInstance<PackagingPane>();
				package.Setup();
				package.ShouldDisplay = true;
				EmulationPane emulation = CreateInstance<EmulationPane>();
				emulation.Setup();
				emulation.ShouldDisplay = false;

				ActiveTab = package;

				HardlightPanes.Add(setup);
				HardlightPanes.Add(package);
				HardlightPanes.Add(emulation);
				//HardlightPanes.Add(new AssetImporterPane());
				//HardlightPanes.Add(new EmulationPane());

				icon = (Texture2D)Resources.Load("Button Icons/NullSpace Logo 256x256", typeof(Texture2D));
				mat = Resources.Load<Material>("EditorIcon");

				initialized = true;
			}
		}

		void CheckIfInvalidSetup()
		{
			if (HardlightPanes == null || HardlightPanes.Count == 0)
			{
				Init();
			}
		}

		void Update()
		{
			for (int i = 0; i < HardlightPanes.Count; i++)
			{
				HardlightPanes[i].Update();
			}
		}

		void OnGUI()
		{
			//Initialize if we aren't initialized
			CheckIfInvalidSetup();

			NSEditorStyles.DrawBackgroundImage(icon, mat, this);

			DrawHardlightEditor();
		}

		void DrawHardlightEditor()
		{
			DrawTitleSection();

			if (Tutorial)
			{
				DrawTutorial();
			}
			else
			{
				DrawPaneTabs();

				DrawVisiblePanes();
			}
			DrawOutputMessages();
		}

		void DrawTitleSection()
		{
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.BeginVertical();
			NSEditorStyles.DrawTitle(new GUIContent(" Hardlight Editor"));
			EditorGUILayout.EndVertical();
			if (NSEditorStyles.DrawButton(NSEditorStyles.CompactMode ? "+" : "-"))
			{
				NSEditorStyles.CompactMode = !NSEditorStyles.CompactMode;
			}
			EditorGUILayout.EndHorizontal();

			//NSEditorStyles.DrawLabel("Holy Crap Dividers!");
			//NSEditorStyles.DrawDivider(1);
			//NSEditorStyles.DrawLabel("In different styles!");
			//NSEditorStyles.DrawDivider(3);
			//NSEditorStyles.DrawLabel("Sizes!");
			//NSEditorStyles.DrawDivider(NSEditorStyles.ColorBoxType.Error, 8);
			//NSEditorStyles.DrawLabel("And colors!");
			//NSEditorStyles.DrawDivider(NSEditorStyles.ColorBoxType.Black, 4);
			//NSEditorStyles.DrawDivider(NSEditorStyles.ColorBoxType.Tutorial, 8);
		}

		void DrawPaneTabs()
		{
			if (DebugHardlightEditor)
			{
				NSEditorStyles.DrawLabel("Active Tab:" + (ActiveTab == null ? "Null Active Tab" : ActiveTab.GetType().ToString()));
			}

			//Draw horizontal row(s) of tab buttons.
			GUILayout.BeginHorizontal();
			if (HardlightPanes != null && HardlightPanes.Count > 0)
			{
				for (int i = 0; i < HardlightPanes.Count; i++)
				{
					if (HardlightPanes[i] != null)
					{
						//if (HardlightPanes[i].Visible)
						//{
						bool Result = HardlightPanes[i].DrawTabButton(HardlightPanes[i].ShouldDisplay);

						//Controls showing only one at a time.
						if (HardlightPanes[i] != ActiveTab && Result)
						{
							if (ActiveTab != null)
								ActiveTab.ShouldDisplay = false;
							ActiveTab = HardlightPanes[i];
						}
						//}
					}
					//NSEditorStyles.OperationToolbarButton(false, new GUIContent("ShortPaneTitle"));
					//NSEditorStyles.OperationToolbarButton(false, new GUIContent("ShortPaneTitle"));

				}
			}
			GUILayout.EndHorizontal();
		}

		void DrawVisiblePanes()
		{
			EditorGUILayout.BeginScrollView(Vector2.zero);
			{
				if (HardlightPanes != null && HardlightPanes.Count > 0)
				{
					//NSEditorStyles.DrawLabel("We have " + HardlightPanes.Count);
					//Draw each of the active panes
					for (int i = 0; i < HardlightPanes.Count; i++)
					{
						//NSEditorStyles.DrawLabel("Inside For Loop");
						if (HardlightPanes[i] != null)
						{
							//NSEditorStyles.DrawLabel("i Not Null");
							if (HardlightPanes[i].ShouldDisplay)
							{
								//NSEditorStyles.DrawLabel("Attempt Draw Pane ");
								HardlightPanes[i].DrawPane();
								NSEditorStyles.DrawDivider();
							}
						}
						//else
						//{
						//	NSEditorStyles.DrawLabel("HardlightPanes[i] is null");

						//}
					}
				}
			}
			EditorGUILayout.EndScrollView();
		}

		void DrawOutputMessages()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Output Messages & Errors");

			List<HelpMessage> outputMessages = new List<HelpMessage>();

			if (ActiveTab != null)
			{
				//for (int i = 0; i < HardlightPanes.Count; i++)
				//{
				if (ActiveTab.outputMessages != null)
				{
					outputMessages.AddRange(ActiveTab.outputMessages);
				}
				//}
			}

			bool disabled = true;
			if (outputMessages != null && outputMessages.Count > 0)
			{
				disabled = false;
			}
			using (new EditorGUI.DisabledGroupScope(disabled))
			{
				if (GUILayout.Button("Clear Messages", EditorStyles.toolbarButton))
				{
					ActiveTab.outputMessages.Clear();
					outputMessages.Clear();
				}
			}

			GUILayout.EndHorizontal();

			if (outputMessages != null && outputMessages.Count > 0)
			{
				//GUIStyle messageBox = new GUIStyle("Box");
				//messageBox.fixedHeight = 200;

				int boxHeight = Mathf.Clamp(outputMessages.Count * 30, 140, 300);
				GUILayout.BeginVertical("Box", GUILayout.MinHeight(boxHeight));
				errorScrollPos = EditorGUILayout.BeginScrollView(errorScrollPos);
				for (int i = outputMessages.Count - 1; i >= 0; i--)
				{
					EditorGUILayout.HelpBox(outputMessages[i].message, outputMessages[i].messageType);
				}
				EditorGUILayout.EndScrollView();

				//float space = Mathf.Clamp(300 - outputMessages.Count * 30, 0, 300);
				//GUILayout.Space(space);
				EditorGUILayout.EndVertical();
			}
		}
		public void DrawEndingContent()
		{
			EditorGUILayout.HelpBox("Output Info\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n", MessageType.None, true);
		}

		void DrawTutorial()
		{

		}

		public void AddOutputMessage(EditorPane caller, HelpMessage message)
		{
			if (ActiveTab != null && ActiveTab == caller)
			{
				//if (message.messageType == MessageType.Error)
				//{
					errorScrollPos = new Vector2(float.MaxValue, float.MaxValue);
				//}
			}
		}
	}
}