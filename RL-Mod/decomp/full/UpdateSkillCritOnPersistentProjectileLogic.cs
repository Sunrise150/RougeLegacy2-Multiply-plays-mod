using MoreMountains.CorgiEngine;
using UnityEngine;

public class UpdateSkillCritOnPersistentProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private bool m_critsWhenDashing;

	protected override void Awake()
	{
		base.Awake();
		base.SourceProjectile.IsPersistentProjectile = true;
	}

	private void FixedUpdate()
	{
		bool flag = false;
		bool flag2 = base.SourceProjectile.ActualCritChance >= 100f;
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (playerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_FreeCrit))
		{
			flag = true;
		}
		if (!flag && m_critsWhenDashing && playerController.MovementState == CharacterStates.MovementStates.Dashing)
		{
			flag = true;
		}
		if (!flag && playerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_Combo))
		{
			BaseStatusEffect statusEffect = playerController.StatusEffectController.GetStatusEffect(StatusEffectType.Player_Combo);
			if ((bool)statusEffect && statusEffect.IsPlaying && statusEffect.TimesStacked >= 15)
			{
				flag = true;
			}
		}
		if (flag && !flag2)
		{
			base.SourceProjectile.ActualCritChance += 100f;
			base.SourceProjectile.ChangeSpriteRendererColor(ProjectileLibrary.GetBuffColor(ProjectileBuffType.PlayerDashAttack));
		}
		else if (!flag && flag2)
		{
			base.SourceProjectile.ActualCritChance -= 100f;
			base.SourceProjectile.ResetSpriteRendererColor();
		}
	}
}
