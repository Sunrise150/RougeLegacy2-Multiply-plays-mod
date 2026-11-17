using System;
using UnityEngine;
using UnityEngine.UI;

public class CooldownIndicatorAbilityName : MonoBehaviour
{
	[SerializeField]
	private CastAbilityType m_abilityCategory;

	[SerializeField]
	private Text m_text;

	private Action<MonoBehaviour, EventArgs> m_onChangeAbility;

	private void Awake()
	{
		m_onChangeAbility = OnChangeAbility;
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.ChangeAbility, m_onChangeAbility);
		m_text.text = "NONE";
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.ChangeAbility, m_onChangeAbility);
	}

	private void OnChangeAbility(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is ChangeAbilityEventArgs changeAbilityEventArgs && changeAbilityEventArgs.CastAbilityType == m_abilityCategory)
		{
			m_text.text = changeAbilityEventArgs.Ability.AbilityType.ToString();
		}
	}
}
