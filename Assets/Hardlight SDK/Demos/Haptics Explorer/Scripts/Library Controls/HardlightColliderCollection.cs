using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Hardlight.SDK.Demos
{
	public class HardlightColliderCollection : MonoBehaviour
	{
		public List<HardlightCollider> SuitObjects = new List<HardlightCollider>();

		private bool initialized = false;

		void Awake()
		{
			if (!initialized)
			{
				Init();
			}
		}
		public void Init()
		{
			initialized = true;
			SuitObjects = new List<HardlightCollider>();
			SuitObjects = FindObjectsOfType<HardlightCollider>().ToList();
			Debug.Log("Detected " + SuitObjects.Count + " suit objects\n", this);
		}
	}
}