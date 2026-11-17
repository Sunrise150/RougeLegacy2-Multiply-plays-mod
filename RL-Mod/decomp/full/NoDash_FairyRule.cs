using System;

public class NoDash_FairyRule : FairyRule
{
	private Action m_onPlayerDash;

	public override string Description => "LOC_ID_FAIRY_RULE_NO_DASHING_1";

	public override FairyRuleID ID => FairyRuleID.NoDash;

	public override bool LockChestAtStart => false;

	private void Awake()
	{
		m_onPlayerDash = OnPlayerDash;
	}

	private void OnPlayerDash()
	{
		SetIsFailed();
	}

	public override void RunRule(FairyRoomController fairyRoomController)
	{
		PlayerManager.GetPlayerController().CharacterDash.DashRelay.AddListener(m_onPlayerDash);
	}

	public override void StopRule()
	{
		base.StopRule();
		PlayerManager.GetPlayerController().CharacterDash.DashRelay.RemoveListener(m_onPlayerDash);
	}
}
