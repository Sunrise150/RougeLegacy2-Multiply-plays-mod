using System.Collections;
using RL_Windows;
using UnityEngine;

public class PlayerHitResponse : BaseCharacterHitResponse
{
	private PlayerController m_playerController;

	private PlayerBlockedEventArgs m_playerBlockedEventArgs;

	private float m_stunStartTime;

	private WaitRL_Yield m_challengeDeathYield;

	public override bool TakesDamageWhileStunned
	{
		get
		{
			if (TraitManager.IsTraitActive(TraitType.NoImmunityWindow))
			{
				return true;
			}
			return base.TakesDamageWhileStunned;
		}
	}

	public override bool TriggerInvincibilityAfterStun
	{
		get
		{
			if (TraitManager.IsTraitActive(TraitType.NoImmunityWindow))
			{
				return false;
			}
			return base.TriggerInvincibilityAfterStun;
		}
	}

	public override bool IsInvincible
	{
		get
		{
			if (!base.IsInvincible)
			{
				return !RewiredMapController.IsMapEnabled(GameInputMode.Game);
			}
			return true;
		}
	}

	public bool CanRecoverFromStun => Time.time > m_stunStartTime + 0.15f;

	protected override void Awake()
	{
		base.Awake();
		m_playerBlockedEventArgs = new PlayerBlockedEventArgs(null);
		m_playerController = m_charController as PlayerController;
		m_challengeDeathYield = new WaitRL_Yield(1.5f, useUnscaledTime: true);
	}

	public override void StartHitResponse(GameObject otherRootGameObj, IDamageObj damageObj, float damageOverride = -1f, bool trueDamage = false, bool fireEvents = true)
	{
		if (m_playerController.IsSpearSpinning)
		{
			Projectile_RL projectile_RL = damageObj as Projectile_RL;
			if ((bool)projectile_RL && projectile_RL.CompareTag("EnemyProjectile") && (projectile_RL.CollisionFlags & ProjectileCollisionFlag.ReflectWeak) != 0)
			{
				return;
			}
		}
		base.StartHitResponse(otherRootGameObj, damageObj, damageOverride, trueDamage, fireEvents);
	}

	protected override void HandleHitResponse(IHitboxController otherHBController)
	{
		if (m_playerController.CharacterDownStrike.IsTriggeringBounce)
		{
			return;
		}
		if (SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockVoidDash) > 0 && m_playerController.CharacterDash.IsDashing)
		{
			if (CollisionType_RL.IsProjectile(otherHBController.RootGameObject))
			{
				Projectile_RL component = otherHBController.RootGameObject.GetComponent<Projectile_RL>();
				if ((bool)component && (component.CollisionFlags & ProjectileCollisionFlag.VoidDashable) != 0)
				{
					return;
				}
			}
			else if (otherHBController.RootGameObject.CompareTag("Breakable"))
			{
				if ((bool)otherHBController.RootGameObject.GetComponent<Breakable_VoidDashOnly>())
				{
					return;
				}
			}
			else if (otherHBController.RootGameObject.CompareTag("Enemy") && (bool)otherHBController.RootGameObject.GetComponent<EnemyVoidDashable>())
			{
				return;
			}
		}
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.NoSpikeDamage).Level > 0 && otherHBController.RootGameObject.CompareTag("Hazard") && (bool)otherHBController.RootGameObject.GetComponent<TallSpike_Hazard>())
		{
			return;
		}
		if (((SaveManager.PlayerSaveData.EnableHouseRules && SaveManager.PlayerSaveData.Assist_DisableEnemyContactDamage) || m_playerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_NoContactDamage)) && otherHBController.RootGameObject.CompareTag("Enemy"))
		{
			EnemyController component2 = otherHBController.RootGameObject.GetComponent<EnemyController>();
			if ((bool)component2 && !component2.AlwaysDealsContactDamage && !component2.AttackingWithContactDamage)
			{
				otherHBController.RemoveFromRepeatHitChecks(m_playerController.gameObject);
				return;
			}
		}
		base.HandleHitResponse(otherHBController);
	}

	protected override float CharacterDamaged(IDamageObj damageObj, GameObject otherRootObj, out CriticalStrikeType critType, out float damageBlocked, float damageOverride = -1f, bool trueDamage = false)
	{
		m_blinkOnHit = true;
		bool flag = false;
		float num = base.CharacterDamaged(damageObj, otherRootObj, out critType, out damageBlocked, damageOverride, trueDamage);
		if (num > 0f)
		{
			RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.BonusDamageOnNextHit);
			if (relic.Level > 0)
			{
				relic.SetIntValue(0, additive: false);
			}
			RelicObj relic2 = SaveManager.PlayerSaveData.GetRelic(RelicType.DamageNoHitChallenge);
			if (relic2.Level > 0 && (!SaveManager.PlayerSaveData.InHubTown || ChallengeManager.IsInChallenge))
			{
				SaveManager.PlayerSaveData.GetRelic(RelicType.DamageNoHitChallengeUsed).SetLevel(relic2.Level, additive: true);
				relic2.SetLevel(0, additive: false);
				flag = true;
			}
			RelicObj relic3 = SaveManager.PlayerSaveData.GetRelic(RelicType.NoAttackDamageBonus);
			if (relic3.Level > 0)
			{
				relic3.SetIntValue(0, additive: false);
				m_playerController.StartNoAttackDamageBonusTimer();
			}
			RelicObj relic4 = SaveManager.PlayerSaveData.GetRelic(RelicType.MagicDamageEnemyCount);
			if (relic4.Level > 0)
			{
				relic4.SetIntValue(0, additive: false);
			}
		}
		if (m_playerController.CurrentHealth <= 0f)
		{
			return num;
		}
		if (num > 0f && TraitManager.IsTraitActive(TraitType.DisarmOnHurt))
		{
			m_playerController.StatusEffectController.StartStatusEffect(StatusEffectType.Player_Disarmed, 3.5f, null);
		}
		if (num > 0f && ChallengeManager.IsInChallenge)
		{
			ChallengeManager.HitsTaken++;
		}
		if (m_playerController.IsBlocking)
		{
			m_playerBlockedEventArgs.Initialize(damageObj);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerBlocked, this, m_playerBlockedEventArgs);
			if (Time.time < m_playerController.BlockStartTime + 0.135f)
			{
				base.BlinkPulseEffect.StartSingleBlinkEffect(Color.yellow);
			}
			else
			{
				base.BlinkPulseEffect.StartSingleBlinkEffect(Color.white);
			}
			m_blinkOnHit = false;
		}
		Vector2 vector = m_playerController.Midpoint;
		vector.y += m_playerController.CollisionBounds.height / 2f;
		string text = "";
		if (TraitManager.IsTraitActive(TraitType.NoHealthBar))
		{
			text = "???";
		}
		else if (TraitManager.IsTraitActive(TraitType.FakeSelfDamage))
		{
			float num2 = Random.Range(Trait_EV.FAKE_SELF_DAMAGE_DAMAGE_TAKEN_MOD.x, Trait_EV.FAKE_SELF_DAMAGE_DAMAGE_TAKEN_MOD.y);
			text = ((int)((0f - num) * num2)).ToString();
		}
		else if (num > 0f || damageBlocked > 0f)
		{
			string text2 = "";
			if (num > 0f)
			{
				text2 += 0f - num;
			}
			if (damageBlocked > 0f)
			{
				text2 = ((!string.IsNullOrEmpty(text2)) ? (text2 + " <size=75%><color=grey>(" + (0f - damageBlocked) + ")</color></size>") : (text2 + "<size=75%><color=grey>(" + (0f - damageBlocked) + ")</color></size>"));
			}
			text = text2;
		}
		foreach (string item in BaseCharacterHitResponse.GLYPHS_TO_ADD_TO_PLAYER_DAMAGE_HELPER)
		{
			text = ((text.Length > 0) ? (text + " [" + item + "]") : (text + "[" + item + "]"));
		}
		TextPopupType popupType = TextPopupType.PlayerHit;
		if (num > 0f)
		{
			if (critType == CriticalStrikeType.Regular)
			{
				popupType = TextPopupType.PlayerHit_Critical;
			}
			else if (critType == CriticalStrikeType.Guaranteed)
			{
				popupType = TextPopupType.PlayerHit_GuaranteedCritical;
			}
			else if (critType == CriticalStrikeType.Super)
			{
				popupType = TextPopupType.PlayerHit_SuperCritical;
			}
		}
		TextPopupManager.DisplayTextAtAbsPos(popupType, text, vector);
		if (num > 0f)
		{
			if (flag)
			{
				TextPopupManager.DisplayLocIDText(TextPopupType.OutOfAmmo, "LOC_ID_RELIC_UI_RELIC_BROKEN_1", StringGenderType.Male, vector + Vector2.up * 1.5f);
			}
			int level = SaveManager.PlayerSaveData.GetRelic(RelicType.ManaRestoreOnHurt).Level;
			if (level > 0)
			{
				float num3 = m_playerController.ActualMaxMana * 2;
				float num4 = Mathf.Clamp(100 * level, 0f, num3 - (float)m_playerController.CurrentManaAsInt);
				float f = Mathf.Min(m_playerController.CurrentMana + num4, num3);
				f = Mathf.CeilToInt(f);
				m_playerController.SetMana(f, additive: false, runEvents: true, canExceedMax: true);
				string text3 = ((!(num4 <= 0f)) ? string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_MANA_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), num4) : LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_MAX_MANA_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale));
				text3 += "  [ManaRestoreOnHurt]";
				TextPopupManager.DisplayText(TextPopupType.MPGained, text3, m_playerController.gameObject, vector + Vector2.down, attachToSource: true);
			}
		}
		if (!damageObj.IsDotDamage && !m_playerController.IsBlocking)
		{
			EffectManager.SetEffectParams("SlowTime_Effect", "m_timeScaleValue", 0.1f);
			EffectManager.PlayEffect(base.gameObject, m_playerController.Animator, "SlowTime_Effect", Vector3.zero, 0.5f, EffectStopType.Gracefully);
		}
		if (!m_playerController.IsBlocking)
		{
			BaseEffect baseEffect = EffectManager.PlayEffect(CameraController.ForegroundOrthoCam.gameObject, null, "ScreenDamageEffect", Vector3.zero, 0f, EffectStopType.Gracefully);
			baseEffect.transform.SetParent(CameraController.ForegroundOrthoCam.transform, worldPositionStays: false);
			Vector3 localScale = Vector3.one * CameraController.ZoomLevel * 2f;
			localScale.x *= AspectRatioManager.CurrentGameAspectRatio / 1.7777778f;
			baseEffect.transform.localScale = localScale;
			baseEffect.DisableDestroyOnRoomChange = true;
		}
		if (otherRootObj.CompareTag("Enemy"))
		{
			EnemyController component = otherRootObj.GetComponent<EnemyController>();
			if ((bool)component)
			{
				if (num > 0f)
				{
					float num5 = 0f;
					int runeEquippedLevel = RuneManager.GetRuneEquippedLevel(RuneType.ReturnDamage);
					runeEquippedLevel += Mathf.RoundToInt(EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.ReturnDamage));
					num5 += RuneLogicHelper.GetDamageReturnPercent(runeEquippedLevel) * m_playerController.GetActualStatValue(PlayerStat.Vitality);
					if (num5 > 0f)
					{
						if (TraitManager.IsTraitActive(TraitType.SkillCritsOnly))
						{
							num5 = 0f;
						}
						component.CharacterHitResponse.StartHitResponse(m_charController.gameObject, m_playerController, num5);
					}
				}
				float num6 = 0f;
				num6 += num * BurdenManager.GetBurdenStatGain(BurdenType.EnemyLifesteal);
				if (num6 > 0f)
				{
					int num7 = Mathf.CeilToInt(num6);
					if (num7 > 0 && component.CurrentHealthAsInt > 0)
					{
						vector = component.Midpoint;
						vector.y += component.CollisionBounds.height / 2f;
						string text4 = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_HEALTH_RESTORE_1", isFemale: false), num7);
						TextPopupManager.DisplayTextAtAbsPos(TextPopupType.HPGained, text4, vector);
						component.SetHealth(num7, additive: true, runEvents: true);
						((BurstEffect)EffectManager.PlayEffect(m_playerController.gameObject, null, "LifestealBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully)).DestinationOverride = component.transform;
					}
				}
				if (component.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Disarm))
				{
					m_playerController.StatusEffectController.StartStatusEffect(StatusEffectType.Player_Disarmed, 0f, damageObj);
				}
			}
		}
		if (m_playerController.IsMushroomBig)
		{
			m_playerController.SetMushroomBig(setMushroomBig: false, animate: true);
		}
		return num;
	}

	protected override void CharacterStunned(IDamageObj damageObj, GameObject otherRootObj)
	{
		m_playerController.StopActiveAbilities(stopPersistentAbilities: false);
		m_stunStartTime = Time.time;
		base.CharacterStunned(damageObj, otherRootObj);
		if (TraitManager.IsTraitActive(TraitType.NoImmunityWindow))
		{
			SetInvincibleTime(0.1f, additive: true, playInvincibility: false);
		}
	}

	protected override void CharacterKnockedBack(IDamageObj damageObj, GameObject otherRootObj)
	{
		if (m_playerController.CurrentlyInRoom != null)
		{
			float num = 1f;
			if (TraitManager.IsTraitActive(TraitType.FakeSelfDamage))
			{
				num = 2.5f;
			}
			if (!SaveManager.ConfigData.DisableSlowdownOnHit)
			{
				EffectManager.SetEffectParams("CameraShake_Effect_Template", "m_shakeSpeed", 10f * num, "m_shakeAmplitude", 10.5f * num);
			}
			else
			{
				EffectManager.SetEffectParams("CameraShake_Effect_Template", "m_shakeSpeed", 5f * num, "m_shakeAmplitude", 5.5f * num);
			}
			EffectManager.PlayEffect(m_playerController.CurrentlyInRoom.gameObject, null, "CameraShake_Effect_Template", Vector3.zero, 0.3f, EffectStopType.Gracefully);
		}
		Vector2 pLAYER_BASE_KNOCKBACK_DISTANCE = Player_EV.PLAYER_BASE_KNOCKBACK_DISTANCE;
		Vector2 vector = damageObj?.ExternalKnockbackMod ?? Vector2.one;
		Vector2 internalKnockbackMod = m_playerController.InternalKnockbackMod;
		pLAYER_BASE_KNOCKBACK_DISTANCE *= internalKnockbackMod * vector;
		if (TraitManager.IsTraitActive(TraitType.PlayerKnockedLow))
		{
			pLAYER_BASE_KNOCKBACK_DISTANCE *= Trait_EV.PLAYER_KNOCKED_LOW_KNOCKBACK_MOD;
		}
		else if (TraitManager.IsTraitActive(TraitType.PlayerKnockedFar))
		{
			pLAYER_BASE_KNOCKBACK_DISTANCE *= Trait_EV.PLAYER_KNOCKED_FAR_KNOCKBACK_MOD;
		}
		if (pLAYER_BASE_KNOCKBACK_DISTANCE != Vector2.zero)
		{
			PerformKnockbackPush(otherRootObj, pLAYER_BASE_KNOCKBACK_DISTANCE);
		}
	}

	protected override void CharacterKilled(IDamageObj damageObj, GameObject otherRootObj)
	{
		DeathDefiedType deathDefiedType = SkillTreeLogicHelper.IsDeathDefied();
		TextPopupManager.DisableAllTextPopups();
		switch (deathDefiedType)
		{
		case DeathDefiedType.Death_Dodge:
		case DeathDefiedType.ExtraLife:
		case DeathDefiedType.ExtraLife_Unity:
		{
			AnimatorControllerParameter[] parameters = m_playerController.Animator.parameters;
			foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
			{
				switch (animatorControllerParameter.type)
				{
				case AnimatorControllerParameterType.Bool:
					m_playerController.Animator.SetBool(animatorControllerParameter.name, value: false);
					break;
				case AnimatorControllerParameterType.Trigger:
					m_playerController.Animator.ResetTrigger(animatorControllerParameter.name);
					break;
				}
			}
			m_playerController.StopActiveAbilities(stopPersistentAbilities: true);
			WindowManager.SetWindowIsOpen(WindowID.DeathDefy, isOpen: true);
			(WindowManager.GetWindowController(WindowID.DeathDefy) as DeathDefyWindowController).DeathDefiedType = deathDefiedType;
			CharacterStunned(damageObj, otherRootObj);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerFakedDeath, null, null);
			if (m_playerController.UseOverrideDefaultTint && m_playerController.DefaultTintOverrideColor == Color.black && !TraitManager.IsTraitActive(TraitType.SmallHitbox))
			{
				StartCoroutine(FakeDeathTintCoroutine());
			}
			break;
		}
		case DeathDefiedType.FailedChallenge:
			m_playerController.StopActiveAbilities(stopPersistentAbilities: true);
			m_playerController.ControllerCorgi.GravityActive(state: false);
			m_playerController.ControllerCorgi.SetForce(Vector2.zero);
			SetInvincibleTime(9999f, additive: false, playInvincibility: false);
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
			StartCoroutine(ChallengeDeathSlowdownCoroutine());
			break;
		default:
			if (m_playerController.UseOverrideDefaultTint && m_playerController.DefaultTintOverrideColor == Color.black && !TraitManager.IsTraitActive(TraitType.SmallHitbox))
			{
				EffectManager.SetEffectParams("RemoveTint_Effect", "m_useUnscaledTime", true);
				EffectManager.PlayEffect(m_playerController.gameObject, m_playerController.Animator, "RemoveTint_Effect", Vector3.zero, 0.25f, EffectStopType.Gracefully);
			}
			base.CharacterKilled(damageObj, otherRootObj);
			break;
		}
	}

	private IEnumerator FakeDeathTintCoroutine()
	{
		EffectManager.SetEffectParams("RemoveTint_Effect", "m_useUnscaledTime", true);
		EffectManager.PlayEffect(m_playerController.gameObject, m_playerController.Animator, "RemoveTint_Effect", Vector3.zero, 0.25f, EffectStopType.Gracefully);
		while (WindowManager.GetIsWindowOpen(WindowID.DeathDefy))
		{
			yield return null;
		}
		EffectManager.SetEffectParams("AddBlackTint_Effect", "m_useUnscaledTime", true);
		EffectManager.PlayEffect(m_playerController.gameObject, m_playerController.Animator, "AddBlackTint_Effect", Vector3.zero, 0.25f, EffectStopType.Gracefully);
	}

	private IEnumerator ChallengeDeathSlowdownCoroutine()
	{
		RLTimeScale.SetTimeScale(TimeScaleType.Game, 0.25f);
		yield return m_challengeDeathYield;
		RLTimeScale.Reset();
		ChallengeManager.ReturnToDriftHouseWithTransition();
	}
}
