using UnityEngine;

public class GlossaryStatusEffectEntry : GlossaryEntry
{
	[SerializeField]
	private StatusEffectType m_statusEffectType;

	public override void Initialize()
	{
		m_icon.sprite = IconLibrary.GetStatusEffectSprite(StatusEffectType_RL.GetStatusBarType(m_statusEffectType));
		if (StatusEffect_EV.STATUS_EFFECT_TITLE_LOC_IDS.TryGetValue(m_statusEffectType, out var value))
		{
			m_titleText.text = LocalizationManager.GetString(value, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			m_titleText.text = m_statusEffectType.ToString() + " - MISSING TITLE LOCID";
		}
		if (StatusEffect_EV.STATUS_EFFECT_DESC_LOC_IDS.TryGetValue(m_statusEffectType, out var value2))
		{
			m_descriptionText.text = LocalizationManager.GetString(value2, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			m_descriptionText.text = m_statusEffectType.ToString() + " - MISSING DESC LOCID";
		}
	}
}
