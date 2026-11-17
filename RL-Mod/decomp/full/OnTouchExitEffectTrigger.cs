using SceneManagement_RL;
using UnityEngine;

public class OnTouchExitEffectTrigger : BaseEffectTrigger, IStandingOnExit
{
	protected Collider2D m_collider;

	public override bool RequiresCollider => true;

	public override Vector3 Midpoint
	{
		get
		{
			if ((bool)m_collider)
			{
				return m_collider.bounds.center;
			}
			return base.transform.position;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_collider = GetComponent<Collider2D>();
		if (!m_collider)
		{
			Debug.Log("<color=red>ERROR: No Collider2D found on OnTouchExit Effect Trigger.</color>");
		}
		else
		{
			m_collider.isTrigger = true;
		}
		Rigidbody2D component = GetComponent<Rigidbody2D>();
		if ((bool)component)
		{
			Object.Destroy(component);
		}
		base.gameObject.layer = LayerMask.NameToLayer("Prop_Hitbox");
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		StopAllCoroutines();
		if (!SceneLoader_RL.IsLoading)
		{
			HandleCollision(collision);
		}
	}

	private void HandleCollision(Collider2D collision)
	{
		if (CollisionType_RL.IsProjectile(collision.gameObject))
		{
			return;
		}
		GameObject root = collision.GetRoot();
		if (!root.activeInHierarchy || !CDGHelper.DoCollisionTypesCollide_V2(base.CanCollideWith, root))
		{
			return;
		}
		IMidpointObj component = root.GetComponent<BaseCharacterController>();
		Vector3 otherObjMidpos = (component ?? root.GetComponent<IMidpointObj>())?.Midpoint ?? root.transform.position;
		EffectTriggerEntry[] triggerArray = base.TriggerArray;
		foreach (EffectTriggerEntry effectTriggerEntry in triggerArray)
		{
			EffectTriggerDirection effectDirectionFromObject = EffectTrigger.GetEffectDirectionFromObject(root, effectTriggerEntry.TriggerDirection);
			EffectTriggerDirection effectDirectionFromObject2 = EffectTrigger.GetEffectDirectionFromObject(m_rootObj, effectTriggerEntry.TriggerDirection);
			if ((effectTriggerEntry.TriggerDirection & effectDirectionFromObject) != 0 || effectTriggerEntry.TriggerDirection == EffectTriggerDirection.Anywhere)
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

	public void OnStandingExit(GameObject otherRootObj)
	{
		if (!CDGHelper.DoCollisionTypesCollide_V2(base.CanCollideWith, otherRootObj))
		{
			return;
		}
		IMidpointObj component = otherRootObj.GetComponent<BaseCharacterController>();
		Vector3 otherObjMidpos = (component ?? otherRootObj.GetComponent<IMidpointObj>())?.Midpoint ?? otherRootObj.transform.position;
		EffectTriggerEntry[] triggerArray = base.TriggerArray;
		foreach (EffectTriggerEntry effectTriggerEntry in triggerArray)
		{
			if ((effectTriggerEntry.TriggerDirection & EffectTriggerDirection.StandingOn) != 0)
			{
				EffectTrigger.InvokeTrigger(effectTriggerEntry, m_rootObj, otherRootObj, Midpoint, otherObjMidpos, EffectTriggerDirection.StandingOn);
			}
		}
	}
}
