using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Sigtrap.Relays;
using UnityEngine;

public class Sentry_Hazard : Hazard, IPointHazard, IHazard, IRootObj, IHasProjectileNameArray
{
	[SerializeField]
	private SpriteRenderer m_sentryEyeRenderer;

	[SerializeField]
	private SpriteRenderer m_sentryWarningSprite;

	[SerializeField]
	private Animator m_sentryWarningAnimator;

	[SerializeField]
	private Interactable m_interactable;

	public Relay<Sentry_Hazard, bool> SentryRestingRelay = new Relay<Sentry_Hazard, bool>();

	public Relay<bool> PlayerWithinRangeChangeRelay = new Relay<bool>();

	private float m_attackStartTime;

	private float m_radius;

	private bool m_lookingAtPlayer;

	private bool m_isShooting;

	private float m_storedWarningScaleAmount;

	private float m_timeThisBecameEnabled;

	private bool m_isResting;

	private Tween m_restShrinkTween;

	private Tween m_restGrowTween;

	private float m_restDurationOverride = -1f;

	private bool m_previousIsPlayerWithinRange;

	private WaitRL_Yield m_waitYield;

	private Action<MonoBehaviour, EventArgs> m_onAbilityCast;

	[NonSerialized]
	private string[] m_projectileNameArray;

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[1] { "SentryBounceBoltProjectile" };
			}
			return m_projectileNameArray;
		}
	}

	public bool IsResting
	{
		get
		{
			return m_isResting;
		}
		private set
		{
			m_isResting = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_onAbilityCast = OnAbilityCast;
		m_waitYield = new WaitRL_Yield(0f);
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerSpellAbilityCast, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerTalentAbilityCast, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerWeaponAbilityCast, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDownstrikeCast, m_onAbilityCast);
		m_timeThisBecameEnabled = Time.time;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerSpellAbilityCast, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerTalentAbilityCast, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerWeaponAbilityCast, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDownstrikeCast, m_onAbilityCast);
		m_previousIsPlayerWithinRange = false;
	}

	public override void Initialize(HazardArgs hazardArgs)
	{
		base.Initialize(hazardArgs);
		if (hazardArgs is PointHazardArgs pointHazardArgs)
		{
			m_radius = pointHazardArgs.Radius;
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast hazardArgs as PointHazardArgs. If you see this message please bug it on Pivotal.</color>");
		}
		m_sentryWarningSprite.gameObject.transform.localScale = Vector3.one;
		float num = 4f;
		float num2 = m_radius * 2f * 1.75f;
		m_storedWarningScaleAmount = num2 / num;
		Vector3 localScale = new Vector3(m_storedWarningScaleAmount, m_storedWarningScaleAmount, m_storedWarningScaleAmount);
		m_sentryWarningSprite.gameObject.transform.localScale = localScale;
		ResetHazard();
	}

	private void OnAbilityCast(object sender, EventArgs args)
	{
		if (!m_isShooting && !IsResting && (!(args is AbilityUsedEventArgs abilityUsedEventArgs) || !abilityUsedEventArgs.Ability.DealsNoDamage || abilityUsedEventArgs.Ability.AbilityType == AbilityType.PacifistWeapon) && CDGHelper.DistanceBetweenPts(PlayerManager.GetPlayerController().Midpoint, base.transform.localPosition) <= m_radius * 1.75f && Time.time > m_attackStartTime + 0.5f)
		{
			StartCoroutine(FireProjectile());
		}
	}

	private IEnumerator FireProjectile()
	{
		while (Time.time < m_timeThisBecameEnabled + 0.875f)
		{
			yield return null;
		}
		m_isShooting = true;
		base.Animator.Play("Shoot_Tell_Intro");
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		base.Animator.Play("Shoot_Attack_Intro");
		m_attackStartTime = Time.time;
		PlayerController playerController = PlayerManager.GetPlayerController();
		float angleInDeg = CDGHelper.AngleBetweenPts(base.transform.localPosition, playerController.Midpoint);
		ProjectileManager.FireProjectile(base.gameObject, "SentryBounceBoltProjectile", Hazard_EV.SENTRY_PROJECTILE_OFFSET, matchFacing: false, angleInDeg);
		m_waitYield.CreateNew(0.1f);
		yield return m_waitYield;
		base.Animator.Play("Shoot_Exit");
		m_sentryEyeRenderer.color = Color.grey;
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		base.Animator.Play("Idle");
		if (CDGHelper.DistanceBetweenPts(playerController.Midpoint, base.transform.localPosition) <= m_radius * 1.75f)
		{
			m_sentryEyeRenderer.color = Color.red;
		}
		else
		{
			m_sentryEyeRenderer.color = Color.grey;
		}
		m_isShooting = false;
	}

	private void FixedUpdate()
	{
		if (!PlayerManager.IsInstantiated)
		{
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (CDGHelper.DistanceBetweenPts(playerController.Midpoint, base.transform.localPosition) <= m_radius * 1.75f && !m_isResting)
		{
			if (!m_lookingAtPlayer)
			{
				m_sentryEyeRenderer.color = Color.red;
				m_sentryWarningAnimator.SetBool("Aware", value: true);
				BaseAbility_RL ability = playerController.CastAbility.GetAbility(CastAbilityType.Weapon);
				if ((bool)ability && ability.AbilityType == AbilityType.AxeSpinnerWeapon && ability.AbilityActive)
				{
					StartCoroutine(FireProjectile());
				}
			}
			m_lookingAtPlayer = true;
		}
		else if (m_lookingAtPlayer)
		{
			m_sentryWarningAnimator.SetBool("Aware", value: false);
			m_sentryEyeRenderer.color = Color.grey;
			m_lookingAtPlayer = false;
		}
		if (m_previousIsPlayerWithinRange != m_lookingAtPlayer)
		{
			PlayerWithinRangeChangeRelay.Dispatch(m_lookingAtPlayer);
			m_previousIsPlayerWithinRange = m_lookingAtPlayer;
		}
		Vector2 vector = CDGHelper.VectorBetweenPts(base.transform.localPosition, playerController.Midpoint);
		vector.Normalize();
		base.Animator.SetFloat("LookDirectionX", vector.x);
		base.Animator.SetFloat("LookDirectionY", vector.y);
	}

	public void ActivateRestState()
	{
		StopAllCoroutines();
		m_sentryWarningAnimator.SetBool("Aware", value: false);
		base.Animator.Play("Idle");
		m_isShooting = false;
		StartCoroutine(RestCoroutine());
	}

	private IEnumerator RestCoroutine()
	{
		SentryRestingRelay.Dispatch(this, u: true);
		IsResting = true;
		m_interactable.SetIsInteractableActive(isActive: false);
		m_sentryEyeRenderer.color = Color.grey;
		m_restShrinkTween = TweenManager.TweenTo(m_sentryWarningSprite.gameObject.transform, 0.25f, Ease.Quad.EaseInOut, "localScale.x", 0, "localScale.y", 0, "localScale.z", 0);
		yield return m_restShrinkTween.TweenCoroutine;
		float waitTime = 8.5f;
		if (m_restDurationOverride != -1f)
		{
			waitTime = m_restDurationOverride;
		}
		m_waitYield.CreateNew(waitTime);
		yield return m_waitYield;
		base.Animator.SetBool("ReactivationTell", value: true);
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		base.Animator.SetBool("ReactivationTell", value: false);
		m_restGrowTween = TweenManager.TweenTo(m_sentryWarningSprite.gameObject.transform, 0.25f, Ease.Quad.EaseInOut, "localScale.x", m_storedWarningScaleAmount, "localScale.y", m_storedWarningScaleAmount, "localScale.z", m_storedWarningScaleAmount);
		yield return m_restGrowTween.TweenCoroutine;
		m_sentryEyeRenderer.color = Color.grey;
		if (CDGHelper.DistanceBetweenPts(PlayerManager.GetPlayerController().Midpoint, base.transform.localPosition) <= m_radius * 1.75f)
		{
			m_sentryEyeRenderer.color = Color.red;
		}
		else
		{
			m_sentryEyeRenderer.color = Color.grey;
		}
		m_interactable.SetIsInteractableActive(isActive: true);
		SentryRestingRelay.Dispatch(this, u: false);
		IsResting = false;
	}

	public void StartInRestState(float duration)
	{
		StartCoroutine(StartInRestStateCoroutine(duration));
	}

	private IEnumerator StartInRestStateCoroutine(float duration)
	{
		IsResting = true;
		m_interactable.SetIsInteractableActive(isActive: false);
		m_sentryEyeRenderer.color = Color.grey;
		m_sentryWarningSprite.gameObject.transform.localScale = Vector3.zero;
		float waitTime = Mathf.Clamp(duration - 0.5f, 0f, float.MaxValue);
		m_waitYield.CreateNew(waitTime);
		yield return m_waitYield;
		base.Animator.SetBool("ReactivationTell", value: true);
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		base.Animator.SetBool("ReactivationTell", value: false);
		m_restGrowTween = TweenManager.TweenTo(m_sentryWarningSprite.gameObject.transform, 0.25f, Ease.Quad.EaseInOut, "localScale.x", m_storedWarningScaleAmount, "localScale.y", m_storedWarningScaleAmount, "localScale.z", m_storedWarningScaleAmount);
		yield return m_restGrowTween.TweenCoroutine;
		m_sentryEyeRenderer.color = Color.grey;
		if (CDGHelper.DistanceBetweenPts(PlayerManager.GetPlayerController().Midpoint, base.transform.localPosition) <= m_radius * 1.75f)
		{
			m_sentryEyeRenderer.color = Color.red;
		}
		else
		{
			m_sentryEyeRenderer.color = Color.grey;
		}
		m_interactable.SetIsInteractableActive(isActive: true);
		SentryRestingRelay.Dispatch(this, u: false);
		IsResting = false;
	}

	public override void ResetHazard()
	{
		if (m_restShrinkTween != null)
		{
			m_restShrinkTween.StopTweenWithConditionChecks(runEndHandlers: false, m_sentryWarningSprite.gameObject.transform);
		}
		if (m_restGrowTween != null)
		{
			m_restGrowTween.StopTweenWithConditionChecks(runEndHandlers: false, m_sentryWarningSprite.gameObject.transform);
		}
		m_attackStartTime = 0f;
		m_isShooting = false;
		base.Animator.Play("Idle");
		base.Animator.SetBool("ReactivationTell", value: false);
		m_lookingAtPlayer = false;
		m_sentryEyeRenderer.color = Color.grey;
		IsResting = false;
		Vector3 localScale = new Vector3(m_storedWarningScaleAmount, m_storedWarningScaleAmount, m_storedWarningScaleAmount);
		m_sentryWarningSprite.gameObject.transform.localScale = localScale;
		if (SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockMemory) > 0)
		{
			m_interactable.SetIsInteractableActive(isActive: true);
		}
		else
		{
			m_interactable.SetIsInteractableActive(isActive: false);
		}
	}

	public void SetRestDurationOverride(float duration)
	{
		m_restDurationOverride = duration;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
