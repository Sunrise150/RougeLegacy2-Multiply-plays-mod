using System;
using UnityEngine;

public class DontGetHit_FairyRule : FairyRule
{
	private Action<MonoBehaviour, EventArgs> m_onPlayerTakeDamage;

	public override string Description => "LOC_ID_FAIRY_RULE_TAKE_NO_DAMAGE_1";

	public override FairyRuleID ID => FairyRuleID.DontGetHit;

	public override bool LockChestAtStart => false;

	private void Awake()
	{
		m_onPlayerTakeDamage = OnPlayerTakeDamage;
	}

	public override void RunRule(FairyRoomController fairyRoomController)
	{
		if (base.State != FairyRoomState.Failed)
		{
			base.State = FairyRoomState.Running;
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHit, m_onPlayerTakeDamage);
		}
	}

	public override void StopRule()
	{
		base.StopRule();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHit, m_onPlayerTakeDamage);
	}

	protected virtual void OnPlayerTakeDamage(object sender, EventArgs eventArgs)
	{
		SetIsFailed();
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHit, m_onPlayerTakeDamage);
	}
}
