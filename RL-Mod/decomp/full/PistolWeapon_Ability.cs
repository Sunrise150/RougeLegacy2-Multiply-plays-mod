using FMODUnity;
using RL_Windows;
using UnityEngine;

public class PistolWeapon_Ability : AimedAbility_RL, IAttack, IAbility
{
	[SerializeField]
	private GameObject m_aimSprite;

	[SerializeField]
	protected StudioEventEmitter m_shootAudioEmitter;

	[SerializeField]
	protected StudioEventEmitter m_emptyClipAudioEmitter;

	[SerializeField]
	protected string m_emptyClipProjectileName;

	[SerializeField]
	private string m_critHitProjectileName;

	private BaseAbility_RL m_reloadAbility;

	private const float LOW_AMMO_AUDIO_PARAMETER_VALUE = 0.1f;

	private bool m_aimSpriteInitialized;

	private Vector2 m_bowPushbackAmount = new Vector2(0f, 3.25f);

	private Vector2 m_bowPushbackAmountNoAmmo = new Vector2(0f, 2.4f);

	public BaseAbility_RL ReloadAbility => m_reloadAbility;

	public override Vector2 PushbackAmount => BowPushbackAmount;

	protected virtual Vector2 BowPushbackAmount
	{
		get
		{
			if (base.CurrentAmmo > 0)
			{
				return m_bowPushbackAmount;
			}
			return m_bowPushbackAmountNoAmmo;
		}
	}

	public override string ProjectileName
	{
		get
		{
			if (base.CurrentAmmo > 8)
			{
				return base.ProjectileName;
			}
			if (base.CurrentAmmo > 0)
			{
				return m_critHitProjectileName;
			}
			return m_emptyClipProjectileName;
		}
	}

	protected override bool CancelTimeSlowOnFire => false;

	protected virtual bool CanManuallyReload => true;

	protected override float TellIntroAnimSpeed => 3.25f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 5f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 2f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0f;

	protected override float ExitAnimSpeed => 3f;

	protected override float ExitAnimExitDelay
	{
		get
		{
			if (SaveManager.PlayerSaveData.GetRelic(RelicType.AttackCooldown).Level > 0)
			{
				return (float)SaveManager.PlayerSaveData.GetRelic(RelicType.AttackCooldown).Level * 0.2f;
			}
			return 0f;
		}
	}

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[3] { m_projectileName, m_critHitProjectileName, m_emptyClipProjectileName };
	}

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		base.Initialize(abilityController, castAbilityType);
		if (CanManuallyReload)
		{
			m_reloadAbility = AbilityLibrary.GetAbility(AbilityType.ReloadTalent);
			m_reloadAbility = Object.Instantiate(m_reloadAbility);
			m_reloadAbility.transform.SetParent(base.transform);
			m_reloadAbility.Initialize(abilityController, castAbilityType);
		}
	}

	protected override void UpdateAimLine()
	{
		Vector3 localEulerAngles = m_aimSprite.transform.localEulerAngles;
		localEulerAngles.z = m_unmoddedAngle;
		m_aimSprite.transform.localEulerAngles = localEulerAngles;
	}

	protected override void Update()
	{
		if (!base.AbilityActive && Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
		{
			m_abilityController.StartAbility(base.CastAbilityType);
		}
		base.Update();
		if (CanManuallyReload && !Interactable.PlayerIsInteracting && !base.AbilityActive && Rewired_RL.Player.GetButtonDown("Interact") && (bool)m_reloadAbility && m_abilityController.IsAbilityPermitted(m_reloadAbility))
		{
			m_abilityController.StartAbility(CastAbilityType.Weapon, castAbilityOverride: true);
		}
		if (!m_aimSprite)
		{
			return;
		}
		if (!m_aimSpriteInitialized)
		{
			m_aimSprite.transform.localScale = new Vector3(1f / m_abilityController.PlayerController.transform.lossyScale.x, 1f / m_abilityController.PlayerController.transform.lossyScale.y, 1f);
			float num = m_abilityController.PlayerController.transform.lossyScale.x / m_abilityController.PlayerController.BaseScaleToOffsetWith;
			Vector2 vector = new Vector2(0f, m_projectileOffset.y * num);
			m_aimSprite.transform.position = m_abilityController.transform.localPosition + (Vector3)vector;
			m_aimSpriteInitialized = true;
		}
		if ((m_abilityController.AnyAbilityInProgress && !m_abilityController.AbilityInProgress(CastAbilityType.Weapon)) || WindowManager.IsAnyWindowOpen || GameManager.IsGamePaused || (!GameManager.IsGamePaused && !RewiredMapController.IsCurrentMapEnabled))
		{
			if (m_aimSprite.gameObject.activeSelf)
			{
				m_aimSprite.gameObject.SetActive(value: false);
			}
		}
		else if (!m_aimSprite.gameObject.activeSelf)
		{
			m_aimSprite.gameObject.SetActive(value: true);
		}
		if (!base.AbilityActive)
		{
			UpdateArrowAim(doNotUpdatePlayerAnims: true);
			UpdateAimLine();
		}
	}

	protected void Base_FireProjectile()
	{
		base.FireProjectile();
	}

	protected override void FireProjectile()
	{
		if (base.CurrentAmmo > 0)
		{
			m_shootAudioEmitter.Play();
		}
		else
		{
			m_emptyClipAudioEmitter.Play();
		}
		if (base.CurrentAmmo <= 8)
		{
			m_shootAudioEmitter.SetParameter("ammoFull", 0.1f);
		}
		base.FireProjectile();
		if (base.CurrentAmmo <= 0)
		{
			m_firedProjectile.RemoveAllStatusEffects();
		}
		if (base.CurrentAmmo < 8)
		{
			m_firedProjectile.ActualCritChance += 100f;
		}
		int level = SaveManager.PlayerSaveData.GetRelic(RelicType.AttackCooldown).Level;
		if (level > 0)
		{
			m_firedProjectile.LifespanTimer += (float)level * 0f;
		}
	}
}
