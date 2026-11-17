using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Enemy Class Data")]
public class EnemyClassData : ScriptableObject
{
	[Header("Basic")]
	[SerializeField]
	private EnemyData m_basicEnemyData;

	[SerializeField]
	private BaseAIScript m_basicAIScript;

	[Space(10f)]
	[Header("Advanced")]
	[SerializeField]
	private EnemyData m_advancedEnemyData;

	[SerializeField]
	private BaseAIScript m_advancedAIScript;

	[Space(10f)]
	[Header("Expert")]
	[SerializeField]
	private EnemyData m_expertEnemyData;

	[SerializeField]
	private BaseAIScript m_expertAIScript;

	[Space(10f)]
	[Header("Miniboss")]
	[SerializeField]
	private EnemyData m_minibossEnemyData;

	[SerializeField]
	private BaseAIScript m_minibossAIScript;

	[Space(10f)]
	[SerializeField]
	private LogicController_SO m_enemyLogicController;

	public EnemyData GetEnemyData(EnemyRank enemyRank)
	{
		return enemyRank switch
		{
			EnemyRank.Advanced => m_advancedEnemyData, 
			EnemyRank.Expert => m_expertEnemyData, 
			EnemyRank.Miniboss => m_minibossEnemyData, 
			_ => m_basicEnemyData, 
		};
	}

	public BaseAIScript GetAIScript(EnemyRank enemyRank)
	{
		return enemyRank switch
		{
			EnemyRank.Advanced => m_advancedAIScript, 
			EnemyRank.Expert => m_expertAIScript, 
			EnemyRank.Miniboss => m_minibossAIScript, 
			_ => m_basicAIScript, 
		};
	}

	public LogicController_SO GetLogicController()
	{
		return m_enemyLogicController;
	}
}
