using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NullSpace.SDK.Demos
{
	public class SuitDefinition : ScriptableObject
	{
		public string SuitName = "Player Body";
		public GameObject SuitRoot;

		[SerializeField]
		public List<AreaFlag> DefinedAreas;

		//The Game Objects to fill the fields (which will get hardlight collider references)
		[SerializeField]
		public List<GameObject> ZoneHolders;

		//the objects added. Will get a nice button list to quick get to each of them.
		[SerializeField]
		public List<HardlightCollider> SceneReferences;

		public int HapticsLayer = 31;
		public bool AddChildObjects = true;
		public bool AddExclusiveTriggerCollider = true;

		public void Init()
		{
			SetDefaultAreas();

			SetupParents();

			GenerateSceneReferences();
		}
		public void SetDefaultAreas()
		{
			if (DefinedAreas == null || DefinedAreas.Count == 0)
			{
				DefinedAreas = new List<AreaFlag>();

				DefinedAreas.Add(AreaFlag.Forearm_Left);
				DefinedAreas.Add(AreaFlag.Upper_Arm_Left);

				DefinedAreas.Add(AreaFlag.Shoulder_Left);
				DefinedAreas.Add(AreaFlag.Back_Left);
				DefinedAreas.Add(AreaFlag.Chest_Left);

				DefinedAreas.Add(AreaFlag.Upper_Ab_Left);
				DefinedAreas.Add(AreaFlag.Mid_Ab_Left);
				DefinedAreas.Add(AreaFlag.Lower_Ab_Left);

				DefinedAreas.Add(AreaFlag.Forearm_Right);
				DefinedAreas.Add(AreaFlag.Upper_Arm_Right);

				DefinedAreas.Add(AreaFlag.Shoulder_Right);
				DefinedAreas.Add(AreaFlag.Back_Right);
				DefinedAreas.Add(AreaFlag.Chest_Right);

				DefinedAreas.Add(AreaFlag.Upper_Ab_Right);
				DefinedAreas.Add(AreaFlag.Mid_Ab_Right);
				DefinedAreas.Add(AreaFlag.Lower_Ab_Right);
			}
		}

		public void SetupParents()
		{
			if (ZoneHolders == null || ZoneHolders.Count == 0)
			{
				//Debug.Log("Resetting Suit Holders\n");
				ZoneHolders = new List<GameObject>();

				for (int i = 0; i < DefinedAreas.Count; i++)
				{
					ZoneHolders.Add(null);
				}
			}
		}

		public void GenerateSceneReferences()
		{
			if (SceneReferences == null || SceneReferences.Count == 0)
			{
				//Debug.Log("Resetting Filled Areas\n");
				SceneReferences = new List<HardlightCollider>();

				for (int i = 0; i < DefinedAreas.Count; i++)
				{
					SceneReferences.Add(null);
				}
			}
		}
	}
}