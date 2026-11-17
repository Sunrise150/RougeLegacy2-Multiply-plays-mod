using Spawn;
using UnityEngine;

public class PropSpawnControllerDataLoader : MonoBehaviour
{
	[SerializeField]
	private PropSpawnControllerData[] m_propSpawnControllerDataArray;

	public PropSpawnControllerData[] PropSpawnControllerDataArray
	{
		get
		{
			return m_propSpawnControllerDataArray;
		}
		set
		{
			m_propSpawnControllerDataArray = value;
		}
	}
}
