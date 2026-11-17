using System;
using System.Collections;
using RLAudio;
using UnityEngine;

public class BridgeBossRoomController : BossRoomController
{
	private const string ON_ENTER_AUDIO = "event:/SFX/Enemies/vo_skellyBoss_agro";

	private const string SECOND_BOSS_SUMMON_AUDIO = "event:/SFX/Enemies/vo_skellyBoss_laugh";

	[SerializeField]
	private GameObject m_secondBossSpawnPosition;

	[SerializeField]
	private bool m_isPrimeVariant;

	private int m_bossesSpawned;

	private bool m_secondBossSpawned;

	private bool m_outroPlaying;

	public override float BossHealthAsPercentage
	{
		get
		{
			if (m_bossesSpawned == 1)
			{
				return 1f;
			}
			if (m_bossesSpawned == 2)
			{
				return 0.5f;
			}
			return 0f;
		}
	}

	public EnemyController Boss2 { get; private set; }

	protected override void InitializeObjectiveCompleteArgs(float bossDefeatedDisplayDuration)
	{
		string @string = LocalizationManager.GetString("LOC_ID_BIG_TEXT_UI_STYGIAN_BOSS_DEFEATED_TITLE_1", isFemale: false);
		if (m_isPrimeVariant)
		{
			@string = LocalizationManager.GetString("LOC_ID_BIG_TEXT_UI_STONE_PRIME_BOSS_DEFEATED_TITLE_1", isFemale: false);
		}
		m_bossDefeatedArgs.Initialize(base.Boss.EnemyType, EnemyRank.Basic, bossDefeatedDisplayDuration, @string);
	}

	protected override IEnumerator StartIntro()
	{
		if (PlayerManager.GetPlayerController().IsFacingRight)
		{
			PlayerManager.GetPlayerController().CharacterCorgi.Flip();
		}
		AudioManager.Play(null, "event:/SFX/Enemies/vo_skellyBoss_agro");
		MusicManager.PlayMusic(SongID.BridgeBossBGM_Tettix_Miniboss_BGM_137, isOverride: false);
		base.Boss.Visuals.SetActive(value: false);
		yield return base.StartIntro();
		m_bossesSpawned++;
	}

	protected override void OnModeShift(object sender, EventArgs args)
	{
		base.OnModeShift(sender, args);
		if (!m_secondBossSpawned)
		{
			m_secondBossSpawned = true;
			StartCoroutine(SpawnSecondBossCoroutine());
		}
	}

	private IEnumerator SpawnSecondBossCoroutine()
	{
		SkeletonBoss_Basic_AIScript aiScript = base.Boss.LogicController.LogicScript as SkeletonBoss_Basic_AIScript;
		while (!aiScript.ModeShiftComplete_SpawnSecondBoss)
		{
			yield return null;
		}
		if (base.Boss.EnemyRank == EnemyRank.Basic)
		{
			Boss2 = EnemyManager.SummonEnemy(base.Boss, EnemyType.SkeletonBossB, EnemyRank.Basic, m_secondBossSpawnPosition.transform.position, useAbsPos: true);
		}
		else
		{
			Boss2 = EnemyManager.SummonEnemy(base.Boss, EnemyType.SkeletonBossB, EnemyRank.Advanced, m_secondBossSpawnPosition.transform.position, useAbsPos: true);
		}
		m_bossesSpawned++;
		AudioManager.Play(null, "event:/SFX/Enemies/vo_skellyBoss_laugh", Boss2.transform.localPosition);
		MusicManager.SetBossEncounterParam(0.51f);
	}
}
