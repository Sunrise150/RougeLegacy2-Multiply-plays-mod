using System.Collections;
using UnityEngine;

public abstract class SpecialPlatform : MonoBehaviour, IPlayHitEffect
{
	protected IHitboxController m_hbController;

	public float Width { get; set; }

	public virtual bool PlayDirectionalHitEffect => true;

	public virtual bool PlayHitEffect => true;

	public virtual string EffectNameOverride => null;

	public abstract void SetState(StateID state);

	protected virtual void Awake()
	{
		m_hbController = GetComponentInChildren<IHitboxController>();
	}

	protected virtual IEnumerator Start()
	{
		if (!m_hbController.IsNativeNull())
		{
			while (!m_hbController.IsInitialized)
			{
				yield return null;
			}
			Collider2D collider = m_hbController.GetCollider(HitboxType.Platform);
			if ((bool)collider)
			{
				collider.tag = "TriggerHazard";
			}
		}
	}
}
