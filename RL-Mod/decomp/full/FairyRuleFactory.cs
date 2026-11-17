using UnityEngine;

public static class FairyRuleFactory
{
	public static FairyRule AddFairyRule(FairyRuleID fairyRuleID, GameObject gameObject)
	{
		FairyRule result = null;
		switch (fairyRuleID)
		{
		case FairyRuleID.TimeLimit:
			return gameObject.AddComponent<TimeLimit_FairyRule>();
		case FairyRuleID.DontTakeDamage:
			return gameObject.AddComponent<DontTakeDamage_FairyRule>();
		case FairyRuleID.NoJumping:
			return gameObject.AddComponent<NoJumping_FairyRule>();
		case FairyRuleID.KillAllEnemies:
			return gameObject.AddComponent<KillAllEnemies_FairyRule>();
		case FairyRuleID.HiddenChest:
			return gameObject.AddComponent<HiddenChest_FairyRule>();
		case FairyRuleID.NoDash:
			return gameObject.AddComponent<NoDash_FairyRule>();
		case FairyRuleID.NoDoubleJump:
			return gameObject.AddComponent<NoDoubleJump_FairyRule>();
		case FairyRuleID.NoAttacking:
			return gameObject.AddComponent<NoAttacking_FairyRule>();
		case FairyRuleID.Puzzle:
			return gameObject.AddComponent<Puzzle_FairyRule>();
		case FairyRuleID.DestroyAllTargets:
			return gameObject.AddComponent<DestroyAllTargets_FairyRule>();
		case FairyRuleID.ReachMe:
			return gameObject.AddComponent<ReachMe_FairyRule>();
		case FairyRuleID.DontGetHit:
			return gameObject.AddComponent<DontGetHit_FairyRule>();
		default:
			if (fairyRuleID != 0)
			{
				Debug.LogFormat("<color=red>[FairyRuleFactory] No case found for ({0})</color>", fairyRuleID);
			}
			return result;
		}
	}
}
