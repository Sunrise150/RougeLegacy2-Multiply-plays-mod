using System;
using System.Collections;
using FMOD.Studio;
using RLAudio;
using UnityEngine;

public class PaintingEnemy_Basic_AIScript : BaseAIScript
{
	private const float ROTATION_SPEED = 720f;

	private const string GENERIC_PROJECTILE_NAME = "PaintingEnemyProjectile";

	private const string FLY_LOOP_AUDIO_PATH = "event:/SFX/Enemies/sfx_enemy_painting_fly_start_loop";

	[SerializeField]
	private Sprite[] m_paintingSprites;

	[SerializeField]
	private Sprite[] m_enemyFaceSprites;

	private SpriteRenderer m_paintingRenderer;

	private SpriteRenderer m_enemyFaceRenderer;

	private bool m_isChasing;

	private bool m_rotateClockwise;

	private float m_projectileCDDuration;

	private Tween m_enemyFaceTween;

	private EventInstance m_flyLoopEventInstance;

	private Action<object, CharacterHitEventArgs> m_onPlayerHit;

	private Action<object, CharacterHitEventArgs> m_onEnemyHit;

	private Action m_onEnemyControllerDisable;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(1.25f, 2f);

	protected override Vector2 WalkAwayDuration => new Vector2(1.25f, 2f);

	protected override Vector2 RandomFollowOffsetX => Vector2.zero;

	protected override Vector2 RandomFollowOffsetY => Vector2.zero;

	protected virtual float ProjectileFireInterval => 0f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[1] { "PaintingEnemyProjectile" };
	}

	private void Awake()
	{
		m_onPlayerHit = OnPlayerHit;
		m_onEnemyHit = OnEnemyHit;
		m_onEnemyControllerDisable = OnEnemyControllerDisable;
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		ObjectReferenceFinder component = base.EnemyController.GetComponent<ObjectReferenceFinder>();
		m_paintingRenderer = component.GetObject("Sprite").GetComponent<SpriteRenderer>();
		m_enemyFaceRenderer = component.GetObject("EnemyFace").GetComponent<SpriteRenderer>();
		base.EnemyController.LogicController.DisableLogicActivationByDistance = true;
		base.EnemyController.LogicController.OverrideLogicDelay(0f);
		base.EnemyController.OnActivatedRelay.AddListener(Awaken);
		base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onEnemyHit);
		base.EnemyController.LockFlip = true;
		m_isChasing = false;
		base.EnemyController.OnDisableRelay.AddListener(m_onEnemyControllerDisable);
		OnEnemyControllerDisable();
	}

	public override void OnEnemyActivated()
	{
		m_projectileCDDuration = Time.time + ProjectileFireInterval;
		base.OnEnemyActivated();
	}

	public override IEnumerator Idle()
	{
		yield return base.WalkTowards();
	}

	private void Shoot()
	{
		float num = CDGHelper.AngleBetweenPts(base.EnemyController.TargetController.Midpoint, base.EnemyController.Midpoint);
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			num += 15f;
			this.FireProjectile("PaintingEnemyProjectile", 0, matchFacing: false, num);
			num -= 30f;
			this.FireProjectile("PaintingEnemyProjectile", 0, matchFacing: false, num);
		}
		else
		{
			this.FireProjectile("PaintingEnemyProjectile", 0, matchFacing: false, num);
		}
		m_projectileCDDuration = Time.time + ProjectileFireInterval;
	}

	private void OnEnable()
	{
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onPlayerHit);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onPlayerHit);
		}
		if (base.IsInitialized)
		{
			base.EnemyController.LogicController.DisableLogicActivationByDistance = true;
		}
		if (m_flyLoopEventInstance.isValid())
		{
			AudioManager.Stop(m_flyLoopEventInstance);
		}
	}

	private void OnDestroy()
	{
		if ((bool)base.EnemyController)
		{
			base.EnemyController.OnActivatedRelay.RemoveListener(Awaken);
			base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onEnemyHit);
		}
		if (m_flyLoopEventInstance.isValid())
		{
			m_flyLoopEventInstance.release();
		}
	}

	private void OnPlayerHit(object sender, CharacterHitEventArgs args)
	{
		if (args.Attacker != base.EnemyController)
		{
			return;
		}
		base.EnemyController.LogicController.TriggerAggro(null, null);
		if (!base.EnemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Invuln))
		{
			return;
		}
		foreach (Renderer item in base.EnemyController.RendererArray)
		{
			if (item.sharedMaterial.HasProperty(ShaderID_RL._ShieldToggle))
			{
				item.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
				BaseStatusEffect.m_matBlockHelper_STATIC.SetInt(ShaderID_RL._ShieldToggle, 1);
				item.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
			}
		}
	}

	private void OnEnemyHit(object sender, CharacterHitEventArgs args)
	{
		base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: false);
	}

	private void OnEnemyControllerDisable()
	{
		if (base.IsInitialized)
		{
			m_isChasing = false;
			int num = Mathf.Clamp((RNGManager.GetSeed(RngID.SpecialProps_RoomSeed) + base.EnemyController.EnemyIndex) % m_paintingSprites.Length, 0, m_paintingSprites.Length - 1);
			m_paintingRenderer.sprite = m_paintingSprites[num];
			m_enemyFaceRenderer.sprite = m_enemyFaceSprites[num];
			Color color = m_enemyFaceRenderer.color;
			color.a = 0f;
			m_enemyFaceRenderer.color = color;
			if (!m_enemyFaceTween.IsNativeNull())
			{
				m_enemyFaceTween.StopTween(runEndHandlers: true);
			}
			base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: true);
		}
	}

	private void Awaken(object sender, EventArgs args)
	{
		if (m_enemyFaceTween != null)
		{
			m_enemyFaceTween.StopTweenWithConditionChecks(runEndHandlers: false, m_enemyFaceRenderer);
		}
		m_enemyFaceTween = TweenManager.TweenTo(m_enemyFaceRenderer, 0.5f, Ease.None, "color.a", 1);
		m_isChasing = true;
		base.EnemyController.StatusEffectController.SetAllStatusEffectsHidden(hide: false);
		Vector3 vector = base.EnemyController.Midpoint - PlayerManager.GetPlayerController().Midpoint;
		base.EnemyController.Heading = vector;
		if (PlayerManager.GetPlayerController().transform.localPosition.x < base.EnemyController.transform.localPosition.x)
		{
			m_rotateClockwise = false;
		}
		else
		{
			m_rotateClockwise = true;
		}
		base.Animator.SetTrigger("Reveal");
		base.Animator.SetTrigger("Spin");
		if (!m_flyLoopEventInstance.isValid())
		{
			m_flyLoopEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_enemy_painting_fly_start_loop", base.transform);
		}
		AudioManager.PlayAttached(this, m_flyLoopEventInstance, base.gameObject);
	}

	public override void ResetScript()
	{
		base.ResetScript();
		base.EnemyController.LockFlip = true;
		base.EnemyController.Pivot.transform.localEulerAngles = Vector3.zero;
	}

	protected void Update()
	{
		if (!base.IsInitialized || !m_isChasing || base.EnemyController.ConditionState != 0)
		{
			return;
		}
		if (!base.LogicController.IsAggroed)
		{
			base.EnemyController.LogicController.TriggerAggro(null, null);
		}
		Vector3 localEulerAngles = base.EnemyController.Pivot.transform.localEulerAngles;
		float z = localEulerAngles.z;
		if (m_rotateClockwise)
		{
			z -= Time.deltaTime * 720f;
			if (z < -360f)
			{
				z += 360f;
			}
		}
		else
		{
			z += Time.deltaTime * 720f;
			if (z > 360f)
			{
				z -= 360f;
			}
		}
		localEulerAngles.z = z;
		base.EnemyController.Pivot.transform.localEulerAngles = localEulerAngles;
		if (ProjectileFireInterval > 0f && Time.time >= m_projectileCDDuration)
		{
			Shoot();
		}
	}
}
