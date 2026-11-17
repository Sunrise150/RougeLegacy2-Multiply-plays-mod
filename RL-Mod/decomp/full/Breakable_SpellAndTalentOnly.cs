public class Breakable_SpellAndTalentOnly : Breakable
{
	protected override void TriggerCollision(IDamageObj damageObj)
	{
		if (!damageObj.gameObject.CompareTag("PlayerProjectile"))
		{
			return;
		}
		Projectile_RL projectile_RL = damageObj as Projectile_RL;
		if (projectile_RL != null)
		{
			bool num = projectile_RL.CastAbilityType == CastAbilityType.Spell || projectile_RL.CastAbilityType == CastAbilityType.Talent;
			bool flag = projectile_RL.CastAbilityType == CastAbilityType.Weapon && TraitManager.IsTraitActive(TraitType.CantAttack);
			if (num || flag)
			{
				base.TriggerCollision(damageObj);
			}
		}
	}
}
