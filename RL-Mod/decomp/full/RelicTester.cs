using System;
using UnityEngine;

public class RelicTester : MonoBehaviour
{
	[SerializeField]
	private RelicType m_relicToAdd;

	[SerializeField]
	private bool m_addOnStart = true;

	[SerializeField]
	private int m_relicLevel = 1;

	public RelicType RelicToAdd
	{
		get
		{
			return m_relicToAdd;
		}
		set
		{
			m_relicToAdd = value;
		}
	}

	private void OnEnable()
	{
		if (m_addOnStart)
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.LevelEditorWorldCreationComplete, AddRelicHandler);
		}
	}

	private void OnDisable()
	{
		if (m_addOnStart)
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.LevelEditorWorldCreationComplete, AddRelicHandler);
		}
	}

	private void AddRelicHandler(MonoBehaviour sender, EventArgs args)
	{
		AddRelic();
	}

	public void AddRelic()
	{
		if (m_relicToAdd != 0)
		{
			SaveManager.PlayerSaveData.GetRelic(m_relicToAdd)?.SetLevel(m_relicLevel, additive: false);
		}
	}

	public void RemoveRelic()
	{
		if (m_relicToAdd != 0)
		{
			SaveManager.PlayerSaveData.GetRelic(m_relicToAdd)?.SetLevel(0, additive: false);
		}
	}
}
