using System.Collections;
using FMODUnity;
using RLAudio;
using SceneManagement_RL;
using UnityEngine;

public class IradInitial_SceneTransition : Transition_V2, ISceneLoadingTransition, ITransition, IAudioEventEmitter
{
	[SerializeField]
	private float m_distortDuration = 1f;

	[SerializeField]
	[EventRef]
	private string m_transitionInAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_transitionOutAudioEventPath;

	private GameObject m_distortInPosObj;

	private GameObject m_distortOutPosObj;

	private HeirloomWarp_Effect m_warpEffect;

	private string m_description = string.Empty;

	private float m_storedFallMultiplier = 1f;

	private WaitRL_Yield m_waitYield;

	public override TransitionID ID => TransitionID.IradInitial;

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

	protected override void Awake()
	{
		base.Awake();
		m_warpEffect = CameraController.ForegroundPerspCam.GetComponent<HeirloomWarp_Effect>();
		m_waitYield = new WaitRL_Yield(0f);
	}

	public void SetDistortInPosition(GameObject obj)
	{
		m_distortInPosObj = obj;
	}

	public void SetDistortOutPosition(GameObject obj)
	{
		m_distortOutPosObj = obj;
	}

	public override IEnumerator Run()
	{
		yield break;
	}

	public IEnumerator TransitionIn()
	{
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		if (!m_distortInPosObj)
		{
			EnemySpawnController[] enemySpawnControllers = PlayerManager.GetCurrentPlayerRoom().SpawnControllerManager.EnemySpawnControllers;
			for (int i = 0; i < enemySpawnControllers.Length; i++)
			{
				EnemyController enemyInstance = enemySpawnControllers[i].EnemyInstance;
				if ((bool)enemyInstance && enemyInstance.EnemyRank == EnemyRank.Miniboss && enemyInstance.EnemyType == EnemyType.EyeballBoss_Middle)
				{
					m_distortInPosObj = enemyInstance.gameObject;
					break;
				}
			}
			if (!m_distortInPosObj)
			{
				m_distortInPosObj = PlayerManager.GetPlayerController().gameObject;
			}
		}
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		PlayerController playerController = PlayerManager.GetPlayerController();
		m_storedFallMultiplier = playerController.FallMultiplierOverride;
		playerController.FallMultiplierOverride = 0.1f;
		Vector3 vector = CameraController.GameCamera.WorldToViewportPoint(m_distortInPosObj.transform.position);
		m_warpEffect.WarpCenterX = vector.x;
		m_warpEffect.WarpCenterY = vector.y;
		m_warpEffect.DistortionAmount = 0f;
		m_warpEffect.enabled = true;
		TweenManager.TweenTo(m_warpEffect, 1.5f, Ease.Quad.EaseInOut, "DistortionAmount", 0.025f);
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		Vector2 vector2 = m_distortInPosObj.transform.position - playerController.transform.position;
		vector2 *= 0.75f;
		TweenManager.TweenBy(playerController.transform, 1f, Ease.Quad.EaseIn, "localPosition.x", vector2.x, "localPosition.y", vector2.y);
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		AudioManager.PlayOneShot(this, m_transitionInAudioEventPath, m_distortInPosObj.transform.position);
		yield return TweenManager.TweenTo(m_warpEffect, m_distortDuration, Ease.Back.EaseIn, "DistortionAmount", 1).TweenCoroutine;
		m_warpEffect.DistortionAmount = 1f;
		yield return null;
		RewiredMapController.SetIsInCutscene(inCutscene: false);
	}

	public IEnumerator TransitionOut()
	{
		PlayerManager.GetPlayerController().FallMultiplierOverride = m_storedFallMultiplier;
		if (!m_distortOutPosObj)
		{
			EnemySpawnController[] enemySpawnControllers = PlayerManager.GetCurrentPlayerRoom().SpawnControllerManager.EnemySpawnControllers;
			for (int i = 0; i < enemySpawnControllers.Length; i++)
			{
				EnemyController enemyInstance = enemySpawnControllers[i].EnemyInstance;
				if ((bool)enemyInstance && enemyInstance.EnemyRank == EnemyRank.Miniboss && enemyInstance.EnemyType == EnemyType.EyeballBoss_Middle)
				{
					m_distortOutPosObj = enemyInstance.gameObject;
					break;
				}
			}
			if (!m_distortOutPosObj)
			{
				m_distortOutPosObj = PlayerManager.GetPlayerController().gameObject;
			}
		}
		AudioManager.PlayOneShot(this, m_transitionOutAudioEventPath, m_distortInPosObj.transform.position);
		Vector3 vector = CameraController.GameCamera.WorldToViewportPoint(m_distortOutPosObj.transform.position);
		m_warpEffect.WarpCenterX = vector.x;
		m_warpEffect.WarpCenterY = vector.y;
		m_warpEffect.DistortionAmount = 1f;
		yield return TweenManager.TweenTo(m_warpEffect, m_distortDuration, Ease.Back.EaseOut, "DistortionAmount", 0).TweenCoroutine;
		m_warpEffect.DistortionAmount = 0f;
		m_warpEffect.enabled = false;
	}

	private void OnDisable()
	{
		m_distortInPosObj = null;
		m_distortOutPosObj = null;
	}
}
