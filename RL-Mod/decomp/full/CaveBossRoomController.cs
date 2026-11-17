using System;

public class CaveBossRoomController : BossRoomController
{
	protected override void OnBossHealthChange(object sender, EventArgs args)
	{
		if (args is HealthChangeEventArgs healthChangeEventArgs && (EnemyController)healthChangeEventArgs.HealthObj == base.Boss)
		{
			base.OnBossHealthChange(sender, args);
		}
	}
}
