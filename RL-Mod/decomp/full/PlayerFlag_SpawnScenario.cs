using System;
using UnityEngine;

public class PlayerFlag_SpawnScenario : SpawnScenario
{
	[SerializeField]
	private string m_playerFlag;

	[SerializeField]
	private bool m_flagIsTrue = true;

	public string PlayerFlag
	{
		get
		{
			return m_playerFlag;
		}
		set
		{
			m_playerFlag = value;
		}
	}

	public bool FlagIsTrue
	{
		get
		{
			return m_flagIsTrue;
		}
		set
		{
			m_flagIsTrue = value;
		}
	}

	public override SpawnScenarioType Type => SpawnScenarioType.PlayerFlag;

	public override string GizmoDescription => $"FLAG: {m_playerFlag}, ISTRUE: {m_flagIsTrue}";

	public override void RunIsTrueCheck(BaseRoom room)
	{
		RunIsTrueCheck();
	}

	public override void RunIsTrueCheck(GridPointManager gridPointManager)
	{
		RunIsTrueCheck();
	}

	private void RunIsTrueCheck()
	{
		if (Enum.TryParse<PlayerSaveFlag>(PlayerFlag, out var result))
		{
			IsTrue = SaveManager.PlayerSaveData.GetFlag(result) == FlagIsTrue;
		}
	}
}
