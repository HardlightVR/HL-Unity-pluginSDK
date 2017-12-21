using UnityEngine;
using System.Collections;

//Contents of this namespace are subject to change
namespace Hardlight.SDK.Experimental
{
	/// <summary>
	/// Defines a collection of prefabs that are used to create visuals for the player body.
	/// One core visual is that of 'No Visual' which is used for the data model that other visuals are dependant upon.
	/// </summary>
	[CreateAssetMenu(menuName = "Hardlight/VR/Mimic Visual Data")]
	[System.Serializable]
	public class BodyVisualPrefabData : ScriptableObject
	{
		[SerializeField]
		public GameObject UpperArmPrefab;
		[SerializeField]
		public GameObject ForearmPrefab;
		[SerializeField]
		public GameObject JointPrefab;
		[SerializeField]
		public GameObject TorsoPrefab;
		[SerializeField]
		public GameObject ShoulderConnectorPrefab;

		/// <summary>
		/// Creates a prefab data clone (not the original so we don't encounter asset breaking problems)
		/// </summary>
		/// <returns></returns>
		public BodyVisualPrefabData Clone()
		{
			hideFlags = HideFlags.DontSave;
			BodyVisualPrefabData clone = ScriptableObject.CreateInstance<BodyVisualPrefabData>();
			clone.UpperArmPrefab = UpperArmPrefab;
			clone.ForearmPrefab = ForearmPrefab;
			clone.JointPrefab = JointPrefab;
			clone.TorsoPrefab = TorsoPrefab;
			clone.ShoulderConnectorPrefab = ShoulderConnectorPrefab;

			clone.name += " (runtime clone)";
			return clone;
		}
	}
}