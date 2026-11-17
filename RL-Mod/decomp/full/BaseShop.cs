using System.Collections;
using System.Runtime.CompilerServices;
using FMODUnity;
using UnityEngine;

public abstract class BaseShop : MonoBehaviour, IDisplaySpeechBubble, IRootObj
{
	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	private StudioEventEmitter m_greetingAudioEmitter;

	protected Interactable m_interactable;

	protected NPCController m_npcController;

	public virtual bool ShouldDisplaySpeechBubble => HasEventDialogue();

	public virtual SpeechBubbleType BubbleType => SpeechBubbleType.Dialogue;

	protected abstract bool HasEventDialogue();

	protected virtual void Awake()
	{
		m_interactable = GetComponent<Interactable>();
		m_npcController = GetComponentInChildren<NPCController>();
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	public virtual void OpenShop()
	{
		if ((bool)m_interactable)
		{
			m_interactable.SetIsInteractableActive(isActive: false);
		}
		StartCoroutine(OpenShopCoroutine());
	}

	protected abstract void OpenShop_Internal();

	public virtual void CloseShop()
	{
		if ((bool)m_interactable)
		{
			m_interactable.SetIsInteractableActive(isActive: true);
		}
		if ((bool)m_npcController && m_npcController.CurrentState != NPCState.Idle)
		{
			m_npcController.SetNPCState(NPCState.Idle);
		}
	}

	protected virtual IEnumerator OpenShopCoroutine()
	{
		if ((bool)m_greetingAudioEmitter)
		{
			m_greetingAudioEmitter.Play();
		}
		if ((bool)m_npcController)
		{
			m_npcController.SetNPCState(NPCState.AtAttention);
		}
		if ((bool)m_playerPositionObj)
		{
			yield return MovePlayerToPos();
		}
		OpenShop_Internal();
	}

	protected virtual IEnumerator MovePlayerToPos()
	{
		PlayerManager.GetPlayerController().SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPositionObj.transform.position;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		if (m_playerPositionObj.transform.lossyScale.x > 0f)
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: false);
		}
		else
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: true);
		}
		float startTime = Time.time;
		while (Time.time < startTime + 0.25f)
		{
			yield return null;
		}
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
