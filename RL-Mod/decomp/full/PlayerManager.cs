using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
	private const string PLAYERMANAGER_NAME = "PlayerManager";

	private const string RESOURCE_PATH = "Prefabs/Managers/PlayerManager";

	[SerializeField]
	private GameObject m_playerPrefab;

	private GameObject m_player;

	private PlayerController m_playerController;

	private static bool m_isDisposed;

	private static PlayerManager m_playerManager;

	private static bool m_isInitialized;

	public static bool IsDisposed => m_isDisposed;

	private static PlayerManager Instance
	{
		get
		{
			if (m_isDisposed)
			{
				Debug.Log("ERROR: PlayerManager Instance is being fetched after PlayerManager has been destroyed. STACK TRACE:" + Environment.StackTrace);
			}
			if (!m_isInitialized)
			{
				Debug.Log("ERROR: PlayerManager has not been initialized.  Instantiate a new instance of PlayerManager.");
			}
			if (!m_playerManager)
			{
				m_playerManager = CDGHelper.FindStaticInstance<PlayerManager>(createNewIfNull: false);
			}
			return m_playerManager;
		}
	}

	public static bool IsInstantiated
	{
		get
		{
			if (m_isInitialized && (bool)m_playerManager)
			{
				return m_playerManager.m_playerController;
			}
			return false;
		}
	}

	public static GameObject GetPlayer()
	{
		return Instance.InternalGetPlayer();
	}

	public static PlayerController GetPlayerController()
	{
		return Instance.InternalGetPlayerController();
	}

	public static BaseRoom GetCurrentPlayerRoom()
	{
		return GetPlayerController().CurrentlyInRoom;
	}

	private GameObject InternalGetPlayer()
	{
		return m_player;
	}

	private PlayerController InternalGetPlayerController()
	{
		return m_playerController;
	}

	private void Awake()
	{
		Initialize();
	}

	public void Initialize()
	{
		m_playerManager = this;
		InitializePlayer();
		m_isInitialized = true;
		m_isDisposed = false;
	}

	private void InitializePlayer()
	{
		m_player = GameObject.FindGameObjectWithTag("Player");
		if (!m_player)
		{
			m_player = UnityEngine.Object.Instantiate(m_playerPrefab);
			m_player.name = "Player";
		}
		UnityEngine.Object.DontDestroyOnLoad(m_player);
		m_playerController = m_player.GetComponent<PlayerController>();
	}

	private void OnDestroy()
	{
		m_isDisposed = true;
		m_isInitialized = false;
		UnityEngine.Object.Destroy(m_player);
		m_playerManager = null;
	}
}
