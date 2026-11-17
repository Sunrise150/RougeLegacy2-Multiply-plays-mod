using UnityEngine;

public class HolidayPropReplacementController : MonoBehaviour
{
	[SerializeField]
	private HolidayType m_holidayType;

	[SerializeField]
	[EnumFlag]
	private BiomeLayer m_biomesToReplace;

	[HelpBox("WARNING: The Breakable state of the replacement prop MUST MATCH (i.e. if the replaced prop is breakable then the replacement prop must be breakable as well). This system also DOES NOT support replacing props with Decos on them.", HelpBoxMessageType.Warning)]
	[Space(10f)]
	[SerializeField]
	private Prop[] m_propPrefabReplacementArray;

	private PropSpawnController m_propSpawner;

	private bool m_propsPooled;

	private bool m_propStored;

	private Prop m_storedPropPrefab;

	private void Awake()
	{
		m_propSpawner = GetComponent<PropSpawnController>();
		if (!m_propSpawner)
		{
			Debug.Log("Could not execute HolidayPropReplacementController.");
		}
		else
		{
			m_propSpawner.BeforePropInstanceInitializedRelay.AddListener(BeforePropInitialized);
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_propSpawner)
		{
			m_propSpawner.BeforePropInstanceInitializedRelay.RemoveListener(BeforePropInitialized);
		}
	}

	private void BeforePropInitialized()
	{
		if (!m_propSpawner)
		{
			return;
		}
		if (!m_propStored)
		{
			m_storedPropPrefab = m_propSpawner.PropPrefab;
			m_propStored = true;
		}
		if (HolidayLookController.IsHoliday(m_holidayType))
		{
			if (!BiomeUtility.IsBiomeInBiomeLayerMask(PlayerManager.GetCurrentPlayerRoom().BiomeType, m_biomesToReplace))
			{
				return;
			}
			if (!m_propsPooled)
			{
				Prop[] propPrefabReplacementArray = m_propPrefabReplacementArray;
				for (int i = 0; i < propPrefabReplacementArray.Length; i++)
				{
					PropManager.AddPropToPool(propPrefabReplacementArray[i], 5);
				}
				m_propsPooled = true;
			}
			int num = (int)Mathf.Abs(m_propSpawner.transform.localPosition.x) % m_propPrefabReplacementArray.Length;
			m_propSpawner.ForcePropPrefab(m_propPrefabReplacementArray[num]);
		}
		else
		{
			m_propSpawner.ForcePropPrefab(m_storedPropPrefab);
		}
	}

	private void OnDisable()
	{
		if (!GameManager.IsApplicationClosing && m_propStored)
		{
			m_propSpawner.ForcePropPrefab(m_storedPropPrefab);
		}
	}
}
