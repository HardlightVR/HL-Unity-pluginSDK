using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VisualDisposer
{
	public enum DisposalBehaviors { ImmediateDelete, RigidbodyDrop, RigidbodyScatter, DestructionVector }
	public DisposalBehaviors DisposalTechnique;
	public List<GameObject> visualsToDispose;
	private Vector3 destructionDirection = Vector3.up;

	public VisualDisposer(DisposalBehaviors Technique = DisposalBehaviors.ImmediateDelete)
	{
		DisposalTechnique = Technique;
		visualsToDispose = new List<GameObject>();
	}
	public VisualDisposer(Vector3 DestructionVector)
	{
		DisposalTechnique = DisposalBehaviors.DestructionVector;
		destructionDirection = DestructionVector;
		visualsToDispose = new List<GameObject>();
	}

	public void RecordVisual(GameObject visualToDispose)
	{
		if (visualToDispose != null)
		{
			visualsToDispose.Add(visualToDispose);
		}
	}

	public void Dispose()
	{
		if (DisposalTechnique == DisposalBehaviors.ImmediateDelete)
		{
			DeleteRecordedVisuals();
		}
		else if (DisposalTechnique == DisposalBehaviors.RigidbodyDrop)
		{
			DropRecordedVisuals();
		}
		else if (DisposalTechnique == DisposalBehaviors.RigidbodyScatter)
		{
			DropRecordedVisuals();
		}
		else if (DisposalTechnique == DisposalBehaviors.DestructionVector)
		{
			DropRecordedVisuals();
		}
	}

	private void DeleteRecordedVisuals()
	{
		for (int i = visualsToDispose.Count - 1; i >= 0; i--)
		{
			GameObject.Destroy(visualsToDispose[i]);
		}
	}
	private void DropRecordedVisuals()
	{
		for (int i = 0; i < visualsToDispose.Count; i++)
		{
			visualsToDispose[i].transform.SetParent(null);

			var rb = visualsToDispose[i].GetComponent<Rigidbody>();
			if (!rb)
			{
				rb = visualsToDispose[i].AddComponent<Rigidbody>();
			}
			if (rb)
			{
				rb.useGravity = true;
				rb.isKinematic = false;
				if (DisposalTechnique == DisposalBehaviors.RigidbodyScatter || DisposalTechnique == DisposalBehaviors.DestructionVector)
				{
					rb.drag = Random.Range(0, 1);
					var force = GetScatterDirection();
					rb.AddForce(force, ForceMode.Impulse);
				}
			}
		}
	}
	private Vector3 GetScatterDirection()
	{
		if (DisposalTechnique == DisposalBehaviors.DestructionVector)
			return destructionDirection * Random.Range(2, 20);
		else
			return (Random.onUnitSphere) * Random.Range(2, 20);
	}
}