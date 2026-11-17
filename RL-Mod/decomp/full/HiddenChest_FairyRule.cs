using System.Collections;

public class HiddenChest_FairyRule : FairyRule
{
	public override string Description => "Hidden Chest";

	public override FairyRuleID ID => FairyRuleID.HiddenChest;

	public override bool LockChestAtStart => false;

	public override void RunRule(FairyRoomController fairyRoomController)
	{
		SetIsRuleActive(isRuleActive: true);
	}

	private void SetIsRuleActive(bool isRuleActive)
	{
		float opacity = 1f;
		float particleOpacity = 1f;
		bool forceDisable = true;
		if (isRuleActive)
		{
			opacity = 0.125f;
			particleOpacity = 0.0875f;
			forceDisable = false;
		}
		ChestSpawnController[] chestSpawnControllers = PlayerManager.GetCurrentPlayerRoom().SpawnControllerManager.ChestSpawnControllers;
		foreach (ChestSpawnController chestSpawnController in chestSpawnControllers)
		{
			if (chestSpawnController.ShouldSpawn)
			{
				ChestObj chestInstance = chestSpawnController.ChestInstance;
				chestInstance.SetOpacity(opacity);
				chestInstance.Interactable.ForceDisableInteractPrompt(forceDisable);
				StartCoroutine(SetParticleOpacity(chestInstance, particleOpacity));
			}
		}
	}

	private IEnumerator SetParticleOpacity(ChestObj chest, float particleOpacity)
	{
		yield return null;
		chest.SetSparkleParticleOpacity(particleOpacity);
	}

	public override void StopRule()
	{
		base.StopRule();
		SetIsRuleActive(isRuleActive: false);
		StopAllCoroutines();
	}
}
