public class SpellswordBoss_Advanced_AIScript : SpellswordBoss_Basic_AIScript
{
	protected override bool UseVariant => true;

	protected override int m_numStaffFireballs_AddSecondMode => 6;

	protected override int m_numDaggerThrowDaggers_SecondModeAdd => 2;
}
