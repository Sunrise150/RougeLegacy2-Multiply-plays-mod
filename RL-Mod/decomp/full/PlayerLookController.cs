using System;
using System.Collections;
using UnityEngine;

public class PlayerLookController : LookController
{
	[SerializeField]
	private bool m_clampPlayerScale;

	[SerializeField]
	private bool m_useCurrentScaleAsBase;

	[SerializeField]
	private bool m_ignoreScaleTraits;

	[SerializeField]
	private bool m_loadFromSaveFile;

	private Vector3 m_storedBaseScale;

	private PlayerController m_playerController;

	private Action<MonoBehaviour, EventArgs> m_onEquippedChanged;

	private static MaterialPropertyBlock m_critMatPropertyBlock;

	private const float CRIT_PULSE_INTERVAL = 0.2f;

	private static Color m_critStartColor;

	private static Color m_critEndColor;

	private Coroutine m_critBlinkCoroutine;

	private bool m_critBlinkEnabled;

	private Color m_storedWeapon1Color;

	private Color m_storedWeapon2Color;

	private int m_critEffectTriggerBitMask;

	public bool IsClassLookInitialized { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		m_onEquippedChanged = OnEquippedChanged;
	}

	public override void Initialize()
	{
		base.Initialize();
		m_playerController = GetComponent<PlayerController>();
		m_storedBaseScale = base.gameObject.transform.localScale;
		if (m_critMatPropertyBlock == null)
		{
			m_critMatPropertyBlock = new MaterialPropertyBlock();
			ColorUtility.TryParseHtmlString("#FFB000", out m_critStartColor);
			ColorUtility.TryParseHtmlString("#795C00", out m_critEndColor);
		}
	}

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.EquippedChanged, m_onEquippedChanged);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.EquippedChanged, m_onEquippedChanged);
	}

	protected virtual IEnumerator Start()
	{
		if (!m_loadFromSaveFile)
		{
			yield break;
		}
		yield return new WaitUntil(() => m_playerController.IsInitialized && m_playerController.CharacterClass.IsInitialized);
		CharacterData currentCharacter = SaveManager.PlayerSaveData.CurrentCharacter;
		if (m_playerController.CharacterClass.OverrideSaveFileValues)
		{
			currentCharacter.ClassType = m_playerController.CharacterClass.ClassType;
			currentCharacter.Weapon = m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon).AbilityType;
		}
		if (currentCharacter.Weapon == AbilityType.None)
		{
			BaseAbility_RL ability = m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon);
			if (ability != null)
			{
				currentCharacter.Weapon = ability.AbilityType;
			}
		}
		InitializeLook(currentCharacter);
	}

	public override void InitializeLook(CharacterData charData)
	{
		base.InitializeLook(charData);
		InitializeScale(charData);
		InitializeTraitLook(charData);
		InitializeEquipmentLook(charData);
		UpdateCustomMeshArray();
		ApplyOutlineScale();
		IsClassLookInitialized = true;
	}

	public void InitializeScale(CharacterData charData)
	{
		Vector3 localScale = new Vector3(1.4f, 1.4f, 1f);
		if (m_useCurrentScaleAsBase)
		{
			localScale = m_storedBaseScale;
		}
		if (!m_ignoreScaleTraits)
		{
			if (charData.TraitOne == TraitType.YouAreLarge || charData.TraitTwo == TraitType.YouAreLarge)
			{
				localScale.x *= 1.5f;
				localScale.y *= 1.5f;
			}
			if (charData.TraitOne == TraitType.YouAreSmall || charData.TraitTwo == TraitType.YouAreSmall)
			{
				localScale.x *= 0.55f;
				localScale.y *= 0.55f;
			}
		}
		if (m_clampPlayerScale)
		{
			localScale.x = Mathf.Clamp(localScale.x, 0.7f, 2.4f);
			localScale.y = Mathf.Clamp(localScale.y, 0.7f, 2.4f);
		}
		base.transform.localScale = localScale;
	}

	private void SetMainColor(SkinnedMeshRenderer renderer, Color color)
	{
		renderer.GetPropertyBlock(base.PropertyBlock);
		base.PropertyBlock.SetColor(ShaderID_RL._MainColor, color);
		renderer.SetPropertyBlock(base.PropertyBlock);
	}

	public void InitializeTraitLook(CharacterData charData)
	{
		base.Animator.SetFloat("BoneStructureType", 0f);
		base.Animator.SetFloat("LimbType", 0f);
		if (charData.TraitOne == TraitType.PlayerKnockedLow || charData.TraitTwo == TraitType.PlayerKnockedLow)
		{
			base.Animator.SetFloat("BoneStructureType", 2f);
		}
		if (charData.TraitOne == TraitType.PlayerKnockedFar || charData.TraitTwo == TraitType.PlayerKnockedFar)
		{
			base.Animator.SetFloat("BoneStructureType", 1f);
		}
		if (charData.TraitOne == TraitType.EnemyKnockedLow || charData.TraitTwo == TraitType.EnemyKnockedLow)
		{
			base.Animator.SetFloat("LimbType", 1f);
		}
		if (charData.TraitOne == TraitType.EnemyKnockedFar || charData.TraitTwo == TraitType.EnemyKnockedFar)
		{
			base.Animator.SetFloat("LimbType", 2f);
		}
		if (charData.ClassType == ClassType.CannonClass)
		{
			if (base.RightEyeGeo.gameObject.activeSelf)
			{
				base.RightEyeGeo.gameObject.SetActive(value: false);
			}
		}
		else if (!base.RightEyeGeo.gameObject.activeSelf)
		{
			base.RightEyeGeo.gameObject.SetActive(value: true);
		}
		if (charData.TraitOne == TraitType.Vampire || charData.TraitTwo == TraitType.Vampire)
		{
			SetMainColor(color: new Color(1f, 1f, 1f, 1f), renderer: base.HeadGeo);
			Color color2 = new Color(0.15f, 0.17f, 0.23f, 1f);
			SetMainColor(base.HelmetHairGeo, color2);
			SetMainColor(base.ChestHairGeo, color2);
			Color color3 = new Color(1f, 0f, 0f, 1f);
			SetMainColor(base.LeftEyeGeo, color3);
			SetMainColor(base.RightEyeGeo, color3);
			base.MouthGeo.sharedMaterial = LookLibrary.VampireFangsMaterial;
		}
		if (charData.TraitOne == TraitType.YouAreBlue || charData.TraitTwo == TraitType.YouAreBlue)
		{
			SetMainColor(color: new Color(14f / 51f, 0.6313726f, 1f), renderer: base.HeadGeo);
		}
		if (charData.TraitOne == TraitType.BounceTerrain || charData.TraitTwo == TraitType.BounceTerrain)
		{
			base.HeadGeo.sharedMaterial = LookLibrary.ClownHeadMaterial;
			SetMainColor(color: new Color(1f, 1f, 1f, 1f), renderer: base.HeadGeo);
			Color color6 = new Color(1f, 0.17f, 0.23f, 1f);
			SetMainColor(base.HelmetHairGeo, color6);
			SetMainColor(base.ChestHairGeo, color6);
			base.LeftEyeGeo.sharedMaterial = LookLibrary.ClownEyesMaterial;
			base.RightEyeGeo.sharedMaterial = LookLibrary.ClownEyesMaterial;
			base.MouthGeo.sharedMaterial = LookLibrary.ClownMouthMaterial;
		}
	}

	public void InitializeEquipmentLook(CharacterData charData)
	{
		LookCreator.InitializeHelmetLook(charData.HeadEquipmentType, this);
		LookCreator.InitializeArmorLook(charData.ChestEquipmentType, this);
		bool hasCantAttackTrait = charData.TraitOne == TraitType.CantAttack || charData.TraitTwo == TraitType.CantAttack;
		LookCreator.InitializeWeaponLook(charData.EdgeEquipmentType, this, charData.Weapon, hasCantAttackTrait);
		LookCreator.InitializeCapeLook(charData.CapeEquipmentType, this);
		UpdateCustomMeshArray();
		ApplyOutlineScale();
		if ((bool)m_playerController)
		{
			m_playerController.RecreateRendererArray();
			m_playerController.ResetRendererArrayColor();
			m_playerController.BlinkPulseEffect.ResetAllBlackFills();
		}
	}

	private void OnEquippedChanged(MonoBehaviour sender, EventArgs args)
	{
		if (m_playerController == null)
		{
			if (PlayerManager.IsInstantiated)
			{
				m_playerController = PlayerManager.GetPlayerController();
			}
			if (m_playerController == null)
			{
				return;
			}
		}
		EquippedChangeEventArgs equippedChangeEventArgs = args as EquippedChangeEventArgs;
		switch (equippedChangeEventArgs.EquipmentCategoryType)
		{
		case EquipmentCategoryType.Chest:
			LookCreator.InitializeArmorLook(equippedChangeEventArgs.EquippedType, this);
			break;
		case EquipmentCategoryType.Head:
			LookCreator.InitializeHelmetLook(equippedChangeEventArgs.EquippedType, this);
			break;
		case EquipmentCategoryType.Cape:
			LookCreator.InitializeCapeLook(equippedChangeEventArgs.EquippedType, this);
			break;
		case EquipmentCategoryType.Weapon:
		{
			AbilityType weaponType = AbilityType.None;
			BaseAbility_RL ability = m_playerController.CastAbility.GetAbility(CastAbilityType.Weapon);
			if (ability != null)
			{
				weaponType = ability.AbilityType;
			}
			bool hasCantAttackTrait = SaveManager.PlayerSaveData.HasTrait(TraitType.CantAttack);
			LookCreator.InitializeWeaponLook(equippedChangeEventArgs.EquippedType, this, weaponType, hasCantAttackTrait);
			break;
		}
		}
		UpdateCustomMeshArray();
		ApplyOutlineScale();
		if ((bool)m_playerController)
		{
			m_playerController.RecreateRendererArray();
			m_playerController.ResetRendererArrayColor();
			m_playerController.BlinkPulseEffect.ResetAllBlackFills();
		}
	}

	public void SetCritBlinkEffectEnabled(bool enable, CritBlinkEffectTriggerType effectTriggerType)
	{
		if (enable)
		{
			m_critEffectTriggerBitMask |= (int)effectTriggerType;
			if (!m_critBlinkEnabled)
			{
				if (m_critBlinkCoroutine != null)
				{
					StopCoroutine(m_critBlinkCoroutine);
				}
				m_critBlinkCoroutine = null;
				m_critBlinkCoroutine = StartCoroutine(CritBlinkEffectCoroutine());
				m_critBlinkEnabled = true;
			}
			return;
		}
		m_critEffectTriggerBitMask &= (int)(~effectTriggerType);
		if (m_critBlinkEnabled && m_critEffectTriggerBitMask == 0)
		{
			if (m_critBlinkCoroutine != null)
			{
				StopCoroutine(m_critBlinkCoroutine);
			}
			m_critBlinkCoroutine = null;
			SkinnedMeshRenderer currentWeaponGeo = m_playerController.LookController.CurrentWeaponGeo;
			if ((bool)currentWeaponGeo)
			{
				currentWeaponGeo.GetPropertyBlock(m_critMatPropertyBlock);
				m_critMatPropertyBlock.SetColor(ShaderID_RL._AddColor, m_storedWeapon1Color);
				currentWeaponGeo.SetPropertyBlock(m_critMatPropertyBlock);
			}
			SkinnedMeshRenderer secondaryWeaponGeo = m_playerController.LookController.SecondaryWeaponGeo;
			if ((bool)secondaryWeaponGeo)
			{
				secondaryWeaponGeo.GetPropertyBlock(m_critMatPropertyBlock);
				m_critMatPropertyBlock.SetColor(ShaderID_RL._AddColor, m_storedWeapon2Color);
				secondaryWeaponGeo.SetPropertyBlock(m_critMatPropertyBlock);
			}
			m_critBlinkEnabled = false;
		}
	}

	public void ForceDisableCritBlinkEffect()
	{
		m_critEffectTriggerBitMask = 0;
		if (m_critBlinkCoroutine != null)
		{
			StopCoroutine(m_critBlinkCoroutine);
		}
		m_critBlinkCoroutine = null;
		if (m_critBlinkEnabled)
		{
			SkinnedMeshRenderer currentWeaponGeo = m_playerController.LookController.CurrentWeaponGeo;
			if ((bool)currentWeaponGeo)
			{
				currentWeaponGeo.GetPropertyBlock(m_critMatPropertyBlock);
				m_critMatPropertyBlock.SetColor(ShaderID_RL._AddColor, m_storedWeapon1Color);
				currentWeaponGeo.SetPropertyBlock(m_critMatPropertyBlock);
			}
			SkinnedMeshRenderer secondaryWeaponGeo = m_playerController.LookController.SecondaryWeaponGeo;
			if ((bool)secondaryWeaponGeo)
			{
				secondaryWeaponGeo.GetPropertyBlock(m_critMatPropertyBlock);
				m_critMatPropertyBlock.SetColor(ShaderID_RL._AddColor, m_storedWeapon2Color);
				secondaryWeaponGeo.SetPropertyBlock(m_critMatPropertyBlock);
			}
			m_critBlinkEnabled = false;
		}
	}

	private IEnumerator CritBlinkEffectCoroutine()
	{
		SkinnedMeshRenderer weapon1Renderer = m_playerController.LookController.CurrentWeaponGeo;
		if ((bool)weapon1Renderer)
		{
			weapon1Renderer.GetPropertyBlock(m_critMatPropertyBlock);
			m_storedWeapon1Color = m_critMatPropertyBlock.GetColor(ShaderID_RL._AddColor);
		}
		SkinnedMeshRenderer weapon2Renderer = m_playerController.LookController.SecondaryWeaponGeo;
		if ((bool)weapon2Renderer)
		{
			weapon2Renderer.GetPropertyBlock(m_critMatPropertyBlock);
			m_storedWeapon2Color = m_critMatPropertyBlock.GetColor(ShaderID_RL._AddColor);
		}
		float intervalStartTime = Time.time;
		bool reverseTween = false;
		while (true)
		{
			float num = (Time.time - intervalStartTime) / 0.2f;
			Color value = (reverseTween ? Color.Lerp(m_critEndColor, m_critStartColor, num) : Color.Lerp(m_critStartColor, m_critEndColor, num));
			if (num >= 1f)
			{
				intervalStartTime = Time.time;
				reverseTween = !reverseTween;
			}
			if ((bool)weapon1Renderer)
			{
				weapon1Renderer.GetPropertyBlock(m_critMatPropertyBlock);
				m_critMatPropertyBlock.SetColor(ShaderID_RL._AddColor, value);
				weapon1Renderer.SetPropertyBlock(m_critMatPropertyBlock);
			}
			if ((bool)weapon2Renderer)
			{
				weapon2Renderer.GetPropertyBlock(m_critMatPropertyBlock);
				m_critMatPropertyBlock.SetColor(ShaderID_RL._AddColor, value);
				weapon2Renderer.SetPropertyBlock(m_critMatPropertyBlock);
			}
			yield return null;
		}
	}
}
