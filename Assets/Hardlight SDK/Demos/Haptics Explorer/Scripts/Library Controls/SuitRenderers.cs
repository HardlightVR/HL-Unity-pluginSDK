using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Hardlight.SDK.Demos
{
	/// <summary>
	/// A dictionary of gameobject->MeshRenderers for the HardlightCollider visible objects.
	/// Lazily populated & cached. You can technically request other objects (which aren't suit objects) but you shouldn't do that. 
	/// </summary>
	public class SuitRenderers : MonoBehaviour
	{
		public Dictionary<GameObject, MeshRenderer> suitRenderers = new Dictionary<GameObject, MeshRenderer>();

		public MeshRenderer GetRenderer(GameObject suitObject)
		{
			if (!suitRenderers.ContainsKey(suitObject))
			{
				MeshRenderer rend = suitObject.GetComponent<MeshRenderer>();
				if (rend != null)
				{
					suitRenderers.Add(suitObject, rend);
				}
			}
			if (suitRenderers.ContainsKey(suitObject))
			{
				return suitRenderers[suitObject];
			}
			return null;
		}
	}
}