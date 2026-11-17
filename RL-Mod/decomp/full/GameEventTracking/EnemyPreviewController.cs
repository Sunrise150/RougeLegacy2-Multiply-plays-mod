using UnityEngine;
using UnityEngine.UI;

namespace GameEventTracking;

public class EnemyPreviewController : MonoBehaviour
{
	[SerializeField]
	private Image m_image;

	public GameObject GameObject => base.gameObject;

	public void Initialise(EnemyTrackerData enemyData)
	{
		m_image.sprite = AssetPreviewManager.GetPreviewImage(enemyData.EnemyType, enemyData.EnemyRank);
	}
}
