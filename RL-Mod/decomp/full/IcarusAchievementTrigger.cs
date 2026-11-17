using UnityEngine;

public class IcarusAchievementTrigger : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if ((collision.CompareTag("Player") || collision.CompareTag("Player_Dodging")) && SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level > 0)
		{
			StoreAPIManager.GiveAchievement(AchievementType.Icarus, StoreType.All);
		}
	}
}
