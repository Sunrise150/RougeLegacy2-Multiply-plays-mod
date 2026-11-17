using RL_Windows;
using UnityEngine;

public class DriftHouseExteriorController : BaseSpecialPropController, IDisplaySpeechBubble
{
	[SerializeField]
	private GameObject m_closedShack;

	[SerializeField]
	private GameObject m_openShack;

	[SerializeField]
	private SpeechBubbleController m_speechBubble;

	private Tunnel m_driftHouseTunnel;

	private bool m_updateSkillTreeButtons;

	public bool ShouldDisplaySpeechBubble
	{
		get
		{
			if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
			{
				return false;
			}
			if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DriftHouseUnlocked))
			{
				if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.EnteredDriftHouseOnce))
				{
					return true;
				}
				if (NPCDialogueManager.CanSpeak(NPCType.NewGamePlusHood) || NPCDialogueManager.CanSpeak(NPCType.ChallengeHood))
				{
					return true;
				}
				if (ChallengeShop.HasEventDialogue() || NewGamePlusShop.HasEventDialogue())
				{
					return true;
				}
			}
			return false;
		}
	}

	public SpeechBubbleType BubbleType
	{
		get
		{
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.EnteredDriftHouseOnce))
			{
				return SpeechBubbleType.PointOfInterest;
			}
			return SpeechBubbleType.Dialogue;
		}
	}

	protected override void InitializePooledPropOnEnter()
	{
		TunnelSpawnController tunnelSpawnController = base.Room.gameObject.FindObjectReference<TunnelSpawnController>("DriftHouseTunnel");
		if ((bool)tunnelSpawnController)
		{
			m_driftHouseTunnel = tunnelSpawnController.GetComponent<TunnelSpawnController>().Tunnel;
			if (!m_driftHouseTunnel.IsNativeNull())
			{
				m_driftHouseTunnel.Interactable.SpeechBubble = m_speechBubble;
				m_driftHouseTunnel.Interactable.TriggerOnEnterEvent.RemoveListener(EnterDriftHouse);
				m_driftHouseTunnel.Interactable.TriggerOnEnterEvent.AddListener(EnterDriftHouse);
			}
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DriftHouseUnlocked))
		{
			m_closedShack.SetActive(value: false);
			m_openShack.SetActive(value: true);
			if (!m_driftHouseTunnel.IsNativeNull())
			{
				m_driftHouseTunnel.SetIsLocked(isLocked: false);
			}
			m_speechBubble.SetSpeechBubbleEnabled(enable: true);
		}
		else
		{
			m_closedShack.SetActive(value: false);
			m_openShack.SetActive(value: false);
			if (!m_driftHouseTunnel.IsNativeNull())
			{
				m_driftHouseTunnel.SetIsLocked(isLocked: true);
			}
			m_speechBubble.SetSpeechBubbleEnabled(enable: false);
		}
		if (m_updateSkillTreeButtons)
		{
			SkillTreeWindowController obj = WindowManager.GetWindowController(WindowID.SkillTree) as SkillTreeWindowController;
			EffectManager.AddAnimatorToDisableList(obj.CastleAnimator);
			obj.ForceUpdateSkillTreeAnimatorParams();
			obj.CastleAnimator.Update(1f);
			EffectManager.RemoveAnimatorFromDisableList(obj.CastleAnimator);
			m_updateSkillTreeButtons = false;
		}
	}

	private void EnterDriftHouse(GameObject gameObj)
	{
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.EnteredDriftHouseOnce))
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.EnteredDriftHouseOnce, value: true);
		}
		m_updateSkillTreeButtons = true;
	}
}
