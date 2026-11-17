public class CutsceneManager
{
	private static Tunnel m_exitRoomTunnel_STATIC;

	private static PlayerSaveFlag m_cutsceneSaveFlag_STATIC;

	private static bool m_playCutscene_STATIC;

	public static bool IsCutsceneActive => m_playCutscene_STATIC;

	public static Tunnel ExitRoomTunnel => m_exitRoomTunnel_STATIC;

	public static PlayerSaveFlag CutsceneSaveFlag => m_cutsceneSaveFlag_STATIC;

	public static void InitializeCutscene(PlayerSaveFlag saveFlag, Tunnel exitRoomTunnel)
	{
		m_playCutscene_STATIC = true;
		m_cutsceneSaveFlag_STATIC = saveFlag;
		m_exitRoomTunnel_STATIC = exitRoomTunnel;
	}

	public static void ResetCutscene()
	{
		m_exitRoomTunnel_STATIC = null;
		m_playCutscene_STATIC = false;
		m_cutsceneSaveFlag_STATIC = PlayerSaveFlag.None;
	}

	public static void SetTraitsEnabled(bool enabled)
	{
		if (!enabled)
		{
			foreach (BaseTrait activeTrait in TraitManager.ActiveTraitList)
			{
				if ((bool)activeTrait.TraitMask)
				{
					activeTrait.DisableOnCutscene();
					activeTrait.gameObject.SetActive(value: false);
				}
			}
			return;
		}
		foreach (BaseTrait activeTrait2 in TraitManager.ActiveTraitList)
		{
			if (!activeTrait2.gameObject.activeSelf)
			{
				activeTrait2.gameObject.SetActive(value: true);
			}
		}
	}
}
