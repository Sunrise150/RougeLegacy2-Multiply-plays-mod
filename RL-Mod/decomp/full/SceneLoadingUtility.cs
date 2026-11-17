using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoadingUtility
{
	private static Dictionary<SceneID, string> SCENE_ID_TO_SCENE_NAME_TABLE = new Dictionary<SceneID, string>
	{
		{
			SceneID.Lineage,
			"Lineage"
		},
		{
			SceneID.MainMenu,
			"MainMenu"
		},
		{
			SceneID.Splash,
			"Splash"
		},
		{
			SceneID.Town,
			"Town"
		},
		{
			SceneID.World,
			"World"
		},
		{
			SceneID.SkillTree,
			"SkillTree"
		},
		{
			SceneID.Disclaimer,
			"Disclaimer"
		},
		{
			SceneID.Pause,
			"Pause"
		},
		{
			SceneID.Tutorial,
			"Tutorial"
		},
		{
			SceneID.Parade,
			"Parade"
		},
		{
			SceneID.Credits,
			"Credits"
		},
		{
			SceneID.Testing,
			"UserReport"
		}
	};

	public static Scene ActiveScene => SceneManager.GetActiveScene();

	public static string GetSceneName(SceneID sceneID)
	{
		return SCENE_ID_TO_SCENE_NAME_TABLE[sceneID];
	}

	public static SceneID GetSceneID(string sceneName)
	{
		SceneID result = SceneID.None;
		if (SCENE_ID_TO_SCENE_NAME_TABLE.ContainsValue(sceneName))
		{
			result = SCENE_ID_TO_SCENE_NAME_TABLE.Single((KeyValuePair<SceneID, string> entry) => entry.Value == sceneName).Key;
		}
		return result;
	}

	public static void LoadScene(SceneID sceneID)
	{
		if (SCENE_ID_TO_SCENE_NAME_TABLE.ContainsKey(sceneID))
		{
			RLTimeScale.Reset();
			SceneManager.LoadScene(SCENE_ID_TO_SCENE_NAME_TABLE[sceneID]);
		}
		else
		{
			Debug.LogFormat("<color=red>[{0}] Table does not contain an entry with Key ({1})</color>", "SceneLoadingUtility", sceneID);
		}
	}
}
