using UnityEngine;

public class OnEventEffectTrigger : BaseEffectTrigger
{
	private BaseCharacterController m_charController;

	public override bool RequiresCollider => false;

	public override Vector3 Midpoint
	{
		get
		{
			if (m_charController != null)
			{
				return m_charController.Midpoint;
			}
			return base.gameObject.transform.position;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_charController = m_rootObj.GetComponent<BaseCharacterController>();
	}

	public void TriggerEffect()
	{
		_ = m_rootObj;
		GameObject rootObj = m_rootObj;
		IMidpointObj component = rootObj.GetComponent<BaseCharacterController>();
		Vector3 otherObjMidpos = (component ?? rootObj.GetComponent<IMidpointObj>())?.Midpoint ?? rootObj.transform.position;
		EffectTriggerEntry[] triggerArray = base.TriggerArray;
		foreach (EffectTriggerEntry effectTriggerEntry in triggerArray)
		{
			EffectTriggerDirection effectDirectionFromObject = EffectTrigger.GetEffectDirectionFromObject(rootObj, effectTriggerEntry.TriggerDirection);
			EffectTriggerDirection effectDirectionFromObject2 = EffectTrigger.GetEffectDirectionFromObject(m_rootObj, effectTriggerEntry.TriggerDirection);
			switch (effectTriggerEntry.DeriveFacing)
			{
			case EffectTargetType.None:
				EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, rootObj, Midpoint, otherObjMidpos, EffectTriggerDirection.Anywhere);
				break;
			case EffectTargetType.Other:
				EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, rootObj, Midpoint, otherObjMidpos, effectDirectionFromObject);
				break;
			case EffectTargetType.Self:
				EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, rootObj, Midpoint, otherObjMidpos, effectDirectionFromObject2);
				break;
			}
		}
	}
}
