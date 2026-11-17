using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.Events;

public class Breakable : MonoBehaviour, IBodyOnEnterHitResponse, IHitResponse, IBodyOnStayHitResponse, ITerrainOnEnterHitResponse, ITerrainOnStayHitResponse, IEffectTriggerEvent_OnDeath, IEffectTriggerEvent_OnDamage, IRootObj, IPreOnDisable, IPlayHitEffect
{
	[SerializeField]
	private bool m_collideWithWeaponHitboxOnly;

	[SerializeField]
	private bool m_doNotDisableOnDeath;

	[SerializeField]
	private bool m_spawnItemOnDestroy = true;

	[SerializeField]
	private Sprite m_brokenStateSprite;

	[SerializeField]
	private int m_baseScaledHP = 1;

	[SerializeField]
	private bool m_displayDamageText;

	[SerializeField]
	private UnityEvent m_destroyedEvent;

	[SerializeField]
	private UnityEvent m_hitEvent;

	[SerializeField]
	[ReadOnlyOnPlay]
	private int m_currentHP = -1;

	private const string DEFAULT_STATE_NAME = "Idle";

	private SpriteRenderer m_spriteRenderer;

	private Sprite m_initialSprite;

	protected BlinkPulseEffect m_hitEffect;

	protected IHitboxController m_hitboxController;

	private Animator m_animator;

	private bool m_attackerIsOnRight;

	private BreakableHitEventArgs m_hitEventArgs;

	private Prop m_prop;

	private BreakableEventArgs m_breakableDestroyedEventArgs;

	private Relay<IPreOnDisable> m_onDisableRelay = new Relay<IPreOnDisable>();

	private Relay<GameObject> m_onDeathEffectTriggerEventRelay = new Relay<GameObject>();

	private Relay<GameObject, float, bool> m_onDamageEffectTriggerRelay = new Relay<GameObject, float, bool>();

	private static List<Vector2> m_dropTypeHelper_STATIC = new List<Vector2>();

	private static List<float> m_dropOddsHelper_STATIC = new List<float>();

	public ItemDropType ItemDropTypeOverride { get; set; }

	public bool IsDirectionalBreak { get; private set; }

	public bool AttackerIsOnRight
	{
		get
		{
			return m_attackerIsOnRight;
		}
		set
		{
			m_attackerIsOnRight = value;
		}
	}

	public bool PlayHitEffect => true;

	public bool PlayDirectionalHitEffect => false;

	public string EffectNameOverride => null;

	public SpriteRenderer SpriteRenderer => m_spriteRenderer;

	public Vector3 Midpoint
	{
		get
		{
			if ((bool)m_spriteRenderer)
			{
				return m_spriteRenderer.bounds.center;
			}
			return base.transform.position;
		}
	}

	public Animator AnimatorComponent
	{
		get
		{
			return m_animator;
		}
		private set
		{
			m_animator = value;
		}
	}

	public int CurrentAnimatorHashState
	{
		get
		{
			if (AnimatorComponent != null)
			{
				return AnimatorComponent.GetCurrentAnimatorStateInfo(0).shortNameHash;
			}
			return -1;
		}
	}

	public IRelayLink<IPreOnDisable> PreOnDisableRelay => m_onDisableRelay.link;

	public IRelayLink<GameObject, float, bool> OnDamageEffectTriggerRelay => m_onDamageEffectTriggerRelay.link;

	public IRelayLink<GameObject> OnDeathEffectTriggerRelay => m_onDeathEffectTriggerEventRelay.link;

	public UnityEvent DestroyedEvent => m_destroyedEvent;

	public virtual bool IsBroken => CurrentHP <= 0;

	public bool DoNotDisableOnDeath
	{
		get
		{
			return m_doNotDisableOnDeath;
		}
		private set
		{
			m_doNotDisableOnDeath = value;
		}
	}

	public int CurrentHP
	{
		get
		{
			return m_currentHP;
		}
		protected set
		{
			m_currentHP = Mathf.Max(0, value);
		}
	}

	public int DefaultHP => m_baseScaledHP;

	protected virtual void Awake()
	{
		AnimatorComponent = GetComponent<Animator>();
		m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		if ((bool)m_spriteRenderer)
		{
			m_initialSprite = m_spriteRenderer.sprite;
		}
		m_hitEffect = GetComponentInChildren<BlinkPulseEffect>();
		m_hitboxController = GetComponentInChildren<IHitboxController>();
		base.tag = "Breakable";
		m_breakableDestroyedEventArgs = new BreakableEventArgs(this);
		if ((bool)AnimatorComponent)
		{
			IsDirectionalBreak = AnimatorUtility.HasParameter(AnimatorComponent, "OpenLeft");
		}
		m_prop = GetComponent<Prop>();
		ForceBrokenState(isBroken: false);
	}

	private void OnDisable()
	{
		ItemDropTypeOverride = ItemDropType.None;
	}

	public void BodyOnEnterHitResponse(IHitboxController hbController)
	{
		TriggerCollision(hbController.DamageObj);
	}

	public void BodyOnStayHitResponse(IHitboxController hbController)
	{
		BodyOnEnterHitResponse(hbController);
	}

	protected virtual void Break(IDamageObj damageObj)
	{
		BroadcastDestroyedEvents(damageObj);
		m_onDisableRelay.Dispatch(this);
		BreakDecos(damageObj);
		if (m_spawnItemOnDestroy || ItemDropTypeOverride != 0)
		{
			DropReward();
		}
		SetBrokenVisuals(isBroken: true);
		SetHitBoxControllerIsActive(isActive: false);
	}

	private void BreakDecos(IDamageObj damageObj)
	{
		if (!m_prop)
		{
			return;
		}
		for (int i = 0; i < m_prop.DecoControllers.Length; i++)
		{
			DecoController decoController = m_prop.DecoControllers[i];
			for (int j = 0; j < decoController.DecoLocations.Length; j++)
			{
				DecoLocation decoLocation = decoController.DecoLocations[j];
				if ((bool)decoLocation.DecoInstance)
				{
					Breakable[] componentsInChildren = decoLocation.DecoInstance.GetComponentsInChildren<Breakable>();
					for (int k = 0; k < componentsInChildren.Length; k++)
					{
						componentsInChildren[k].TriggerCollision(damageObj);
					}
				}
			}
		}
	}

	private void SetBrokenVisuals(bool isBroken)
	{
		bool flag = m_spriteRenderer;
		if (flag)
		{
			m_spriteRenderer.enabled = true;
		}
		if (!isBroken)
		{
			if (flag && m_spriteRenderer.sprite != m_initialSprite)
			{
				m_spriteRenderer.sprite = m_initialSprite;
			}
		}
		else if (DoNotDisableOnDeath)
		{
			if (flag && (bool)m_brokenStateSprite)
			{
				m_spriteRenderer.sprite = m_brokenStateSprite;
			}
		}
		else
		{
			DisableDecos();
			base.gameObject.SetActive(value: false);
		}
	}

	private void DisableDecos()
	{
		if (!m_prop)
		{
			return;
		}
		for (int i = 0; i < m_prop.DecoControllers.Length; i++)
		{
			DecoController decoController = m_prop.DecoControllers[i];
			for (int j = 0; j < decoController.DecoLocations.Length; j++)
			{
				DecoLocation decoLocation = decoController.DecoLocations[j];
				if ((bool)decoLocation.DecoInstance)
				{
					decoLocation.DecoInstance.gameObject.SetActive(value: false);
				}
			}
		}
	}

	public virtual void ForceBrokenState(bool isBroken)
	{
		if (isBroken)
		{
			CurrentHP = 0;
			if (IsDirectionalBreak)
			{
				if (AttackerIsOnRight)
				{
					AnimatorComponent.SetTrigger("OpenLeft");
					AnimatorComponent.Update(2f);
					AnimatorComponent.Update(2f);
				}
				else
				{
					AnimatorComponent.SetTrigger("OpenRight");
					AnimatorComponent.Update(2f);
					AnimatorComponent.Update(2f);
				}
			}
		}
		else
		{
			CurrentHP = DefaultHP;
		}
		SetBrokenVisuals(isBroken);
		SetHitBoxControllerIsActive(!isBroken);
	}

	public void SetAnimatorState(int stateHash)
	{
		if (!(AnimatorComponent == null) && stateHash != -1)
		{
			if (stateHash != -1)
			{
				AnimatorComponent.Play(stateHash, 0, 1f);
			}
			else
			{
				AnimatorComponent.Play("Idle", 0, 0f);
			}
		}
	}

	private void SetHitBoxControllerIsActive(bool isActive)
	{
		if (m_hitboxController == null)
		{
			m_hitboxController = GetComponentInChildren<IHitboxController>();
		}
		m_hitboxController.DisableAllCollisions = !isActive;
	}

	private void DropReward()
	{
		ItemDropType itemDropType = ItemDropType.None;
		if (ItemDropTypeOverride != 0)
		{
			itemDropType = ItemDropTypeOverride;
		}
		else
		{
			if (ChallengeManager.IsInChallenge || !m_prop.Room.AllowItemDrops)
			{
				return;
			}
			m_dropTypeHelper_STATIC.Clear();
			m_dropTypeHelper_STATIC.AddRange(Economy_EV.BREAKABLE_ITEM_DROP_TYPE_ODDS);
			float num = (float)SaveManager.PlayerSaveData.GetRelic(RelicType.MeatChanceUp).Level * 0.01f;
			if (num > 0f)
			{
				for (int i = 0; i < m_dropTypeHelper_STATIC.Count; i++)
				{
					Vector2 value = m_dropTypeHelper_STATIC[i];
					if (value.x == 50f)
					{
						value.y += num;
						m_dropTypeHelper_STATIC[i] = value;
					}
				}
			}
			int randomOdds = CDGHelper.GetRandomOdds(GetDropOdds(m_dropTypeHelper_STATIC));
			itemDropType = (ItemDropType)m_dropTypeHelper_STATIC[randomOdds].x;
		}
		bool largeSpurt = TraitManager.IsTraitActive(TraitType.ItemsGoFlying);
		if (itemDropType == ItemDropType.None)
		{
			float currentStatGain = SkillTreeManager.GetSkillTreeObj(SkillTreeType.Equipment_Ore_Find_Up).CurrentStatGain;
			float currentStatGain2 = SkillTreeManager.GetSkillTreeObj(SkillTreeType.Rune_Ore_Find_Up).CurrentStatGain;
			float y = 1f - (currentStatGain + currentStatGain2);
			m_dropTypeHelper_STATIC.Clear();
			m_dropTypeHelper_STATIC.Add(new Vector2(70f, currentStatGain));
			m_dropTypeHelper_STATIC.Add(new Vector2(80f, currentStatGain2));
			m_dropTypeHelper_STATIC.Add(new Vector2(0f, y));
			int randomOdds2 = CDGHelper.GetRandomOdds(GetDropOdds(m_dropTypeHelper_STATIC));
			itemDropType = (ItemDropType)m_dropTypeHelper_STATIC[randomOdds2].x;
		}
		if (itemDropType != 0)
		{
			int itemDropValue = Economy_EV.GetItemDropValue(itemDropType);
			Vector3 midpoint = Midpoint;
			ItemDropManager.DropItem(itemDropType, itemDropValue, midpoint, largeSpurt);
		}
	}

	private List<float> GetDropOdds(List<Vector2> dropTypeList)
	{
		m_dropOddsHelper_STATIC.Clear();
		int count = dropTypeList.Count;
		for (int i = 0; i < count; i++)
		{
			m_dropOddsHelper_STATIC.Add(dropTypeList[i].y);
		}
		return m_dropOddsHelper_STATIC;
	}

	public void TerrainOnEnterHitResponse(IHitboxController hbController)
	{
		if (!m_collideWithWeaponHitboxOnly)
		{
			TriggerCollision(hbController.DamageObj);
		}
	}

	public void TerrainOnStayHitResponse(IHitboxController hbController)
	{
		TerrainOnEnterHitResponse(hbController);
	}

	protected virtual void TriggerCollision(IDamageObj damageObj)
	{
		if (IsBroken)
		{
			return;
		}
		int num = Mathf.Clamp(Mathf.FloorToInt(damageObj.ActualDamage), 1, int.MaxValue);
		if (m_displayDamageText)
		{
			Bounds bounds = m_hitboxController.GetCollider(HitboxType.Body).bounds;
			TextPopupManager.DisplayTextAtAbsPos(absPos: new Vector2(bounds.center.x, bounds.max.y), popupType: TextPopupType.EnemyHit, text: (-num).ToString());
		}
		CurrentHP--;
		if (CurrentHP <= 0)
		{
			if ((bool)m_hitEffect)
			{
				m_hitEffect.StartSingleBlinkEffect();
			}
			GameObject gameObject = damageObj.gameObject;
			Projectile_RL projectile_RL = damageObj as Projectile_RL;
			if ((bool)projectile_RL && projectile_RL.UseOwnerCollisionPoint)
			{
				gameObject = projectile_RL.Owner.gameObject;
			}
			if (gameObject.transform.position.x < base.transform.position.x)
			{
				AttackerIsOnRight = false;
			}
			else
			{
				AttackerIsOnRight = true;
			}
			Break(damageObj);
		}
		else
		{
			if ((bool)m_hitEffect)
			{
				m_hitEffect.StartSingleBlinkEffect();
			}
			BroadcastHitEvents(damageObj);
		}
	}

	private void BroadcastDestroyedEvents(IDamageObj damageObj)
	{
		if (m_destroyedEvent != null)
		{
			m_destroyedEvent.Invoke();
		}
		m_onDeathEffectTriggerEventRelay.Dispatch(damageObj.gameObject);
		InitializeHitEventArgs(damageObj);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.BreakableDestroyed, this, m_hitEventArgs);
	}

	private void InitializeHitEventArgs(IDamageObj damageObj)
	{
		if (m_hitEventArgs == null)
		{
			m_hitEventArgs = new BreakableHitEventArgs(this, damageObj);
		}
		else
		{
			m_hitEventArgs.Initialize(this, damageObj);
		}
	}

	protected void BroadcastHitEvents(IDamageObj damageObj)
	{
		m_onDamageEffectTriggerRelay.Dispatch(damageObj.gameObject, damageObj.ActualDamage, v: false);
		if (m_hitEvent != null)
		{
			m_hitEvent.Invoke();
		}
		InitializeHitEventArgs(damageObj);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.BreakableHit, this, m_hitEventArgs);
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IPreOnDisable.get_gameObject()
	{
		return base.gameObject;
	}
}
