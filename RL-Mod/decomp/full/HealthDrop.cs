using UnityEngine;

public class HealthDrop : BaseItemDrop
{
	private static MaxHealthChangeEventArgs m_healthChangeArgs_STATIC = new MaxHealthChangeEventArgs(0f, 0f);

	private static RelicChangedEventArgs m_relicChangedEventArgs_STATIC = new RelicChangedEventArgs(RelicType.None);

	public override ItemDropType ItemDropType => ItemDropType.HealthDrop;

	protected override void Collect(GameObject collector)
	{
		GainHealth();
		base.Collect(collector);
	}

	protected virtual void GainHealth(float hpGain = 0f)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (!playerController)
		{
			return;
		}
		if (hpGain == 0f)
		{
			float num = (float)SaveManager.PlayerSaveData.GetRelic(RelicType.FoodHealsMore).Level * 0.08f;
			hpGain = (float)playerController.ActualMaxHealth * (0f + num);
			float num2 = playerController.ActualMagic * 2f * (1f + SkillTreeLogicHelper.GetPotionMods());
			hpGain += num2;
		}
		RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.MeatMaxHealth);
		if (relic.IntValue < 3 * relic.Level && playerController.CurrentHealth >= (float)playerController.ActualMaxHealth)
		{
			float num3 = 0.1f;
			int actualMaxHealth = playerController.ActualMaxHealth;
			SaveManager.PlayerSaveData.TemporaryMaxHealthMods += num3;
			playerController.InitializeHealthMods();
			int actualMaxHealth2 = playerController.ActualMaxHealth;
			playerController.SetHealth(playerController.ActualMaxHealth, additive: false, runEvents: true);
			m_healthChangeArgs_STATIC.Initialise(actualMaxHealth2, actualMaxHealth);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerMaxHealthChange, this, m_healthChangeArgs_STATIC);
			relic.SetIntValue(1, additive: true);
		}
		IncrementFoodChallengeRelicCounter(this);
		bool flag = true;
		BaseAbility_RL ability = playerController.CastAbility.GetAbility(CastAbilityType.Talent);
		if (TraitManager.IsTraitActive(TraitType.NoMeat) && ItemDropType != ItemDropType.MushroomDrop && ItemDropType != ItemDropType.CandyDrop && ItemDropType != ItemDropType.CookieDrop)
		{
			playerController.CharacterHitResponse.StartHitResponse(playerController.gameObject, playerController, hpGain * 1f);
			flag = false;
		}
		Vector2 absPos = playerController.Midpoint;
		absPos.y += playerController.CollisionBounds.height / 2f;
		if ((bool)ability && ability.AbilityType == AbilityType.CookingTalent && ability.CurrentAmmo < ability.MaxAmmo)
		{
			int num4 = 1;
			string text = ((ability.CurrentAmmo >= ability.MaxAmmo - num4) ? LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_COOK_TALENT_CHARGE_MAX_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale) : string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_COOK_TALENT_CHARGE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), num4));
			TextPopupManager.DisplayTextAtAbsPos(TextPopupType.DownstrikeAmmoGain, text, absPos);
			absPos.y += playerController.CollisionBounds.height / 2f;
		}
		if (flag)
		{
			hpGain = Mathf.CeilToInt(hpGain);
			if (TraitManager.IsTraitActive(TraitType.MegaHealth))
			{
				hpGain = 0f;
			}
			playerController.SetHealth(hpGain, additive: true, runEvents: true);
			string text2 = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_HEALTH_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), (int)hpGain);
			TextPopupManager.DisplayTextAtAbsPos(TextPopupType.HPGained, text2, absPos);
		}
	}

	public static void IncrementFoodChallengeRelicCounter(MonoBehaviour itemDrop)
	{
		RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.FoodChallenge);
		if (relic.Level > 0)
		{
			relic.SetIntValue(1, additive: true);
			if (relic.IntValue >= 2)
			{
				PlayerController playerController = PlayerManager.GetPlayerController();
				float num = playerController.ActualMaxHealth;
				relic.SetIntValue(0, additive: false);
				relic.SetLevel(-1, additive: true);
				RelicObj relic2 = SaveManager.PlayerSaveData.GetRelic(RelicType.FoodChallengeUsed);
				relic2.SetLevel(1, additive: true);
				m_relicChangedEventArgs_STATIC.Initialize(RelicType.FoodChallengeUsed);
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RelicPurified, itemDrop, m_relicChangedEventArgs_STATIC);
				relic2.SetFloatValue((float)playerController.ActualMaxHealth - num, additive: false);
			}
		}
	}
}
