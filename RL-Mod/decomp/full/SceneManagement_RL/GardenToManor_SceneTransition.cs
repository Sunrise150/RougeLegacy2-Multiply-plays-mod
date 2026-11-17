using System.Collections;
using RLAudio;
using RL_Windows;
using UnityEngine;

namespace SceneManagement_RL;

public class GardenToManor_SceneTransition : Transition_V2, ISceneLoadingTransition, ITransition
{
	[SerializeField]
	private float m_timeToFade = 1f;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private PlayerLookController m_playerModel;

	[SerializeField]
	private GameObject m_biomeLightController;

	[SerializeField]
	private GameObject m_skyObj;

	private Animator m_swipeAnimator;

	private WaitRL_Yield m_waitRL;

	public override TransitionID ID => TransitionID.GardenToManor;

	protected override void Awake()
	{
		base.Awake();
		m_waitRL = new WaitRL_Yield(0f, useUnscaledTime: true);
		m_canvasGroup.alpha = 0f;
		m_swipeAnimator = GetComponent<Animator>();
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
		m_skyObj.SetLayerRecursively(23, includeInactive: true);
	}

	public IEnumerator TransitionIn()
	{
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		m_canvasGroup.alpha = 0f;
		m_playerModel.InitializeLook(SaveManager.PlayerSaveData.CurrentCharacter);
		m_playerModel.gameObject.SetActive(value: false);
		m_playerModel.transform.localScale *= 0.5f;
		m_playerModel.gameObject.SetLayerRecursively(5, includeInactive: true);
		m_biomeLightController.gameObject.SetActive(value: false);
		m_skyObj.SetActive(value: false);
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, m_timeToFade, Ease.None, "alpha", 1).TweenCoroutine;
		TweenManager.StopAllTweens(runEndHandlers: false);
	}

	public IEnumerator TransitionOut()
	{
		m_playerModel.gameObject.SetActive(value: true);
		m_biomeLightController.gameObject.SetActive(value: true);
		if (!WindowManager.GetIsWindowLoaded(WindowID.SkillTree))
		{
			WindowManager.LoadWindow(WindowID.SkillTree);
		}
		(WindowManager.GetWindowController(WindowID.SkillTree) as SkillTreeWindowController).EnableGardenTransitionState(enableTransitionState: true);
		WindowManager.SetWindowIsOpen(WindowID.SkillTree, isOpen: true);
		m_waitRL.CreateNew(0.5f, useUnscaledTime: true);
		yield return m_waitRL;
		Vector3 position = CameraController.UICamera.transform.position;
		position.z = 0f;
		position += new Vector3(-7.4f, -6.5f, 0f);
		m_playerModel.transform.position = position;
		Vector3 position2 = CameraController.UICamera.transform.position;
		position2.z = 0f;
		m_skyObj.transform.position = position2;
		m_skyObj.SetActive(value: true);
		Animator castleAnimator = (WindowManager.GetWindowController(WindowID.SkillTree) as SkillTreeWindowController).CastleAnimator;
		EffectManager.AddAnimatorToDisableList(castleAnimator);
		castleAnimator.SetBool("StartingStairs", value: true);
		castleAnimator.SetBool("Entrance_1", value: true);
		castleAnimator.Update(1f);
		castleAnimator.Update(1f);
		EffectManager.RemoveAnimatorFromDisableList(castleAnimator);
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_ending_stairwayClimb", CameraController.GameCamera.transform.position);
		m_playerModel.Animator.Play("WalkUpStairs", 0, 0f);
		TweenManager.TweenBy(m_playerModel.transform, 4f, Ease.None, "localPosition.y", 1.5f);
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, m_timeToFade, Ease.None, "alpha", 0).TweenCoroutine;
		m_waitRL.CreateNew(3f, useUnscaledTime: true);
		yield return m_waitRL;
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_ending_enterCastle", CameraController.GameCamera.transform.position);
		m_swipeAnimator.SetBool("Covered", value: true);
		yield return null;
		AnimatorStateInfo currentAnimatorStateInfo = m_swipeAnimator.GetCurrentAnimatorStateInfo(0);
		float waitTime = currentAnimatorStateInfo.length * Mathf.Abs(currentAnimatorStateInfo.speed) * currentAnimatorStateInfo.speedMultiplier;
		m_waitRL.CreateNew(waitTime, useUnscaledTime: true);
		yield return m_waitRL;
		WindowManager.SetWindowIsOpen(WindowID.SkillTree, isOpen: false);
		(WindowManager.GetWindowController(WindowID.SkillTree) as SkillTreeWindowController).EnableGardenTransitionState(enableTransitionState: false);
		m_playerModel.gameObject.SetActive(value: false);
		m_biomeLightController.gameObject.SetActive(value: false);
		m_skyObj.SetActive(value: false);
		yield return null;
		yield return null;
		m_swipeAnimator.SetBool("Covered", value: false);
		yield return null;
		currentAnimatorStateInfo = m_swipeAnimator.GetCurrentAnimatorStateInfo(0);
		waitTime = currentAnimatorStateInfo.length * Mathf.Abs(currentAnimatorStateInfo.speed) * currentAnimatorStateInfo.speedMultiplier;
		m_waitRL.CreateNew(waitTime, useUnscaledTime: true);
		yield return m_waitRL;
		RewiredMapController.SetIsInCutscene(inCutscene: false);
	}

	public override IEnumerator Run()
	{
		yield break;
	}
}
