using System;
using UnityEngine;

public class OnDamageEffectTrigger : BaseEffectTrigger
{
	private IEffectTriggerEvent_OnDamage[] m_effectTriggerEventArray;

	private Collider2D m_collider;

	private BaseCharacterController m_charController;

	private Action<GameObject, float, bool> m_invokeOnDamageTrigger;

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
		m_effectTriggerEventArray = m_rootObj.GetComponentsInChildren<IEffectTriggerEvent_OnDamage>();
		m_invokeOnDamageTrigger = InvokeOnDamageTrigger;
	}

	private void OnEnable()
	{
		IEffectTriggerEvent_OnDamage[] effectTriggerEventArray = m_effectTriggerEventArray;
		for (int i = 0; i < effectTriggerEventArray.Length; i++)
		{
			effectTriggerEventArray[i].OnDamageEffectTriggerRelay.AddListener(m_invokeOnDamageTrigger);
		}
	}

	private void OnDisable()
	{
		IEffectTriggerEvent_OnDamage[] effectTriggerEventArray = m_effectTriggerEventArray;
		for (int i = 0; i < effectTriggerEventArray.Length; i++)
		{
			effectTriggerEventArray[i].OnDamageEffectTriggerRelay.RemoveListener(m_invokeOnDamageTrigger);
		}
	}

	private void InvokeOnDamageTrigger(GameObject attacker, float damageTaken, bool isCrit)
	{
		GameObject root = attacker.GetRoot();
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
				Debug.Log("<color=yellow>Warning: Invalid Effect Trigger Direction(" + EffectTriggerDirection.StandingOn.ToString() + ") for OnDeathEffectTrigger. Effect invoking anyway.");
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
