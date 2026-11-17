using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD_Ability : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_tempTitleText;

	[SerializeField]
	private Image m_cooldownImage;

	[SerializeField]
	private TMP_Text m_cooldownText;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private CastAbilityType m_castAbilityType;

	[SerializeField]
	private TMP_Text m_ammoText;

	[SerializeField]
	private Image m_icon;

	[SerializeField]
	private TMP_Text m_inputText;

	[SerializeField]
	private TMP_Text m_manaCostText;

	[SerializeField]
	private GameObject m_abilityReadyGO;

	[SerializeField]
	private Image m_cooldownIcon;

	[SerializeField]
	private GameObject m_disarmedGO;

	private const string DECIMAL_COUNT_VALUE_STRING = "<size=60%><voffset=20>.{0:D2}</voffset></size>";

	private const string NULL_COOLDOWN_STRING = "--";

	private BaseAbility_RL m_assignedAbility;

	private Animator m_animator;

	private Tween m_cdChangeTween;

	private int m_trackedValue;

	private StringBuilder m_valueStringBuilder;

	private StringBuilder m_prevValueStringBuilder;

	private bool m_displayingNullCooldownString;

	private Action<MonoBehaviour, EventArgs> m_onChangeAbilityHandler;

	private Action<MonoBehaviour, EventArgs> m_onAmmoChangeHandler;

	private Action<MonoBehaviour, EventArgs> m_onManaChangeHandler;

	private Action<MonoBehaviour, EventArgs> m_onUpdateAbilityDisarmStateHandler;

	private Action<object, CooldownEventArgs> m_beginCooldown;

	private void Awake()
	{
		m_valueStringBuilder = new StringBuilder();
		m_prevValueStringBuilder = new StringBuilder();
		m_onChangeAbilityHandler = OnChangeAbilityHandler;
		m_onAmmoChangeHandler = OnAmmoChangeHandler;
		m_onManaChangeHandler = OnManaChangeHandler;
		m_onUpdateAbilityDisarmStateHandler = OnUpdateAbilityDisarmStateHandler;
		m_beginCooldown = BeginCooldown;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.ChangeAbility, m_onChangeAbilityHandler);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.UpdateAbilityHUD, m_onChangeAbilityHandler);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerAmmoChange, m_onAmmoChangeHandler);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerManaChange, m_onManaChangeHandler);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.UpdateAbilityDisarmState, m_onUpdateAbilityDisarmStateHandler);
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.ChangeAbility, m_onChangeAbilityHandler);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.UpdateAbilityHUD, m_onChangeAbilityHandler);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerAmmoChange, m_onAmmoChangeHandler);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerManaChange, m_onManaChangeHandler);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.UpdateAbilityDisarmState, m_onUpdateAbilityDisarmStateHandler);
	}

	private void OnEnable()
	{
		m_abilityReadyGO.SetActive(value: false);
		InitializeAbilityReadyGO();
	}

	private void InitializeAbilityReadyGO()
	{
		if ((bool)m_assignedAbility && (m_assignedAbility.ActualCost > 0 || !(m_assignedAbility.ActualCooldownTime <= 0f)))
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			if ((float)m_assignedAbility.ActualCost < playerController.CurrentMana && !m_assignedAbility.IsOnCooldown)
			{
				m_abilityReadyGO.SetActive(value: true);
			}
		}
	}

	private void OnChangeAbilityHandler(MonoBehaviour sender, EventArgs eventArgs)
	{
		UpdateAssignedAbility();
		InitializeAbilityReadyGO();
		UpdateDisarmedState();
	}

	private void OnUpdateAbilityDisarmStateHandler(MonoBehaviour sender, EventArgs args)
	{
		UpdateDisarmedState();
	}

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		m_animator = m_abilityReadyGO.GetComponent<Animator>();
		UpdateAssignedAbility();
		m_abilityReadyGO.SetActive(value: false);
		InitializeAbilityReadyGO();
		UpdateDisarmedState();
	}

	private void UpdateAssignedAbility()
	{
		if (!PlayerManager.IsInstantiated)
		{
			return;
		}
		if ((bool)m_assignedAbility)
		{
			m_assignedAbility.OnBeginCooldownRelay.RemoveListener(m_beginCooldown);
		}
		BaseAbility_RL ability = PlayerManager.GetPlayerController().CastAbility.GetAbility(m_castAbilityType);
		if ((bool)ability)
		{
			m_assignedAbility = ability;
			m_tempTitleText.text = AbilityLibrary.GetAbility(ability.AbilityType).AbilityData.Name;
			ability.OnBeginCooldownRelay.AddListener(m_beginCooldown);
			m_cooldownImage.enabled = false;
			m_cooldownText.gameObject.SetActive(value: false);
			m_cooldownIcon.sprite = IconLibrary.GetAbilityCooldownIcon(ability.CooldownRegenType, returnNullIfNotFound: true);
			m_cooldownIcon.gameObject.SetActive(value: false);
			int actualCost = ability.ActualCost;
			if (actualCost > 0)
			{
				m_manaCostText.gameObject.SetActive(value: true);
				m_manaCostText.text = actualCost.ToString();
			}
			else
			{
				m_manaCostText.gameObject.SetActive(value: false);
			}
			Sprite sprite = null;
			switch (m_castAbilityType)
			{
			case CastAbilityType.Weapon:
				sprite = IconLibrary.GetAbilityIcon(ability.AbilityType, returnNullIfNotFound: true);
				m_inputText.text = "[Attack]";
				break;
			case CastAbilityType.Spell:
				sprite = IconLibrary.GetAbilityIcon(ability.AbilityType, returnNullIfNotFound: true);
				m_inputText.text = "[Spell]";
				break;
			case CastAbilityType.Talent:
				sprite = IconLibrary.GetAbilityIcon(ability.AbilityType, returnNullIfNotFound: true);
				m_inputText.text = "[Talent]";
				break;
			}
			if ((bool)sprite)
			{
				m_tempTitleText.gameObject.SetActive(value: false);
				m_icon.gameObject.SetActive(value: true);
				m_icon.sprite = sprite;
			}
			else
			{
				m_tempTitleText.gameObject.SetActive(value: true);
				m_icon.gameObject.SetActive(value: false);
			}
			UpdateAmmo(ability);
			OnManaChangeHandler(null, null);
			BeginCooldown(null, null);
		}
		else
		{
			m_tempTitleText.text = "NULL";
		}
	}

	private void OnAmmoChangeHandler(object sender, EventArgs args)
	{
		IAbility abilityObj = (args as PlayerAmmoChangeEventArgs).AbilityObj;
		if (abilityObj != null && abilityObj.CastAbilityType == m_castAbilityType)
		{
			UpdateAmmo(abilityObj);
		}
	}

	private void OnManaChangeHandler(object sender, EventArgs args)
	{
		if (!m_assignedAbility || m_assignedAbility.ActualCost <= 0)
		{
			return;
		}
		if (PlayerManager.GetPlayerController().CurrentMana < (float)m_assignedAbility.ActualCost)
		{
			m_abilityReadyGO.SetActive(value: false);
			m_cooldownImage.enabled = true;
			return;
		}
		if (!m_abilityReadyGO.activeSelf)
		{
			m_abilityReadyGO.SetActive(value: true);
			m_animator.Play("Intro");
		}
		m_cooldownImage.enabled = false;
	}

	private void UpdateAmmo(IAbility ability)
	{
		m_ammoText.gameObject.SetActive(value: false);
		if (ability.MaxAmmo > 0)
		{
			if (!m_abilityReadyGO.activeSelf)
			{
				m_abilityReadyGO.SetActive(value: true);
			}
			m_ammoText.gameObject.SetActive(value: true);
			m_ammoText.color = Color.green;
			m_ammoText.text = ability.CurrentAmmo.ToString();
			if (ability.CurrentAmmo == ability.MaxAmmo)
			{
				m_ammoText.color = Color.green;
			}
			else if (ability.CurrentAmmo == 0)
			{
				m_ammoText.color = Color.red;
				m_abilityReadyGO.SetActive(value: false);
			}
		}
	}

	private void BeginCooldown(object sender, CooldownEventArgs eventArgs)
	{
		if (base.gameObject.activeInHierarchy)
		{
			StopAllCoroutines();
			StartCoroutine(CooldownCoroutine());
		}
	}

	private IEnumerator CooldownCoroutine()
	{
		if (!(m_assignedAbility.ActualCooldownTime > 0f) || (m_assignedAbility.MaxAmmo != 0 && (m_assignedAbility.MaxAmmo == 0 || m_assignedAbility.CurrentAmmo != 0)))
		{
			yield break;
		}
		m_manaCostText.gameObject.SetActive(value: false);
		if (m_assignedAbility.MaxAmmo > 0)
		{
			m_ammoText.gameObject.SetActive(value: false);
		}
		m_abilityReadyGO.SetActive(value: false);
		m_cooldownImage.enabled = true;
		if (!m_cooldownText.gameObject.activeSelf)
		{
			m_cooldownText.gameObject.SetActive(value: true);
		}
		m_cooldownIcon.gameObject.SetActive(value: true);
		m_trackedValue = 0;
		bool flag = m_assignedAbility.IsOnCooldown && (m_assignedAbility.MaxAmmo == 0 || (m_assignedAbility.MaxAmmo > 0 && m_assignedAbility.CurrentAmmo == 0));
		if (!flag)
		{
			UpdateTrackedValue();
		}
		while (flag)
		{
			if (UpdateTrackedValue())
			{
				if (m_cdChangeTween != null && m_cdChangeTween.isActiveAndEnabled)
				{
					m_cdChangeTween.StopTweenWithConditionChecks(runEndHandlers: false, m_cooldownIcon.gameObject.transform);
				}
				m_cooldownIcon.gameObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
				m_cdChangeTween = TweenManager.TweenTo(m_cooldownIcon.gameObject.transform, 0.25f, Ease.Back.EaseOutLarge, "localScale.x", 1, "localScale.y", 1, "localScale.z", 1);
			}
			yield return null;
			flag = m_assignedAbility.IsOnCooldown && (m_assignedAbility.MaxAmmo == 0 || (m_assignedAbility.MaxAmmo > 0 && m_assignedAbility.CurrentAmmo == 0));
		}
		int actualCost = m_assignedAbility.ActualCost;
		if (actualCost > 0)
		{
			m_manaCostText.gameObject.SetActive(value: true);
			m_manaCostText.text = actualCost.ToString();
		}
		else
		{
			m_manaCostText.gameObject.SetActive(value: false);
		}
		if (m_assignedAbility.MaxAmmo > 0)
		{
			m_ammoText.gameObject.SetActive(value: true);
		}
		m_cooldownText.gameObject.SetActive(value: false);
		m_cooldownIcon.gameObject.SetActive(value: false);
		if (m_assignedAbility.MaxAmmo > 0 || m_assignedAbility.ActualCost <= 0 || (m_assignedAbility.ActualCost > 0 && (float)m_assignedAbility.ActualCost <= PlayerManager.GetPlayerController().CurrentMana))
		{
			m_cooldownImage.enabled = false;
			m_abilityReadyGO.SetActive(value: true);
			m_animator.Play("Intro");
		}
	}

	private bool UpdateTrackedValue()
	{
		m_valueStringBuilder.Clear();
		if (m_assignedAbility.DecreaseCooldownOverTime)
		{
			m_trackedValue = (int)m_assignedAbility.CooldownTimer + 1;
		}
		else
		{
			m_trackedValue = (int)m_assignedAbility.CooldownTimer;
		}
		m_valueStringBuilder.Append(m_trackedValue);
		if (m_assignedAbility.DecreaseCooldownWhenHit && CDGHelper.IsPercent(m_assignedAbility.CooldownTimer))
		{
			float num = (m_assignedAbility.CooldownTimer - (float)m_trackedValue) * 100f;
			if (num > 0f)
			{
				m_valueStringBuilder.AppendFormat("<size=60%><voffset=20>.{0:D2}</voffset></size>", (int)num);
			}
		}
		bool result = false;
		if (m_assignedAbility.DisplayPausedAbilityCooldown)
		{
			if (!m_displayingNullCooldownString)
			{
				m_cooldownText.text = "--";
				m_displayingNullCooldownString = true;
				m_prevValueStringBuilder.Clear();
				result = true;
			}
		}
		else
		{
			m_displayingNullCooldownString = false;
			if (!m_prevValueStringBuilder.Equals(m_valueStringBuilder))
			{
				m_cooldownText.SetText(m_valueStringBuilder);
				m_prevValueStringBuilder.Clear();
				m_prevValueStringBuilder.Append(m_valueStringBuilder);
				result = true;
			}
		}
		return result;
	}

	private void UpdateDisarmedState()
	{
		if (!PlayerManager.IsInstantiated || !m_assignedAbility)
		{
			return;
		}
		if (PlayerManager.GetPlayerController().StatusEffectController.HasStatusEffect(StatusEffectType.Player_Disarmed) || (TraitManager.IsTraitActive(TraitType.CantAttack) && !m_assignedAbility.DealsNoDamage))
		{
			if (!m_disarmedGO.activeSelf)
			{
				m_disarmedGO.SetActive(value: true);
				m_disarmedGO.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
				TweenManager.TweenTo(m_disarmedGO.transform, 0.15f, Ease.Back.EaseInLarge, "localScale.x", 1, "localScale.y", 1);
			}
		}
		else
		{
			m_disarmedGO.SetActive(value: false);
		}
	}
}
