using System;

public class EnemySummonedEventArgs : EventArgs
{
	public ISummoner Summoner { get; private set; }

	public EnemyController SummonedEnemy { get; private set; }

	public EnemySummonedEventArgs(EnemyController summonedEnemy, ISummoner summoner)
	{
		Initialize(summonedEnemy, summoner);
	}

	public void Initialize(EnemyController summonedEnemy, ISummoner summoner)
	{
		SummonedEnemy = summonedEnemy;
		Summoner = summoner;
	}
}
