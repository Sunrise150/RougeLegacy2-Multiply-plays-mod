using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortraitTunnelComponent : MonoBehaviour, IRoomConsumer
{
	[SerializeField]
	private PlayerSaveFlagAnimatorDictionary m_rootsTable;

	[SerializeField]
	private PlayerSaveFlagAnimatorDictionary m_portraitsTable;

	[SerializeField]
	private Sprite m_bridgeBossMurkyPortraitSprite;

	[SerializeField]
	private Sprite m_bridgeBossClearPortraitSprite;

	[SerializeField]
	private SpriteRenderer m_bridgeBossPortraitRenderer;

	private WaitRL_Yield m_waitYield;

	public BaseRoom Room { get; private set; }

	public bool IsPortraitComplete
	{
		get
		{
			foreach (KeyValuePair<PlayerSaveFlag, Animator> item in m_rootsTable)
			{
				if (!SaveManager.PlayerSaveData.GetFlag(item.Key))
				{
					return false;
				}
			}
			return true;
		}
	}

	private void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f, useUnscaledTime: true);
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		foreach (KeyValuePair<PlayerSaveFlag, Animator> item in m_rootsTable)
		{
			UpdateBossDefeatedState(item.Key);
		}
	}

	private void UpdateBossDefeatedState(PlayerSaveFlag bossDefeatedFlag)
	{
		SetBossDefeated(bossDefeatedFlag, SaveManager.PlayerSaveData.GetFlag(bossDefeatedFlag));
	}

	public void SetBossDefeated(PlayerSaveFlag playerFlag, bool defeated)
	{
		if (!m_rootsTable.ContainsKey(playerFlag) || !m_portraitsTable.ContainsKey(playerFlag))
		{
			return;
		}
		if (playerFlag == PlayerSaveFlag.BridgeBoss_Defeated && (bool)m_bridgeBossPortraitRenderer)
		{
			if (BurdenManager.GetBurdenLevel(BurdenType.BridgeBossUp) > 0)
			{
				m_bridgeBossPortraitRenderer.sprite = m_bridgeBossClearPortraitSprite;
			}
			else
			{
				m_bridgeBossPortraitRenderer.sprite = m_bridgeBossMurkyPortraitSprite;
			}
		}
		Animator animator = m_rootsTable[playerFlag];
		Animator animator2 = m_portraitsTable[playerFlag];
		if (defeated)
		{
			animator2.Play("RevealedIdle");
			animator2.Update(1f);
			animator.Play("RetractedIdle");
			animator.Update(1f);
		}
		else
		{
			animator2.Play("Idle");
			animator2.Update(1f);
			animator.Play("Idle");
			animator.Update(1f);
		}
	}

	public IEnumerator BossDefeatedCoroutine(PlayerSaveFlag playerFlag)
	{
		SetBossDefeated(playerFlag, defeated: false);
		if (m_rootsTable.ContainsKey(playerFlag) && m_portraitsTable.ContainsKey(playerFlag))
		{
			Animator rootsAnimator = m_rootsTable[playerFlag];
			m_portraitsTable[playerFlag].SetTrigger("Reveal");
			m_waitYield.CreateNew(1f);
			yield return m_waitYield;
			rootsAnimator.SetTrigger("Retract");
			m_waitYield.CreateNew(3f);
			yield return m_waitYield;
		}
	}
}
