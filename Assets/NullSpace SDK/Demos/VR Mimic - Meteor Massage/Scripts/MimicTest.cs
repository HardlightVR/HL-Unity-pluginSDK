using UnityEngine;
using System.Collections;

namespace NullSpace.SDK
{
	public class MimicTest : MonoBehaviour
	{
		public LayerMask ValidLayers;
		//HapticSequence seq;
		void Start()
		{
			StartCoroutine(Begin());
			//seq = new HapticSequence();
			//seq.LoadFromAsset("Haptics/pulse");
		}

		void Update()
		{
			#region Test Sphere Flag Checking
			if (Input.GetKeyDown(KeyCode.F5))
			{
				HardlightSuit body = HardlightSuit.Find();
				if (body != null)
				{
					Vector3 randPos = body.transform.position + Random.onUnitSphere / 2;
					float dist = Random.Range(0, 0.5f);
					float dur = 7.0f;
					Debug.DrawLine(randPos, randPos + Vector3.up * dist, Color.red, dur);
					Debug.DrawLine(randPos, randPos - Vector3.up * dist, Color.red, dur);
					Debug.DrawLine(randPos, randPos + Vector3.right * dist, Color.red, dur);
					Debug.DrawLine(randPos, randPos - Vector3.right * dist, Color.red, dur);
					Debug.DrawLine(randPos, randPos + Vector3.forward * dist, Color.red, dur);
					Debug.DrawLine(randPos, randPos - Vector3.forward * dist, Color.red, dur);
					body.FindAllFlagsWithinRange(randPos, dist);
				}
			}
			#endregion
			#region Do single point-to-nearest haptic
			if (Input.GetKeyDown(KeyCode.F7))
			{
				HardlightSuit body = HardlightSuit.Find();
				if (body != null)
				{
					Vector3 randPos = body.transform.position + Random.onUnitSphere / 2;
					body.Hit(randPos, "pulse");
				}
			}
			#endregion
			#region Do 500 point-to-nearest haptic. For testing to see pad hit rates
			if (Input.GetKeyDown(KeyCode.F8))
			{
				HardlightSuit body = HardlightSuit.Find();
				if (body != null)
				{
					for (int i = 0; i < 500; i++)
					{
						Vector3 randPos = body.transform.position + Random.onUnitSphere / 2;
						body.Hit(randPos, "pulse");
					}
				}
			}
			#endregion
			#region Line of Sight Find Nearest
			if (Input.GetKeyDown(KeyCode.F9))
			{
				HardlightSuit body = HardlightSuit.Find();
				if (body != null)
				{
					Vector3 randPos = body.transform.position + Random.onUnitSphere / 2;
					body.FindNearbyLocation(randPos, true, ValidLayers);
				}
			}
			#endregion
			#region Request Random Location
			if (Input.GetKeyDown(KeyCode.F10))
			{
				Vector3 pos = HardlightSuit.Find().FindRandomLocation().transform.position;
				HardlightSuit body = HardlightSuit.Find();
				if (body != null)
				{
					Vector3 randPos = body.transform.position + Random.onUnitSphere / 2;
					Debug.DrawLine(randPos, pos, Color.cyan, 6.0f);
				}
			}
			#endregion
			#region Test NumberOfArea flag counting and IsSingleArea
			if (Input.GetKeyDown(KeyCode.F11))
			{
				AreaFlag flag = (AreaFlag.All_Areas).RemoveArea(AreaFlag.Back_Both);
				Debug.Log(flag.NumberOfAreas() + "\n" + flag.IsSingleArea());

				flag = AreaFlag.Back_Both;
				Debug.Log(flag.NumberOfAreas() + "\n" + flag.IsSingleArea());

				flag = AreaFlag.Right_All;
				Debug.Log(flag.NumberOfAreas() + "\n" + flag.IsSingleArea());

				flag = AreaFlag.Mid_Ab_Left;
				Debug.Log(flag.NumberOfAreas() + "\n" + flag.IsSingleArea());
			}
			#endregion
		}

		IEnumerator Begin()
		{
			yield return new WaitForSeconds(.5f);

			//This sets up a base body. It hands in the camera and the layer to hide.
			VRMimic.Initialize();
		}
	}
}