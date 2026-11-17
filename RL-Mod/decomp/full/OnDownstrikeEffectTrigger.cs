using System;
using UnityEngine;

public class OnDownstrikeEffectTrigger : BaseEffectTrigger
{
	private Action<Projectile_RL, GameObject> m_onPlayerBounce;

	public override bool RequiresCollider => false;

	public override Vector3 Midpoint => base.transform.position;

	protected override void Awake()
	{
		base.Awake();
		m_onPlayerBounce = OnPlayerBounce;
	}

	private void OnEnable()
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController().CharacterDownStrike.OnSuccessfulDownstrikeRelay.AddListener(m_onPlayerBounce);
		}
	}

	private void OnDisable()
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController().CharacterDownStrike.OnSuccessfulDownstrikeRelay.RemoveListener(m_onPlayerBounce);
		}
	}

	private void OnPlayerBounce(Projectile_RL downstrikeProj, GameObject rootCollidedObj)
	{
		if (rootCollidedObj == m_rootObj)
		{
			TriggerBounceEvent(downstrikeProj);
		}
	}

	private void TriggerBounceEvent(Projectile_RL downstrikeProj)
	{
		Vector3 vector = downstrikeProj.Midpoint;
		if ((bool)downstrikeProj.HitboxController.LastCollidedWith)
		{
			vector = downstrikeProj.HitboxController.LastCollidedWith.ClosestPoint(downstrikeProj.Midpoint);
		}
		GameObject gameObject = downstrikeProj.gameObject;
		EffectTriggerEntry[] triggerArray = base.TriggerArray;
		foreach (EffectTriggerEntry effectTriggerEntry in triggerArray)
		{
			EffectTriggerDirection effectDirectionFromObject = EffectTrigger.GetEffectDirectionFromObject(gameObject, effectTriggerEntry.TriggerDirection);
			EffectTriggerDirection effectDirectionFromObject2 = EffectTrigger.GetEffectDirectionFromObject(m_rootObj, effectTriggerEntry.TriggerDirection);
			effectTriggerEntry.OffsetType = EffectOffsetType.Midpoint;
			switch (effectTriggerEntry.DeriveFacing)
			{
			case EffectTargetType.None:
				EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, gameObject, vector, vector, EffectTriggerDirection.Anywhere);
				break;
			case EffectTargetType.Other:
				EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, gameObject, vector, vector, effectDirectionFromObject);
				break;
			case EffectTargetType.Self:
				EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, gameObject, vector, vector, effectDirectionFromObject2);
				break;
			}
		}
	}
}
