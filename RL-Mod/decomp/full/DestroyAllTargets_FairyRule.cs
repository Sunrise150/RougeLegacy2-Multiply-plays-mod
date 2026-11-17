public class DestroyAllTargets_FairyRule : KillAllEnemies_FairyRule
{
	public override string Description => string.Format(LocalizationManager.GetString("LOC_ID_FAIRY_RULE_TARGETS_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale));

	public override FairyRuleID ID => FairyRuleID.DestroyAllTargets;
}
