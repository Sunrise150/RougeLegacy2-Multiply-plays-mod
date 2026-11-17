using UnityEngine;

public class OnInteractEffectTrigger : BaseEffectTrigger
{
	private Interactable m_interactableComponent;

	private Collider2D m_collider;

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
		m_interactableComponent = GetComponentInParent<Interactable>();
	}

	private void OnEnable()
	{
		m_interactableComponent.TriggerOnEnterEvent.AddListener(TriggerInteractEvent);
	}

	private void OnDisable()
	{
		m_interactableComponent.TriggerOnEnterEvent.RemoveListener(TriggerInteractEvent);
	}

	private void TriggerInteractEvent(GameObject otherObj)
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
