using System.Collections.Generic;
using UnityEngine;

public class ScenePrefab_Loader : MonoBehaviour
{
	[SerializeField]
	private string m_prefabPath;

	private static Dictionary<string, GameObject> m_scenePrefabDict = new Dictionary<string, GameObject>();

	private void Awake()
	{
		if (!m_scenePrefabDict.TryGetValue(m_prefabPath, out var value))
		{
			value = CDGResources.Load<GameObject>(m_prefabPath);
			m_scenePrefabDict.Add(m_prefabPath, value);
		}
		Object.Instantiate(value);
	}
}
