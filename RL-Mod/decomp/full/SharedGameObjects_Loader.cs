using System.Collections;
using FMODUnity;
using UnityEngine;

public class SharedGameObjects_Loader : MonoBehaviour
{
	[SerializeField]
	private string m_sharedGameObjectsPath;

	private static SharedGameObjects_Loader m_instance;

	private void Awake()
	{
		if (m_instance == null)
		{
			m_instance = this;
			base.transform.SetParent(null);
			Object.DontDestroyOnLoad(base.gameObject);
			if (SceneLoadingUtility.ActiveScene.name != SceneLoadingUtility.GetSceneName(SceneID.Splash))
			{
				CDGResources.Init(loadSynchronous: true);
				RuntimeManager.LoadBank("Master.strings");
				RuntimeManager.LoadBank("Master");
				RuntimeManager.LoadBank("Music");
				RuntimeManager.WaitForAllLoads();
				GameObject gameObject = new GameObject("SharedGameObjects_Game");
				Object.DontDestroyOnLoad(gameObject);
				GameObject gameObject2 = CDGResources.Load<GameObject>(m_sharedGameObjectsPath);
				for (int i = 0; i < gameObject2.transform.childCount; i++)
				{
					Object.Instantiate(gameObject2.transform.GetChild(i).gameObject, gameObject.transform).GetComponent<ILoadable>()?.LoadSync();
				}
			}
		}
		else
		{
			Object.DestroyImmediate(base.gameObject);
		}
	}

	public IEnumerator Initialize()
	{
		CDGAsyncLoadRequest<GameObject> sharedGameObjectsReq = CDGResources.LoadAsync<GameObject>(m_sharedGameObjectsPath);
		while (!sharedGameObjectsReq.IsDone)
		{
			yield return null;
		}
		GameObject rootSharedGameObjects = new GameObject("SharedGameObjects_Game");
		Object.DontDestroyOnLoad(rootSharedGameObjects);
		GameObject prefab = sharedGameObjectsReq.Asset;
		for (int i = 0; i < prefab.transform.childCount; i++)
		{
			ILoadable component = Object.Instantiate(prefab.transform.GetChild(i).gameObject, rootSharedGameObjects.transform).GetComponent<ILoadable>();
			if (component != null)
			{
				yield return component.LoadAsync();
			}
			yield return null;
		}
	}
}
