using SceneManagement_RL;
using UnityEngine;

public class LoadSceneComponent : MonoBehaviour
{
	[SerializeField]
	private SceneID m_sceneToLoad;

	[SerializeField]
	private TransitionID m_transitionType;

	public void LoadScene()
	{
		SceneLoader_RL.LoadScene(m_sceneToLoad, m_transitionType);
	}
}
