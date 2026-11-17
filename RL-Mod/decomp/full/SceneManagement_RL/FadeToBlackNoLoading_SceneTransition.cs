using System.Collections;
using UnityEngine;

namespace SceneManagement_RL;

public class FadeToBlackNoLoading_SceneTransition : Transition_V2, ISceneLoadingTransition, ITransition
{
	[SerializeField]
	private float m_timeToFade = 1f;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	public override TransitionID ID => TransitionID.FadeToBlackNoLoading;

	protected override void Awake()
	{
		base.Awake();
		m_canvasGroup.alpha = 0f;
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
	}

	public IEnumerator TransitionIn()
	{
		m_canvasGroup.alpha = 0f;
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, m_timeToFade, Ease.None, "alpha", 1).TweenCoroutine;
	}

	public IEnumerator TransitionOut()
	{
		m_canvasGroup.alpha = 1f;
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, m_timeToFade, Ease.None, "alpha", 0).TweenCoroutine;
	}

	public override IEnumerator Run()
	{
		yield break;
	}
}
