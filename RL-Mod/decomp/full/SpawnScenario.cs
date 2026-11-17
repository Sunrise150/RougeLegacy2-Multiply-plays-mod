using System;
using UnityEngine;

[Serializable]
public abstract class SpawnScenario
{
	[SerializeField]
	protected SpawnScenarioCheckStage m_checkStage;

	[SerializeField]
	[ReadOnly]
	private bool m_isTrue;

	public virtual SpawnScenarioCheckStage CheckStage => m_checkStage;

	public abstract string GizmoDescription { get; }

	public virtual bool IsTrue
	{
		get
		{
			return m_isTrue;
		}
		protected set
		{
			m_isTrue = value;
		}
	}

	public abstract SpawnScenarioType Type { get; }

	public string GetDataAsString()
	{
		return JsonUtility.ToJson(this);
	}

	public string GetTypeAsString()
	{
		return GetType().ToString();
	}

	public virtual void RunIsTrueCheck(BaseRoom room)
	{
	}

	public virtual void RunIsTrueCheck(GridPointManager gridPointManager)
	{
	}

	public virtual void Start()
	{
	}
}
