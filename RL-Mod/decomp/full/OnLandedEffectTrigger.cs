using System;
using System.Collections;
using UnityEngine;

public class OnLandedEffectTrigger : BaseEffectTrigger
{
	private IEffectTriggerEvent_OnLanded[] m_effectTriggerEventArray;

	private Collider2D m_collider;

	private BaseCharacterController m_charController;

	private CorgiController_RL m_corgiController;

	private Action<CorgiController_RL> m_invokeOnLandedTrigger;

	public override bool RequiresCollider => false;

	public override Vector3 Midpoint
	{
		get
		{
			if (m_charController != null)
			{
				return m_charController.Midpoint;
			}
			if (m_collider != null)
			{
				return m_collider.bounds.center;
			}
			return base.gameObject.transform.position;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_charController = m_rootObj.GetComponent<BaseCharacterController>();
		if (m_charController == null)
		{
			m_collider = GetComponent<Collider2D>();
		}
		m_effectTriggerEventArray = m_rootObj.GetComponentsInChildren<IEffectTriggerEvent_OnLanded>();
		m_corgiController = m_rootObj.GetComponent<CorgiController_RL>();
		m_invokeOnLandedTrigger = InvokeOnLandedTrigger;
	}

	private void OnEnable()
	{
		StartCoroutine(AddTrigger());
	}

	private IEnumerator AddTrigger()
	{
		while (!m_corgiController.IsInitialized)
		{
			yield return null;
		}
		IEffectTriggerEvent_OnLanded[] effectTriggerEventArray = m_effectTriggerEventArray;
		for (int i = 0; i < effectTriggerEventArray.Length; i++)
		{
			effectTriggerEventArray[i].OnLandedEffectTriggerRelay.AddListener(m_invokeOnLandedTrigger);
		}
	}

	private void OnDisable()
	{
		IEffectTriggerEvent_OnLanded[] effectTriggerEventArray = m_effectTriggerEventArray;
		for (int i = 0; i < effectTriggerEventArray.Length; i++)
		{
			effectTriggerEventArray[i].OnLandedEffectTriggerRelay.RemoveListener(m_invokeOnLandedTrigger);
		}
	}

	private void InvokeOnLandedTrigger(CorgiController_RL otherCorgi)
	{
		GameObject gameObject = null;
		BaseCharacterController baseCharacterController = null;
		GameObject gameObject2 = otherCorgi.gameObject;
		if (gameObject2 == null)
		{
			baseCharacterController = PlayerManager.GetPlayerController();
			gameObject = baseCharacterController.gameObject;
		}
		else
		{
			gameObject = gameObject2.GetRoot();
			baseCharacterController = gameObject.GetComponent<BaseCharacterController>();
		}
		IMidpointObj midpointObj = baseCharacterController;
		Vector3 otherObjMidpos = (midpointObj ?? gameObject.GetComponent<IMidpointObj>())?.Midpoint ?? gameObject.transform.position;
		EffectTriggerEntry[] triggerArray = base.TriggerArray;
		foreach (EffectTriggerEntry effectTriggerEntry in triggerArray)
		{
			EffectTriggerDirection effectTriggerDirection = EffectTriggerDirection.StandingOn;
			EffectTriggerDirection otherDirection = EffectTriggerDirection.StandingOn;
			bool flag = false;
			if ((effectTriggerEntry.TriggerDirection & EffectTriggerDirection.StandingOn) != 0 && (effectTriggerEntry.TriggerDirection & (effectTriggerEntry.TriggerDirection - 1)) == 0)
			{
				Debug.Log("<color=yellow>Warning: Invalid Effect Trigger Direction(" + EffectTriggerDirection.StandingOn.ToString() + ") for OnDeathEffectTrigger. Effect invoking anyway.");
				flag = true;
			}
			else
			{
				flag = (effectTriggerEntry.TriggerDirection & effectTriggerDirection) != 0 || effectTriggerEntry.TriggerDirection == EffectTriggerDirection.Anywhere;
			}
			if (flag)
			{
				switch (effectTriggerEntry.DeriveFacing)
				{
				case EffectTargetType.None:
					EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, gameObject, Midpoint, otherObjMidpos, EffectTriggerDirection.None);
					break;
				case EffectTargetType.Other:
					EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, gameObject, Midpoint, otherObjMidpos, effectTriggerDirection);
					break;
				case EffectTargetType.Self:
					EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, gameObject, Midpoint, otherObjMidpos, otherDirection);
					break;
				}
			}
		}
	}
}
