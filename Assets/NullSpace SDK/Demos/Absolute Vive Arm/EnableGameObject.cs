using UnityEngine;
using System.Collections;

public class EnableGameObject : MonoBehaviour
{
	public GameObject toEnable;
	void Start()
	{
		if (toEnable)
		{
			toEnable.SetActive(true);
		}
	}
}