using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	public class MimicTest : MonoBehaviour
	{
		public Camera targetCamera;
		public LayerMask ValidLayers;
		HardlightSuit suit;
		void Awake()
		{
			//This sets up a base body. It hands in the camera so the body follows/mimics the likely movements of the user. It also hides the body from the camera (since the body is by default on layer 31)
			VRMimic.Initialize(targetCamera.gameObject);

			//Hold onto a reference to the suit. Gives us access to a variety of helper functions.
			suit = HardlightSuit.Find();
		}

		void Update()
		{
			#region Test Sphere Flag Checking
			if (Input.GetKeyDown(KeyCode.F5))
			{
				float dist = Random.Range(0, 0.5f);

				TestFindAllFlags(dist);
			}
			#endregion
			#region Do single point-to-nearest haptic
			if (Input.GetKeyDown(KeyCode.F7))
			{
				Vector3 randPos = suit.transform.position + Random.onUnitSphere / 2;
				suit.HitNearest(randPos, "pulse");
			}
			#endregion
			#region Line of Sight Find Nearest
			if (Input.GetKeyDown(KeyCode.F9))
			{
				Vector3 randomDir = Random.onUnitSphere * 3;
				randomDir.y = 0;
				suit.FindNearbyLocation(suit.transform.position + randomDir, true, ValidLayers, 15);
			}
			#endregion
			#region Example Request Random Location
			if (Input.GetKeyDown(KeyCode.F10))
			{
				ExampleRequestRandomLocation();
			}
			#endregion
			#region Test NumberOfArea flag counting and IsSingleArea
			if (Input.GetKeyDown(KeyCode.F11))
			{
				TestAreaFlagExtensions();
			}
			#endregion
		}

		private void TestFindAllFlags(float sphereRadius)
		{
			Vector3 randPos = suit.transform.position + Random.onUnitSphere / 2;
			float indicatorDuration = 7.0f;

			#region Displays the 'sphere' that we're finding all flags around
			Debug.DrawLine(randPos, randPos + Vector3.up * sphereRadius, Color.red, indicatorDuration);
			Debug.DrawLine(randPos, randPos - Vector3.up * sphereRadius, Color.red, indicatorDuration);
			Debug.DrawLine(randPos, randPos + Vector3.right * sphereRadius, Color.red, indicatorDuration);
			Debug.DrawLine(randPos, randPos - Vector3.right * sphereRadius, Color.red, indicatorDuration);
			Debug.DrawLine(randPos, randPos + Vector3.forward * sphereRadius, Color.red, indicatorDuration);
			Debug.DrawLine(randPos, randPos - Vector3.forward * sphereRadius, Color.red, indicatorDuration);
			#endregion

			//Useful for explosive projectiles
			suit.GetAreasWithinRange(randPos, sphereRadius);
		}
		private void ExampleRequestRandomLocation()
		{
			//We use the suit's find random location option - this gives us a random pad.
			//Great for if your bad guys 
			HapticLocation randomLocation = suit.FindRandomLocation(AreaFlag.All_Areas);

			//Directly away from the randomly requested pad.
			Vector3 away = (randomLocation.transform.position - suit.transform.position);

			//The random pads location.
			Vector3 randLocationPosition = randomLocation.transform.position;

			//A bit of fuzzing so the lines stand apart
			Vector3 fuzzyOffset = Random.insideUnitSphere * .1f;

			//Debug.Log("Requested Random Location : " + randomLocation.Where.ToString() + "\n" + randomLocation.name.ToString() + "\t\t" + away.ToString());

			//Draw a line to show the result
			Debug.DrawLine(randLocationPosition + fuzzyOffset,
						   randLocationPosition + fuzzyOffset + away.normalized * 5,
								Color.cyan, 8.0f);
		}
		private void TestAreaFlagExtensions()
		{
			//This section digs into some of the AreaFlagExtension methods.

			string Report = "[AreaFlag Tests]\n" + "\t[Number of Areas/Single Area Tests]\n\n";
			AreaFlag flag = AreaFlag.Mid_Ab_Left;

			Report += "How many flags in Mid Ab Left [1]:" + flag.NumberOfAreas() + " - is single area? [T] " + flag.IsSingleArea() + "\n";

			flag = AreaFlag.Back_Both;
			Report += "How many flags in Mid Ab Left [2]: " + flag.NumberOfAreas() + " - is single area? [F] " + flag.IsSingleArea() + "\n";

			flag = (AreaFlag.All_Areas).RemoveArea(AreaFlag.Back_Both);
			Report += "How many flags in All but Back Both [14]: " + flag.NumberOfAreas() + " - is single area? [T] " + flag.IsSingleArea() + "\n";

			flag = AreaFlag.Right_All;
			Report += "How many flags in Right All [8]: " + flag.NumberOfAreas() + " - is single area? [T] " + flag.IsSingleArea() + "\n\n";

			flag = AreaFlag.All_Areas;
			Report += "AreaFlag HasFlag Tests:\nDoes All_Areas contain Lower Left Ab? (T): " + flag.HasFlag(AreaFlag.Lower_Ab_Left) +
				"\nDoes All_Areas contain Right_All? (T):  " + flag.HasFlag(AreaFlag.Right_All) +
				"\nDoes Right_All contain Back_Both? (F): " + AreaFlag.Right_All.HasFlag(AreaFlag.Back_Both) + "\n";

			Debug.Log(Report);
		}
	}
}