using UnityEngine;
using UnityEngine.SceneManagement;

public class StatusBarEntryManager : MonoBehaviour
{
	private const int POOL_SIZE = 20;

	[SerializeField]
	private StatusBarEntry m_statusBarPrefab;

	private GenericPool_RL<StatusBarEntry> m_statusBarPool;

	private static StatusBarEntryManager m_statusBarManager;

	public static bool IsInitialized { get; private set; }

	private static StatusBarEntryManager Instance
	{
		get
		{
			if (m_statusBarManager == null)
			{
				m_statusBarManager = CDGHelper.FindStaticInstance<StatusBarEntryManager>(createNewIfNull: false);
			}
			return m_statusBarManager;
		}
	}

	private void Awake()
	{
		Initialize();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		DisableAllStatusBarEntries();
	}

	private void OnDestroy()
	{
		IsInitialized = false;
		SceneManager.sceneLoaded -= OnSceneLoaded;
		m_statusBarPool.DestroyPool();
		m_statusBarPool = null;
		m_statusBarManager = null;
	}

	private void Initialize()
	{
		m_statusBarPool = new GenericPool_RL<StatusBarEntry>();
		m_statusBarPool.Initialize(m_statusBarPrefab, 20, disablePooling: false);
		IsInitialized = true;
	}

	public static StatusBarEntry GetFreeStatusBarEntry()
	{
		if (!IsInitialized)
		{
			Instance.Initialize();
		}
		return Instance.m_statusBarPool.GetFreeObj();
	}

	public static void DisableAllStatusBarEntries()
	{
		if (IsInitialized)
		{
			Instance.m_statusBarPool.DisableAll();
		}
	}
}
