using TMPro;
using UnityEngine;

public class SeedPanelController : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_seedText;

	private void OnEnable()
	{
		if (SceneLoadingUtility.ActiveScene.name == SceneLoadingUtility.GetSceneName(SceneID.World))
		{
			int currentSeed = RNGSeedManager.GetCurrentSeed(SceneLoadingUtility.ActiveScene.name);
			m_seedText.text = "Seed: " + RNGSeedManager.GetSeedAsHex(currentSeed) + "-" + BurdenManager.GetBurdenLevel(BurdenType.RoomCount);
		}
		else
		{
			m_seedText.text = string.Empty;
		}
	}
}
