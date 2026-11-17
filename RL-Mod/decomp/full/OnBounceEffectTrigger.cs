using System;
using UnityEngine;

public class OnBounceEffectTrigger : BaseEffectTrigger
{
	private IMidpointObj m_midpointObj;

	private BounceCollision m_bounce;

	private Action<GameObject> m_triggerBounceEvent;

	public override bool RequiresCollider => false;

	public override Vector3 Midpoint
	{
		get
		{
			if (m_midpointObj != null)
			{
				return m_midpointObj.Midpoint;
			}
			return base.transform.position;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_midpointObj = m_rootObj.GetComponent<IMidpointObj>();
		m_bounce = m_rootObj.GetComponentInChildren<BounceCollision>();
		if (m_bounce == null)
		{
			Debug.Log("<color=yellow>WARNING: OnBounce Effect Trigger listening on object that has no BounceCollision component.</color>");
		}
		m_triggerBounceEvent = TriggerBounceEvent;
	}

	private void OnEnable()
	{
		if ((bool)m_bounce)
		{
			m_bounce.OnBounceRelay.AddListener(m_triggerBounceEvent);
		}
	}

	private void OnDisable()
	{
		if ((bool)m_bounce)
		{
			m_bounce.OnBounceRelay.RemoveListener(m_triggerBounceEvent);
		}
	}

	private void TriggerBounceEvent(GameObject otherObj)
	{
		GameObject root = otherObj.GetRoot();
		IMidpointObj component = root.GetComponent<BaseCharacterController>();
		Vector3 otherObjMidpos = (component ?? root.GetComponent<IMidpointObj>())?.Midpoint ?? root.transform.position;
		EffectTriggerEntry[] triggerArray = base.TriggerArray;
		foreach (EffectTriggerEntry effectTriggerEntry in triggerArray)
		{
			EffectTriggerDirection effectDirectionFromObject = EffectTrigger.GetEffectDirectionFromObject(root, effectTriggerEntry.TriggerDirection);
			EffectTriggerDirection effectDirectionFromObject2 = EffectTrigger.GetEffectDirectionFromObject(m_rootObj, effectTriggerEntry.TriggerDirection);
			switch (effectTriggerEntry.DeriveFacing)
			{
			case EffectTargetType.None:
				EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, root, Midpoint, otherObjMidpos, EffectTriggerDirection.Anywhere);
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
