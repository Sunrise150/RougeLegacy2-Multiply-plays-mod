using System.Collections;
using UnityEngine;

public class RewiredInputManager_Loader : MonoBehaviour, ILoadable
{
	[SerializeField]
	private string m_debugRewiredInputManagerPath;

	[SerializeField]
	private string m_retailRewiredInputManagerPath;

	[SerializeField]
	private string m_gameCoreRewiredInputManagerPath;

	private string GetPath()
	{
		return m_retailRewiredInputManagerPath;
	}

	public void LoadSync()
	{
		Object.Instantiate(CDGResources.Load<GameObject>(GetPath()), null);
	}

	public IEnumerator LoadAsync()
	{
		CDGAsyncLoadRequest<GameObject> req = CDGResources.LoadAsync<GameObject>(GetPath());
		while (!req.IsDone)
		{
			yield return null;
		}
		Object.Instantiate(req.Asset, null);
	}
}
