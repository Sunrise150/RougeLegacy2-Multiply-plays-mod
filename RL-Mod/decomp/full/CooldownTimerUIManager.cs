using System;
using System.Collections;
using UnityEngine;

public class CooldownTimerUIManager : MonoBehaviour
{
	[SerializeField]
	private CooldownTimerUI m_attackTimer;

	[SerializeField]
	private CooldownTimerUI m_spellTimer;

	[SerializeField]
	private CooldownTimerUI m_talentTimer;

	private Action<MonoBehaviour, EventArgs> m_onChangeAbility;

	private void Awake()
	{
		m_onChangeAbility = OnChangeAbility;
	}

	private IEnumerator Start()
	{
		Debug.LogWarningFormat("<color=blue>{0}: [{1}] is waiting for Player to be instantiated and for Character Class Component to be initialised...</color>", Time.frameCount, this);
		yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		yield return new WaitUntil(() => PlayerManager.GetPlayerController().CharacterClass.IsInitialized);
		Debug.LogWarningFormat("<color=green>{0}: [{1}] finished waiting for Player to be instantiated and for Character Class Component to be initialised.</color>", Time.frameCount, this);
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.ChangeAbility, m_onChangeAbility);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.ChangeAbility, m_onChangeAbility);
	}

	private void OnChangeAbility(MonoBehaviour sender, EventArgs eventArgs)
	{
		InjectCooldown();
	}

	public void InjectCooldown()
	{
		CastAbility_RL component = PlayerManager.GetPlayer().GetComponent<CastAbility_RL>();
		if (component == null)
		{
			throw new MissingComponentException("CastAbility_RL");
		}
		BaseAbility_RL ability = component.GetAbility(CastAbilityType.Weapon);
		if (ability == null)
		{
			Debug.LogWarningFormat("{0}: Unable to find Weapon Ability Type", Time.frameCount);
		}
		m_attackTimer.Cooldown = ability;
		ability = component.GetAbility(CastAbilityType.Spell);
		if (ability == null)
		{
			Debug.LogWarningFormat("{0}: Unable to find Spell Ability Type", Time.frameCount);
		}
		m_spellTimer.Cooldown = ability;
		ability = component.GetAbility(CastAbilityType.Talent);
		if (ability == null)
		{
			Debug.LogWarningFormat("<color=red>{0}: ({1}) Unable to find Talent Ability Type</color>", Time.frameCount, this);
		}
		m_talentTimer.Cooldown = ability;
	}
}
