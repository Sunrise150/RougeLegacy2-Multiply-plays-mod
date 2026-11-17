using System.Collections;
using RL_Windows;
using UnityEngine;

public class ClownEntranceRoomController : BaseSpecialRoomController
{
	private ChestObj m_bronzeChest;

	private ChestObj m_silverChest;

	private ChestObj m_goldChest;

	private Tunnel m_tunnel;

	public ChestObj GetChest(ClownGoalType goalType)
	{
		return goalType switch
		{
			ClownGoalType.Bronze => m_bronzeChest, 
			ClownGoalType.Silver => m_silverChest, 
			ClownGoalType.Gold => m_goldChest, 
			_ => null, 
		};
	}

	private IEnumerator Start()
	{
		while (base.Room == null)
		{
			yield return null;
		}
		ObjectReferenceFinder finder = base.Room.gameObject.GetComponent<ObjectReferenceFinder>();
		GameObject @object = finder.GetObject("ClownRoomTunnel");
		TunnelSpawnController tunnelSpawner = @object.GetComponent<TunnelSpawnController>();
		while (tunnelSpawner.Tunnel == null)
		{
			yield return null;
		}
		m_tunnel = tunnelSpawner.Tunnel;
		GameObject object2 = finder.GetObject("BronzeChest");
		ChestSpawnController bronzeChestSpawner = object2.GetComponent<ChestSpawnController>();
		GameObject object3 = finder.GetObject("SilverChest");
		ChestSpawnController silverChestSpawner = object3.GetComponent<ChestSpawnController>();
		GameObject object4 = finder.GetObject("GoldChest");
		ChestSpawnController goldChestSpawner = object4.GetComponent<ChestSpawnController>();
		while (bronzeChestSpawner.ChestInstance == null || silverChestSpawner.ChestInstance == null || goldChestSpawner.ChestInstance == null)
		{
			yield return null;
		}
		m_bronzeChest = bronzeChestSpawner.ChestInstance;
		m_silverChest = silverChestSpawner.ChestInstance;
		m_goldChest = goldChestSpawner.ChestInstance;
		while (!m_bronzeChest.IsInitialised || !m_silverChest.IsInitialised || !m_goldChest.IsInitialised)
		{
			yield return null;
		}
		m_bronzeChest.gameObject.SetActive(value: false);
		m_silverChest.gameObject.SetActive(value: false);
		m_goldChest.gameObject.SetActive(value: false);
		m_tunnel.SetIsLocked(isLocked: true);
	}

	public void UnlockChestReward(ClownGoalType goalType)
	{
		switch (goalType)
		{
		case ClownGoalType.Bronze:
			GetChest(ClownGoalType.Bronze).gameObject.SetActive(value: true);
			break;
		case ClownGoalType.Silver:
			GetChest(ClownGoalType.Bronze).gameObject.SetActive(value: true);
			GetChest(ClownGoalType.Silver).gameObject.SetActive(value: true);
			break;
		case ClownGoalType.Gold:
			GetChest(ClownGoalType.Bronze).gameObject.SetActive(value: true);
			GetChest(ClownGoalType.Silver).gameObject.SetActive(value: true);
			GetChest(ClownGoalType.Gold).gameObject.SetActive(value: true);
			break;
		}
		m_tunnel.SetIsLocked(isLocked: true);
	}

	public void TriggerDialogue(GameObject obj)
	{
		m_tunnel.SetIsLocked(isLocked: false);
		DialogueManager.StartNewDialogue();
		DialogueManager.AddNonLocDialogue("Clown", "Clown challenge time!");
		DialogueManager.AddNonLocDialogue("Clown", "The goal is to destroy ALL targets.  You are only allowed to attack 20 times.");
		DialogueManager.AddNonLocDialogue("Clown", "Don't suck.");
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
	}
}
