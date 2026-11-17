using System;
using System.Collections;
using Cinemachine;
using FMOD.Studio;
using RLAudio;
using Rewired;
using SceneManagement_RL;
using UnityEngine;

public class ParadeController : MonoBehaviour
{
	[Serializable]
	private class ParadeActionOrderEntry
	{
		public GameObject ActionObj;

		public float ExitDelay;
	}

	[SerializeField]
	private Transform m_startingCamPos;

	[SerializeField]
	private GameObject m_fastForwardObj;

	[SerializeField]
	private CinemachineVirtualCamera m_vcam;

	[SerializeField]
	private ParadePortrait m_portraitPrefab;

	[SerializeField]
	private ParadeActionOrderEntry[] m_actionOrderArray;

	private IParadeAction[][] m_actionArray;

	private Action<InputActionEventData> m_toggleSpeedUp;

	private EventInstance m_ambienceEventInstance;

	private EventInstance m_rootMoveEventInstance;

	public CinemachineVirtualCamera VirtualCamera => m_vcam;

	private void Awake()
	{
		m_toggleSpeedUp = ToggleSpeedUp;
		m_actionArray = new IParadeAction[m_actionOrderArray.Length][];
		for (int i = 0; i < m_actionOrderArray.Length; i++)
		{
			ParadeActionController componentInChildren = m_actionOrderArray[i].ActionObj.GetComponentInChildren<ParadeActionController>();
			if (!componentInChildren)
			{
				continue;
			}
			m_actionArray[i] = new IParadeAction[componentInChildren.transform.childCount];
			for (int j = 0; j < m_actionArray[i].Length; j++)
			{
				IParadeAction component = componentInChildren.transform.GetChild(j).gameObject.GetComponent<IParadeAction>();
				if (!component.IsNativeNull())
				{
					m_actionArray[i][j] = component;
				}
			}
		}
		Renderer[] componentsInChildren = base.gameObject.FindObjectReference("FinalBossBlack").GetComponentsInChildren<Renderer>();
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		Renderer[] array = componentsInChildren;
		foreach (Renderer obj in array)
		{
			obj.GetPropertyBlock(materialPropertyBlock);
			materialPropertyBlock.SetFloat("_TextureBlend", 1f);
			obj.SetPropertyBlock(materialPropertyBlock);
		}
		if (!m_fastForwardObj.transform.parent.gameObject.activeSelf)
		{
			m_fastForwardObj.transform.parent.gameObject.SetActive(value: true);
		}
		m_fastForwardObj.SetActive(value: false);
	}

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(0.2f);
		RewiredMapController.SetMap(GameInputMode.Window);
		MusicManager.PlayMusic(SongID.Parade_ASITP_Videri_Enemy_Parade, isOverride: false);
	}

	private void OnEnable()
	{
		AnimBehaviourEventEmitter.DisableEmitter_STATIC = true;
		RewiredMapController.SetMap(GameInputMode.Window);
		Rewired_RL.Player.AddInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		Rewired_RL.Player.AddInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Confirm");
		if (!m_ambienceEventInstance.isValid())
		{
			m_ambienceEventInstance = AudioUtility.GetEventInstance("event:/Environment/amb_eyeballBoss", CameraController.UICamera.transform);
		}
		if (!m_rootMoveEventInstance.isValid())
		{
			m_rootMoveEventInstance = AudioUtility.GetEventInstance("event:/UI/FrontEnd/ui_fe_credits_roots_move_loop", CameraController.UICamera.transform);
		}
		if (m_ambienceEventInstance.isValid())
		{
			AudioManager.Play(null, m_ambienceEventInstance);
		}
		if (m_rootMoveEventInstance.isValid())
		{
			m_rootMoveEventInstance.setParameterByName("creditsRootMove", 0f, ignoreseekspeed: true);
			AudioManager.Play(null, m_rootMoveEventInstance);
		}
		StartCoroutine(BeginParade());
	}

	private void OnDisable()
	{
		AnimBehaviourEventEmitter.DisableEmitter_STATIC = false;
		if (m_ambienceEventInstance.isValid())
		{
			AudioManager.Stop(m_ambienceEventInstance);
		}
		if (m_rootMoveEventInstance.isValid())
		{
			AudioManager.Stop(m_rootMoveEventInstance);
		}
		if (!GameManager.IsApplicationClosing)
		{
			Rewired_RL.Player.RemoveInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			Rewired_RL.Player.RemoveInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Confirm");
		}
	}

	private void OnDestroy()
	{
		if (m_ambienceEventInstance.isValid())
		{
			m_ambienceEventInstance.release();
		}
		if (m_rootMoveEventInstance.isValid())
		{
			m_rootMoveEventInstance.release();
		}
	}

	private void ToggleSpeedUp(InputActionEventData eventData)
	{
		if (!eventData.GetButtonUp())
		{
			RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 15f);
			m_fastForwardObj.SetActive(value: true);
		}
		else
		{
			RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
			m_fastForwardObj.SetActive(value: false);
		}
	}

	private IEnumerator BeginParade()
	{
		Vector3 localPosition = m_startingCamPos.localPosition;
		localPosition.z = m_vcam.transform.position.z;
		m_vcam.transform.position = localPosition;
		for (int i = 0; i < m_actionArray.Length; i++)
		{
			for (int j = 0; j < m_actionArray[i].Length; j++)
			{
				IParadeAction paradeAction = m_actionArray[i][j];
				if (!paradeAction.IsNativeNull())
				{
					yield return paradeAction.TriggerAction(this);
				}
			}
			if (m_actionOrderArray[i].ExitDelay > 0f)
			{
				float nextActionDelay2 = Mathf.Min(m_actionOrderArray[i].ExitDelay, 3f);
				nextActionDelay2 += Time.time;
				while (Time.time < nextActionDelay2)
				{
					yield return null;
				}
			}
		}
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.SeenParade, value: true);
		float fadeOutDelay = Time.time + 3f;
		while (Time.time < fadeOutDelay)
		{
			yield return null;
		}
		CreditsController.IsEndingScroll = true;
		SceneLoader_RL.LoadScene(SceneID.Credits, TransitionID.FadeToBlackNoLoading);
	}

	public void SetRootMoveAudioParam(bool moving)
	{
		if (m_rootMoveEventInstance.isValid())
		{
			m_rootMoveEventInstance.setParameterByName("creditsRootMove", moving ? 1f : 0f);
		}
	}

	public void CreateEnemyPortrait(EnemyController enemyController)
	{
		if ((bool)enemyController)
		{
			Transform transform = enemyController.transform.FindDeep("Visuals");
			if ((bool)transform && transform.childCount > 0)
			{
				ParadePortrait component = UnityEngine.Object.Instantiate(m_portraitPrefab.gameObject, base.transform).GetComponent<ParadePortrait>();
				component.EnemyType = enemyController.EnemyType;
				component.EnemyRank = enemyController.EnemyRank;
				component.name = enemyController.EnemyType.ToString() + " " + enemyController.EnemyRank.ToString() + " Portrait";
				component.NameText.text = enemyController.EnemyType.ToString() + " " + enemyController.EnemyRank;
				GameObject gameObject = UnityEngine.Object.Instantiate(transform.GetChild(0).gameObject, component.transform);
				float scale = EnemyClassLibrary.GetEnemyData(enemyController.EnemyType, enemyController.EnemyRank).Scale;
				float x = gameObject.transform.localScale.x;
				gameObject.transform.localScale = new Vector3(scale, scale, scale) * x;
				Animator component2 = enemyController.GetComponent<Animator>();
				component.Animator.runtimeAnimatorController = component2.runtimeAnimatorController;
				component.Animator.avatar = component2.avatar;
				component.Model = gameObject;
			}
			else
			{
				Debug.Log("<color=red>ERROR: Could not create enemy of type: " + enemyController.EnemyType.ToString() + "." + enemyController.EnemyRank.ToString() + " because Visuals child GO not found.</color>");
			}
		}
	}
}
