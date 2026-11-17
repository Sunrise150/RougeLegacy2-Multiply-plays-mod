using UnityEngine;

public abstract class BaseTrait : MonoBehaviour
{
	[SerializeField]
	private TraitData m_traitData;

	[SerializeField]
	private TraitType[] m_incompatibleTraits;

	[SerializeField]
	protected MobilePostProcessOverrideController m_postProcessOverrideController;

	[SerializeField]
	protected SpriteRenderer m_traitMask;

	public bool IsFirstTrait { get; set; }

	public abstract TraitType TraitType { get; }

	public SpriteRenderer TraitMask => m_traitMask;

	public TraitType[] IncompatibleTraits => m_incompatibleTraits;

	public TraitData TraitData => m_traitData;

	public MobilePostProcessOverrideController PostProcessOverrideController => m_postProcessOverrideController;

	public bool IsPaused { get; private set; }

	protected virtual void Awake()
	{
	}

	public virtual void DisableOnDeath()
	{
	}

	public virtual void DisableOnCutscene()
	{
	}

	public virtual void AssignGreenMask()
	{
		if ((bool)m_traitMask)
		{
			m_traitMask.color = Color.green;
		}
	}

	public virtual void SetPaused(bool paused)
	{
		IsPaused = paused;
	}
}
