using UnityEngine.SceneManagement;

public static class GameUtility
{
	public const string LEVEL_EDITOR_NAME = "LevelEditor";

	public const string GAME_SCENE_NAME = "World";

	private static bool m_isSceneOpen;

	private static bool m_isInLevelEditor;

	private static bool m_isInGame;

	private static bool m_hasChecked;

	private static int m_difficulty;

	private static bool m_applicationQuitting;

	public static bool IsApplicationQuitting => m_applicationQuitting;

	public static bool SceneHasRooms
	{
		get
		{
			if (IsInLevelEditor)
			{
				return true;
			}
			Scene activeScene = SceneLoadingUtility.ActiveScene;
			if (activeScene.name == SceneLoadingUtility.GetSceneName(SceneID.World) || activeScene.name == SceneLoadingUtility.GetSceneName(SceneID.Town) || activeScene.name == SceneLoadingUtility.GetSceneName(SceneID.Tutorial) || activeScene.name == SceneLoadingUtility.GetSceneName(SceneID.Lineage) || activeScene.name == SceneLoadingUtility.GetSceneName(SceneID.Parade))
			{
				return true;
			}
			return false;
		}
	}

	public static bool IsInGame => !IsInLevelEditor;

	public static bool IsInLevelEditor => false;

	public static int MaxDifficulty => 5;

	public static int Difficulty
	{
		get
		{
			return m_difficulty;
		}
		set
		{
			m_difficulty = value;
			if (m_difficulty > MaxDifficulty)
			{
				m_difficulty = 0;
			}
		}
	}
}
