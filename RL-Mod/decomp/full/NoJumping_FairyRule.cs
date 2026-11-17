using System;
using UnityEngine;

public class NoJumping_FairyRule : FairyRule
{
	private Action<MonoBehaviour, EventArgs> m_onPlayerJump;

	public override string Description => "LOC_ID_FAIRY_RULE_NO_JUMPING_1";

	public override FairyRuleID ID => FairyRuleID.NoJumping;

	public override bool LockChestAtStart => false;

	private void Awake()
	{
		m_onPlayerJump = OnPlayerJump;
	}

	public override void RunRule(FairyRoomController fairyRoomController)
	{
		if (base.State != FairyRoomState.Failed)
		{
			base.State = FairyRoomState.Running;
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerJump, m_onPlayerJump);
		}
	}

	public override void StopRule()
	{
		base.StopRule();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerJump, m_onPlayerJump);
	}

	private void OnPlayerJump(object sender, EventArgs eventArgs)
	{
		SetIsFailed();
	}
}
