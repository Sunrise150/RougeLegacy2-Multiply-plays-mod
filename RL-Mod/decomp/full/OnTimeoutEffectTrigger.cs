using System;
using UnityEngine;

public class OnTimeoutEffectTrigger : BaseEffectTrigger
{
	private IEffectTriggerEvent_OnTimeout[] m_effectTriggerEventArray;

	private Collider2D m_collider;

	private BaseCharacterController m_charController;

	private Action<GameObject> m_invokeTimeoutTrigger;

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
		if (!m_charController)
		{
			m_collider = GetComponent<Collider2D>();
		}
		m_effectTriggerEventArray = m_rootObj.GetComponentsInChildren<IEffectTriggerEvent_OnTimeout>();
		m_invokeTimeoutTrigger = InvokeTimeoutTrigger;
	}

	private void OnEnable()
	{
		IEffectTriggerEvent_OnTimeout[] effectTriggerEventArray = m_effectTriggerEventArray;
		for (int i = 0; i < effectTriggerEventArray.Length; i++)
		{
			effectTriggerEventArray[i].OnTimeoutEffectTriggerRelay.AddListener(m_invokeTimeoutTrigger);
		}
	}

	private void OnDisable()
	{
		IEffectTriggerEvent_OnTimeout[] effectTriggerEventArray = m_effectTriggerEventArray;
		for (int i = 0; i < effectTriggerEventArray.Length; i++)
		{
			effectTriggerEventArray[i].OnTimeoutEffectTriggerRelay.RemoveListener(m_invokeTimeoutTrigger);
		}
	}

	private void InvokeTimeoutTrigger(GameObject otherObj)
	{
		GameObject root = otherObj.GetRoot();
		IMidpointObj component = root.GetComponent<BaseCharacterController>();
		Vector3 otherObjMidpos = (component ?? root.GetComponent<IMidpointObj>())?.Midpoint ?? root.transform.position;
		EffectTriggerEntry[] triggerArray = base.TriggerArray;
		foreach (EffectTriggerEntry effectTriggerEntry in triggerArray)
		{
			EffectTriggerDirection effectDirectionFromObject = EffectTrigger.GetEffectDirectionFromObject(root, effectTriggerEntry.TriggerDirection);
			EffectTriggerDirection effectDirectionFromObject2 = EffectTrigger.GetEffectDirectionFromObject(m_rootObj, effectTriggerEntry.TriggerDirection);
			bool flag = false;
			if ((effectTriggerEntry.TriggerDirection & EffectTriggerDirection.StandingOn) != 0 && (effectTriggerEntry.TriggerDirection & (effectTriggerEntry.TriggerDirection - 1)) == 0)
			{
				Debug.Log("<color=yellow>Warning: Invalid Effect Trigger Direction(" + EffectTriggerDirection.StandingOn.ToString() + ") for OnTimeoutEffectTrigger. Effect invoking anyway.");
				flag = true;
			}
			else
			{
				flag = (effectTriggerEntry.TriggerDirection & effectDirectionFromObject) != 0 || effectTriggerEntry.TriggerDirection == EffectTriggerDirection.Anywhere;
			}
			if (flag)
			{
				switch (effectTriggerEntry.DeriveFacing)
				{
				case EffectTargetType.None:
					EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, root, Midpoint, otherObjMidpos, EffectTriggerDirection.None);
					break;
				case EffectTargetType.Other:
					EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, root, Midpoint, otherObjMidpos, effectDirectionFromObject);
					break;
				case EffectTargetType.Self:
					EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, root, Midpoint, otherObjMidpos, effectDirectionFromObject2);
					break;
				}
			}
		}
	}
}
