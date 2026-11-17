using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SharedWorldObjects_Loader : MonoBehaviour
{
	[SerializeField]
	private string m_sharedWorldObjects;

	[SerializeField]
	private List<string> m_destroyImmediatelyOnLoadScenes;

	[SerializeField]
	private List<string> m_destroyOnUnloadScenes;

	private static SharedWorldObjects_Loader m_instance;

	private GameObject m_rootGameObject;

	public static bool IsInitialized { get; private set; }

	private void Awake()
	{
		if (m_instance == null)
		{
			m_instance = this;
			base.transform.SetParent(null);
			Object.DontDestroyOnLoad(base.gameObject);
			SceneManager.sceneUnloaded -= OnSceneUnloaded;
			SceneManager.sceneUnloaded += OnSceneUnloaded;
			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneLoaded += OnSceneLoaded;
			StartCoroutine(LoadAsync());
		}
		else
		{
			Object.DestroyImmediate(base.gameObject);
		}
	}

	private void LoadSync()
	{
		m_rootGameObject = new GameObject("SharedWorldObjects", typeof(SharedWorldObjects));
		Object.DontDestroyOnLoad(m_rootGameObject);
		GameObject gameObject = CDGResources.Load<GameObject>(m_sharedWorldObjects);
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			Object.Instantiate(gameObject.transform.GetChild(i).gameObject, m_rootGameObject.transform).GetComponent<ILoadable>()?.LoadSync();
		}
		IsInitialized = true;
	}

	private IEnumerator LoadAsync()
	{
		m_rootGameObject = new GameObject("SharedWorldObjects", typeof(SharedWorldObjects));
		Object.DontDestroyOnLoad(m_rootGameObject);
		CDGAsyncLoadRequest<GameObject> sharedWorldObjectsReq = CDGResources.LoadAsync<GameObject>(m_sharedWorldObjects);
		while (!sharedWorldObjectsReq.IsDone)
		{
			yield return null;
		}
		GameObject prefab = sharedWorldObjectsReq.Asset;
		for (int i = 0; i < prefab.transform.childCount; i++)
		{
			ILoadable component = Object.Instantiate(prefab.transform.GetChild(i).gameObject, m_rootGameObject.transform).GetComponent<ILoadable>();
			if (component != null)
			{
				yield return component.LoadAsync();
			}
			yield return null;
		}
		yield return null;
		IsInitialized = true;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (m_destroyImmediatelyOnLoadScenes.Contains(scene.name))
		{
			SharedWorldObjectPoolManager.DestroyAllPools();
			Object.DestroyImmediate(base.gameObject);
		}
	}

	private void OnSceneUnloaded(Scene scene)
	{
		if (m_destroyOnUnloadScenes.Contains(scene.name))
		{
			SharedWorldObjectPoolManager.DestroyAllPools();
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (m_instance == this)
		{
			Object.Destroy(m_rootGameObject);
			SceneManager.sceneUnloaded -= OnSceneUnloaded;
			SceneManager.sceneLoaded -= OnSceneLoaded;
			IsInitialized = false;
			m_instance = null;
		}
	}
}
