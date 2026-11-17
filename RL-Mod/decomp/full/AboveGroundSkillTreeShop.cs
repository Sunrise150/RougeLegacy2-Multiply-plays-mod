using System;
using System.Collections;
using System.Runtime.CompilerServices;
using RLAudio;
using RL_Windows;
using SceneManagement_RL;
using UnityEngine;

public class AboveGroundSkillTreeShop : MonoBehaviour, IRootObj, IAudioEventEmitter
{
	[SerializeField]
	private GameObject m_playerEnterPositionObj;

	[SerializeField]
	private GameObject m_playerExitPositionObj;

	private Action<object, EventArgs> m_skillTreeWindowClosed;

	public bool IsSkillTreeOpen { get; private set; }

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

	public void OpenSkillTree()
	{
		if (!IsSkillTreeOpen)
		{
			IsSkillTreeOpen = true;
			StartCoroutine(OpenSkillTreeCoroutine());
		}
	}

	private IEnumerator OpenSkillTreeCoroutine()
	{
		yield return MovePlayerToPosition(m_playerEnterPositionObj.transform.position);
		if (!WindowManager.GetIsWindowLoaded(WindowID.SkillTree))
		{
			WindowManager.LoadWindow(WindowID.SkillTree);
		}
		AudioManager.PlayOneShot(this, "event:/UI/FrontEnd/ui_fe_docks_transition_toSkilltree");
		WindowManager.SetWindowIsOpen(WindowID.SkillTree, isOpen: true, TransitionID.QuickSwipe);
	}

	private void SkillTreeWindowClosed(object sender, EventArgs eventArgs)
	{
		StartCoroutine(CloseSkillTreeCoroutine());
	}

	private IEnumerator CloseSkillTreeCoroutine()
	{
		yield return null;
		PlayerManager.GetPlayerController().transform.position = m_playerEnterPositionObj.transform.position;
		PlayerManager.GetPlayerController().ControllerCorgi.SetRaysParameters();
		PlayerManager.GetPlayerController().ControllerCorgi.ResetState();
		yield return MovePlayerToPosition(m_playerExitPositionObj.transform.position);
		IsSkillTreeOpen = false;
	}

	private IEnumerator MovePlayerToPosition(Vector3 playerPos)
	{
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(playerPos, checkXAxisOnly: true);
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
