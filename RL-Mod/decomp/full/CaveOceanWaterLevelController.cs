using UnityEngine;

public class CaveOceanWaterLevelController : MonoBehaviour
{
	private PropSpawnController m_propSpawnController;

	private void Awake()
	{
		m_propSpawnController = GetComponent<PropSpawnController>();
		m_propSpawnController.OnPropInstanceInitializedRelay.AddListener(PlaceOcean);
	}

	private void OnDestroy()
	{
		if ((bool)m_propSpawnController)
		{
			m_propSpawnController.OnPropInstanceInitializedRelay.RemoveListener(PlaceOcean);
		}
	}

	private void PlaceOcean()
	{
		int num = 0;
		if (BossID_RL.IsBossBeaten(BossID.Castle_Boss))
		{
			num++;
		}
		if (BossID_RL.IsBossBeaten(BossID.Bridge_Boss))
		{
			num++;
		}
		if (BossID_RL.IsBossBeaten(BossID.Forest_Boss))
		{
			num++;
		}
		if (BossID_RL.IsBossBeaten(BossID.Study_Boss))
		{
			num++;
		}
		if (BossID_RL.IsBossBeaten(BossID.Tower_Boss))
		{
			num++;
		}
		if (BossID_RL.IsBossBeaten(BossID.Cave_Boss))
		{
			num++;
		}
		Vector3 position = m_propSpawnController.PropInstance.transform.position;
		position.y += 2.5f * (float)num;
		m_propSpawnController.PropInstance.transform.position = position;
	}
}
