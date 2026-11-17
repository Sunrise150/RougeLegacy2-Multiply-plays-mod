using System.Collections;
using UnityEngine;

public class EnemyHitResponse : BaseCharacterHitResponse
{
	private EnemyController m_enemyController;

	private static float m_timeSlowdownCDTimer;

	private bool m_hasInteractable;

	private WaitRL_Yield m_stunWaitYield;

	protected override void Awake()
	{
		base.Awake();
		m_enemyController = m_charController as EnemyController;
		m_hasInteractable = m_enemyController.GetComponent<Interactable>() != null;
	}

	protected override void HandleHitResponse(IHitboxController otherHBController)
	{
		if ((otherHBController == null || otherHBController.CollisionType != CollisionType.Hazard || !m_enemyController.IsFlying) && (otherHBController.CollisionType != CollisionType.Player || !m_hasInteractable))
		{
			base.HandleHitResponse(otherHBController);
		}
	}

	protected override float CharacterDamaged(IDamageObj damageObj, GameObject otherRootObj, out CriticalStrikeType critType, out float damageBlocked, float damageOverride = -1f, bool trueDamage = false)
	{
		if (damageObj.gameObject.CompareTag("PlayerProjectile") && m_enemyController.CanIncrementRelicHitCounter)
		{
			if (!m_enemyController.TakesNoDamage && damageObj.ActualDamage > 0f)
			{
				RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.BonusDamageOnNextHit);
				if (relic.Level > 0 && relic.IntValue < 6)
				{
					relic.SetIntValue(1, additive: true);
				}
			}
			RelicObj relic2 = SaveManager.PlayerSaveData.GetRelic(RelicType.DamageAuraOnHit);
			if (relic2.Level > 0)
			{
				PlayerController playerController = PlayerManager.GetPlayerController();
				Projectile_RL projectile_RL = damageObj as Projectile_RL;
				if (projectile_RL.CastAbilityType == CastAbilityType.Weapon && !(projectile_RL is DownstrikeProjectile_RL) && playerController.DamageAuraProjectile != projectile_RL)
				{
					if (!playerController.DamageAuraProjectile || !playerController.DamageAuraProjectile.isActiveAndEnabled)
					{
						playerController.DamageAuraProjectile = ProjectileManager.FireProjectile(playerController.gameObject, "RelicDamageAuraOnHitProjectile", new Vector2(0f, 1f));
						playerController.DamageAuraProjectile.CastAbilityType = CastAbilityType.Talent;
						playerController.DamageAuraProjectile.transform.SetParent(playerController.transform);
					}
					else
					{
						playerController.DamageAuraProjectile.LifespanTimer = playerController.DamageAuraProjectile.Lifespan;
					}
					playerController.DamageAuraProjectile.LifespanTimer += 1.5f * (float)(relic2.Level - 1);
				}
			}
		}
		float num = base.CharacterDamaged(damageObj, otherRootObj, out critType, out damageBlocked, damageOverride, trueDamage);
		if (num <= 0f)
		{
			return 0f;
		}
		Vector2 absPos = m_enemyController.Midpoint;
		absPos.y += m_enemyController.CollisionBounds.height / 2f;
		if (!TraitManager.IsTraitActive(TraitType.NoEnemyHealthBar))
		{
			string text = (0f - num).ToString();
			if ((damageObj.gameObject.CompareTag("Player") || damageObj.gameObject.CompareTag("PlayerProjectile") || damageObj.gameObject.CompareTag("EnemyStatusEffect")) && TraitManager.IsTraitActive(TraitType.FakeSelfDamage))
			{
				float num2 = Random.Range(Trait_EV.FAKE_SELF_DAMAGE_DAMAGE_DEALT_MOD.x, Trait_EV.FAKE_SELF_DAMAGE_DAMAGE_DEALT_MOD.y);
				text = ((int)((0f - num) * num2)).ToString();
			}
			foreach (string item in BaseCharacterHitResponse.GLYPHS_TO_ADD_TO_ENEMY_DAMAGE_HELPER)
			{
				text = ((text.Length > 0) ? (text + " [" + item + "]") : (text + "[" + item + "]"));
			}
			TextPopupType popupType = TextPopupType.EnemyHit;
			if (critType == CriticalStrikeType.Regular)
			{
				popupType = TextPopupType.EnemyHit_Critical;
			}
			else if (critType == CriticalStrikeType.Guaranteed)
			{
				popupType = TextPopupType.EnemyHit_GuaranteedCritical;
			}
			else if (critType == CriticalStrikeType.Super)
			{
				popupType = TextPopupType.EnemyHit_SuperCritical;
			}
			TextPopupManager.DisplayTextAtAbsPos(popupType, text, absPos);
		}
		if (m_enemyController.CurrentHealth > 0f)
		{
			if (Time.time > m_timeSlowdownCDTimer && (otherRootObj.CompareTag("Player") || otherRootObj.CompareTag("PlayerProjectile")))
			{
				EffectManager.SetEffectParams("SlowTime_Effect", "m_timeScaleValue", 0.5f);
				EffectManager.PlayEffect(base.gameObject, m_charController.Animator, "SlowTime_Effect", Vector3.zero, 0.065f, EffectStopType.Gracefully);
			}
			m_timeSlowdownCDTimer = Time.time + 0.55f;
		}
		if (!damageObj.IsDotDamage)
		{
			PlayDirectionalHitEffect(damageObj, otherRootObj);
			EffectManager.PlayHitEffectCamShake(0.2f);
		}
		PlayerController playerController2 = PlayerManager.GetPlayerController();
		float num3 = 0f;
		if (!otherRootObj.CompareTag("Hazard"))
		{
			Projectile_RL component = damageObj.gameObject.GetComponent<Projectile_RL>();
			if ((bool)component)
			{
				if (component.CompareTag("PlayerProjectile") && component.CastAbilityType == CastAbilityType.Weapon && !(component is DownstrikeProjectile_RL))
				{
					if (m_enemyController.IsVulnerableToLifeSteal && TraitManager.IsTraitActive(TraitType.Vampire) && m_enemyController.EnemyType != EnemyType.Dummy && m_enemyController.EnemyType != EnemyType.Eggplant)
					{
						num3 += num * 0.2f;
					}
					if (playerController2.CharacterClass.ClassType == ClassType.MagicWandClass && !m_enemyController.DisableHPMPBonuses && !m_enemyController.TakesNoDamage)
					{
						m_enemyController.StatusEffectController.StartStatusEffect(StatusEffectType.Enemy_ManaBurn, 0f, component);
					}
					RelicObj relic3 = SaveManager.PlayerSaveData.GetRelic(RelicType.OnHitAreaDamage);
					int relicMaxStack = Relic_EV.GetRelicMaxStack(relic3.RelicType, relic3.Level);
					if (relic3.Level > 0 && relic3.IntValue >= relicMaxStack)
					{
						Projectile_RL projectile_RL2 = ProjectileManager.FireProjectile(component.Owner, "RelicOnHitAreaDamageProjectile", m_enemyController.Midpoint, matchFacing: true, 0f, 1f, useAbsPos: true);
						projectile_RL2.CastAbilityType = CastAbilityType.Talent;
						projectile_RL2.MagicScale += 0.75f * (float)(relic3.Level - 1);
						projectile_RL2.ActualCritDamage = ProjectileManager.CalculateProjectileCritDamage(projectile_RL2, getMagicCritDmg: true);
						if (m_enemyController.CurrentHealth <= 0f)
						{
							EffectManager.PlayEffect(damageObj.gameObject, null, "RelicOnHitAreaDamage_Chains_Effect", m_enemyController.Midpoint, 0f, EffectStopType.Gracefully);
						}
						relic3.SetIntValue(0, additive: false);
						playerController2.StartOnHitAreaDamageTimer();
					}
				}
				m_enemyController.LastDamageTakenType = component.DamageType;
			}
		}
		if (m_enemyController.CurrentHealth <= 0f && !m_enemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_InvulnWindow))
		{
			if (!m_enemyController.DisableXPBonuses && SkillTreeLogicHelper.IsTotemUnlocked() && !ChallengeManager.IsInChallenge && !Mastery_EV.IsMaxMasteryRank(SaveManager.PlayerSaveData.CurrentCharacter.ClassType, 0, includeRunXP: true))
			{
				int level = m_enemyController.Level;
				int value = Mastery_EV.CalculateRankV2(SaveManager.PlayerSaveData.GetClassXP(playerController2.CharacterClass.ClassType) + SaveManager.PlayerSaveData.RunAccumulatedXP);
				value = Mathf.Clamp(value, 0, Mastery_EV.GetMaxMasteryRank());
				int a = 50 + level * 2;
				a = Mathf.Min(a, Mastery_EV.GetMaxBaseXP(value));
				float num4 = 1f;
				if (!m_enemyController.IsBoss)
				{
					switch (m_enemyController.EnemyRank)
					{
					case EnemyRank.Advanced:
						num4 += 0.25f;
						break;
					case EnemyRank.Expert:
						num4 += 1.5f;
						break;
					case EnemyRank.Miniboss:
						num4 += 4f;
						break;
					}
				}
				else
				{
					num4 += 14f;
				}
				int num5 = Mathf.FloorToInt((float)a * num4);
				num4 = 1f;
				num4 += SkillTreeLogicHelper.GetClassXPMod();
				if (SaveManager.PlayerSaveData.GetRelic(RelicType.NoGoldXPBonus).Level > 0)
				{
					num4 += (1f + Economy_EV.GetGoldGainMod()) * NPC_EV.GetArchitectGoldMod();
					num4 = Mathf.Min(3f, num4);
				}
				int num6 = Mathf.FloorToInt((float)num5 * num4);
				playerController2.UpdateFrameAccumulatedXP(num6);
				playerController2.SetMasteryXP(num6, additive: true);
			}
			if (!damageObj.gameObject.CompareTag("Player"))
			{
				if (m_enemyController.IsVulnerableToLifeSteal)
				{
					if (critType != 0)
					{
						RelicObj relic4 = SaveManager.PlayerSaveData.GetRelic(RelicType.CritKillsHeal);
						num3 += (float)playerController2.ActualMaxHealth * ((float)relic4.Level * 0.06f);
					}
					float value2 = RuneLogicHelper.GetLifeStealPercent() * playerController2.GetActualStatValue(PlayerStat.Strength);
					value2 = Mathf.Clamp(value2, RuneLogicHelper.GetNumLifeStealRunes(), 999f);
					num3 += (float)Mathf.CeilToInt(value2);
					float value3 = RuneLogicHelper.GetSoulStealPercent() * playerController2.GetActualStatValue(PlayerStat.Magic);
					value3 = Mathf.Clamp(value3, RuneLogicHelper.GetNumSoulStealRunes(), 999f);
					num3 += (float)Mathf.CeilToInt(value3);
				}
				if (!m_enemyController.DisableHPMPBonuses && !m_enemyController.IsBoss && m_enemyController.EnemyRank != EnemyRank.Miniboss)
				{
					RelicObj relic5 = SaveManager.PlayerSaveData.GetRelic(RelicType.SpellKillMaxMana);
					if (relic5.Level > 0 && playerController2.CurrentHealthAsInt >= playerController2.ActualMaxHealth)
					{
						int num7 = relic5.Level * 200;
						if (relic5.IntValue < num7)
						{
							relic5.SetIntValue(5, additive: true);
							if (relic5.IntValue > num7)
							{
								relic5.SetIntValue(num7, additive: false);
							}
							float num8 = playerController2.ActualMaxMana;
							playerController2.InitializeManaMods();
							float num9 = (float)playerController2.ActualMaxMana - num8;
							playerController2.SetMana(num9, additive: true, runEvents: true);
							absPos = playerController2.Midpoint;
							absPos.y += playerController2.CollisionBounds.height / 2f;
							string @string = LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_MAX_MANA_UP_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
							TextPopupManager.DisplayTextAtAbsPos(TextPopupType.MPGained, string.Format(@string, num9), absPos);
						}
					}
					RelicObj relic6 = SaveManager.PlayerSaveData.GetRelic(RelicType.FreeEnemyKill);
					if (relic6.Level > 0)
					{
						int num10 = 6;
						num10 -= relic6.Level - 1;
						if (relic6.IntValue >= num10)
						{
							relic6.SetIntValue(0, additive: false);
						}
						else
						{
							relic6.SetIntValue(1, additive: true);
						}
					}
					RelicObj relic7 = SaveManager.PlayerSaveData.GetRelic(RelicType.FreeHitRegenerate);
					if (relic7.Level > 0)
					{
						int num11 = 6;
						num11 -= relic7.Level - 1;
						if (relic7.IntValue < num11)
						{
							relic7.SetIntValue(1, additive: true);
						}
					}
					RelicObj relic8 = SaveManager.PlayerSaveData.GetRelic(RelicType.MagicDamageEnemyCount);
					int num12 = Mathf.RoundToInt(5f) * relic8.Level;
					if (relic8.Level > 0 && relic8.IntValue < num12)
					{
						relic8.SetIntValue(1, additive: true);
						playerController2.InitializeMagicMods();
					}
				}
			}
		}
		if (num3 > 0f && !TraitManager.IsTraitActive(TraitType.MegaHealth))
		{
			int num13 = Mathf.CeilToInt(num3);
			if (num13 > 0)
			{
				playerController2.UpdateFrameAccumulatedLifeSteal(num13);
				playerController2.SetHealth(num13, additive: true, runEvents: true);
				absPos = playerController2.Midpoint;
				absPos.y += playerController2.CollisionBounds.height / 2f;
				EffectManager.PlayEffect(m_enemyController.gameObject, m_enemyController.Animator, "LifestealBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully);
			}
		}
		return num;
	}

	private IEnumerator ResetStunAnimState()
	{
		m_enemyController.Animator.SetBool(base.StunnedAnimParamName, value: true);
		if (m_stunWaitYield == null)
		{
			m_stunWaitYield = new WaitRL_Yield(0f);
		}
		m_stunWaitYield.CreateNew(0.15f);
		yield return m_stunWaitYield;
		m_enemyController.Animator.SetBool(base.StunnedAnimParamName, value: false);
	}

	protected override void CharacterKnockedBack(IDamageObj damageObj, GameObject otherRootObj)
	{
		Vector2 vector = damageObj?.ExternalKnockbackMod ?? Vector2.one;
		Vector2 internalKnockbackMod = m_enemyController.InternalKnockbackMod;
		Vector2 eNEMY_BASE_KNOCKBACK_DISTANCE = Enemy_EV.ENEMY_BASE_KNOCKBACK_DISTANCE;
		eNEMY_BASE_KNOCKBACK_DISTANCE *= internalKnockbackMod * vector;
		if (TraitManager.IsTraitActive(TraitType.EnemyKnockedLow))
		{
			eNEMY_BASE_KNOCKBACK_DISTANCE *= Trait_EV.ENEMY_KNOCKED_LOW_KNOCKBACK_MOD;
		}
		else if (TraitManager.IsTraitActive(TraitType.EnemyKnockedFar))
		{
			eNEMY_BASE_KNOCKBACK_DISTANCE *= Trait_EV.ENEMY_KNOCKED_FAR_KNOCKBACK_MOD;
		}
		if (!(eNEMY_BASE_KNOCKBACK_DISTANCE != Vector2.zero))
		{
			return;
		}
		if (m_enemyController.IsFlying)
		{
			Vector2 vector2 = base.gameObject.transform.position - otherRootObj.transform.position;
			vector2.Normalize();
			if ((bool)m_enemyController.GetComponent<BounceCollision>())
			{
				float magnitude = m_enemyController.Velocity.magnitude;
				m_enemyController.SetVelocity(vector2.x * magnitude, vector2.y * magnitude, additive: false);
				return;
			}
			if (m_enemyController.FlyingMovementType == FlyingMovementType.Override)
			{
				m_enemyController.FlyingMovementType = FlyingMovementType.Stop;
			}
			float magnitude2 = eNEMY_BASE_KNOCKBACK_DISTANCE.magnitude;
			m_enemyController.FlightKnockbackAcceleration = vector2 * magnitude2;
		}
		else
		{
			PerformKnockbackPush(otherRootObj, eNEMY_BASE_KNOCKBACK_DISTANCE);
		}
	}
}
