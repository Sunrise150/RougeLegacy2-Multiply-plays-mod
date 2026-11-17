using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SpawnChest_SummonRule : BaseSummonRule
{
	private Action<MonoBehaviour, EventArgs> m_onPlayerEnter;

	public override SummonRuleType RuleType => SummonRuleType.SpawnChest;

	public override string RuleLabel => "Spawn Chest";

	public override void Initialize(SummonRuleController summonController)
	{
		base.Initialize(summonController);
		m_onPlayerEnter = OnPlayerEnter;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnter);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnter);
	}

	private void OnPlayerEnter(object sender, EventArgs args)
	{
		ChestSpawnController chestSpawnController = ((base.SerializedObject != null) ? (base.SerializedObject as ChestSpawnController) : null);
		if ((bool)chestSpawnController)
		{
			ChestObj chestInstance = chestSpawnController.ChestInstance;
			if ((bool)chestInstance)
			{
				chestInstance.SetOpacity(0f);
				chestInstance.SetChestLockState(ChestLockState.Locked);
				chestInstance.gameObject.SetActive(value: false);
			}
		}
	}

	public override IEnumerator RunSummonRule()
	{
		ChestSpawnController chestSpawnController = ((base.SerializedObject != null) ? (base.SerializedObject as ChestSpawnController) : null);
		if (chestSpawnController != null && chestSpawnController.ChestInstance != null)
		{
			ChestObj chest = chestSpawnController.ChestInstance;
			if (chest != null)
			{
				float chestOpacity = 0f;
				chest.gameObject.SetActive(value: true);
				Vector3 position = chest.transform.position;
				position.y += 1f;
				chest.transform.position = position;
				chest.Interactable.SetIsInteractableActive(isActive: false);
				TweenManager.TweenBy(chest.transform, 1f, Ease.Quad.EaseOut, "position.y", -1);
				while (chestOpacity < 1f)
				{
					chest.SetOpacity(chestOpacity);
					chestOpacity += Time.deltaTime;
					yield return null;
				}
				chest.Interactable.SetIsInteractableActive(isActive: true);
				chest.SetChestLockState(ChestLockState.Unlocked);
			}
		}
		base.IsRuleComplete = true;
	}
}
