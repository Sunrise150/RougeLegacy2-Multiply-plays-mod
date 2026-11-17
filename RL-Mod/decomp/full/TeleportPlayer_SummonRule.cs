using System;
using System.Collections;
using SceneManagement_RL;
using UnityEngine;

[Serializable]
public class TeleportPlayer_SummonRule : BaseSummonRule
{
	[SerializeField]
	private TransitionID m_transitionID = TransitionID.QuickSwipe;

	private WaitUntil m_waitUntilPlayerEnterRoomYield;

	public override SummonRuleType RuleType => SummonRuleType.TeleportPlayer;

	public override Color BoxColor => Color.red;

	public override string RuleLabel => "Teleport Player";

	public override void Initialize(SummonRuleController summonController)
	{
		base.Initialize(summonController);
		m_waitUntilPlayerEnterRoomYield = new WaitUntil(() => PlayerManager.IsInstantiated && (bool)PlayerManager.GetCurrentPlayerRoom());
	}

	public override IEnumerator RunSummonRule()
	{
		yield return m_waitUntilPlayerEnterRoomYield;
		Tunnel tunnelToUse = null;
		TunnelSpawnController tunnelSpawnController = ((base.SerializedObject != null) ? (base.SerializedObject as TunnelSpawnController) : null);
		if ((bool)tunnelSpawnController)
		{
			tunnelToUse = tunnelSpawnController.Tunnel;
		}
		if ((bool)tunnelToUse)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			if (!playerController.IsDead)
			{
				playerController.CastAbility.StopPersistentAbility(CastAbilityType.Weapon);
				playerController.CastAbility.StopPersistentAbility(CastAbilityType.Spell);
				playerController.CastAbility.StopPersistentAbility(CastAbilityType.Talent);
				base.SummonController.StopArena(arenaComplete: true);
				base.IsRuleComplete = true;
				if (m_transitionID != 0)
				{
					SceneLoader_RL.RunTransitionWithLogic(delegate
					{
						TeleportPlayer(tunnelToUse);
					}, m_transitionID, cleanup: false);
				}
				else
				{
					tunnelToUse.ForceEnterTunnel(animate: false);
				}
			}
			else
			{
				Debug.Log("<color=yellow>Cannot execute TeleportPlayer Summon Rule. Player is dead.</color>");
			}
		}
		else
		{
			Debug.Log("<color=yellow>Cannot execute TeleportPlayer Summon Rule. No tunnels were assigned. Skipping rule...</color>");
		}
		base.IsRuleComplete = true;
	}

	private void TeleportPlayer(Tunnel tunnelToUse)
	{
		PlayerManager.GetPlayerController().CharacterHitResponse.StopInvincibleTime();
		tunnelToUse.ForceEnterTunnel(animate: false);
	}
}
