using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NullSpace.SDK.Editor
{
	public class EditorPane : ScriptableObject
	{
		public static HardlightEditor HLEditor
		{
			get
			{
				return HardlightEditor.myWindow;
			}
		}
		public List<HelpMessage> outputMessages;

		private bool shouldDisplay = false;
		/// <summary>
		/// Whether to display the content (not used for the tab)
		/// </summary>
		public bool ShouldDisplay
		{
			get { return shouldDisplay && Visible; }
			set { shouldDisplay = value; }
		}

		/// <summary>
		/// Whether to hide ALL elements of this pane.
		/// Used to hide the content+tab
		/// </summary>
		public bool Visible { get { return true; } internal set { } }

		public int CurrentTutorialIndex
		{
			get
			{
				return currentTutorialIndex;
			}

			set
			{
				currentTutorialIndex = value;
			}
		}

		public int LargestTutorialIndex
		{
			get
			{
				return largestTutorialIndex;
			}

			set
			{
				largestTutorialIndex = value;
			}
		}

		/// <summary>
		/// Presents a Fwd/Back button and individual tutorial elements.
		/// </summary>
		private bool TutorialModeActive;
		private int currentTutorialIndex = 0;
		private int largestTutorialIndex = -1;
		public string PaneTitle = " Suit Setup";
		public string ShortPaneTitle = "Suit Setup";
		public string WalkthroughText = "Start Tutorial";
		/// <summary>
		/// This variable is for an easy check to see if this pane has been appropriately configured.
		/// </summary>
		public bool Initialized = false;

		public virtual void Setup()
		{
			Initialized = true;
			outputMessages = new List<HelpMessage>();
		}

		/// <summary>
		/// For checking if the EditorPane has the content appropriate to display correctly.
		/// Note: If you return false, you might want to set Initialized to false. (To Re-trigger Setup())
		/// </summary>
		/// <returns>Will this pane be able to draw correctly.</returns>
		public virtual bool IsValid()
		{
			if (outputMessages == null)
			{
				Initialized = false;
				return false;
			}
			return true;
		}

		public void SetupIfInvalid()
		{
			//If we are valid and initialized
			if (IsValid() && Initialized)
			{
				//Do nothing
			}
			else
			//If we are not valid or we have not been initialized
			//This else is equivalent to [if (!IsValid() || !Initialized)] but that's less readible.
			{
				Initialized = true;
				//Debug.Log("[" + PaneTitle + "]\n\tPerforming Setup!\n");
				Setup();
			}
		}

		/// <summary>
		/// Handles the drawing of this pane
		/// 1. Sets up the pane if it is invalid - see IsValid()
		/// 2. Draws the Title content of the pane.
		/// 3. If Tutorial Active - Draws tutorial buttons
		/// 4. DrawPaneContent()
		/// </summary>
		public void DrawPane()
		{
			SetupIfInvalid();

			DrawTitleContent();
			if (TutorialModeActive)
			{
				DrawTutorialButtons();
			}

			DrawPaneContent();
		}
		public virtual void DrawTitleContent()
		{
			NSEditorStyles.OpenHorizontal();
			NSEditorStyles.DrawTitle(PaneTitle);
			if (!TutorialModeActive)
			{
				//Draw a walkthrough button
				DrawWalkthroughButton(WalkthroughText);
			}
			else
			{
				//Draw a exit tutorial button
				if (NSEditorStyles.TutorialToolbarButton(false, "Exit Tutorial"))
				{
					TutorialModeActive = false;
				}
			}
			NSEditorStyles.CloseHorizontal();
		}
		public virtual void DrawPaneContent()
		{
			IsTutorialStep(2, () =>
			{
				NSEditorStyles.DrawLabel("A tutorial's content can even jump around and be out of order with the code order!");
			});


			NSEditorStyles.DrawLabel("This is text");


			IsTutorialStep(0, () =>
			{
				NSEditorStyles.DrawLabel("Welcome to a tutorial");
			});
			IsTutorialStep(1, () =>
			{
				NSEditorStyles.DrawLabel("A tutorial can contain much content");
			});
			IsTutorialStep(2, () =>
			{
				GUILayout.Space(20);
				NSEditorStyles.DrawLabel("Divided up amongst multiple entries.");
			});


			IsTutorialStep(3, () =>
			{
				NSEditorStyles.DrawLabel("Isn't that cool!");
			});

			NSEditorStyles.DrawLabel("This is ending text");

		}
		public virtual bool DrawTabButton(bool maintainPressed)
		{
			bool result = NSEditorStyles.OperationToolbarButton(false, new GUIContent(ShortPaneTitle), maintainPressed);
			if (result)
			{
				ShouldDisplay = !ShouldDisplay;
			}

			//Debug.Log(GetType().ToString() + "   " + ShouldDisplay + "\n");
			return ShouldDisplay;
		}

		#region Tutorial Display Content
		//READ ME
		/// <summary>
		/// This is a simple delegate for easily calling content wrapped in tutorial boxes.
		/// To do this - use a line like
		/// TutorialHighlight(<other parameters>, () =>
		///		{
		///		[Delegate Code here]
		///		});
		/// </summary>

		public delegate void TutorialStepDelegate();
		/// <summary>
		/// This function ALWAYS calls the StepDelegate, but highlights the Delegate Element during tutorials
		/// </summary>
		/// <param name="StepDelegate"></param>
		/// <param name="boxType"></param>
		public void TutorialHighlight(TutorialStepDelegate StepDelegate, ColorBoxType boxType = ColorBoxType.Tutorial)
		{
			TutorialHighlight(TutorialModeActive, StepDelegate, boxType);
		}

		/// <summary>
		/// This function ALWAYS calls the StepDelegate, but highlights the Delegate Element during the indicated step.
		/// </summary>
		/// <param name="tutorialStepNumber"></param>
		/// <param name="StepDelegate"></param>
		/// <param name="boxType"></param>
		public void TutorialHighlight(int tutorialStepNumber, TutorialStepDelegate StepDelegate, ColorBoxType boxType = ColorBoxType.Tutorial)
		{
			TutorialHighlight(IsTutorialStep(tutorialStepNumber), StepDelegate, boxType);
		}

		/// <summary>
		/// This function ALWAYS calls the StepDelegate, but highlights the Delegate Element during the indicated step.
		/// Only highlights when it is BOTH that step and shouldHightlight is true.
		/// </summary>
		/// <param name="tutorialStepNumber"></param>
		/// <param name="shouldHightlight"></param>
		/// <param name="StepDelegate"></param>
		/// <param name="boxType"></param>
		public void TutorialHighlight(int tutorialStepNumber, bool shouldHightlight, TutorialStepDelegate StepDelegate, ColorBoxType boxType = ColorBoxType.Tutorial)
		{
			TutorialHighlight(IsTutorialStep(tutorialStepNumber) && shouldHightlight, StepDelegate, boxType);
		}
		/// <summary>
		/// This function ALWAYS calls the StepDelegate, but highlights the Delegate Element when told too.
		/// Will not highlight outside of tutorial mode
		/// </summary>
		/// <param name="shouldHighlight"></param>
		/// <param name="StepDelegate"></param>
		/// <param name="boxType"></param>
		public void TutorialHighlight(bool shouldHighlight, TutorialStepDelegate StepDelegate, ColorBoxType boxType = ColorBoxType.Tutorial)
		{
			if (shouldHighlight && TutorialModeActive)
			{
				//EditorGUILayout.BeginVertical("Box");
				EditorGUILayout.BeginVertical(NSEditorStyles.GetColoredHelpBoxStyle(boxType));
			}

			StepDelegate();

			if (shouldHighlight && TutorialModeActive)
			{
				//EditorGUILayout.EndVertical();
				EditorGUILayout.EndVertical();
			}

		}

		public bool IsStepWithinRange(int minAcceptable, int maxAcceptable)
		{
			if (CurrentTutorialIndex >= minAcceptable && CurrentTutorialIndex <= maxAcceptable)
			{
				return true;
			}
			return false;
		}

		public void IsTutorialStep(TutorialStepDelegate StepDelegate, bool forceTutorialBoxDraw = false)
		{
			bool ShouldDisplayTutorial = false;

			//If we're in a tutorial OR if we always want to draw the box.
			ShouldDisplayTutorial = TutorialModeActive || forceTutorialBoxDraw;

			if (ShouldDisplayTutorial)
			{
				TutorialStepBox(StepDelegate);
			}
		}

		public void IsTutorialStep(bool ShouldDisplayTutorial, TutorialStepDelegate StepDelegate)
		{
			//If we're in a tutorial OR if we always want to draw the box.
			ShouldDisplayTutorial = TutorialModeActive && ShouldDisplayTutorial;

			if (ShouldDisplayTutorial)
			{
				TutorialStepBox(StepDelegate);
			}
		}

		public void IsTutorialStep(int tutorialStepNumber, TutorialStepDelegate StepDelegate, ColorBoxType boxType = ColorBoxType.Tutorial)
		{
			IsTutorialStep(tutorialStepNumber, true, StepDelegate, boxType);
		}

		public void IsTutorialStep(int tutorialStepNumber, bool ShouldDisplayTutorial, TutorialStepDelegate StepDelegate, ColorBoxType boxType = ColorBoxType.Tutorial)
		{
			bool returnVal = false;
			if (CurrentTutorialIndex == tutorialStepNumber)
			{
				//Draw tutorial content.
				returnVal = true;
			}
			if (tutorialStepNumber > LargestTutorialIndex)
			{
				LargestTutorialIndex = tutorialStepNumber;
			}

			if (!TutorialModeActive)
				returnVal = false;

			if (returnVal && ShouldDisplayTutorial)
			{
				TutorialStepBox(StepDelegate, tutorialStepNumber, boxType);
			}
		}

		public void IsTutorialStep(int[] indices, TutorialStepDelegate StepDelegate, ColorBoxType boxType = ColorBoxType.Tutorial)
		{
			for (int i = 0; i < indices.Length; i++)
			{
				if (indices[i] == CurrentTutorialIndex)
				{
					IsTutorialStep(CurrentTutorialIndex, StepDelegate, boxType);
				}
			}
		}

		/// <summary>
		/// Draws the actual tutorial box content.
		/// Does not check if it should or shouldn't display. Call IsTutorialStep instead.
		/// </summary>
		/// <param name="StepDelegate">The content to execute when this tutorial box is displayed</param>
		/// <param name="tutorialStepNumber">-1 will not show step number information</param>
		public void TutorialStepBox(TutorialStepDelegate StepDelegate, int tutorialStepNumber = -1, ColorBoxType boxType = ColorBoxType.Tutorial)
		{
			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.BeginVertical(NSEditorStyles.GetColoredHelpBoxStyle(boxType));
			StepDelegate();
			if (tutorialStepNumber != -1)
			{
				EditorGUILayout.BeginHorizontal();
				NSEditorStyles.DrawLabel("[" + tutorialStepNumber + "]");
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();
		}

		private void DrawTutorialButtons()
		{
			//GUILayout.Space(4);
			NSEditorStyles.DrawTitle("Tutorial", 0);
			NSEditorStyles.OpenHorizontal();

			//Draw a back button
			if (NSEditorStyles.TutorialToolbarButton(CurrentTutorialIndex <= 0, "Previous"))
			{
				//Decrement the current tutorial index
				CurrentTutorialIndex--;
			}

			//Draw a restart button
			if (NSEditorStyles.TutorialToolbarButton(CurrentTutorialIndex <= 0, "Restart"))
			{
				//Set the tutorial index back to 0.
				CurrentTutorialIndex = 0;
			}

			if (CurrentTutorialIndex > LargestTutorialIndex - 1)
			{
				//Draw a finish button
				if (NSEditorStyles.TutorialToolbarButton(false, "Finish!"))
				{
					//Increment the current tutorial index
					TutorialModeActive = false;
				}
			}
			else
			{
				//Draw a next button
				if (NSEditorStyles.TutorialToolbarButton(CurrentTutorialIndex > LargestTutorialIndex - 1, "Next"))
				{
					//OutputMessage("Next button hit", MessageType.Info);
					//Increment the current tutorial index
					CurrentTutorialIndex++;
				}
			}

			NSEditorStyles.DrawLabel("Current Index: " + CurrentTutorialIndex);
			NSEditorStyles.CloseHorizontal();
			GUILayout.Space(2);
		}

		protected HelpMessage OutputMessage(string message, MessageType typeOfMessage = MessageType.None)
		{
			if (outputMessages == null)
			{
				outputMessages = new List<HelpMessage>();
			}
			HelpMessage newMessage = new HelpMessage(message, typeOfMessage);
			outputMessages.Add(newMessage);

			if (HLEditor != null)
			{
				//Tell our window we had a message (which scrolls to the bottom if it was an error)
				HLEditor.AddOutputMessage(this, newMessage);
			}

			return newMessage;
		}

		private void DrawWalkthroughButton(string text)
		{
			if (NSEditorStyles.TutorialToolbarButton(false, text))
			{
				TutorialModeActive = true;
				CurrentTutorialIndex = 0;
			}
		}

		//This wants to behave like GUILayout.BeginHorizontal kind of thing.
		//Where it opens a region that you can draw in.
		//If we're on that tutorial index, it'll display the bodied content
		private bool IsTutorialStep(int specificIndex = -1)
		{
			if (!TutorialModeActive)
				return false;
			bool returnVal = false;
			if (CurrentTutorialIndex == specificIndex)
			{
				//Draw tutorial content.
				returnVal = true;
			}

			return returnVal;
		}
		#endregion
		public virtual void Update()
		{

		}
		public void Repaint()
		{
			if (HLEditor != null)
			{
				//Debug.Log("Repaint\n");
				HLEditor.Repaint();
			}
		}
	}
}