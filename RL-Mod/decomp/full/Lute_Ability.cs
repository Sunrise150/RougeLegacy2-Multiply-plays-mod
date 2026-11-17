using UnityEngine;

public class Lute_Ability : AimedAbilityFast_RL, IAttack, IAbility
{
	[SerializeField]
	private GameObject m_endAimIndicatorPivot;

	protected override float TellIntroAnimSpeed => 3f;

	protected override float TellIntroAnimExitDelay => 0.01f;

	protected override float TellAnimSpeed => 1.25f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1.25f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.25f;

	protected override float AttackAnimExitDelay => 0.175f;

	protected override float ExitAnimSpeed => 1.25f;

	protected override float ExitAnimExitDelay => 0f;

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		if ((bool)m_endAimIndicatorPivot)
		{
			m_endAimIndicatorPivot.transform.GetChild(0).transform.localPosition = new Vector3(m_aimLineLength + 0.5f, 0f, 0f);
		}
		base.Initialize(abilityController, castAbilityType);
	}

	protected override void FireProjectile()
	{
		Vector2 vector = CDGHelper.AngleToVector(0f - m_aimAngle);
		if (m_abilityController.PlayerController.IsFacingRight)
		{
			vector.x = 0f - vector.x;
		}
		float aimAngle = m_aimAngle;
		Vector2 vector2 = CDGHelper.RotatedPoint(new Vector2(ProjectileOffset.x, 0f), Vector2.zero, aimAngle);
		vector2.y += ProjectileOffset.y;
		base.FireProjectile();
		if (m_firedProjectile.transform.localScale.x < 0f)
		{
			m_firedProjectile.Flip();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (m_isAiming && (bool)m_endAimIndicatorPivot)
		{
			m_endAimIndicatorPivot.transform.SetLocalEulerZ(m_unmoddedAngle);
		}
	}
}
