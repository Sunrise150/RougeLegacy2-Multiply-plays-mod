public class StudyBoss_Advanced_AIScript : StudyBoss_Basic_AIScript
{
	protected override bool m_advancedAttack => true;

	protected override int m_verticalBeamAttackCount => 3;

	protected override float m_verticalBeamLifetime => 11f;

	protected override int m_dashAttackFireballCountAtEnd => 4;

	protected override int m_shieldAttackProjectileCount => 10;
}
