using System.Collections.Generic;
using UnityEngine;

public class Difficulty_SpawnScenario : SpawnScenario
{
	[SerializeField]
	private int m_level;

	[SerializeField]
	private int m_comparisonOperator;

	private static Dictionary<int, ComparisonOperators_RL> INDEX_TO_COMPARISON_OPERATOR_TABLE = new Dictionary<int, ComparisonOperators_RL>
	{
		{
			0,
			ComparisonOperators_RL.EqualTo
		},
		{
			1,
			ComparisonOperators_RL.LessThan
		},
		{
			2,
			ComparisonOperators_RL.GreaterThan
		},
		{
			3,
			ComparisonOperators_RL.EqualsOrLessThan
		},
		{
			4,
			ComparisonOperators_RL.EqualsOrGreaterThan
		}
	};

	private static Dictionary<int, string> INDEX_TO_STRING_TABLE = new Dictionary<int, string>
	{
		{ 0, "==" },
		{ 1, "<" },
		{ 2, ">" },
		{ 3, "<=" },
		{ 4, ">=" }
	};

	public int Difficulty
	{
		get
		{
			return m_level;
		}
		set
		{
			m_level = value;
		}
	}

	public int ComparisonOperatorIndex
	{
		get
		{
			return m_comparisonOperator;
		}
		set
		{
			m_comparisonOperator = value;
		}
	}

	public override string GizmoDescription => INDEX_TO_STRING_TABLE[ComparisonOperatorIndex] + Difficulty;

	public override SpawnScenarioType Type => SpawnScenarioType.Difficulty;

	public override void RunIsTrueCheck(BaseRoom room)
	{
		GetIsTrue();
	}

	public override void RunIsTrueCheck(GridPointManager gridPointManager)
	{
		GetIsTrue();
	}

	private void GetIsTrue()
	{
		switch (INDEX_TO_COMPARISON_OPERATOR_TABLE[ComparisonOperatorIndex])
		{
		case ComparisonOperators_RL.EqualTo:
			IsTrue = GameUtility.Difficulty == Difficulty;
			break;
		case ComparisonOperators_RL.NotEqualTo:
			IsTrue = GameUtility.Difficulty != Difficulty;
			break;
		case ComparisonOperators_RL.GreaterThan:
			IsTrue = GameUtility.Difficulty > Difficulty;
			break;
		case ComparisonOperators_RL.LessThan:
			IsTrue = GameUtility.Difficulty < Difficulty;
			break;
		case ComparisonOperators_RL.EqualsOrGreaterThan:
			IsTrue = GameUtility.Difficulty >= Difficulty;
			break;
		case ComparisonOperators_RL.EqualsOrLessThan:
			IsTrue = GameUtility.Difficulty <= Difficulty;
			break;
		}
	}
}
