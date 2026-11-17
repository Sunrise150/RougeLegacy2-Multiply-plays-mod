using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicHUDIconController : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_levelText;

	[SerializeField]
	private TMP_Text m_intValueText;

	[SerializeField]
	private Image m_sprite;

	public RelicType RelicType { get; private set; }

	public void SetRelicType(RelicType relicType)
	{
		if (relicType != 0)
		{
			Sprite relicSprite = IconLibrary.GetRelicSprite(relicType, getLargeIcon: false);
			m_sprite.sprite = relicSprite;
		}
		else
		{
			m_sprite.sprite = null;
		}
		RelicType = relicType;
	}

	public void UpdateRelic()
	{
		if (RelicType == RelicType.None)
		{
			return;
		}
		RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType);
		int level = relic.Level;
		m_levelText.text = level.ToString();
		int relicMaxStack = Relic_EV.GetRelicMaxStack(RelicType, level);
		int intValue = relic.IntValue;
		if (relic.CountsBackwards)
		{
			m_intValueText.text = (relicMaxStack - intValue).ToString();
		}
		else
		{
			m_intValueText.text = intValue.ToString();
		}
		if (relicMaxStack == -1)
		{
			if (m_intValueText.gameObject.activeSelf)
			{
				m_intValueText.gameObject.SetActive(value: false);
			}
			return;
		}
		if (!m_intValueText.gameObject.activeSelf)
		{
			m_intValueText.gameObject.SetActive(value: true);
		}
		if (intValue >= relicMaxStack)
		{
			if (relic.CountsBackwards)
			{
				m_intValueText.color = Color.red;
			}
			else
			{
				m_intValueText.color = Color.yellow;
			}
		}
		else
		{
			m_intValueText.color = Color.white;
		}
	}

	public void PlayPurifyEffect()
	{
		EffectManager.PlayEffect(base.gameObject, null, "RelicPurifiedUI_Effect", Vector3.zero, 0f, EffectStopType.Gracefully).transform.SetParent(base.transform, worldPositionStays: false);
		PlayerController playerController = PlayerManager.GetPlayerController();
		EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "RelicPurified_Effect", playerController.Midpoint, 0f, EffectStopType.Gracefully).transform.SetParent(playerController.transform, worldPositionStays: true);
		TextPopupManager.DisplayTextDefaultPos(TextPopupType.DownstrikeAmmoGain, LocalizationManager.GetString("LOC_ID_RELIC_UI_RELIC_PURIFIED_1", isFemale: false), playerController, attachToSource: true);
	}
}
