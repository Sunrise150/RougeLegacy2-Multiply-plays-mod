using System;
using TMPro;
using UnityEngine;

public class MapTextUpdater : MonoBehaviour, ILocalizable
{
	[SerializeField]
	private TMP_Text m_text;

	[SerializeField]
	private GameObject m_dangerGO;

	[SerializeField]
	private GameObject m_grayStarsGO;

	[SerializeField]
	private GameObject m_yellowStarsGO;

	[SerializeField]
	private GameObject m_unknownStarsGO;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private void Awake()
	{
		m_refreshText = RefreshText;
	}

	private void OnEnable()
	{
		RefreshText(null, null);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		m_text.text = LocalizationManager.GetString(NowEnteringHUDController.CurrentLocID, isFemale: false);
		int num = NowEnteringHUDController.CurrentRiskLevel;
		bool flag = PlayerManager.IsInstantiated && (bool)PlayerManager.GetCurrentPlayerRoom() && PlayerManager.GetCurrentPlayerRoom().BiomeType == BiomeType.Tutorial;
		EndingSpawnRoomTypeController endingSpawnRoomTypeController = null;
		if (PlayerManager.IsInstantiated && (bool)PlayerManager.GetCurrentPlayerRoom())
		{
			endingSpawnRoomTypeController = PlayerManager.GetCurrentPlayerRoom().GetComponent<EndingSpawnRoomTypeController>();
		}
		if ((bool)endingSpawnRoomTypeController)
		{
			if (endingSpawnRoomTypeController.EndingSpawnRoomType == EndingSpawnRoomType.Docks)
			{
				m_text.text = LocalizationManager.GetString(BiomeDataLibrary.GetData(BiomeType.HubTown).BiomeNameLocID, isFemale: false);
				num = -2;
			}
			else if (endingSpawnRoomTypeController.EndingSpawnRoomType < EndingSpawnRoomType.AboveGround)
			{
				m_text.text = LocalizationManager.GetString("LOC_ID_LOCATION_TITLE_HOME_1", isFemale: false);
				num = 7;
			}
			else if (endingSpawnRoomTypeController.EndingSpawnRoomType == EndingSpawnRoomType.AboveGround)
			{
				m_text.text = LocalizationManager.GetString("LOC_ID_LOCATION_SUBTITLE_HOME_1", isFemale: false);
				num = -2;
			}
		}
		else if (SaveManager.PlayerSaveData.InHubTown)
		{
			m_text.text = LocalizationManager.GetString(BiomeDataLibrary.GetData(BiomeType.HubTown).BiomeNameLocID, isFemale: false);
			num = -2;
		}
		else if (flag)
		{
			m_text.text = "";
			num = -2;
		}
		switch (num)
		{
		case -2:
			m_grayStarsGO.SetActive(value: false);
			m_yellowStarsGO.SetActive(value: false);
			m_unknownStarsGO.SetActive(value: false);
			m_dangerGO.SetActive(value: false);
			return;
		case -1:
			m_grayStarsGO.SetActive(value: false);
			m_yellowStarsGO.SetActive(value: false);
			m_unknownStarsGO.SetActive(value: true);
			m_dangerGO.SetActive(value: true);
			return;
		}
		m_grayStarsGO.SetActive(value: true);
		m_yellowStarsGO.SetActive(value: true);
		m_unknownStarsGO.SetActive(value: false);
		m_dangerGO.SetActive(value: true);
		GameObject gameObject = m_grayStarsGO.transform.GetChild(6).gameObject;
		GameObject gameObject2 = m_yellowStarsGO.transform.GetChild(6).gameObject;
		if (num > 6)
		{
			gameObject.gameObject.SetActive(value: true);
			gameObject2.gameObject.SetActive(value: true);
		}
		else
		{
			gameObject.gameObject.SetActive(value: false);
			gameObject2.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < m_yellowStarsGO.transform.childCount; i++)
		{
			GameObject gameObject3 = m_yellowStarsGO.transform.GetChild(i).gameObject;
			if (i < num)
			{
				gameObject3.SetActive(value: true);
			}
			else
			{
				gameObject3.SetActive(value: false);
			}
		}
	}
}
