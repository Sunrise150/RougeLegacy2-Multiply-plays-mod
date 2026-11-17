public class Puzzle_FairyRule : FairyRule
{
	public override string Description => "LOC_ID_FAIRY_RULE_PUZZLE_1";

	public override FairyRuleID ID => FairyRuleID.Puzzle;

	public override bool LockChestAtStart => false;

	public override void RunRule(FairyRoomController fairyRoomController)
	{
		base.State = FairyRoomState.Running;
	}
}
