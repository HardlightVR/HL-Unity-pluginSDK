using UnityEngine;
using System.Collections;

namespace NullSpace.SDK.Demos
{
	public class AutoPitcher : MonoBehaviour
	{
		/// <summary>
		/// The elapsed duration range between pitches (sometimes it'll be longer, sometimes shorter)
		/// </summary>
		Vector2 pitchFrequencyRange = new Vector2(.40f, 1.6f);
		/// <summary>
		/// The range of spawn distances for new meteors.
		/// </summary>
		Vector2 spawnDistRange = new Vector2(25f, 35f);
		/// <summary>
		/// The range of pitch speeds.
		/// Note: Projectiles of higher indices get a bit of speed upping
		/// </summary>
		Vector2 pitchSpeedRange = new Vector2(10f, 15f);
		float maxSpeed = 45;

		/// <summary>
		/// Set to false to cancel pitching.
		/// Setting to true will not resume pitching (probably not important for this sample code)
		/// </summary>
		public bool Pitching = false;
		/// <summary>
		/// Controls if the AutoPitcher will accidentally shoot at an incorrect non-body position.
		/// </summary>
		public bool CanMiss = false;

		private int counter = 0;
		private int levelUpCounter = 10;
		private int level = 0;
		private System.DateTime start;

		/// <summary>
		/// Our level-up indicator.
		/// </summary>
		public ParticleSystem EscalateEffect;

		/// <summary>
		/// The different projectiles to shoot
		/// </summary>
		public GameObject[] validProjectiles;

		void Start()
		{
			start = System.DateTime.Now;
			StartCoroutine(AutoPitch());
		}

		IEnumerator AutoPitch()
		{
			//Delay our start so the VRMimic/BodyMimic can get initialized.
			yield return new WaitForSeconds(1);
			Pitching = true;
			while (Pitching)
			{
				yield return new WaitForSeconds(Random.Range(pitchFrequencyRange.x, pitchFrequencyRange.y));
				Pitch();
			}
		}

		private void Update()
		{
			//Testing to force individual pitch types.
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				Pitch(0);
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				Pitch(1);
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				Pitch(2);
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				Pitch(3);
			}
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				Escalate();
			}
		}
		/// <summary>
		/// Random range 0-15. Condenses it into an index (0-3) on a weighted hand-set scale.
		/// </summary>
		/// <returns></returns>
		int WeightedRandomProjectileIndex()
		{
			int val = Random.Range(0, 15);
			if (val < 5)
			{
				return 0;
			}
			if (val < 8)
			{
				return 1;
			}
			if (val < 11)
			{
				return 2;
			}
			if (val < 13)
			{
				return 3;
			}
			return 0;
		}

		void Pitch(int index = -1)
		{
			if (index < 0)
			{
				index = WeightedRandomProjectileIndex();

				//Prevent you from getting heavier hitting projectiles.
				index = Mathf.Clamp(index, 0, level);

				//for (int i = 0; i < 100; i++)
				//{
				//	index = WeightedRandomProjectileIndex();
				//	Debug.Log(index + "\n");
				//}
			}

			GameObject go = validProjectiles[Mathf.Clamp(index, 0, validProjectiles.Length - 1)];

			go = Instantiate(go, RequestSpawnPosition(), Quaternion.identity) as GameObject;

			//Shoot the projectile towards the player.
			Rigidbody rb = go.GetComponent<Rigidbody>();

			//Find the world position of the target (a random node)
			Vector3 target = HardlightSuit.Find().FindRandomLocation().transform.position;

			if (CanMiss)
			{
				//A fifth of the time
				if (Random.Range(0, 50) > 40)
				{
					//We'll random miss the player.
					target += Vector3.right * Random.Range(-0.5f, 0.5f);
					target += Vector3.forward * Random.Range(-0.5f, 0.5f);
				}
			}

			//Draw a line to show where it's going.
			Debug.DrawLine(go.transform.position, target, Color.red, 8.0f);

			if (rb != null)
			{
				//Add some force to send the projectile on it's way.
				float speed = Random.Range(pitchSpeedRange.x, pitchSpeedRange.y) + Mathf.Clamp(2 * ((index)), 0, 15);

				//Clamp the speed to prevent tunneling.
				speed = Mathf.Clamp(speed, 5, maxSpeed);

				rb.AddForce((target - go.transform.position) * speed);

				//So we can see the speed
				go.name += "  V = " + speed;
			}

			//Track the amount of pitches we've made
			counter++;

			//When we've pitched enough
			if (counter > levelUpCounter)
			{
				//Up the experience intensity
				Escalate();
			}

			//Ensure the projectiles that miss are cleaned up.
			Destroy(go, 30);
		}

		/// <summary>
		/// Escalate the experience over time
		/// Ups the pitch speeds and frequencies.
		/// Level is also used to control which projectiles get used.
		/// </summary>
		void Escalate()
		{
			//Level controls
			level++;
			levelUpCounter += 2 + level;
			counter = 0;

			//Firing frequency
			pitchFrequencyRange.x = Mathf.Clamp(pitchFrequencyRange.x - .03f, .2f, 5);
			pitchFrequencyRange.y = Mathf.Clamp(pitchFrequencyRange.y - .06f, .5f, 5);

			//Firing speeds (slowly creeps into bigger ranges)
			pitchSpeedRange.x = Mathf.Clamp(pitchSpeedRange.x + 3, 5, maxSpeed);
			pitchSpeedRange.y = Mathf.Clamp(pitchSpeedRange.y + 4, 5, maxSpeed);

			//Debug.Log("Escalate: " + (System.DateTime.Now - start).TotalSeconds + " New Pitch Speed Range: " + pitchSpeedRange.ToString() + "\n");

			if (EscalateEffect != null)
			{
				//Display the level up effect.
				EscalateEffect.Play();
			}
		}

		/// <summary>
		/// Pick a random spawn position. In front of where the player is looking. A bit left or right and varying degrees of up.
		/// </summary>
		/// <returns></returns>
		Vector3 RequestSpawnPosition()
		{
			//Get the VR Camera from the Mimic tools. Save the transform for easier referencing
			Transform camTransform = VRMimic.Instance.VRCamera.transform;

			Vector3 fwd = camTransform.forward;
			fwd.y = 0;

			//We never fire from below or behind the player.
			return (fwd * Random.Range(spawnDistRange.x, spawnDistRange.y) +
						Random.Range(-20, 20) * camTransform.right +
						Random.Range(3, 25) * camTransform.up);
		}
	}
}