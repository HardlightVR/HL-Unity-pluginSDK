using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace NullSpace.SDK.Demos
{
	public class ContainerItem : MonoBehaviour
	{
		[Header("Container Item Attributes")]
		public PopulateContainer container;
		public int index = -1;
		public void Index(int ind)
		{
			this.index = ind;
		}

		public void RemoveFromContainer()
		{
			if (container)
			{ container.RemoveFromContainer(transform); }
		}
	}
}