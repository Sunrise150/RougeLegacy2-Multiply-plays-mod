using UnityEngine;

public class DragonPistolWeapon_Ability : PistolWeapon_Ability
{
	[SerializeField]
	private string m_fireballProjectileName;

	protected override bool CanManuallyReload => false;

	public override string ProjectileName
	{
		get
		{
			if (base.CurrentAmmo > 5)
			{
				return base.ProjectileName;
			}
			if (base.CurrentAmmo > 0)
			{
				return m_fireballProjectileName;
			}
			return m_emptyClipProjectileName;
		}
	}

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[3] { m_projectileName, m_fireballProjectileName, m_emptyClipProjectileName };
	}

	protected override void FireProjectile()
	{
		if (base.CurrentAmmo > 5)
		{
			m_shootAudioEmitter.Play();
		}
		else if (base.CurrentAmmo <= 0)
		{
			m_emptyClipAudioEmitter.Play();
		}
		Base_FireProjectile();
		int level = SaveManager.PlayerSaveData.GetRelic(RelicType.AttackCooldown).Level;
		if (level > 0)
		{
			m_firedProjectile.LifespanTimer += (float)level * 0f;
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		base.StopAbility(abilityInterrupted);
		base.CurrentAmmo = base.MaxAmmo;
	}
}
