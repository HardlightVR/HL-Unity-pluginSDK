using UnityEngine;
using System.Collections;
using Hardlight.SDK.FileUtilities;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// This is an example type of projectile (fired by AutoPitcher)
	/// It contains no movement code.
	/// Heavily commented if you're wondering how anything was done.
	/// Requires Rigidbody and Collider.
	/// </summary>
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(Collider))]
	public class ExampleProjectile : MonoBehaviour
	{
		/// <summary>
		/// Where the projectile was last frame.
		/// </summary>
		private Vector3 lastPosition;

		public enum CollisionType { Hit, HitImpulse, BigImpact, RepeatedImpulse }
		/// <summary>
		/// This example projectile supports 4 types of haptics
		/// Hit - Single effect hit.
		/// HitImpulse - Play an emanating effect.
		/// BigImpact - Hits all pads within a Unity Distance of the impact point.
		/// RepeatedImpulse - Plays a repeating impulse starting at the closest HapticLocation to impact
		/// </summary>
		public CollisionType typeOfCollision = CollisionType.Hit;

		/// <summary>
		/// This is the area for the instaneous BigImpact CollisionType.
		/// Most body regions are within ~.6f Unity units of each other.
		/// </summary>
		public float BigImpactArea = .35f;

		/// <summary>
		/// Disable this to prevent the projectile from hitting other objects.
		/// This allows you to visually destroy the projectile with SFX or something before Destroying the GameObject and losing access to collision/position information
		/// </summary>
		public bool CanCollide = true;

		/// <summary>
		/// Does what it says on the tin.
		/// Has a destroy timer
		/// </summary>
		public bool DestroyAfterCollision = true;

		/// <summary>
		/// The delay to destroy the projectile (after collisions)
		/// </summary>
		public float DestroyDelay = 1.5f;

		/// <summary>
		/// If the script exists within a more complex hierarchy, you might want to use this field to destroy the root projectile.
		/// </summary>
		public GameObject rootObject;

		/// <summary>
		/// Calls a GetComponent for a Collider at start. Checks if it is a Trigger.
		/// Might get caught up if there are multiple Colliders. You can disable this harm free.
		/// </summary>
		private bool CheckIfTrigger = true;

		private void Start()
		{
			if (CheckIfTrigger)
			{
				//If the projectile ISN'T a trigger
				if (!GetComponent<Collider>().isTrigger)
				{
					//This error serves to catch if you hit the easily achieved mistake of forgetting to check the 'IsTrigger' checkbox.
					Debug.LogError("Example Projectile [] has a collider that is not a trigger. If you aren't experiencing haptics, this might be why.\n\tYou can expose then disable CheckIfTrigger if this error is superfluous (meaning you have multiple colliders and one is a trigger.");
				}
			}
		}

		private void LateUpdate()
		{
			//This line is commented so we can see the movement progress.
			//Debug.DrawLine(lastPosition, transform.position, Color.green);
			//We hold onto our last position to use when we collide. This lets us avoid tunneling.
			lastPosition = transform.position;
		}

		public void OnTriggerEnter(Collider col)
		{
			if (CanCollide)
			{
				Collide(col, transform.position);
			}
		}

		public void Collide(Collider col, Vector3 where)
		{
			#region Hit Player
			//Layer 31 is the default haptics layer.
			//This lets us check if what we hit is a 'haptic object' since thats all the Example Projectile wants to hit.
			if (col.gameObject.layer == HardlightManager.HAPTIC_LAYER)
			{
				//Debug.DrawLine(transform.position, transform.position + Vector3.up * 100, Color.cyan, 15);
				bool hapticCollisionOccurred = false;

				//Is what we hit a Hardlight Suit?
				HardlightSuit body = col.gameObject.GetComponent<HardlightSuit>();

				//We make some assumptions about the impact point.
				//Our projectiles are moving fast, so we keep track of our position last frame (lastPosition) and use that instead of our current one.
				//I won't pretend to know what is best for your game (Desert of Danger uses a highly sophisticated predictive system because the projectiles are nearly hitscan)
				//This demo uses last frame to avoid tunneling and accidentally hitting the back pads.

				//If we hit a suit
				if (body != null)
				{
					CollideWithBody(body, col, where);
					hapticCollisionOccurred = true;
				}
				else
				{
					HardlightCollider individualCollider = col.gameObject.GetComponent<HardlightCollider>();
					if (individualCollider != null)
					{
						CollideWithBody(HardlightSuit.Find(), col, where);

						//CollideWithHardlightCollider(individualCollider, where);
						hapticCollisionOccurred = true;
					}
				}

				if (hapticCollisionOccurred && DestroyAfterCollision)
				{
					DestroyProjectile(DestroyDelay);
				}
			}
			#endregion
		}

		public void CollideWithBody(HardlightSuit body, Collider col, Vector3 where)
		{
			//Default sequence, gets reassigned
			HapticSequence seq = body.GetSequence("pulse");
			
			//Depending on the type of projectile that we are.
			switch (typeOfCollision)
			{
				case CollisionType.Hit:

					//Hit the body with a simple effect name - which should be in "Resources/Haptics/<Name>"
					body.HitNearest(lastPosition, "double_click");

					break;
				case CollisionType.HitImpulse:

					//Plays a simple impulse on the assumed impact point.
					body.HitImpulse(lastPosition, Effect.Pulse, .2f, .35f, 1, 2);

					break;
				case CollisionType.BigImpact:

					//This plays an effect across all found pads within a range.
					body.HitNearby(lastPosition, "buzz", BigImpactArea);

					break;
				case CollisionType.RepeatedImpulse:

					//More helper function usage. This asset is from the Death and Dying pack (which is a very intense effect)
					seq = body.GetSequence("pain_short");

					//This makes use of the other HitImpulse which allows for repeated effects.
					body.HitImpulse(lastPosition, seq, .2f, 2, 3, .15f, 1.0f);

					break;
			}
		}

		//public void CollideWithHardlightCollider(HardlightCollider collider, Vector3 where)
		//{
		//	HardlightSuit.Find().HitNearest(lastPosition, "double_click");

		//	//Debug.Log("HIT INDIVIDUAL COLLIDER\n" + collider.name + "     " + where);
		//}

		/// <summary>
		/// Cleans up our projectile, marks that we can't collide anymore.
		/// </summary>
		/// <param name="DestroyTime"></param>
		public void DestroyProjectile(float DestroyTime)
		{
			//Say we can't collide anymore
			CanCollide = false;

			//Destroy the projectile after a little bit (so we could do some sort of collision visual)
			if (rootObject != null)
			{
				Destroy(rootObject, DestroyTime);
			}
			else
			{
				Destroy(gameObject, DestroyTime);
			}
		}
	}
}