using UnityEngine;
using System.Collections;

namespace Hardlight.SDK
{
	public class BodyMimicTestFunctions : MonoBehaviour
	{
		BodyMimic mimic;
		void Start()
		{
			mimic = VRMimic.Instance.ActiveBodyMimic;
		}

		void Update()
		{
			if (mimic)
			{
				if (Input.GetKeyDown(KeyCode.V))
				{
					//Delete
					mimic.DisposeVisuals(new VisualDisposer());
				}
				if (Input.GetKeyDown(KeyCode.C))
				{
					mimic.DisposeVisuals(new VisualDisposer(VisualDisposer.DisposalBehaviors.RigidbodyDrop));
				}
				if (Input.GetKeyDown(KeyCode.X))
				{
					mimic.DisposeVisuals(new VisualDisposer(VisualDisposer.DisposalBehaviors.RigidbodyScatter));
				}
				if (Input.GetKeyDown(KeyCode.Z))
				{
					var disposer = new VisualDisposer(Random.onUnitSphere * 1.5f);
					mimic.DisposeVisuals(disposer);
				}
				if (Input.GetKeyDown(KeyCode.S))
				{
					var disposer = new VisualDisposer((Random.onUnitSphere + Vector3.up * 5).normalized * .5f);
					mimic.DisposeVisuals(disposer);
				}
				if (Input.GetKeyDown(KeyCode.Space))
				{
					mimic.CreateVisuals(mimic.VisualPrefabs, new VisualDisposer());
				}
			}
		}
	}
}