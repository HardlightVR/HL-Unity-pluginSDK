using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace NullSpace.SDK.Demos
{
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