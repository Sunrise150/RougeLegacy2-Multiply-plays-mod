using System.Collections.Generic;
using UnityEngine;

public class PrefabInstantiator : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> m_prefabs;

	[SerializeField]
	private bool m_parentToThisGameObject = true;

	private void Awake()
	{
		foreach (GameObject prefab in m_prefabs)
		{
			Transform parent = null;
			if (m_parentToThisGameObject)
			{
				parent = base.transform;
			}
			Object.Instantiate(prefab, parent);
		}
	}
}
