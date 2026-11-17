public static class TriggerLBCooldown_AIExtension
{
	public static void TriggerAttackCooldown(this BaseAIScript aiScript, float cooldown, bool ignoreMods)
	{
		aiScript.LogicController.SetLBCooldown(aiScript.LogicController.CurrentLogicBlockName, cooldown, ignoreMods);
	}
}
