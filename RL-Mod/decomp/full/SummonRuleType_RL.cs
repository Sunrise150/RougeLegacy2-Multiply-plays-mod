public class SummonRuleType_RL
{
	public static BaseSummonRule GetSummonRule(SummonRuleType ruleType)
	{
		BaseSummonRule result = null;
		switch (ruleType)
		{
		case SummonRuleType.SummonEnemies:
			result = new SummonEnemy_SummonRule();
			break;
		case SummonRuleType.WaitUntilAllEnemiesDead:
			result = new WaitUntilAllEnemiesDead_SummonRule();
			break;
		case SummonRuleType.DebugTrace:
			result = new DebugTrace_SummonRule();
			break;
		case SummonRuleType.WaitForXSeconds:
			result = new WaitForXSeconds_SummonRule();
			break;
		case SummonRuleType.SetSummonPool:
			result = new SetSummonPool_SummonRule();
			break;
		case SummonRuleType.SetSummonPoolDifficulty:
			result = new SetSummonPoolDifficulty_SummonRule();
			break;
		case SummonRuleType.EndArena:
			result = new EndArena_SummonRule();
			break;
		case SummonRuleType.WaitUntilXRemaining:
			result = new WaitUntilXRemaining_SummonRule();
			break;
		case SummonRuleType.SetSpawnPoints:
			result = new SetSpawnPoints_SummonRule();
			break;
		case SummonRuleType.SetSummonPoolLevelMod:
			result = new SetSummonPoolLevelMod_SummonRule();
			break;
		case SummonRuleType.StartArena:
			result = new StartArena_SummonRule();
			break;
		case SummonRuleType.TeleportPlayer:
			result = new TeleportPlayer_SummonRule();
			break;
		case SummonRuleType.AwardHeirloom:
			result = new AwardHeirloom_SummonRule();
			break;
		case SummonRuleType.RunDialogue:
			result = new RunDialogue_SummonRule();
			break;
		case SummonRuleType.WaitForBroadcast:
			result = new WaitForBroadcast_SummonRule();
			break;
		case SummonRuleType.SpawnChest:
			result = new SpawnChest_SummonRule();
			break;
		case SummonRuleType.WaitUntilChestCollected:
			result = new WaitUntilChestCollected_SummonRule();
			break;
		case SummonRuleType.KillAllEnemies:
			result = new KillAllEnemies_SummonRule();
			break;
		case SummonRuleType.DisplayObjectiveComplete:
			result = new DisplayObjectiveComplete_SummonRule();
			break;
		case SummonRuleType.SlowTime:
			result = new SlowTime_SummonRule();
			break;
		case SummonRuleType.EndChallenge:
			result = new EndChallenge_SummonRule();
			break;
		case SummonRuleType.PlayMusic:
			result = new PlayMusic_SummonRule();
			break;
		case SummonRuleType.SetGlobalTimer:
			result = new SetGlobalTimer_SummonRule();
			break;
		case SummonRuleType.SaveCurrentEnemyHP:
			result = new SaveCurrentEnemyHP_SummonRule();
			break;
		case SummonRuleType.WaitUntilEnemyHP:
			result = new WaitUntilEnemyHP_SummonRule();
			break;
		case SummonRuleType.TogglePlayerInvincibility:
			result = new TogglePlayerInvincibility_SummonRule();
			break;
		case SummonRuleType.WaitUntilModeShift:
			result = new WaitUntilModeShift_SummonRule();
			break;
		case SummonRuleType.SetEnemiesDefeated:
			result = new SetEnemiesDefeated_SummonRule();
			break;
		}
		return result;
	}
}
