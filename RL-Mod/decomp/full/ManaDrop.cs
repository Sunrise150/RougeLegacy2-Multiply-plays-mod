using UnityEngine;
using UnityEngine.Events;

public class ManaDrop : BaseItemDrop
{
	private static HealthChangeEventArgs m_healthChangeArgs_STATIC;

	[SerializeField]
	private UnityEvent m_maxManaMessageShownUnityEvent;

	public override ItemDropType ItemDropType => ItemDropType.ManaDrop;

	protected override void Collect(GameObject collector)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (playerController != null)
		{
			float num = 0.5f;
			int num2 = Mathf.CeilToInt((float)playerController.ActualMaxMana * num);
			if (SkillTreeManager.GetSkillObjLevel(SkillTreeType.Potions_Free_Cast_Up) > 0)
			{
				playerController.SpellOrbs = 1;
			}
			playerController.SetMana(num2, additive: true, runEvents: true);
			Vector2 absPos = playerController.Midpoint;
			absPos.y += playerController.CollisionBounds.height / 2f;
			string text = null;
			if (playerController.CurrentManaAsInt >= playerController.ActualMaxMana)
			{
				if (m_maxManaMessageShownUnityEvent != null)
				{
					m_maxManaMessageShownUnityEvent.Invoke();
				}
				text = LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_MAX_MANA_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			}
			else
			{
				text = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_MANA_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), num2);
			}
			TextPopupManager.DisplayTextAtAbsPos(TextPopupType.MPGained, text, absPos);
			float armorHealthMod = RuneLogicHelper.GetArmorHealthMod();
			if (armorHealthMod > 0f)
			{
				int num3 = Mathf.CeilToInt(armorHealthMod * (float)playerController.ActualArmor);
				int num4 = Mathf.CeilToInt(armorHealthMod * (float)playerController.ActualMaxHealth);
				int num5 = playerController.ActualArmor - playerController.CurrentArmor;
				playerController.CurrentArmor += num3;
				playerController.SetHealth(num4, additive: true, runEvents: true);
				string text2 = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_HEALTH_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), num4);
				if (num5 > 0)
				{
					num3 = Mathf.Min(num3, num5);
					text2 += string.Format(" <size=75%><color=grey>(" + LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_ARMOR_RESTORED_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale) + ")</color></size>", num3);
				}
				absPos.y += 0.75f;
				TextPopupManager.DisplayTextAtAbsPos(TextPopupType.HPGained, text2, absPos);
			}
			if (SkillTreeManager.GetSkillObjLevel(SkillTreeType.Potion_Recharge_Talent) > 0)
			{
				BaseAbility_RL ability = playerController.CastAbility.GetAbility(CastAbilityType.Talent);
				if ((bool)ability && ability.AbilityType == AbilityType.CookingTalent && ability.CurrentAmmo < ability.MaxAmmo)
				{
					int num6 = 1;
					string text3 = ((ability.CurrentAmmo >= ability.MaxAmmo - num6) ? LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_COOK_TALENT_CHARGE_MAX_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale) : string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_COOK_TALENT_CHARGE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), num6));
					absPos.y -= 1.5f;
					TextPopupManager.DisplayTextAtAbsPos(TextPopupType.DownstrikeAmmoGain, text3, absPos);
				}
				playerController.CastAbility.ResetAbilityCooldowns(CastAbilityType.Talent);
			}
			RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.ManaDamageReduction);
			if (relic.Level > 0 && relic.IntValue > 0)
			{
				relic.SetIntValue(-1, additive: true);
			}
			HealthDrop.IncrementFoodChallengeRelicCounter(this);
		}
		base.Collect(collector);
	}
}
