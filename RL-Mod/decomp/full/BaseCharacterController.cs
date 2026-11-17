using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MoreMountains.CorgiEngine;
using Sigtrap.Relays;
using UnityEngine;

public abstract class BaseCharacterController : MonoBehaviour, IHealth, IDefenseObj, IRootObj, IMidpointObj, IEffectVelocity, IPreOnDisable, IPivotObj, IEffectTriggerEvent_OnDeath, IHeading
{
	private float m_baseScale;

	private float m_baseStrength;

	private float m_baseKnockbackDefense;

	private float m_baseStunDefense;

	private float m_baseMagic;

	private float m_baseDamage;

	protected bool m_disableFriction;

	[SerializeField]
	[HideInInspector]
	private float m_fallMultiplierOverride = 1f;

	[SerializeField]
	[HideInInspector]
	private float m_ascentMultiplierOverride = 1f;

	[SerializeField]
	private GameObject m_visuals;

	[SerializeField]
	private bool m_takesNoDamage;

	private static MaterialPropertyBlock m_charMaterialPropertyBlock_STATIC;

	protected CorgiController_RL m_controllerCorgi;

	protected BaseCharacterHitResponse m_characterHitResponse;

	protected Character m_characterCorgi;

	protected SpawnPositionController m_spawnPositionController;

	protected StatusEffectController m_statusEffectController;

	protected StatusBarController m_statusBarController;

	protected IHitboxController m_hitboxController;

	protected Animator m_animator;

	private Vector2 m_heading = new Vector2(1f, 0f);

	private float m_orientation;

	private bool m_knockedIntoAir;

	private float m_knockedIntoAirStartTime;

	private bool m_wasGroundedLastFixedUpdate;

	private Vector2 m_internalKnockbackMod = Vector2.one;

	private Vector2 m_externalKnockbackMod = Vector2.one;

	protected HealthChangeEventArgs m_healthChangeEventArgs;

	private bool m_useCachedVisualBounds;

	private Bounds_RL m_cachedVisualBounds;

	protected Relay<GameObject> m_onDeathEffectTriggerRelay = new Relay<GameObject>();

	protected Relay<IPreOnDisable> m_onPreDisableRelay = new Relay<IPreOnDisable>();

	private Relay<object, HealthChangeEventArgs> m_healthChangeRelay = new Relay<object, HealthChangeEventArgs>();

	private Relay<object, HealthChangeEventArgs> m_maxHealthChangeRelay = new Relay<object, HealthChangeEventArgs>();

	protected Vector2 m_speedHolder;

	public IRelayLink<GameObject> OnDeathEffectTriggerRelay => m_onDeathEffectTriggerRelay.link;

	public BlinkPulseEffect BlinkPulseEffect => m_characterHitResponse.BlinkPulseEffect;

	public List<Renderer> RendererArray { get; protected set; }

	public List<RendererArrayEntry> RendererArrayDefaultTint { get; protected set; }

	public bool UseOverrideDefaultTint { get; set; }

	public Color DefaultTintOverrideColor { get; set; }

	public IRelayLink<IPreOnDisable> PreOnDisableRelay => m_onPreDisableRelay.link;

	public Rect CollisionBounds => m_controllerCorgi.AbsBounds;

	public Bounds VisualBounds => VisualBoundsObj.Bounds;

	public Bounds_RL VisualBoundsObj
	{
		get
		{
			if (!m_useCachedVisualBounds)
			{
				m_useCachedVisualBounds = true;
				m_cachedVisualBounds = GetComponent<Bounds_RL>();
			}
			return m_cachedVisualBounds;
		}
	}

	public abstract float BaseScaleToOffsetWith { get; }

	public bool TakesNoDamage
	{
		get
		{
			return m_takesNoDamage;
		}
		set
		{
			m_takesNoDamage = value;
		}
	}

	public IRelayLink<object, HealthChangeEventArgs> HealthChangeRelay => m_healthChangeRelay.link;

	public IRelayLink<object, HealthChangeEventArgs> MaxHealthChangeRelay => m_maxHealthChangeRelay.link;

	public GameObject Pivot => m_characterCorgi.CharacterModel;

	public bool IsInvincible => m_characterHitResponse.IsInvincible;

	public float InvincibilityTimer => m_characterHitResponse.InvincibleTimer;

	public float BaseInvincibilityDuration { get; set; }

	public float ActualInvincibilityDuration => BaseInvincibilityDuration + InvincibilityDurationAdd;

	public float InvincibilityDurationAdd { get; set; }

	public float StunDuration { get; set; }

	public Vector2 EffectVelocity => Velocity;

	public GameObject Visuals
	{
		get
		{
			return m_visuals;
		}
		set
		{
			m_visuals = value;
		}
	}

	public bool LockFlip
	{
		get
		{
			return m_characterCorgi.LockFlip;
		}
		set
		{
			m_characterCorgi.LockFlip = value;
		}
	}

	public float FallMultiplierOverride
	{
		get
		{
			return m_fallMultiplierOverride;
		}
		set
		{
			m_fallMultiplierOverride = value;
			if ((bool)m_controllerCorgi)
			{
				m_controllerCorgi.Parameters.FallMultiplier = value;
			}
		}
	}

	public float AscentMultiplierOverride
	{
		get
		{
			return m_ascentMultiplierOverride;
		}
		set
		{
			m_ascentMultiplierOverride = value;
			if ((bool)m_controllerCorgi)
			{
				m_controllerCorgi.Parameters.AscentMultiplier = value;
			}
		}
	}

	public Vector3 Midpoint
	{
		get
		{
			if (IsInitialized)
			{
				return m_controllerCorgi.BoundsCenter;
			}
			return base.transform.position;
		}
	}

	public Vector2 Heading
	{
		get
		{
			return m_heading;
		}
		set
		{
			if (m_heading != value)
			{
				m_heading = value;
				m_heading.Normalize();
				m_orientation = Mathf.Atan2(m_heading.y, m_heading.x);
			}
		}
	}

	public float HeadingX
	{
		get
		{
			return m_heading.x;
		}
		set
		{
			if (m_heading.x != value)
			{
				m_heading.x = value;
				m_heading.Normalize();
				m_orientation = Mathf.Atan2(m_heading.y, m_heading.x);
			}
		}
	}

	public float HeadingY
	{
		get
		{
			return m_heading.y;
		}
		set
		{
			if (m_heading.y != value)
			{
				m_heading.y = value;
				m_heading.Normalize();
				m_orientation = Mathf.Atan2(m_heading.y, m_heading.x);
			}
		}
	}

	public float Orientation
	{
		get
		{
			return m_orientation;
		}
		set
		{
			if (m_orientation != value)
			{
				m_orientation = value;
				m_heading.x = Mathf.Cos(m_orientation);
				m_heading.y = Mathf.Sin(m_orientation);
			}
		}
	}

	public virtual bool DisableFriction
	{
		get
		{
			return m_disableFriction;
		}
		set
		{
			m_disableFriction = value;
		}
	}

	public float BaseScale
	{
		get
		{
			return m_baseScale;
		}
		set
		{
			m_baseScale = value;
		}
	}

	public virtual float ActualScale => BaseScale;

	public virtual Vector2 InternalKnockbackMod
	{
		get
		{
			return m_internalKnockbackMod;
		}
		set
		{
			m_internalKnockbackMod.x = value.x;
			m_internalKnockbackMod.y = value.y;
		}
	}

	public virtual Vector2 ExternalKnockbackMod
	{
		get
		{
			return m_externalKnockbackMod;
		}
		set
		{
			m_externalKnockbackMod.x = value.x;
			m_externalKnockbackMod.y = value.y;
		}
	}

	public virtual float BaseKnockbackDefense
	{
		get
		{
			return m_baseKnockbackDefense;
		}
		set
		{
			m_baseKnockbackDefense = value;
		}
	}

	public virtual float ActualKnockbackDefense => BaseKnockbackDefense;

	public virtual float BaseStunDefense
	{
		get
		{
			return m_baseStunDefense;
		}
		set
		{
			m_baseStunDefense = value;
		}
	}

	public virtual float ActualStunDefense => BaseStunDefense;

	public float BaseDamage
	{
		get
		{
			m_baseDamage = BaseStrength + BaseMagic;
			return m_baseDamage;
		}
	}

	public virtual float ActualDamage => Mathf.Clamp(ActualStrength + ActualMagic, 0f, float.MaxValue);

	public virtual float ActualCritChance => 0f;

	public virtual float ActualCritDamage => 0f;

	public float BaseStrength
	{
		get
		{
			return m_baseStrength;
		}
		set
		{
			m_baseStrength = Mathf.Clamp(value, 0f, float.MaxValue);
		}
	}

	public float StrengthMod { get; protected set; }

	public float StrengthTemporaryMod { get; protected set; }

	public int StrengthAdd { get; protected set; }

	public int StrengthTemporaryAdd { get; protected set; }

	public virtual float ActualStrength => Mathf.Clamp((BaseStrength + (float)StrengthAdd + (float)StrengthTemporaryAdd) * (1f + StrengthMod + StrengthTemporaryMod), 0f, float.MaxValue);

	public float BaseMagic
	{
		get
		{
			return m_baseMagic;
		}
		set
		{
			m_baseMagic = Mathf.Clamp(value, 0f, float.MaxValue);
		}
	}

	public float MagicMod { get; protected set; }

	public float MagicTemporaryMod { get; protected set; }

	public int MagicAdd { get; protected set; }

	public int MagicTemporaryAdd { get; protected set; }

	public virtual float ActualMagic => Mathf.Clamp((BaseMagic + (float)MagicAdd + (float)MagicTemporaryAdd) * (1f + MagicMod + MagicTemporaryMod), 0f, float.MaxValue);

	public virtual int BaseMaxHealth { get; protected set; }

	public int MaxHealthAdd { get; protected set; }

	public int MaxHealthTemporaryAdd { get; protected set; }

	public float MaxHealthMod { get; protected set; }

	public float MaxHealthTemporaryMod { get; protected set; }

	public abstract int ActualMaxHealth { get; }

	public virtual float CurrentHealth { get; protected set; }

	public int CurrentHealthAsInt => Mathf.CeilToInt(CurrentHealth);

	public bool KnockedIntoAir
	{
		get
		{
			return m_knockedIntoAir;
		}
		set
		{
			m_knockedIntoAir = value;
			if (m_knockedIntoAir)
			{
				m_knockedIntoAirStartTime = Time.time;
			}
		}
	}

	public Vector2 Velocity => m_controllerCorgi.Velocity;

	public bool IsFalling => m_controllerCorgi.State.IsFalling;

	public bool IsGrounded
	{
		get
		{
			if (m_controllerCorgi.State.IsGrounded)
			{
				return CurrentHealth > 0f;
			}
			return false;
		}
	}

	public bool IsFacingRight => m_characterCorgi.IsFacingRight;

	public CharacterStates.MovementStates MovementState
	{
		get
		{
			return m_characterCorgi.MovementState.CurrentState;
		}
		set
		{
			m_characterCorgi.MovementState.ChangeState(value);
		}
	}

	public CharacterStates.MovementStates PreviousMovementState => m_characterCorgi.MovementState.PreviousState;

	public CharacterStates.CharacterConditions ConditionState
	{
		get
		{
			return m_characterCorgi.ConditionState.CurrentState;
		}
		set
		{
			m_characterCorgi.ConditionState.ChangeState(value);
		}
	}

	[Obsolete("This is a SUPER dangerous call. Better to set the state to what you actually want")]
	public CharacterStates.CharacterConditions PreviousConditionState => m_characterCorgi.ConditionState.PreviousState;

	public IHitboxController HitboxController => m_hitboxController;

	public SpawnPositionController SpawnPositionController => m_spawnPositionController;

	public BaseCharacterHitResponse CharacterHitResponse => m_characterHitResponse;

	public CorgiController_RL ControllerCorgi => m_controllerCorgi;

	public Character CharacterCorgi => m_characterCorgi;

	public Animator Animator => m_animator;

	public StatusEffectController StatusEffectController => m_statusEffectController;

	public StatusBarController StatusBarController => m_statusBarController;

	public bool IsDead { get; protected set; }

	public bool IsInitialized { get; protected set; }

	public virtual void SetHealth(float value, bool additive, bool runEvents)
	{
		if (CurrentHealth > (float)ActualMaxHealth)
		{
			CurrentHealth = ActualMaxHealth;
		}
		float currentHealth = CurrentHealth;
		float num = value;
		if (additive)
		{
			num = CurrentHealth + value;
		}
		CurrentHealth = Mathf.Clamp(num, 0f, ActualMaxHealth);
		if (runEvents)
		{
			m_healthChangeEventArgs.Initialise(this, num, currentHealth);
			m_healthChangeRelay.Dispatch(this, m_healthChangeEventArgs);
		}
	}

	public virtual void SetVelocity(float velocityX, float velocityY, bool additive)
	{
		m_speedHolder.x = velocityX;
		m_speedHolder.y = velocityY;
		if (additive)
		{
			m_controllerCorgi.AddForce(m_speedHolder);
		}
		else
		{
			m_controllerCorgi.SetForce(m_speedHolder);
		}
	}

	public virtual void SetVelocityX(float velocity, bool additive)
	{
		m_speedHolder.x = velocity;
		if (additive)
		{
			m_controllerCorgi.AddHorizontalForce(velocity);
		}
		else
		{
			m_controllerCorgi.SetHorizontalForce(velocity);
		}
	}

	public virtual void SetVelocityY(float velocity, bool additive)
	{
		m_speedHolder.y = velocity;
		if (additive)
		{
			m_controllerCorgi.AddVerticalForce(velocity);
		}
		else
		{
			m_controllerCorgi.SetVerticalForce(velocity);
		}
	}

	public void SetFacing(bool faceRight)
	{
		if (faceRight)
		{
			if (!IsFacingRight)
			{
				m_characterCorgi.Flip();
			}
		}
		else if (IsFacingRight)
		{
			m_characterCorgi.Flip();
		}
	}

	[Obsolete("This is a SUPER dangerous call. Better to set the state to what you actually want")]
	public void RestorePreviousMovementState()
	{
		m_characterCorgi.MovementState.RestorePreviousState();
	}

	public void RestorePreviousCharacterCondition()
	{
		m_characterCorgi.ConditionState.RestorePreviousState();
	}

	public void DisableGroundedState()
	{
		m_controllerCorgi.State.IsCollidingBelow = false;
		m_controllerCorgi.State.JustGotGrounded = false;
	}

	protected virtual void Awake()
	{
		if (m_charMaterialPropertyBlock_STATIC == null)
		{
			m_charMaterialPropertyBlock_STATIC = new MaterialPropertyBlock();
		}
		RendererArray = new List<Renderer>();
		RendererArrayDefaultTint = new List<RendererArrayEntry>();
		RecreateRendererArray();
		m_controllerCorgi = base.gameObject.GetComponent<CorgiController_RL>();
		m_characterCorgi = base.gameObject.GetComponent<Character>();
		m_characterHitResponse = base.gameObject.GetComponent<BaseCharacterHitResponse>();
		m_hitboxController = base.gameObject.GetComponentInChildren<IHitboxController>();
		m_animator = m_characterCorgi.CharacterAnimator;
		m_spawnPositionController = base.gameObject.GetComponent<SpawnPositionController>();
		m_statusEffectController = base.gameObject.GetComponentInChildren<StatusEffectController>();
		m_statusEffectController.Initialize(this);
		m_statusBarController = base.gameObject.GetComponentInChildren<StatusBarController>();
		m_healthChangeEventArgs = new HealthChangeEventArgs(this, 0f, 0f);
	}

	public void RecreateRendererArray()
	{
		if (m_charMaterialPropertyBlock_STATIC == null)
		{
			m_charMaterialPropertyBlock_STATIC = new MaterialPropertyBlock();
		}
		for (int i = 0; i < RendererArray.Count; i++)
		{
			if ((bool)RendererArray[i])
			{
				Renderer renderer = RendererArray[i];
				renderer.GetPropertyBlock(m_charMaterialPropertyBlock_STATIC);
				RendererArrayEntry rendererArrayEntry = RendererArrayDefaultTint[i];
				if (rendererArrayEntry.HasAlphaColor)
				{
					m_charMaterialPropertyBlock_STATIC.SetColor(ShaderID_RL._AlphaBlendColor, rendererArrayEntry.DefaultAlphaColor);
				}
				if (rendererArrayEntry.HasRimColor)
				{
					m_charMaterialPropertyBlock_STATIC.SetColor(ShaderID_RL._RimLightColor, rendererArrayEntry.DefaultRimLightColor);
				}
				if (rendererArrayEntry.HasMultiplyColor)
				{
					m_charMaterialPropertyBlock_STATIC.SetColor(ShaderID_RL._MultiplyColor, rendererArrayEntry.DefaultMultiplyColor);
				}
				if (rendererArrayEntry.HasAddColor)
				{
					m_charMaterialPropertyBlock_STATIC.SetColor(ShaderID_RL._AddColor, rendererArrayEntry.DefaultAddColor);
				}
				if (rendererArrayEntry.HasRimBias)
				{
					m_charMaterialPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimBias, rendererArrayEntry.DefaultRimBias);
				}
				if (rendererArrayEntry.HasRimScale)
				{
					m_charMaterialPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimScale, rendererArrayEntry.DefaultRimScale);
				}
				renderer.SetPropertyBlock(m_charMaterialPropertyBlock_STATIC);
			}
		}
		m_charMaterialPropertyBlock_STATIC.Clear();
		RendererArray.Clear();
		RendererArrayDefaultTint.Clear();
		Visuals.GetComponentsInChildren(includeInactive: true, RendererArray);
		for (int j = 0; j < RendererArray.Count; j++)
		{
			Renderer renderer2 = RendererArray[j];
			RendererArrayEntry item = default(RendererArrayEntry);
			Material sharedMaterial = renderer2.sharedMaterial;
			if ((bool)sharedMaterial)
			{
				item.HasAlphaColor = sharedMaterial.HasProperty(ShaderID_RL._AlphaBlendColor);
				item.HasRimColor = sharedMaterial.HasProperty(ShaderID_RL._RimLightColor);
				item.HasMultiplyColor = sharedMaterial.HasProperty(ShaderID_RL._MultiplyColor);
				item.HasAddColor = sharedMaterial.HasProperty(ShaderID_RL._AddColor);
				item.HasRimBias = sharedMaterial.HasProperty(ShaderID_RL._RimBias);
				item.HasRimScale = sharedMaterial.HasProperty(ShaderID_RL._RimScale);
				if (item.HasAlphaColor)
				{
					item.DefaultAlphaColor = sharedMaterial.GetColor(ShaderID_RL._AlphaBlendColor);
				}
				if (item.HasRimColor)
				{
					item.DefaultRimLightColor = sharedMaterial.GetColor(ShaderID_RL._RimLightColor);
				}
				if (item.HasMultiplyColor)
				{
					item.DefaultMultiplyColor = sharedMaterial.GetColor(ShaderID_RL._MultiplyColor);
				}
				if (item.HasAddColor)
				{
					item.DefaultAddColor = sharedMaterial.GetColor(ShaderID_RL._AddColor);
				}
				if (item.HasRimBias)
				{
					item.DefaultRimBias = sharedMaterial.GetFloat(ShaderID_RL._RimBias);
				}
				if (item.HasRimScale)
				{
					item.DefaultRimScale = sharedMaterial.GetFloat(ShaderID_RL._RimScale);
				}
			}
			RendererArrayDefaultTint.Add(item);
		}
	}

	public void ResetRendererArrayColor()
	{
		UseOverrideDefaultTint = false;
		if (UseOverrideDefaultTint)
		{
			for (int i = 0; i < RendererArray.Count; i++)
			{
				Renderer renderer = RendererArray[i];
				if (!renderer || m_charMaterialPropertyBlock_STATIC == null)
				{
					PrintOutMatError(renderer, i);
				}
				if ((bool)renderer.sharedMaterial)
				{
					renderer.GetPropertyBlock(m_charMaterialPropertyBlock_STATIC);
					RendererArrayEntry rendererArrayEntry = RendererArrayDefaultTint[i];
					if (rendererArrayEntry.HasAlphaColor)
					{
						m_charMaterialPropertyBlock_STATIC.SetColor(ShaderID_RL._AlphaBlendColor, DefaultTintOverrideColor);
					}
					if (rendererArrayEntry.HasRimColor)
					{
						m_charMaterialPropertyBlock_STATIC.SetColor(ShaderID_RL._RimLightColor, DefaultTintOverrideColor);
					}
					if (rendererArrayEntry.HasRimBias)
					{
						m_charMaterialPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimBias, 0f);
					}
					if (rendererArrayEntry.HasRimScale)
					{
						m_charMaterialPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimLightColor, 0f);
					}
					renderer.SetPropertyBlock(m_charMaterialPropertyBlock_STATIC);
				}
			}
			return;
		}
		for (int j = 0; j < RendererArray.Count; j++)
		{
			Renderer renderer2 = RendererArray[j];
			if (!renderer2 || m_charMaterialPropertyBlock_STATIC == null)
			{
				PrintOutMatError(renderer2, j);
			}
			if ((bool)renderer2.sharedMaterial)
			{
				renderer2.GetPropertyBlock(m_charMaterialPropertyBlock_STATIC);
				RendererArrayEntry rendererArrayEntry2 = RendererArrayDefaultTint[j];
				if (rendererArrayEntry2.HasAlphaColor)
				{
					m_charMaterialPropertyBlock_STATIC.SetColor(ShaderID_RL._AlphaBlendColor, rendererArrayEntry2.DefaultAlphaColor);
				}
				if (rendererArrayEntry2.HasRimColor)
				{
					m_charMaterialPropertyBlock_STATIC.SetColor(ShaderID_RL._RimLightColor, rendererArrayEntry2.DefaultRimLightColor);
				}
				if (rendererArrayEntry2.HasMultiplyColor)
				{
					m_charMaterialPropertyBlock_STATIC.SetColor(ShaderID_RL._MultiplyColor, rendererArrayEntry2.DefaultMultiplyColor);
				}
				if (rendererArrayEntry2.HasAddColor)
				{
					m_charMaterialPropertyBlock_STATIC.SetColor(ShaderID_RL._AddColor, rendererArrayEntry2.DefaultAddColor);
				}
				if (rendererArrayEntry2.HasRimBias)
				{
					m_charMaterialPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimBias, rendererArrayEntry2.DefaultRimBias);
				}
				if (rendererArrayEntry2.HasRimScale)
				{
					m_charMaterialPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimScale, rendererArrayEntry2.DefaultRimScale);
				}
				renderer2.SetPropertyBlock(m_charMaterialPropertyBlock_STATIC);
			}
		}
	}

	private void PrintOutMatError(Renderer renderer, int rendererIndex)
	{
		Debug.Log("MAT ERROR!");
		Debug.Log("ObjectName: " + base.name);
		Debug.Log("RENDERER NULL? " + !renderer);
		Debug.Log("MAT BLOCK NULL? " + m_charMaterialPropertyBlock_STATIC == null);
		if (!renderer)
		{
			Debug.Log("Renderer null on index: " + rendererIndex);
			Debug.Log("Renderer Array size: " + RendererArray.Count);
		}
		Debug.Log("Crashed on Player Info: ");
		Debug.Log("Class Type: " + SaveManager.PlayerSaveData.CurrentCharacter.ClassType);
		Debug.Log("Traits 1: " + SaveManager.PlayerSaveData.CurrentCharacter.TraitOne);
		Debug.Log("Traits 2: " + SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo);
		Debug.Log("Equipped Weapon: " + SaveManager.PlayerSaveData.CurrentCharacter.EdgeEquipmentType);
		Debug.Log("Equipped Chest: " + SaveManager.PlayerSaveData.CurrentCharacter.EdgeEquipmentType);
		Debug.Log("Equipped Helm: " + SaveManager.PlayerSaveData.CurrentCharacter.EdgeEquipmentType);
		Debug.Log("Equipped Cape: " + SaveManager.PlayerSaveData.CurrentCharacter.EdgeEquipmentType);
		Debug.Log("Equipped Trinket: " + SaveManager.PlayerSaveData.CurrentCharacter.EdgeEquipmentType);
	}

	protected virtual IEnumerator Start()
	{
		while (!m_controllerCorgi.IsInitialized || !m_characterCorgi.IsInitialized)
		{
			yield return null;
		}
		ResetCharacter();
		IsInitialized = true;
	}

	protected virtual void FixedUpdate()
	{
		if (!IsInitialized || !m_controllerCorgi)
		{
			return;
		}
		bool flag = false;
		if (IsGrounded && KnockedIntoAir && m_knockedIntoAirStartTime + 0.1f < Time.time)
		{
			KnockedIntoAir = false;
			flag = true;
		}
		int num;
		if (IsGrounded)
		{
			num = ((!m_wasGroundedLastFixedUpdate) ? 1 : 0);
			if (num != 0)
			{
				OnJustGrounded();
			}
		}
		else
		{
			num = 0;
		}
		if ((num != 0 || flag) && Velocity.x != 0f && (Velocity.y <= 0f || (Velocity.y > 0f && IsGrounded && m_controllerCorgi.State.BelowSlopeAngle != 0f)) && !DisableFriction && m_knockedIntoAirStartTime + 0.1f < Time.time)
		{
			SetVelocityX(0f, additive: false);
		}
		m_wasGroundedLastFixedUpdate = IsGrounded;
	}

	protected virtual void OnJustGrounded()
	{
	}

	public abstract float CalculateDamageTaken(IDamageObj damageObj, out CriticalStrikeType critType, out float damageBlocked, float damageOverride = -1f, bool trueDamage = false, bool pureCalculation = true);

	public virtual void KillCharacter(GameObject killer, bool broadcastEvent)
	{
		IsDead = true;
		StatusEffectController.StopAllStatusEffects(interrupted: false);
		ConditionState = CharacterStates.CharacterConditions.Dead;
	}

	public virtual void ResetBaseValues()
	{
		if (IsInitialized)
		{
			AscentMultiplierOverride = m_ascentMultiplierOverride;
			FallMultiplierOverride = m_fallMultiplierOverride;
		}
		ExternalKnockbackMod = Vector2.one;
		InternalKnockbackMod = Vector2.one;
	}

	public virtual void ResetStates()
	{
		if ((bool)m_characterCorgi && m_characterCorgi.IsInitialized)
		{
			ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		LockFlip = false;
		if (m_characterHitResponse.IsStunned)
		{
			m_characterHitResponse.StopCharacterStunned();
		}
	}

	public virtual void ResetMods()
	{
		StrengthMod = 0f;
		StrengthAdd = 0;
		StrengthTemporaryAdd = 0;
		StrengthTemporaryMod = 0f;
		MagicMod = 0f;
		MagicAdd = 0;
		MagicTemporaryAdd = 0;
		MagicTemporaryMod = 0f;
		MaxHealthMod = 0f;
		MaxHealthAdd = 0;
		MaxHealthTemporaryAdd = 0;
		MaxHealthTemporaryMod = 0f;
	}

	public virtual void ResetCharacter()
	{
		IsDead = false;
		ResetStates();
		ResetBaseValues();
		ResetMods();
		ResetHealth();
		StatusEffectController.StopAllStatusEffects(interrupted: true);
		StatusBarController.ResetPositionAndScale(this);
		DisableFriction = false;
	}

	public virtual void ResetHealth()
	{
		SetHealth(ActualMaxHealth, additive: false, runEvents: true);
	}

	public void ResetIsDead()
	{
		IsDead = false;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IEffectVelocity.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IPreOnDisable.get_gameObject()
	{
		return base.gameObject;
	}
}
