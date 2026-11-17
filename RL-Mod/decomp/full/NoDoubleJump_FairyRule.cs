using System;
using UnityEngine;

public class NoDoubleJump_FairyRule : FairyRule
{
	private Action<MonoBehaviour, EventArgs> m_onPlayerDoubleJump;

	private bool m_isListeningForDoubleJumpEvent;

	public override string Description => "LOC_ID_FAIRY_RULE_NO_DOUBLE_JUMP_1";

	public override FairyRuleID ID => FairyRuleID.NoDoubleJump;

	public override bool LockChestAtStart => false;

	private void Awake()
	{
		m_onPlayerDoubleJump = OnPlayerDoubleJump;
	}

	private void OnPlayerDoubleJump(MonoBehaviour sender, EventArgs eventArgs)
	{
		SetIsFailed();
	}

	public override void RunRule(FairyRoomController fairyRoomController)
	{
		m_isListeningForDoubleJumpEvent = true;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDoubleJump, m_onPlayerDoubleJump);
	}

	public override void StopRule()
	{
		base.StopRule();
		if (m_isListeningForDoubleJumpEvent)
		{
			m_isListeningForDoubleJumpEvent = false;
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDoubleJump, m_onPlayerDoubleJump);
		}
	}
}
