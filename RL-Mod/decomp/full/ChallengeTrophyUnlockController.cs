using System;
using UnityEngine;

public class ChallengeTrophyUnlockController : MonoBehaviour, IRoomConsumer
{
	[SerializeField]
	private ChallengeType m_challengeToCheckUnlock;

	private PropSpawnController m_propSpawner;

	public BaseRoom Room { get; private set; }

	private void Awake()
	{
		m_propSpawner = GetComponent<PropSpawnController>();
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
		if (m_propSpawner == null || m_propSpawner.PropInstance.IsNativeNull())
		{
			throw new Exception("Trophy Prop Spawner or its prop instance was null!");
		}
		GameObject gameObject = m_propSpawner.PropInstance.gameObject;
		ChallengeTrophyRank challengeTrophyRank = ChallengeManager.GetChallengeTrophyRank(m_challengeToCheckUnlock);
		if (challengeTrophyRank != 0)
		{
			gameObject.SetActive(value: true);
			SpriteRenderer componentInChildren = m_propSpawner.PropInstance.GetComponentInChildren<SpriteRenderer>();
			switch (challengeTrophyRank)
			{
			case ChallengeTrophyRank.Gold:
				componentInChildren.sprite = IconLibrary.GetChallengeIcon(m_challengeToCheckUnlock, ChallengeLibrary.ChallengeIconEntryType.Gold);
				break;
			case ChallengeTrophyRank.Silver:
				componentInChildren.sprite = IconLibrary.GetChallengeIcon(m_challengeToCheckUnlock, ChallengeLibrary.ChallengeIconEntryType.Silver);
				break;
			case ChallengeTrophyRank.Bronze:
				componentInChildren.sprite = IconLibrary.GetChallengeIcon(m_challengeToCheckUnlock, ChallengeLibrary.ChallengeIconEntryType.Bronze);
				break;
			}
		}
		else
		{
			gameObject.SetActive(value: false);
		}
	}
}
