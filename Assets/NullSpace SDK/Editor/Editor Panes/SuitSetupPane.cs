using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace NullSpace.SDK.Editor
{
	public class SuitSetupPane : EditorPane
	{
		public Rect windowRect = new Rect(100, 100, 200, 200);

		bool QuickButtonFoldout = true;
		Vector2 scrollPos;

		Texture2D icon;
		Material mat;

		[SerializeField]
		public List<EditorSuitConfig> Suits;

		[SerializeField]
		public class EditorSuitConfig
		{
			//This holds all the scene references to objects.
			public SuitDefinition MyDefinition;
			public EditorPane myPane;

			public bool CanChangeValues = true;
			public List<Object> ObjectsToDestroy;
			public Vector2 errorScrollPos;
			public bool TopFoldout = true;
			bool ObjectFieldFoldout = true;
			string childAppendName = "'s Suit Body Collider";

			private int ComponentsToDestroy;
			private int GameObjectsToDestroy;
			public EditorSuitConfig()
			{
				CheckSuitDefinition();

				ObjectsToDestroy = new List<Object>();
			}

			public void OnGUI()
			{
				CheckSuitDefinition();

				//Local scrollview
				//scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

				//Opening Greetings
				myPane.IsTutorialStep(0, () =>
				{
					NSEditorStyles.DrawLabel("Welcome to the Suit Setup Tool-torial!\nThis is a tool for setting up a suit on a collection of existing scene objects.\nIt comes with an existing prefab to give you the general gist.\nYou don't need to use this tool at all, it is intended to be a quick way to configure different objects to the body for area/positional haptics.", 105, 14);
				});

				//Closing Words Tutorial Content
				myPane.IsTutorialStep(11, () =>
				{
					NSEditorStyles.DrawLabel("Closing Words\n" +
						"This tool will attempt to auto-find any existing suit definitions. Suit Definitions should be prefab friendly (assuming you aren't referencing outside of the created prefab.\n\n" +
						"This tool supports undoing most operations [Ctrl+Z].\nUndo functionality has passed most testing but might not undo all operations.\n\n"

						, 105, 14);
				});


				GUILayoutOption[] options = new GUILayoutOption[0];
				string suitDisplayName = MyDefinition.SuitName.Length > 0 ? MyDefinition.SuitName : MyDefinition.SuitRoot == null ? "Unnamed Suit" : MyDefinition.SuitRoot.name;
				myPane.TutorialHighlight(!TopFoldout && myPane.IsStepWithinRange(1, 8), () =>
				{
					TopFoldout = NSEditorStyles.DrawGUILayoutToggle(TopFoldout, "Show Suit: " + suitDisplayName);
				});
				#region Top Foldout
				if (TopFoldout)
				{
					//NOTE TO SELF: Add tooltips
					//new GUIContent("Test Float", "Here is a tooltip")

					EditorGUILayout.BeginVertical("Box");
					SuitNaming(options);

					//ObjectFieldFoldout = NSEditorStyles.DrawFoldout(ObjectFieldFoldout, "Show Suit Configuration");
					myPane.TutorialHighlight(!ObjectFieldFoldout && myPane.IsStepWithinRange(2, 6), () =>
					{
						ObjectFieldFoldout = NSEditorStyles.DrawGUILayoutToggle(ObjectFieldFoldout, "Show Suit Configuration");
					});

					if (ObjectFieldFoldout)
					{
						DrawOptions(options);
						GUILayout.Space(12);

						DrawAssignmentAndDisplay();
					}
					else
					{
						myPane.IsTutorialStep(myPane.IsStepWithinRange(2, 6), () =>
						{
							NSEditorStyles.DrawLabel("Expand this foldout to continue!", 105, 14);
						});
					}

					DrawProcessControls();


				}
				else
				{
					myPane.IsTutorialStep(myPane.IsStepWithinRange(1, 8), () =>
					{
						NSEditorStyles.DrawLabel("Expand this foldout to continue!", 105, 14);
					});
				}
				#endregion

				//EditorGUILayout.EndScrollView();
			}

			private void CheckSuitDefinition()
			{
				if (MyDefinition == null)
				{
					MyDefinition = ScriptableObject.CreateInstance<SuitDefinition>();
					MyDefinition.Init();
				}
			}

			#region Options
			void DrawOptions(GUILayoutOption[] options)
			{
				GUIContent content = new GUIContent();
				bool hasDefinition = false;

				EditorGUILayout.BeginVertical("Box");
				#region Suit Auto-Configuration
				//EditorGUILayout.LabelField("Suit Options");
				myPane.TutorialHighlight(3, () =>
				{
					SuitRootObjectField(options);

					content = new GUIContent("Autoconfigure based on Root Object", "Uses common names to try and establish the different suit body colliders for the suit.");

					bool Result = NSEditorStyles.OperationButton(MyDefinition.SuitRoot == null, content);
					if (Result)
					{
						HardlightSuit definition = MyDefinition.SuitRoot.GetComponent<HardlightSuit>();

						if (definition != null)
						{
							PopulateFromSuitDefinition(definition);
						}
						else
						{
							//	//Check if they have anything assigned.
							//	//If they do, warn them this will overwrite it.
							AutoFindElementsFromRoot(MyDefinition.SuitRoot);
						}
						hasDefinition = (definition != null);
					}
				});

				myPane.IsTutorialStep(3, () =>
				{
					NSEditorStyles.DrawLabel("Once you have a root object, you can tell it to draw from a predefined SuitDefinition.\n\n" + (hasDefinition ?
						"\tIt looks like you have an existing definition. This will populate from the SuitDefinition" :
						"\tIt looks like you don't have an existing SuitDefinition")

						, 105, 14);
				});
				#endregion

				//content = new GUIContent("Can Change Values", "Can't be adjusted if you have a current layout. Remove HardlightCollider to adjust config.");
				//CanChangeValues = CreateStyledToggle(false, CanChangeValues, content, innerOptions);

				GUILayoutOption[] twoColumns = NSEditorStyles.NColumnsLayoutOptions();

				EditorGUILayout.BeginHorizontal();
				content = new GUIContent("Add Child Objects", "Create child objects instead of directly adding to the targeted object.");

				myPane.TutorialHighlight(4, () =>
				{
					//The toggle for whether or not to add fresh child objects.
					MyDefinition.AddChildObjects = NSEditorStyles.DrawGUILayoutToggle(false, MyDefinition.AddChildObjects, content, twoColumns);
				});
				//MyDefinition.AddChildObjects = CreateStyledToggle(!CanChangeValues, MyDefinition.AddChildObjects, content, twoColumns);

				content = new GUIContent("Add New Colliders", "Adds SuitBodyCollider objects instead of using existing ones or letting you configure it manually.\nWill add the colliders to child objects if that is also selected.");

				myPane.TutorialHighlight(5, () =>
				{
					//The toggle for whether or not to add trigger colliders.
					MyDefinition.AddExclusiveTriggerCollider = NSEditorStyles.DrawGUILayoutToggle(false, MyDefinition.AddExclusiveTriggerCollider, content, twoColumns);
				});

				EditorGUILayout.EndHorizontal();

				//This tutorial content is outside of the Horizontal region (for presentation reasons)
				myPane.IsTutorialStep(4, () =>
				{
					NSEditorStyles.DrawLabel("This option controls whether new child objects will be created to store the added components that define which part of the suit this relates too.\n" + ""

						, 105, 14);
				});

				myPane.IsTutorialStep(5, () =>
				{
					NSEditorStyles.DrawLabel("Do you want to add trigger colliders to the different suit objects?\nThe added colliders will be box colliders (which is accurate enough for most action gameplay)\nDisable this if you want to detect collisions elsewhere or use a more specific collider shape.\n" + ""

						, 105, 14);
				});
				EditorGUILayout.EndVertical();
			}

			void SuitNaming(GUILayoutOption[] options)
			{
				myPane.TutorialHighlight(1, () =>
				{
					MyDefinition.SuitName = NSEditorStyles.TextField("Suit Name", MyDefinition.SuitName, options);
				});

				myPane.IsTutorialStep(1, () =>
				{
					NSEditorStyles.DrawLabel("Let's start by giving a name to your Suit.", 105, 14);
				});
			}
			void SuitRootObjectField(GUILayoutOption[] options)
			{
				GUIContent content = new GUIContent("Suit Root", "For modifying an existing configuration. In the future this will try to find the related objects based on common naming conventions.");

				myPane.TutorialHighlight(2, () =>
				{
					if (MyDefinition.SuitRoot != null)
						Undo.RecordObject(MyDefinition.SuitRoot, "Suit Root Selected");
					MyDefinition.SuitRoot = EditorGUILayout.ObjectField(content, MyDefinition.SuitRoot, typeof(GameObject), true, options) as GameObject;
				});

				myPane.IsTutorialStep(2, () =>
				{
					NSEditorStyles.DrawLabel("This field is for an in-scene object.\nYou can create a SuitDefinition with this tool and it will attach a component to that object.", 105, 14);
				});

				//Disallow Prefabs
				if (MyDefinition.SuitRoot != null && PrefabUtility.GetPrefabType(MyDefinition.SuitRoot) == PrefabType.Prefab)
				{
					MyDefinition.SuitRoot = null;
				}
			}
			#endregion

			#region Core Columns
			void DrawAssignmentAndDisplay()
			{
				GUILayoutOption[] threeColumns = NSEditorStyles.NColumnsLayoutOptions(3);
				GUIStyle columnStyle = NSEditorStyles.GetColumnStyle();

				for (int i = 0; i < MyDefinition.DefinedAreas.Count; i++)
				{
					//New Horizontal
					EditorGUILayout.BeginHorizontal("Box");

					EditorGUILayout.BeginHorizontal(columnStyle);
					//Create the default option display
					DisplayAreaFlag(i, threeColumns);
					EditorGUILayout.EndHorizontal();

					myPane.TutorialHighlight(6, () =>
					{
						EditorGUILayout.BeginHorizontal(columnStyle);
						//The object field for selecting the relevant object
						CreateSuitObjectField(i, threeColumns);
						EditorGUILayout.EndHorizontal();
					});

					EditorGUILayout.BeginHorizontal(columnStyle);
					//If we have a suitbodycollider for this index
					SuitQuickButton(i, threeColumns);
					EditorGUILayout.EndHorizontal();

					//End Horizontal
					EditorGUILayout.EndHorizontal();
				}

				myPane.IsTutorialStep(6, () =>
				{
					NSEditorStyles.DrawLabel("Drag or select Scene Objects that are part of your suit into each of the corresponding locations."
						, 105, 14);
				});
			}

			void DisplayAreaFlag(int index, GUILayoutOption[] options)
			{
				//GUILayout.Label("Area Flag Display");
				if (MyDefinition.DefinedAreas != null && MyDefinition.DefinedAreas.Count > index)
				{
					AreaFlag o = MyDefinition.DefinedAreas[index];
					string label = o.ToString();

					//Make the label but remove the underscores
					GUILayout.Label(label.Replace('_', ' '), options);
				}
			}

			void CreateSuitObjectField(int index, GUILayoutOption[] options)
			{
				//GUILayout.Label("Suit Object Field " + SuitHolders.Count);
				if (MyDefinition.ZoneHolders != null && MyDefinition.ZoneHolders.Count > index)
				{
					GameObject o = MyDefinition.ZoneHolders[index];
					//GUIContent label = new GUIContent("Intended Parent");
					MyDefinition.ZoneHolders[index] = EditorGUILayout.ObjectField(o, typeof(GameObject), true, options) as GameObject;

					//Disallow Prefabs
					if (MyDefinition.ZoneHolders[index] != null && PrefabUtility.GetPrefabType(MyDefinition.ZoneHolders[index]) == PrefabType.Prefab)
					{
						MyDefinition.ZoneHolders[index] = null;
					}

					//If the value changes
					if (MyDefinition.ZoneHolders[index] != null && o != MyDefinition.ZoneHolders[index])
					{
						HardlightCollider suit = LookupSceneReference(index);
						AssignQuickButton(suit, index);
					}
				}
			}

			//When we change the field. Lookup that object and assign the quickbutton if it can
			HardlightCollider LookupSceneReference(int index)
			{
				GameObject targObj = MyDefinition.ZoneHolders[index];
				if (MyDefinition.ZoneHolders[index] != null)
				{
					string lookupName = MyDefinition.ZoneHolders[index].name + childAppendName;
					Transform objToCheck = MyDefinition.AddChildObjects ? targObj.transform.FindChild(lookupName) : targObj.transform;

					if (objToCheck != null)
					{
						return objToCheck.gameObject.GetComponent<HardlightCollider>();
					}
				}
				return null;
			}
			bool AssignQuickButton(HardlightCollider suit, int index)
			{
				if (suit != null && MyDefinition.SceneReferences[index] == null)
				{
					MyDefinition.SceneReferences[index] = suit;
					return true;
				}
				return false;
			}

			void SuitQuickButton(int index, GUILayoutOption[] options)
			{
				if (MyDefinition.SceneReferences != null && MyDefinition.SceneReferences.Count > index)
				{
					bool invalidObj = MyDefinition.SceneReferences[index] == null;
					string buttonLabel = string.Empty;

					if (invalidObj)
					{
						//Disabling
						EditorGUI.BeginDisabledGroup(invalidObj);
						buttonLabel = "Unassigned";
					}
					else
					{
						buttonLabel = MyDefinition.SceneReferences[index].name;

						//Max out the string size?
					}

					//Color this button differently if the AreaFlag doesn't match?
					if (GUILayout.Button(buttonLabel, EditorStyles.toolbarButton, options))
					{
						//Go to that object.
						Selection.activeGameObject = MyDefinition.SceneReferences[index].gameObject;
					}

					if (invalidObj)
					{
						//Re-enable
						EditorGUI.EndDisabledGroup();
					}
				}
			}
			#endregion

			#region End Process Controls
			void DrawProcessControls()
			{
				GUILayout.BeginHorizontal();

				SuitSetupOperation();

				SuitRemovalOperation();

				GUILayout.EndHorizontal();

				//Suit Setup Operation Tutorial Content
				myPane.IsTutorialStep(7, () =>
				{
					NSEditorStyles.DrawLabel("IMPORTANT: This box will create HardlightCollider components.\nIt will add them to the provided objects (or create children and add the components to the children)\nThis operation will not add excessive components (instead adding flags to the components.\n\nThis operation will also add a SuitDefinition to the provided root object, which is useful for modifying the configuration later.", 105, 14);
				}, ColorBoxType.Warning);
				if ((MyDefinition.SuitRoot == null))
				{
					myPane.IsTutorialStep(7, () =>
					{
						NSEditorStyles.DrawLabel("You have not set a Suit Root. Last chance..."
							, 105, 14);
					}, ColorBoxType.Error);
				}
				//Suit Removal Operation Tutorial Content
				myPane.IsTutorialStep(8, () =>
				{
					NSEditorStyles.DrawLabel("This will remove all of the SuitDefinitions from the root & node objects.\nIt will delete the information defined in this EditorWindow.\nThis means you could change whether " + ""

						, 105, 14);
				});


				EditorGUILayout.EndVertical();
			}
			string SuitSetupOperation()
			{
				string output = string.Empty;
				string tooltip = "This will create Suit components on" + (MyDefinition.AddChildObjects ? " children of the selected objects" : " the selected objects");
				GUIContent content = new GUIContent("Create HardlightCollider", tooltip);
				bool OperationForbidden = CountValidSuitHolders() < 1 && MyDefinition.HasRoot;
				bool Result = false;

				myPane.TutorialHighlight(7, () =>
				{
					Result = NSEditorStyles.OperationButton(OperationForbidden, content);

				});

				if (Result)
				{
					//If we should add child objects
					if (MyDefinition.AddChildObjects)
						output += AddChildNodesForSuit();
					//Then create the component (it will handle the Collider functionalities)
					output += AddComponentForSuit();
				}


				return output;
			}

			void SuitRemovalOperation()
			{
				string output = string.Empty;
				bool disabled = true;
				if (CountHardlightCollider() > 0)
				{
					disabled = false;
				}

				using (new EditorGUI.DisabledGroupScope(disabled))
				{
					bool beginOperation = false;
					myPane.TutorialHighlight(8, () =>
					{
						beginOperation = GUILayout.Button("Remove HardlightCollider");
					});

					if (beginOperation)
					{
						output += DetectComponentsToRemove();
						Debug.Log(output + "\n");
						if (ObjectsToDestroy.Count > 0)
						{
							string dialogText = GameObjectsToDestroy + " game objects marked to be destroyed\n" + ComponentsToDestroy + " components marked to be removed.";
							bool userResult = EditorUtility.DisplayDialog("Delete Component Objects", dialogText, "Remove", "Cancel");
							if (userResult)
							{
								DeleteMarkedObjects();
								CanChangeValues = true;

								//If they choose to delete their current suit
								//Do they also want to remove the root's definition
								//This will handle if it doesn't have a root.
								HandleHardlightRemovalChoice();
							}
						}
					}
				}
			}

			int CountValidSuitHolders()
			{
				int validCount = 0;
				if (MyDefinition.ZoneHolders != null && MyDefinition.ZoneHolders.Count == MyDefinition.DefinedAreas.Count)
				{
					for (int i = 0; i < MyDefinition.ZoneHolders.Count; i++)
					{
						if (MyDefinition.ZoneHolders[i] != null)
						{
							validCount++;
						}
					}
				}

				return validCount;
			}
			int CountHardlightCollider()
			{
				int validCount = 0;
				if (MyDefinition.SceneReferences != null && MyDefinition.SceneReferences.Count == MyDefinition.DefinedAreas.Count)
				{
					for (int i = 0; i < MyDefinition.SceneReferences.Count; i++)
					{
						if (MyDefinition.SceneReferences[i] != null)
						{
							validCount++;
						}
					}
				}

				return validCount;
			}


			#endregion

			#region Processing Functions
			void AutoFindElementsFromRoot(GameObject Root)
			{
				List<HardlightCollider> suitObjects = Root.GetComponentsInChildren<HardlightCollider>().ToList();

				if (suitObjects.Count > 0)
				{
					//for (int i = 0; i < suitObjects.Count; i++)
					//{
					//if (suitObjects[i].regionID.HasFlag(DefaultOptions[i]))
					//{
					//	AssignQuickButton(suitObjects[i], i);
					//}
					//}

					for (int optionIndex = 0; optionIndex < MyDefinition.DefinedAreas.Count; optionIndex++)
					{
						for (int suitIndex = 0; suitIndex < suitObjects.Count; suitIndex++)
						{
							if (suitObjects[suitIndex].regionID.HasFlag(MyDefinition.DefinedAreas[optionIndex]))
							{
								//Set the object field
								if (MyDefinition.AddChildObjects && suitObjects[suitIndex].transform.parent != null)
								{
									MyDefinition.ZoneHolders[optionIndex] = suitObjects[suitIndex].transform.parent.gameObject;
								}
								if (!MyDefinition.AddChildObjects)
								{
									MyDefinition.ZoneHolders[optionIndex] = suitObjects[suitIndex].gameObject;
								}

								//Assign the quick button.
								AssignQuickButton(suitObjects[suitIndex], optionIndex);
							}
						}

					}

					//Index out of bounds - gotta search another way
					//for (int i = 0; i < DefaultOptions.Count; i++)
					//{
					//	if (suitObjects[i].regionID.HasFlag(DefaultOptions[i]))
					//	{
					//		AssignQuickButton(suitObjects[i], i);
					//	}
					//}
				}
			}

			public void PopulateFromSuitDefinition(HardlightSuit hardlightSuit)
			{
				if (hardlightSuit != null)
				{
					//Assign the visible suit root.
					MyDefinition.SuitRoot = hardlightSuit.gameObject;

					//ERROR: This isn't an actual valid check yet.
					if (hardlightSuit.Definition.CountValidZoneHolders() <= 0)
					{
						hardlightSuit.CheckListValidity();

						MyDefinition.ZoneHolders = hardlightSuit.ZoneHolders.ToList();
						MyDefinition.SceneReferences = hardlightSuit.SceneReferences.ToList();
						MyDefinition.HapticsLayer = hardlightSuit.HapticsLayer;
						MyDefinition.AddChildObjects = hardlightSuit.AddChildObjects;
						MyDefinition.AddExclusiveTriggerCollider = hardlightSuit.AddExclusiveTriggerCollider;
					}
					else
					{
						//This field is populated when the Definition is created.
						//MyDefinition.DefinedAreas = hardlightSuit.Definition.DefinedAreas.ToList();
						MyDefinition.ZoneHolders = hardlightSuit.Definition.ZoneHolders.ToList();
						MyDefinition.SceneReferences = hardlightSuit.Definition.SceneReferences.ToList();
						MyDefinition.HapticsLayer = hardlightSuit.Definition.HapticsLayer;
						//We can't change stuff, we imported it
						CanChangeValues = false;
						Debug.Log("Disallow modification\n");
						MyDefinition.AddChildObjects = hardlightSuit.Definition.AddChildObjects;
						MyDefinition.AddExclusiveTriggerCollider = hardlightSuit.Definition.AddExclusiveTriggerCollider;
					}
				}
			}

			//This function is for AUTOMATICALLY configuring a body object based on common names
			//It is not done
			void ProcessSuitRoot()
			{

			}

			string ClearHardlightCollider()
			{
				Debug.LogError("This is not used\nYou probably shouldn't use this - it was a temporary call and was refactored into RemoveComponentsForSuit\n");
				string output = string.Empty;
				for (int i = 0; i < MyDefinition.SceneReferences.Count; i++)
				{
					if (MyDefinition.SceneReferences[i] != null)
					{
						output += "Removing Suit Body Collider from " + MyDefinition.SceneReferences[i].name + " which had RegionID: " + MyDefinition.DefinedAreas[i].ToString();
					}
				}

				//Yep, we're going through it twice that way we can have a good output info.
				for (int i = 0; i < MyDefinition.SceneReferences.Count; i++)
				{
					//If something is on multiple objects, it'll get cleaned up only once.
					if (MyDefinition.SceneReferences[i] != null)
					{
						DestroyImmediate(MyDefinition.SceneReferences[i]);
					}
				}

				output = "Remove SuitBodyCollider scripts - Operation Finished\n\n" + output + "\n";
				return output;
			}

			#region Adding
			string AddChildNodesForSuit()
			{
				string output = string.Empty;
				for (int i = 0; i < MyDefinition.ZoneHolders.Count; i++)
				{
					if (MyDefinition.ZoneHolders[i] != null)
					{
						GameObject go = new GameObject();
						Undo.RegisterCreatedObjectUndo(go, "Add Suit Child Node");

						//Undo.SetTransformParent(myGameObject.transform, newTransformParent);
						Undo.SetTransformParent(go.transform, MyDefinition.ZoneHolders[i].transform, "Set New Suit Node as Child");

						//We don't use this anymore, instead we register the transform change with Undo
						//go.transform.SetParent(MyDefinition.SuitHolders[i].transform);

						go.name = MyDefinition.ZoneHolders[i].name + childAppendName;
					}
				}
				return output;
			}

			string AddComponentForSuit()
			{
				string output = string.Empty;

				HardlightSuit hardlightSuit = null;
				if (MyDefinition.SuitRoot != null)
				{
					Undo.RecordObject(MyDefinition.SuitRoot, "Configuring Suit Definition");
					hardlightSuit = MyDefinition.SuitRoot.GetComponent<HardlightSuit>();
					if (hardlightSuit == null)
					{
						hardlightSuit = Undo.AddComponent<HardlightSuit>(MyDefinition.SuitRoot);
						//SuitRoot.AddComponent<DefinedSuit>();
					}
				}

				if (hardlightSuit != null)
				{
					Undo.RecordObject(hardlightSuit, "Filling out Defined Suit fields");
					hardlightSuit.Definition.AddChildObjects = MyDefinition.AddChildObjects;
					hardlightSuit.Definition.AddExclusiveTriggerCollider = MyDefinition.AddExclusiveTriggerCollider;
					hardlightSuit.Definition.ZoneHolders = MyDefinition.ZoneHolders.ToList();
					hardlightSuit.Definition.DefinedAreas = MyDefinition.DefinedAreas.ToList();
					hardlightSuit.Definition.SuitRoot = MyDefinition.SuitRoot;

					hardlightSuit.AddChildObjects = MyDefinition.AddChildObjects;
					hardlightSuit.AddExclusiveTriggerCollider = MyDefinition.AddExclusiveTriggerCollider;
					hardlightSuit.ZoneHolders = MyDefinition.ZoneHolders.ToList();
					hardlightSuit.DefinedAreas = MyDefinition.DefinedAreas.ToList();
					hardlightSuit.SuitRoot = MyDefinition.SuitRoot;
				}

				#region Handle the zone holders
				for (int i = 0; i < MyDefinition.ZoneHolders.Count; i++)
				{
					if (MyDefinition.ZoneHolders[i] != null)
					{
						Undo.RecordObject(MyDefinition.ZoneHolders[i], "Add Suit Node to Marked Objects");

						output += "Processing " + MyDefinition.ZoneHolders[i].name + "";
						GameObject targetGO = MyDefinition.AddChildObjects ? MyDefinition.ZoneHolders[i].transform.FindChild(MyDefinition.ZoneHolders[i].name + childAppendName).gameObject : MyDefinition.ZoneHolders[i];

						Collider col = null;
						//Check if it has one already
						HardlightCollider suit = targetGO.GetComponent<HardlightCollider>();
						//Debug.Log("Checking: " + targetGO + "\n" + "  " + (suit == null), targetGO);
						#region If there exists a Hardlight Collider component.
						if (suit == null)
						{
							//Add one if it doesn't
							//suit = targetGO.AddComponent<SuitBodyCollider>(); - Not undo-able

							suit = Undo.AddComponent<HardlightCollider>(targetGO);
							Undo.RecordObject(suit.gameObject, "Modifying Suit Object");

							if (MyDefinition.AddExclusiveTriggerCollider)
							{
								col = AddColliderForSuit(suit);
							}
							else
							{
								col = FindColliderOnSuit(suit);
							}

							//Debug.Log((hardlightSuit == null) + "\n");
							//if (hardlightSuit != null)
							//{
							//	Undo.RecordObject(hardlightSuit, "Filling out Defined Suit fields");
							//	Debug.Log("Checking: " + suit.name + "\n");

							//	if (suit != null)
							//	{
							//		hardlightSuit.Definition.SceneReferences[i] = suit;
							//	}
							//	//hardlightSuit.SceneReferences[i] = suit;
							//	//Transplant actions.
							//}

							output += "\t  Adding Suit Body Collider to " + MyDefinition.ZoneHolders[i].name + "";
						}
						#endregion
						else
						{
							Debug.LogError("Unimplemented\n");
						}

						output += "\t  Adding " + MyDefinition.DefinedAreas[i].ToString() + " " + MyDefinition.ZoneHolders[i].name + "\n";

						//Add this region to it.
						suit.regionID = suit.regionID | MyDefinition.DefinedAreas[i];

						//Save the collider if we made one
						if (col != null)
						{ suit.myCollider = col; }

						MyDefinition.SceneReferences[i] = suit;

						//Don't let the user change anything until they've deleted these?
						//These functions aren't robust enough yet.
						Debug.Log("Disallow modification\n");
						CanChangeValues = false;
					}
				}
				#endregion

				CopyToHardlightSuit(hardlightSuit);

				output = "Creating SuitBodyCollider - Operation Finished\n\n" + output + "\n";
				return output;
			}

			void CopyToHardlightSuit(HardlightSuit hardlightSuit)
			{
				if (hardlightSuit == null)
				{
					Debug.Log("HardlightSuit is null\n");
				}
				if (hardlightSuit != null)
				{
					hardlightSuit.AddChildObjects = MyDefinition.AddChildObjects;
					hardlightSuit.AddExclusiveTriggerCollider = MyDefinition.AddExclusiveTriggerCollider;
					hardlightSuit.ZoneHolders = MyDefinition.ZoneHolders.ToList();
					hardlightSuit.DefinedAreas = MyDefinition.DefinedAreas.ToList();
					hardlightSuit.SceneReferences = MyDefinition.SceneReferences.ToList();
					hardlightSuit.SuitRoot = MyDefinition.SuitRoot;
				}
			}

			Collider FindColliderOnSuit(HardlightCollider suit)
			{
				GameObject go = suit.gameObject;

				Collider col = go.GetComponent<Collider>();
				return col;
			}

			Collider AddColliderForSuit(HardlightCollider suit)
			{
				GameObject go = suit.gameObject;

				Collider col = Undo.AddComponent<BoxCollider>(go);
				//Collider col = go.AddComponent<BoxCollider>();
				col.gameObject.layer = MyDefinition.HapticsLayer;
				col.isTrigger = true;
				return col;
			}
			#endregion

			#region Removal
			string DetectComponentsToRemove()
			{
				string output = string.Empty;

				//We want to clear out the list
				ObjectsToDestroy.Clear();

				ComponentsToDestroy = 0;
				GameObjectsToDestroy = 0;

				if (MyDefinition.SceneReferences != null)
				{
					for (int i = 0; i < MyDefinition.SceneReferences.Count; i++)
					{
						if (MyDefinition.SceneReferences[i] != null)
						{
							//If add child objects
							if (MyDefinition.AddChildObjects)
							{
								int componentCount = MyDefinition.SceneReferences[i].gameObject.GetComponents<Component>().Length;

								//Debug.Log("Component Count is equal to " + componentCount + "\n");

								if (componentCount < 4)
								{
									ComponentsToDestroy++;
									if (MyDefinition.AddExclusiveTriggerCollider)
										ComponentsToDestroy++;
									GameObjectsToDestroy++;
									//Mark for deletion - they'll have to confirm removal.
									ObjectsToDestroy.Add(MyDefinition.SceneReferences[i].gameObject);
									output += "Marking : " + MyDefinition.SceneReferences[i].gameObject + "'s SuitBodyCollider Component for removal - has " + componentCount + " components\n";
								}
							}
							//Attached to the parrent
							else
							{
								if (MyDefinition.ZoneHolders[i] != null)
								{
									HardlightCollider suit = MyDefinition.ZoneHolders[i].GetComponent<HardlightCollider>();

									if (suit != null)
									{
										ComponentsToDestroy++;
										ObjectsToDestroy.Add(suit);
										output += "Marking : " + MyDefinition.ZoneHolders[i].name + "'s SuitBodyCollider Component for removal\n";
									}

									if (MyDefinition.AddExclusiveTriggerCollider && suit.myCollider != null)
									{
										ComponentsToDestroy++;
										ObjectsToDestroy.Add(suit.myCollider);
										output += "Marking : " + MyDefinition.ZoneHolders[i] + "'s " + suit.myCollider.GetType().ToString() + " Component for removal\n";
									}
								}
							}
						}
					}
				}

				output += "Operation Finished - " + ObjectsToDestroy.Count + " objects marked to destory\n";

				return output;
			}
			void DeleteMarkedObjects()
			{
				for (int i = ObjectsToDestroy.Count - 1; i > -1; i--)
				{
					if (ObjectsToDestroy[i] != null)
					{
						Undo.DestroyObjectImmediate(ObjectsToDestroy[i]);
					}
				}
				ObjectsToDestroy.Clear();
			}

			void HandleHardlightRemovalChoice()
			{
				HardlightSuit suit = GetRootHardlightSuitComponent();
				if (suit)
				{
					bool userResult = EditorUtility.DisplayDialog("Remove HardlightSuit Component", "Delete Hardlight Suit Definition\n(Stores a SuitDefinition for fast reimplementation)", "Don't Remove", "Remove");
					if (!userResult)
					{
						DeleteHardlightSuitComponent(suit);
					}
				}
			}
			HardlightSuit GetRootHardlightSuitComponent()
			{
				if (MyDefinition.SuitRoot != null)
				{
					HardlightSuit suit = MyDefinition.SuitRoot.GetComponent<HardlightSuit>();
					if (suit)
					{
						return suit;
					}
				}
				return null;
			}
			void DeleteHardlightSuitComponent(HardlightSuit suit)
			{
				Undo.DestroyObjectImmediate(suit);
			}
			#endregion
			#endregion
		}

		#region IsValid & Setup
		public override bool IsValid()
		{
			bool isValid = base.IsValid();
			if (Suits == null || !isValid)
			{
				isValid = true;
				Initialized = false;
			}

			return isValid;
		}

		public override void Setup()
		{
			PaneTitle = "Suit Setup";
			ShortPaneTitle = "Suit Setup";
			Suits = new List<EditorSuitConfig>();

			icon = (Texture2D)Resources.Load("Button Icons/NullSpace Logo 256x256", typeof(Texture2D));
			mat = Resources.Load<Material>("EditorIcon");

			List<HardlightSuit> existingDefinitions = FindObjectsOfType<HardlightSuit>().ToList();

			//Debug.Log("Found: " + existingDefinitions.Count + "\n");

			EditorSuitConfig suit = null;

			for (int i = 0; i < existingDefinitions.Count; i++)
			{
				suit = AddSuitConfiguration();
				suit.PopulateFromSuitDefinition(existingDefinitions[i]);
			}

			if (Suits.Count == 0)
			{
				suit = AddSuitConfiguration();
			}

			base.Setup();
		}
		#endregion

		EditorSuitConfig AddSuitConfiguration()
		{
			EditorSuitConfig suit = new EditorSuitConfig();
			Suits.Add(suit);
			suit.myPane = this;
			return suit;
		}

		void DrawQuickButtonsForHardlightColliders()
		{
			//GUILayoutOption[] options = new GUILayoutOption[0];
			GUILayoutOption[] threeColumns = NSEditorStyles.NColumnsLayoutOptions(3);
			GUIContent content = new GUIContent(string.Empty);
			//Toggle to show a list of all the suits
			List<HardlightCollider> suitObjects = new List<HardlightCollider>();

			suitObjects = FindObjectsOfType<HardlightCollider>().ToList();

			if (suitObjects.Count > 0)
			{
				if (QuickButtonFoldout)
				{
					EditorGUILayout.BeginVertical("box");
				}
				string showQuickButtonName = QuickButtonFoldout ? "Hide" : "Show";

				TutorialHighlight(!QuickButtonFoldout && IsStepWithinRange(9, 9), () =>
				{
					QuickButtonFoldout = NSEditorStyles.DrawGUILayoutToggle(QuickButtonFoldout, showQuickButtonName + " Existing Hardlight Colliders");
				});

				if (QuickButtonFoldout)
				{
					TutorialHighlight(9, () =>
					{
						bool horizOpen = false;
						for (int i = 0; i < suitObjects.Count; i++)
						{
							//Selectively opening horizontals enforces 3 columns and makes multiple rows.
							if (i % 3 == 0)
							{
								if (horizOpen) EditorGUILayout.EndHorizontal();
								EditorGUILayout.BeginHorizontal();
								horizOpen = true;
							}
							if (suitObjects[i] != null)
							{
								content = new GUIContent(suitObjects[i].name, "Quick Navigate to " + suitObjects[i].name);
								//Create a select button
								NSEditorStyles.QuickSelectButton(false, suitObjects[i].gameObject, content, threeColumns);
							}
						}
						if (horizOpen) EditorGUILayout.EndHorizontal();
					});

					IsTutorialStep(9, QuickButtonFoldout, () =>
					{
						NSEditorStyles.DrawLabel("Quickly navigates to existing Suit Body Collider objects in the scene.\n"
							, 105, 14);
					});

					if (QuickButtonFoldout)
					{
						EditorGUILayout.EndVertical();
					}


				}
				else
				{
					IsTutorialStep(9, () =>
					{
						NSEditorStyles.DrawLabel(" Open this to continue! " + ""

							, 105, 14);
					});
				}
			}
		}

		//Unsure if this is necessary - remove?
		void OnInspectorUpdate()
		{
			Repaint();
		}
		public override void Update()
		{
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift) && Input.GetKey(KeyCode.RightControl))
			{
				for (int i = 0; i < Suits.Count; i++)
				{
					Suits[i].CanChangeValues = true;
				}
			}
			base.Update();
		}

		public override void DrawTitleContent()
		{
			base.DrawTitleContent();
		}
		public override void DrawPaneContent()
		{
			//base.DrawPaneContent();
			//Make a button that auto looks things up in children of an object
			if (icon != null)
			{
				NSEditorStyles.DrawBackgroundImage(icon, mat, HLEditor);
			}

			//EditorGUILayout.InspectorTitlebar(true, this, true);


			if (Suits != null && Suits.Count > 1)
			{
				bool allowExpandAll = Suits != null && Suits.Count > 1;
				bool ShouldCollapse = true;
				if (allowExpandAll)
				{
					ShouldCollapse = Suits.Where(x => x.TopFoldout).Count() > 0;
				}
				GUIContent content = new GUIContent(ShouldCollapse ? "Collapse All" : "Expand All");
				bool result = NSEditorStyles.OperationButton(!allowExpandAll, content);

				if (result)
				{
					for (int i = 0; i < Suits.Count; i++)
					{
						Suits[i].TopFoldout = !ShouldCollapse;
					}
				}
			}

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

			for (int i = 0; i < Suits.Count; i++)
			{
				Suits[i].OnGUI();
			}

			EditorGUILayout.EndScrollView();

			DrawQuickButtonsForHardlightColliders();

			bool clicked = false;
			TutorialHighlight(10, () =>
			{
				clicked = GUILayout.Button("Add Suit Configuration");
			});

			if (clicked)
			{
				AddSuitConfiguration();
			}

			IsTutorialStep(10, () =>
			{
				NSEditorStyles.DrawLabel("This button adds additional suit configurations.\nWe don't anticipate anyone will need multiple definitions yet but this is a future oriented feature.", 105, 14);
			});

			//Label describing section
			//[TODO]

		}

		#region [Incomplete] Functions for body root processing
		//The feature target here would be a couple click process
		//Step 1: Assign the root node
		//Step 2: Set to AutoConfigure based on root object.
		//		A function recursively searches the child objects.
		//		For each SuitHolder that is desired, it creates a list of Transforms sorted by confidence levels.
		//		The user can then customize and select options for which spots get the body colliders

		void ProcessRootObject()
		{

		}

		public class LookupObject
		{

		}
		//Record confidence? Highlight the poor matches
		//Green/good for the ones that we light
		//Look at UE4/Unity's Mecanim default rigs - this will be a common naming convention
		//Mixamo's rig setup
		//Note: Spines might be lettered or numbered or vague
		//List<Transform> RecursiveSearchForRelevantObjects(Transform root)
		//{

		//}
		#endregion

		#region Editor Saving
		private void OnEnable()
		{
			QuickButtonFoldout = ImprovedEditorPrefs.GetBool("NS-QuickButton", QuickButtonFoldout);
			//Debug.Log("Looking for NS-QuickButton - " + EditorPrefs.HasKey("NS-QuickButton") + "   \n" + EditorPrefs.GetBool("NS-QuickButton"));
		}

		private void OnDisable()
		{
			EditorPrefs.SetBool("NS-QuickButton", QuickButtonFoldout);
		}
		#endregion
	}
}