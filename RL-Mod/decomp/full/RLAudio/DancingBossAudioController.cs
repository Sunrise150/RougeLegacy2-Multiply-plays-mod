using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class DancingBossAudioController : MonoBehaviour, IAudioEventEmitter
{
	private enum AttackStateID
	{
		None,
		TellIntro,
		TellHold,
		AttackIntro,
		AttackHold,
		AttackExit
	}

	private enum FlyingStateID
	{
		Stopped,
		Flying
	}

	[SerializeField]
	[EventRef]
	private string m_hoopMovementAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_hoopMovementEndAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_flyingAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_stopFlyingAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_changeDirectionWhileFlyingAudioPath = string.Empty;

	[SerializeField]
	[Range(1f, 100f)]
	private float m_maxFlyingSpeed = 5f;

	[SerializeField]
	[EventRef]
	private string m_summonSentriesStartAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_summonSentriesCompleteAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_verticalWaveShootAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_verticalWaveSpawnPortalsAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_verticalWaveDespawnPortalsAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_horizontalWaveShootAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_horizontalWaveSpawnPortalsAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_horizontalWaveDespawnPortalsAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_spreadAttackSpawnPortalsAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_spreadAttackIntroAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_spreadAttackStartAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_spreadAttackCompleteAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_spreadAttackDespawnPortalsAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_summonTellStartAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_summonTellCompleteAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_summonAttackStartAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_summonAttackCompleteAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_summonSpawnPortalAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_summonDespawnPortalAudioPath = string.Empty;

	private Animator m_animator;

	private BossRoomController m_bossRoomController;

	private EnemyController m_enemyController;

	private string m_description = string.Empty;

	private AttackStateID m_previousAttackState;

	private const string HOOP_PARAMETER = "dancingBoss_idleSpeed";

	private const string FLYING_SPEED_PARAMETER = "danceBoss_flySpeed";

	private const int INITIAL_WAVE_AUDIO_SOURCE_COUNT = 4;

	private const string PHASE_ANIMATOR_PARAMETER = "Phase";

	private FlyingStateID m_currentFlyingState;

	private Vector2 m_previousFlightDirection;

	private bool m_isIntroComplete;

	private List<GameObject> m_waveShootLoopAudioSources;

	private WaitForSeconds m_waitUntilSummonPortalDespawnStart = new WaitForSeconds(1.2f);

	private Vector3 m_spreadAttackSummonPosition;

	private EventInstance m_hoopAudioEventInstance;

	private EventInstance m_flyingAudioEventInstance;

	private EventInstance m_spreadAttackSummoningAudioEventInstance;

	private EventInstance m_summonAttackAudioEventInstance;

	private EventInstance m_spreadAttackEventInstance;

	private EventInstance m_waveAttackEventInstance;

	private EventInstance m_summoningPortalsEventInstance;

	private Action m_onBossDefeated;

	private Action m_onSentrySummonStart;

	private Action m_onSentrySummonComplete;

	private Action<DancingBossWaveFiredEventArgs> m_onWaveFired;

	private Action<Vector3> m_onSpreadAttackStart;

	private Action m_onSpreadAttackComplete;

	private Action m_onSummonShotsTellStart;

	private Action m_onSummonShotsTellComplete;

	private Action m_onSummonShotsAttackStart;

	private Action m_onSummonShotsAttackComplete;

	private Action<Vector2> m_onSummonShotsPortalOpened;

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
		m_animator = GetComponent<Animator>();
		m_enemyController = GetComponent<EnemyController>();
		m_onBossDefeated = OnBossDefeated;
		m_onSentrySummonStart = OnSentrySummonStart;
		m_onSentrySummonComplete = OnSentrySummonComplete;
		m_onWaveFired = OnWaveFired;
		m_onSpreadAttackStart = OnSpreadAttackStart;
		m_onSpreadAttackComplete = OnSpreadAttackComplete;
		m_onSummonShotsTellStart = OnSummonShotsTellStart;
		m_onSummonShotsAttackStart = OnSummonShotsAttackStart;
		m_onSummonShotsAttackComplete = OnSummonShotsAttackComplete;
		m_onSummonShotsTellComplete = OnSummonShotsTellComplete;
		m_onSummonShotsPortalOpened = OnSummonShotsPortalOpened;
	}

	private void Start()
	{
		StartCoroutine(WaitUntilIntroIsComplete());
	}

	private void OnDisable()
	{
		m_isIntroComplete = false;
		DancingBoss_Basic_AIScript obj = m_enemyController.LogicController.LogicScript as DancingBoss_Basic_AIScript;
		if ((bool)m_bossRoomController)
		{
			m_bossRoomController.BossDefeatedRelay.RemoveListener(m_onBossDefeated);
		}
		obj.SummonSentriesStartRelay.RemoveListener(m_onSentrySummonStart);
		obj.SummonSentriesCompleteRelay.RemoveListener(m_onSentrySummonComplete);
		obj.WaveFiredRelay.RemoveListener(m_onWaveFired);
		obj.SpreadAttackStartRelay.RemoveListener(m_onSpreadAttackStart);
		obj.SpreadAttackCompleteRelay.RemoveListener(m_onSpreadAttackComplete);
		obj.SummonShotsTellStartRelay.RemoveListener(m_onSummonShotsTellStart);
		obj.SummonShotsTellCompleteRelay.RemoveListener(m_onSummonShotsTellComplete);
		obj.SummonShotsAttackStartRelay.RemoveListener(m_onSummonShotsAttackStart);
		obj.SummonShotsAttackCompleteRelay.RemoveListener(m_onSummonShotsAttackComplete);
		obj.SummonShotsPortalOpenedRelay.RemoveListener(m_onSummonShotsPortalOpened);
		StopAudioEventInstances();
	}

	private void OnDestroy()
	{
		if (m_waveShootLoopAudioSources != null)
		{
			for (int num = m_waveShootLoopAudioSources.Count - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(m_waveShootLoopAudioSources[num]);
				m_waveShootLoopAudioSources[num] = null;
			}
			m_waveShootLoopAudioSources = null;
		}
		if (m_hoopAudioEventInstance.isValid())
		{
			m_hoopAudioEventInstance.release();
		}
		if (m_flyingAudioEventInstance.isValid())
		{
			m_flyingAudioEventInstance.release();
		}
		if (m_spreadAttackSummoningAudioEventInstance.isValid())
		{
			m_spreadAttackSummoningAudioEventInstance.release();
		}
		if (m_summonAttackAudioEventInstance.isValid())
		{
			m_summonAttackAudioEventInstance.release();
		}
		if (m_spreadAttackEventInstance.isValid())
		{
			m_spreadAttackEventInstance.release();
		}
		if (m_waveAttackEventInstance.isValid())
		{
			m_waveAttackEventInstance.release();
		}
		if (m_summoningPortalsEventInstance.isValid())
		{
			m_summoningPortalsEventInstance.release();
		}
	}

	private IEnumerator WaitUntilIntroIsComplete()
	{
		while (m_enemyController.Room == null)
		{
			yield return null;
		}
		if (!m_bossRoomController)
		{
			m_bossRoomController = m_enemyController.Room.gameObject.GetComponent<BossRoomController>();
		}
		bool hasEnteredIntroState;
		do
		{
			hasEnteredIntroState = m_animator.GetCurrentAnimatorStateInfo(0).IsTag("Intro");
			yield return null;
		}
		while (!hasEnteredIntroState);
		bool hasEnteredIdleState;
		do
		{
			hasEnteredIdleState = m_animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle");
			yield return null;
		}
		while (!hasEnteredIdleState);
		OnIntroComplete();
	}

	private void OnIntroComplete()
	{
		m_animator.SetInteger("Phase", 0);
		DancingBoss_Basic_AIScript obj = m_enemyController.LogicController.LogicScript as DancingBoss_Basic_AIScript;
		if ((bool)m_bossRoomController)
		{
			m_bossRoomController.BossDefeatedRelay.AddListener(m_onBossDefeated);
		}
		obj.SummonSentriesStartRelay.AddListener(m_onSentrySummonStart);
		obj.SummonSentriesCompleteRelay.AddListener(m_onSentrySummonComplete);
		obj.WaveFiredRelay.AddListener(m_onWaveFired);
		obj.SpreadAttackStartRelay.AddListener(m_onSpreadAttackStart);
		obj.SpreadAttackCompleteRelay.AddListener(m_onSpreadAttackComplete);
		obj.SummonShotsTellStartRelay.AddListener(m_onSummonShotsTellStart);
		obj.SummonShotsTellCompleteRelay.AddListener(m_onSummonShotsTellComplete);
		obj.SummonShotsAttackStartRelay.AddListener(m_onSummonShotsAttackStart);
		obj.SummonShotsAttackCompleteRelay.AddListener(m_onSummonShotsAttackComplete);
		obj.SummonShotsPortalOpenedRelay.AddListener(m_onSummonShotsPortalOpened);
		m_hoopAudioEventInstance = AudioUtility.GetEventInstance(m_hoopMovementAudioPath, base.transform);
		AudioManager.Play(this, m_hoopAudioEventInstance);
		m_flyingAudioEventInstance = AudioUtility.GetEventInstance(m_flyingAudioPath, base.transform);
		m_isIntroComplete = true;
	}

	private void OnSpreadIntroAnimStart()
	{
		AudioManager.PlayOneShotAttached(this, m_spreadAttackIntroAudioPath, m_enemyController.gameObject);
	}

	private void OnSpreadIntroAnimComplete()
	{
	}

	private void OnSpreadAttackAnimStart()
	{
		if (!m_spreadAttackEventInstance.isValid())
		{
			m_spreadAttackEventInstance = AudioUtility.GetEventInstance(m_spreadAttackStartAudioPath, m_enemyController.transform);
		}
		AudioManager.PlayAttached(this, m_spreadAttackEventInstance, m_enemyController.gameObject);
	}

	private void OnSpreadAttackAnimComplete()
	{
		AudioManager.PlayOneShotAttached(this, m_spreadAttackCompleteAudioPath, m_enemyController.gameObject);
	}

	private void OnSpreadAttackStart(Vector3 summonPosition)
	{
		m_spreadAttackSummonPosition = summonPosition;
		if (!m_spreadAttackSummoningAudioEventInstance.isValid())
		{
			m_spreadAttackSummoningAudioEventInstance = AudioUtility.GetEventInstance(m_spreadAttackSpawnPortalsAudioPath, base.transform);
		}
		AudioManager.Play(this, m_spreadAttackSummoningAudioEventInstance, summonPosition);
	}

	private void OnSpreadAttackComplete()
	{
		if (m_spreadAttackSummoningAudioEventInstance.isValid())
		{
			AudioManager.Stop(m_spreadAttackSummoningAudioEventInstance);
		}
		AudioManager.PlayOneShot(this, m_spreadAttackDespawnPortalsAudioPath, m_spreadAttackSummonPosition);
	}

	private void OnSummonShotsTellStart()
	{
		AudioManager.PlayOneShot(this, m_summonTellStartAudioPath, m_enemyController.transform.position);
	}

	private void OnSummonShotsTellComplete()
	{
		AudioManager.PlayOneShot(this, m_summonTellCompleteAudioPath, m_enemyController.transform.position);
	}

	private void OnSummonShotsAttackStart()
	{
		if (!m_summonAttackAudioEventInstance.isValid())
		{
			m_summonAttackAudioEventInstance = AudioUtility.GetEventInstance(m_summonAttackStartAudioPath, base.transform);
		}
		AudioManager.Play(this, m_summonAttackAudioEventInstance, m_enemyController.transform.position);
	}

	private void OnSummonShotsAttackComplete()
	{
		if (m_summonAttackAudioEventInstance.isValid())
		{
			AudioManager.Stop(m_summonAttackAudioEventInstance);
		}
		AudioManager.PlayOneShot(this, m_summonAttackCompleteAudioPath, m_enemyController.transform.position);
	}

	private void OnSummonShotsPortalOpened(Vector2 position)
	{
		StartCoroutine(PlaySummonShotsPortalAudio(position));
	}

	private IEnumerator PlaySummonShotsPortalAudio(Vector2 position)
	{
		AudioManager.PlayDelayedOneShot(this, m_summonSpawnPortalAudioPath, position);
		yield return m_waitUntilSummonPortalDespawnStart;
		AudioManager.PlayDelayedOneShot(this, m_summonDespawnPortalAudioPath, position);
	}

	private void OnWaveFired(DancingBossWaveFiredEventArgs waveFiredEventArgs)
	{
		StartCoroutine(PlayWaveFiredAudio(waveFiredEventArgs));
	}

	private IEnumerator PlayWaveFiredAudio(DancingBossWaveFiredEventArgs waveInfo)
	{
		string shootAudioPath = m_verticalWaveShootAudioPath;
		string eventPath = m_verticalWaveSpawnPortalsAudioPath;
		string despawnAudioPath = m_verticalWaveDespawnPortalsAudioPath;
		if (waveInfo.Side == RoomSide.Top || waveInfo.Side == RoomSide.Bottom)
		{
			shootAudioPath = m_horizontalWaveShootAudioPath;
			eventPath = m_horizontalWaveSpawnPortalsAudioPath;
			despawnAudioPath = m_horizontalWaveDespawnPortalsAudioPath;
		}
		if (!m_summoningPortalsEventInstance.isValid())
		{
			m_summoningPortalsEventInstance = AudioUtility.GetEventInstance(eventPath, base.transform);
		}
		AudioManager.Play(this, m_summoningPortalsEventInstance, waveInfo.Origin);
		yield return m_waitUntilSummonPortalDespawnStart;
		AudioManager.Stop(m_summoningPortalsEventInstance);
		AudioManager.PlayOneShot(this, despawnAudioPath, waveInfo.Origin);
		Vector3 direction = Vector2.zero;
		switch (waveInfo.Side)
		{
		case RoomSide.Top:
			direction = Vector2.down;
			break;
		case RoomSide.Bottom:
			direction = Vector2.up;
			break;
		case RoomSide.Left:
			direction = Vector2.right;
			break;
		case RoomSide.Right:
			direction = Vector2.left;
			break;
		}
		GameObject waveShootLoopAudioSource = GetWaveShootLoopAudioSource();
		waveShootLoopAudioSource.transform.position = waveInfo.Origin;
		waveShootLoopAudioSource.SetActive(value: true);
		if (!m_waveAttackEventInstance.isValid())
		{
			m_waveAttackEventInstance = AudioUtility.GetEventInstance(shootAudioPath, waveShootLoopAudioSource.transform);
		}
		AudioManager.PlayAttached(this, m_waveAttackEventInstance, waveShootLoopAudioSource);
		float timeStart = Time.time;
		while (Time.time - timeStart < waveInfo.LifeSpan)
		{
			waveShootLoopAudioSource.transform.position += waveInfo.Speed * direction * Time.deltaTime;
			yield return null;
		}
		AudioManager.Stop(m_waveAttackEventInstance);
		waveShootLoopAudioSource.SetActive(value: false);
	}

	private GameObject GetWaveShootLoopAudioSource()
	{
		if (m_waveShootLoopAudioSources == null)
		{
			m_waveShootLoopAudioSources = new List<GameObject>(4);
			for (int i = 0; i < 4; i++)
			{
				GameObject gameObject = new GameObject("Wave Shoot Loop Audio Source");
				m_waveShootLoopAudioSources.Add(gameObject);
				gameObject.transform.SetParent(m_enemyController.Room.gameObject.transform);
				gameObject.SetActive(value: false);
			}
		}
		GameObject gameObject2 = null;
		for (int j = 0; j < 4; j++)
		{
			if (!m_waveShootLoopAudioSources[j].activeInHierarchy)
			{
				gameObject2 = m_waveShootLoopAudioSources[j];
				break;
			}
		}
		if (gameObject2 == null)
		{
			gameObject2 = new GameObject("Wave Shoot Loop Audio Source");
			gameObject2.transform.SetParent(m_enemyController.Room.gameObject.transform);
			m_waveShootLoopAudioSources.Add(gameObject2);
		}
		return gameObject2;
	}

	private void OnSentrySummonStart()
	{
		AudioManager.PlayOneShot(this, m_summonSentriesStartAudioPath);
		m_animator.SetInteger("Phase", 1);
	}

	private void OnSentrySummonComplete()
	{
		AudioManager.PlayOneShot(this, m_summonSentriesCompleteAudioPath);
	}

	private void OnBossDefeated()
	{
		if ((bool)m_bossRoomController)
		{
			m_bossRoomController.BossDefeatedRelay.RemoveListener(m_onBossDefeated);
		}
		StopAudioEventInstances();
		AudioManager.PlayOneShotAttached(this, m_hoopMovementEndAudioPath, base.gameObject);
	}

	private void StopAudioEventInstances()
	{
		if (m_hoopAudioEventInstance.isValid())
		{
			AudioManager.Stop(m_hoopAudioEventInstance);
		}
		if (m_flyingAudioEventInstance.isValid())
		{
			AudioManager.Stop(m_flyingAudioEventInstance);
		}
		if (m_waveAttackEventInstance.isValid())
		{
			AudioManager.Stop(m_waveAttackEventInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		if (m_summoningPortalsEventInstance.isValid())
		{
			AudioManager.Stop(m_summoningPortalsEventInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		if (m_spreadAttackSummoningAudioEventInstance.isValid())
		{
			AudioManager.Stop(m_spreadAttackSummoningAudioEventInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		if (m_summonAttackAudioEventInstance.isValid())
		{
			AudioManager.Stop(m_summonAttackAudioEventInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
	}

	private void Update()
	{
		if (m_isIntroComplete)
		{
			UpdateHoopAudio();
			UpdateFlyingAudio();
		}
	}

	private void UpdateFlyingAudio()
	{
		bool flag = m_enemyController.Velocity.sqrMagnitude > 0f;
		if (flag && m_currentFlyingState == FlyingStateID.Stopped)
		{
			AudioManager.PlayAttached(this, m_flyingAudioEventInstance, base.gameObject);
			m_currentFlyingState = FlyingStateID.Flying;
		}
		else if (!flag && m_currentFlyingState == FlyingStateID.Flying)
		{
			AudioManager.Stop(m_flyingAudioEventInstance);
			AudioManager.PlayOneShotAttached(this, m_stopFlyingAudioPath, base.gameObject);
			m_currentFlyingState = FlyingStateID.Stopped;
		}
		if (flag)
		{
			float magnitude = m_enemyController.Velocity.magnitude;
			float value = 0f;
			if (magnitude > 0f)
			{
				value = ((!(magnitude < m_maxFlyingSpeed)) ? 1f : (magnitude / m_maxFlyingSpeed));
			}
			if (m_flyingAudioEventInstance.isValid() && AudioUtility.GetHasParameter(m_flyingAudioEventInstance, "danceBoss_flySpeed"))
			{
				m_flyingAudioEventInstance.setParameterByName("danceBoss_flySpeed", value);
			}
		}
		Vector2 normalized = m_enemyController.Velocity.normalized;
		if (Mathf.Sign(m_previousFlightDirection.x) != Mathf.Sign(normalized.x) || Mathf.Sign(m_previousFlightDirection.y) != Mathf.Sign(normalized.y))
		{
			AudioManager.PlayOneShotAttached(this, m_changeDirectionWhileFlyingAudioPath, base.gameObject);
			m_previousFlightDirection = normalized;
		}
	}

	private void UpdateHoopAudio()
	{
		AttackStateID attackState = GetAttackState();
		if (attackState == m_previousAttackState)
		{
			return;
		}
		if (attackState == AttackStateID.AttackHold)
		{
			if (m_hoopAudioEventInstance.isValid())
			{
				m_hoopAudioEventInstance.setParameterByName("dancingBoss_idleSpeed", 1f);
			}
		}
		else if (m_previousAttackState == AttackStateID.AttackHold && m_hoopAudioEventInstance.isValid())
		{
			m_hoopAudioEventInstance.setParameterByName("dancingBoss_idleSpeed", 0f);
		}
		m_previousAttackState = attackState;
	}

	private AttackStateID GetAttackState()
	{
		AttackStateID result = AttackStateID.None;
		if (m_animator != null)
		{
			if (GetIsAnimatorStateTag("Tell_Intro"))
			{
				result = AttackStateID.TellIntro;
			}
			else if (GetIsAnimatorStateTag("Tell_Hold"))
			{
				result = AttackStateID.TellHold;
			}
			else if (GetIsAnimatorStateTag("Attack_Intro"))
			{
				result = AttackStateID.AttackIntro;
			}
			else if (GetIsAnimatorStateTag("Attack_Hold"))
			{
				result = AttackStateID.AttackHold;
			}
			else if (GetIsAnimatorStateTag("Attack_Exit"))
			{
				result = AttackStateID.AttackExit;
			}
		}
		return result;
	}

	private bool GetIsAnimatorStateTag(string tag)
	{
		return m_animator.GetCurrentAnimatorStateInfo(0).IsTag(tag);
	}
}
