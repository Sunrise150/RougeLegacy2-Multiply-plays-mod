public class ForestBossRoomController : BossRoomController
{
	private IHazardSpawnController[] m_sentrySpawners;

	protected override void Awake()
	{
		m_sentrySpawners = base.gameObject.GetComponentsInChildren<IHazardSpawnController>();
		base.Awake();
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		IHazardSpawnController[] sentrySpawners = m_sentrySpawners;
		for (int i = 0; i < sentrySpawners.Length; i++)
		{
			Sentry_Hazard sentry_Hazard = sentrySpawners[i].Hazard as Sentry_Hazard;
			if ((bool)sentry_Hazard)
			{
				sentry_Hazard.gameObject.SetActive(value: false);
			}
		}
	}
}
