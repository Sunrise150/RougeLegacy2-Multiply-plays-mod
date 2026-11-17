using System;
using System.Collections;
using System.Runtime.CompilerServices;
using RLAudio;
using RL_Windows;
using SceneManagement_RL;
using UnityEngine;

public class SkillTreeShop : MonoBehaviour, IRootObj, IAudioEventEmitter
{
	[SerializeField]
	private GameObject m_playerEnterPositionObj;

	[SerializeField]
	private GameObject m_playerExitPositionObj;

	[SerializeField]
	private GameObject m_castleParentObj;

	private bool m_isSkillTreeOpen;

	private Action<object, EventArgs> m_skillTreeWindowClosed;

	public GameObject HubTownCastleParentObj => m_castleParentObj;

	public bool IsSkillTreeOpen => m_isSkillTreeOpen;

	public string Description => ToString();

	private void Awake()
	{
		m_skillTreeWindowClosed = SkillTreeWindowClosed;
	}

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Closed, m_skillTreeWindowClosed);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Closed, m_skillTreeWindowClosed);
	}

	private void Start()
	{
		m_playerEnterPositionObj.SetActive(value: false);
		m_playerExitPositionObj.SetActive(value: false);
	}

	public void OpenSkillTree(bool animate)
	{
		if (!m_isSkillTreeOpen)
		{
			m_isSkillTreeOpen = true;
			if (animate)
			{
				StartCoroutine(OpenSkillTreeCoroutine());
				return;
			}
			PlayerManager.GetPlayerController().gameObject.transform.position = m_playerEnterPositionObj.transform.position;
			AudioManager.PlayOneShot(this, "event:/UI/FrontEnd/ui_fe_castle_open");
			WindowManager.SetWindowIsOpen(WindowID.SkillTree, isOpen: true);
			EnableSkillTreeCastle(enableInHubTown: false);
		}
	}

	private IEnumerator OpenSkillTreeCoroutine()
	{
		yield return MovePlayerToPosition(m_playerEnterPositionObj.transform.position);
		AudioManager.PlayOneShot(this, "event:/UI/FrontEnd/ui_fe_docks_transition_toSkilltree");
		WindowManager.SetWindowIsOpen(WindowID.SkillTree, isOpen: true, TransitionID.QuickSwipe);
		while (WindowManager.ActiveWindow == null)
		{
			yield return null;
		}
		EnableSkillTreeCastle(enableInHubTown: false);
	}

	private IEnumerator MovePlayerToPosition(Vector3 playerPos)
	{
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(playerPos, checkXAxisOnly: true);
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	private void EnableSkillTreeCastle(bool enableInHubTown)
	{
		if (!enableInHubTown)
		{
			if (HubTownCastleParentObj.transform.childCount > 0)
			{
				if (!WindowManager.GetIsWindowLoaded(WindowID.SkillTree))
				{
					WindowManager.LoadWindow(WindowID.SkillTree);
				}
				WindowController windowController = WindowManager.GetWindowController(WindowID.SkillTree);
				GameObject obj = HubTownCastleParentObj.transform.GetChild(0).gameObject;
				SkillTreeWindowController skillTreeWindowController = windowController as SkillTreeWindowController;
				obj.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				obj.transform.SetParent(skillTreeWindowController.SkillTreeCastleParentObj.transform, worldPositionStays: false);
				obj.SetLayerRecursively(5);
			}
		}
		else
		{
			WindowController windowController2 = WindowManager.GetWindowController(WindowID.SkillTree);
			if ((bool)windowController2)
			{
				GameObject obj2 = (windowController2 as SkillTreeWindowController).SkillTreeCastleParentObj.transform.GetChild(0).gameObject;
				obj2.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
				obj2.transform.SetParent(HubTownCastleParentObj.transform, worldPositionStays: false);
				obj2.SetLayerRecursively(26);
			}
		}
	}

	private void SkillTreeWindowClosed(object sender, EventArgs eventArgs)
	{
		EnableSkillTreeCastle(enableInHubTown: true);
		StartCoroutine(CloseSkillTreeCoroutine());
	}

	private IEnumerator CloseSkillTreeCoroutine()
	{
		yield return null;
		yield return MovePlayerToPosition(m_playerExitPositionObj.transform.position);
		m_isSkillTreeOpen = false;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
