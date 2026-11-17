using System;
using RLAudio;
using UnityEngine;

public class EggplantTriggerController : MonoBehaviour
{
	private Interactable m_interactable;

	private EnemyController m_enemyController;

	private Action<object, EventArgs> m_onPlayerEnterRoom;

	private void Awake()
	{
		m_enemyController = GetComponent<EnemyController>();
		m_interactable = GetComponent<Interactable>();
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		PlayerSaveFlag eggplantFlag = GetEggplantFlag();
		bool flag = false;
		if (eggplantFlag != 0)
		{
			flag = !SaveManager.PlayerSaveData.GetFlag(eggplantFlag);
		}
		if (flag)
		{
			m_interactable.SetIsInteractableActive(isActive: true);
			if (SaveManager.PlayerSaveData.InHubTown)
			{
				m_enemyController.gameObject.SetActive(value: false);
			}
		}
		else
		{
			m_interactable.SetIsInteractableActive(isActive: false);
			if (!SaveManager.PlayerSaveData.InHubTown)
			{
				m_enemyController.gameObject.SetActive(value: false);
			}
		}
	}

	private PlayerSaveFlag GetEggplantFlag()
	{
		return m_enemyController.EnemyRank switch
		{
			EnemyRank.Basic => PlayerSaveFlag.FoundEggplant_Basic, 
			EnemyRank.Advanced => PlayerSaveFlag.FoundEggplant_Advanced, 
			EnemyRank.Expert => PlayerSaveFlag.FoundEggplant_Expert, 
			EnemyRank.Miniboss => PlayerSaveFlag.FoundEggplant_Miniboss, 
			_ => PlayerSaveFlag.None, 
		};
	}

	public void CollectEggplant()
	{
		PlayerSaveFlag eggplantFlag = GetEggplantFlag();
		if (eggplantFlag != 0)
		{
			AudioManager.PlayOneShot(null, "event:/Cut_Scenes/sfx_cutscene_ngPlusTeleport_playerAppear", base.gameObject.transform.position);
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/sfx_npc_eggplant_vanish", base.gameObject.transform.position);
			m_enemyController.gameObject.SetActive(value: false);
			SaveManager.PlayerSaveData.SetFlag(eggplantFlag, value: true);
			SaveManager.SaveCurrentProfileGameData(SaveDataType.Player, SavingType.FileOnly, ignoreTimeRestriction: true);
		}
	}
}
