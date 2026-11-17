using System.Collections.Generic;
using UnityEngine;

public class SpawnPositionController : MonoBehaviour
{
	private SpawnPositionObj[] m_spawnPositionArray;

	private static List<string> m_warningIssuedTracker = new List<string>();

	public SpawnPositionObj[] SpawnPositionArray => m_spawnPositionArray;

	public bool HasSpawnPosition(int index)
	{
		return index < m_spawnPositionArray.Length;
	}

	public Vector3 GetSpawnPosition(int index)
	{
		if (index < m_spawnPositionArray.Length)
		{
			return m_spawnPositionArray[index].gameObject.transform.position;
		}
		Debug.Log("<color=yellow>Spawn Position index: " + index + " out of array bounds.</color>");
		return Vector3.zero;
	}

	public Vector3 GetLocalSpawnPosition(int index)
	{
		if (index < m_spawnPositionArray.Length)
		{
			return m_spawnPositionArray[index].gameObject.transform.localPosition;
		}
		Debug.Log("<color=yellow>Spawn Position index: " + index + " out of array bounds.</color>");
		return Vector3.zero;
	}

	private void Awake()
	{
		SpawnPositionObj[] componentsInChildren = GetComponentsInChildren<SpawnPositionObj>(includeInactive: true);
		m_spawnPositionArray = new SpawnPositionObj[componentsInChildren.Length];
		SpawnPositionObj[] array = componentsInChildren;
		foreach (SpawnPositionObj spawnPositionObj in array)
		{
			int index = spawnPositionObj.Index;
			if (m_spawnPositionArray[index] != null)
			{
				string text = base.name;
				if (!m_warningIssuedTracker.Contains(text))
				{
					m_warningIssuedTracker.Add(text);
					Debug.Log("<color=red>ERROR: Duplicate spawn position object index found. Name: <b>" + text + "</b> - Index: <b>" + index + "</b></color>");
				}
			}
			else
			{
				m_spawnPositionArray[index] = spawnPositionObj;
				Vector3 localPosition = spawnPositionObj.transform.localPosition;
				localPosition.z = 0f;
				spawnPositionObj.transform.localPosition = localPosition;
				spawnPositionObj.gameObject.SetActive(value: false);
			}
		}
	}
}
