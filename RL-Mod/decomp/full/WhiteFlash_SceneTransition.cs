using System.Collections;
using RLAudio;
using SceneManagement_RL;
using UnityEngine;

public class WhiteFlash_SceneTransition : Transition_V2, ISceneLoadingTransition, ITransition
{
	[SerializeField]
	private CanvasGroup m_canvasGroup;

	public override TransitionID ID => TransitionID.WhiteFlash;

	protected override void Awake()
	{
		base.Awake();
		m_canvasGroup.alpha = 0f;
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
	}

	public IEnumerator TransitionIn()
	{
		m_canvasGroup.alpha = 0f;
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_postDeath_transition_flash", CameraController.GameCamera.transform.position);
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, 0.05f, Ease.None, "alpha", 1).TweenCoroutine;
	}

	public IEnumerator TransitionOut()
	{
		m_canvasGroup.alpha = 1f;
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, 0.25f, Ease.None, "alpha", 0).TweenCoroutine;
	}

	public override IEnumerator Run()
	{
		yield break;
	}
}
