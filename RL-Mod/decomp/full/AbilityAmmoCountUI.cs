using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AbilityAmmoCountUI : MonoBehaviour
{
	[SerializeField]
	private CastAbilityType m_abilityType;

	[SerializeField]
	private Text m_ammoCountText;

	private Action<MonoBehaviour, EventArgs> m_onAbilityUsed;

	private Action<MonoBehaviour, EventArgs> m_onAbilityCooldownOver;

	private void Awake()
	{
		m_onAbilityCooldownOver = OnAbilityCooldownOver;
		m_onAbilityUsed = OnAbilityUsed;
	}

	private IEnumerator Start()
	{
		Debug.LogWarningFormat("{0}: ({1}) is waiting for Player to be instantiated and for Character Class Component to be initialised...", Time.frameCount, this);
		m_ammoCountText.text = "-";
		while (!PlayerManager.IsInstantiated || !PlayerManager.GetPlayerController().CharacterClass.IsInitialized)
		{
			yield return null;
		}
		Debug.LogWarningFormat("{0}: ({1}) finished waiting for Player to be instantiated and for Character Class Component to be initialised.", Time.frameCount, this);
		if (PlayerManager.GetPlayerController().CastAbility != null)
		{
			IAbility ability = PlayerManager.GetPlayerController().CastAbility.GetAbility(m_abilityType);
			if (ability != null)
			{
				if (ability.MaxAmmo > 0)
				{
					m_ammoCountText.text = ability.AbilityData.MaxAmmo.ToString();
				}
				else
				{
					m_ammoCountText.enabled = false;
				}
			}
			else
			{
				Debug.LogFormat("<color=red>|{0}| Failed to get Ability Type ({1})</color>", this, m_abilityType);
				m_ammoCountText.text = "-";
			}
		}
		else
		{
			Debug.LogFormat("<color=red>|{0}| Character Abilities is null</color>", this);
		}
		switch (m_abilityType)
		{
		case CastAbilityType.Weapon:
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerWeaponAbilityCast, m_onAbilityUsed);
			break;
		case CastAbilityType.Spell:
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerSpellAbilityCast, m_onAbilityUsed);
			break;
		case CastAbilityType.Talent:
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerTalentAbilityCast, m_onAbilityUsed);
			break;
		default:
			throw new ArgumentException("m_abilityType");
		}
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.AbilityCooldownOver, m_onAbilityCooldownOver);
	}

	private void OnDestroy()
	{
		switch (m_abilityType)
		{
		case CastAbilityType.Weapon:
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerWeaponAbilityCast, m_onAbilityUsed);
			break;
		case CastAbilityType.Spell:
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerSpellAbilityCast, m_onAbilityUsed);
			break;
		case CastAbilityType.Talent:
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerTalentAbilityCast, m_onAbilityUsed);
			break;
		default:
			throw new ArgumentException("m_abilityType");
		}
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.AbilityCooldownOver, m_onAbilityCooldownOver);
	}

	private void OnAbilityCooldownOver(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is AbilityCooldownOverEventArgs abilityCooldownOverEventArgs)
		{
			if (abilityCooldownOverEventArgs.Ability != null)
			{
				m_ammoCountText.text = abilityCooldownOverEventArgs.Ability.CurrentAmmo.ToString();
				return;
			}
			Debug.LogFormat("{0}: args.Ability is null", Time.frameCount);
		}
		else
		{
			Debug.LogFormat("<color=red>{0}: Unable to cast EventArgs as AbilityUsedEventArgs</color>", Time.frameCount);
		}
	}

	private void OnAbilityUsed(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is AbilityUsedEventArgs abilityUsedEventArgs)
		{
			if (abilityUsedEventArgs.Ability != null)
			{
				m_ammoCountText.text = abilityUsedEventArgs.Ability.CurrentAmmo.ToString();
				return;
			}
			Debug.LogFormat("{0}: args.Ability is null", Time.frameCount);
		}
		else
		{
			Debug.LogFormat("<color=red>{0}: Unable to cast EventArgs as AbilityUsedEventArgs</color>", Time.frameCount);
		}
	}
}
