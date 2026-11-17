using System;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class NoAttacking_FairyRule : FairyRule
{
	private Action<MonoBehaviour, EventArgs> m_onAbilityCast;

	public override string Description => "LOC_ID_FAIRY_RULE_NO_ATTACKING_1";

	public override FairyRuleID ID => FairyRuleID.NoAttacking;

	public override bool LockChestAtStart => false;

	private void Awake()
	{
		m_onAbilityCast = OnAbilityCast;
	}

	public override void RunRule(FairyRoomController fairyRoomController)
	{
		CharacterDownStrike_RL characterDownStrike = PlayerManager.GetPlayerController().CharacterDownStrike;
		characterDownStrike.OnDownStrikeEvent = (OnDownStrikeDelegate)Delegate.Combine(characterDownStrike.OnDownStrikeEvent, new OnDownStrikeDelegate(base.SetIsFailed));
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerSpellAbilityCast, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerTalentAbilityCast, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerWeaponAbilityCast, m_onAbilityCast);
	}

	private void OnAbilityCast(object sender, EventArgs args)
	{
		if (!(args is AbilityUsedEventArgs abilityUsedEventArgs) || !abilityUsedEventArgs.Ability.DealsNoDamage)
		{
			SetIsFailed();
		}
	}

	public override void StopRule()
	{
		base.StopRule();
		CharacterDownStrike_RL characterDownStrike = PlayerManager.GetPlayerController().CharacterDownStrike;
		characterDownStrike.OnDownStrikeEvent = (OnDownStrikeDelegate)Delegate.Remove(characterDownStrike.OnDownStrikeEvent, new OnDownStrikeDelegate(base.SetIsFailed));
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerSpellAbilityCast, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerTalentAbilityCast, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerWeaponAbilityCast, m_onAbilityCast);
	}
}
