public class FlyingBurst_Expert_AIScript : FlyingBurst_Basic_AIScript
{
	protected override float MeleeFireSpread => 180f;

	protected override int NumFireballsMelee => 6;

	protected override int NumFireballsBasic => 0;

	protected override int m_bigShake_NumFireballsBasic => 2;

	protected override float m_bigShake_BasicFireSpread => 50f;
}
