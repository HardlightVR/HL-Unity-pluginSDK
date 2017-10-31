using UnityEngine;
using System.Collections;

namespace Hardlight.SDK.Demos
{
	//Helper class that adds prefabs to the container
	//Can be used to clear/remove items
	public class PopulateContainer : MonoBehaviour
	{
		[SerializeField]
		private GameObject prefab;
		public RectTransform container;

		public int count;

		public GameObject Prefab
		{
			get
			{
				return prefab;
			}

			set
			{
				if (value == null)
				{
					Debug.LogError("Attempted to set [" + name + "]'s Populate Container prefab to null\n", this);
				}
				prefab = value;
			}
		}

		public GameObject AddPrefabToContainerReturn(bool worldPositionStays = false)
		{
			if (Prefab != null)
			{
				var instance = Instantiate<GameObject>(Prefab);
				instance.transform.SetParent(container, worldPositionStays);

				var indexer = instance.GetComponent<ContainerItem>();
				if (indexer)
				{
					indexer.container = this;
					indexer.Index(count++);
				}
				return instance;
			}
			Debug.LogError("[" + name + "]'s Populate Container Prefab has not been assigned\n", this);
			return null;
		}

		public void AddPrefabToContainer()
		{
			AddPrefabToContainerReturn();
		}

		public void RemoveFromContainer(Transform item)
		{
			item.SetParent(null);
			Destroy(item.gameObject);

			count = 0;
			foreach (Transform child in container)
			{
				var indexer = child.GetComponent<ContainerItem>();
				if (indexer) indexer.Index(count++);
			}
		}

		public void Clear()
		{
			count = 0;
			foreach (Transform child in container)
			{
				var indexer = child.GetComponent<ContainerItem>();
				if (indexer) Destroy(child.gameObject);
			}
		}
	}
}