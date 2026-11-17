using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class StartArena_SummonRule : BaseSummonRule
{
	[SerializeField]
	private bool m_triggerWithInteractAction;

	private WaitUntil m_waitUntilPlayerEnterRoomYield;

	public override SummonRuleType RuleType => SummonRuleType.StartArena;

	public override string RuleLabel => "Start Arena";

	public override void Initialize(SummonRuleController summonController)
	{
		base.Initialize(summonController);
		m_waitUntilPlayerEnterRoomYield = new WaitUntil(() => PlayerManager.IsInstantiated && PlayerManager.GetCurrentPlayerRoom() != null);
	}

	public override IEnumerator RunSummonRule()
	{
		BoxCollider2D collider = ((base.SerializedObject != null) ? (base.SerializedObject as BoxCollider2D) : null);
		if ((bool)collider)
		{
			yield return m_waitUntilPlayerEnterRoomYield;
			PlayerController playerController = PlayerManager.GetPlayerController();
			bool flag = collider.bounds.Contains(playerController.Midpoint);
			while (!flag || (!Rewired_RL.Player.GetButtonDown("Interact") && m_triggerWithInteractAction))
			{
				if (m_triggerWithInteractAction)
				{
					if (flag)
					{
						if (!playerController.IsInteractIconVisible)
						{
							playerController.IsInteractIconVisible = true;
						}
					}
					else if (playerController.IsInteractIconVisible)
					{
						playerController.IsInteractIconVisible = false;
					}
				}
				yield return null;
				flag = collider.bounds.Contains(playerController.Midpoint);
			}
			playerController.IsInteractIconVisible = false;
		}
		base.SummonController.StartArena();
		base.IsRuleComplete = true;
	}
}
