using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class CheerOnKillsAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_cheerShortAudioPath;

	[SerializeField]
	[EventRef]
	private string m_cheerLongAudioPath;

	[SerializeField]
	[EventRef]
	private string m_rosesThrownAudioPath;

	[SerializeField]
	[EventRef]
	private string m_playerDeathAudioPath;

	private string m_description = string.Empty;

	public string Description
	{
		get
		{
			if (m_description == string.Empty)
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		CheerOnKills_Trait component = GetComponent<CheerOnKills_Trait>();
		component.CheerAfterKillRelay.AddListener(OnCheerAfterKill);
		component.RosesThrownRelay.AddListener(OnRosesThrown);
		component.PlayerDeathRelay.AddListener(OnPlayerDeath);
	}

	private void OnPlayerDeath()
	{
		AudioManager.PlayOneShot(this, m_playerDeathAudioPath);
	}

	private void OnRosesThrown()
	{
		AudioManager.PlayOneShot(this, m_rosesThrownAudioPath);
	}

	private void OnCheerAfterKill(EnemyTypeAndRank enemyTypeAndRank)
	{
		string path = m_cheerShortAudioPath;
		if (GetUseLongCheer(enemyTypeAndRank))
		{
			path = m_cheerLongAudioPath;
		}
		AudioManager.PlayOneShot(this, path);
	}

	private bool GetUseLongCheer(EnemyTypeAndRank enemyTypeAndRank)
	{
		bool flag = false;
		switch (enemyTypeAndRank.Type)
		{
		case EnemyType.SpellswordBoss:
		case EnemyType.DancingBoss:
		case EnemyType.SkeletonBossA:
		case EnemyType.SkeletonBossB:
		case EnemyType.StudyBoss:
		case EnemyType.EyeballBoss_Left:
		case EnemyType.EyeballBoss_Right:
		case EnemyType.EyeballBoss_Bottom:
		case EnemyType.EyeballBoss_Middle:
		case EnemyType.CaveBoss:
		case EnemyType.TraitorBoss:
		case EnemyType.FinalBoss:
			flag = true;
			break;
		}
		if (!flag)
		{
			EnemyRank rank = enemyTypeAndRank.Rank;
			if ((uint)(rank - 2) <= 1u)
			{
				flag = true;
			}
		}
		return flag;
	}
}
