using System.Collections;
using UnityEngine;

namespace SceneManagement_RL;

public class FadeToBlackWithLoading_SceneTransition : Transition_V2, ISceneLoadingTransition, ITransition
{
	[SerializeField]
	private float m_timeToFade = 1f;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private CanvasGroup m_loadingIndicatorCanvasGroup;

	[SerializeField]
	private RectTransform m_rotatingGear;

	public override TransitionID ID => TransitionID.FadeToBlackWithLoading;

	protected override void Awake()
	{
		base.Awake();
		m_canvasGroup.alpha = 0f;
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
	}

	public IEnumerator TransitionIn()
	{
		m_loadingIndicatorCanvasGroup.alpha = 0f;
		m_canvasGroup.alpha = 0f;
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, m_timeToFade, Ease.None, "alpha", 1).TweenCoroutine;
		yield return TweenManager.TweenTo_UnscaledTime(m_loadingIndicatorCanvasGroup, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
	}

	public IEnumerator TransitionOut()
	{
		m_canvasGroup.alpha = 1f;
		yield return TweenManager.TweenTo_UnscaledTime(m_loadingIndicatorCanvasGroup, 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, m_timeToFade, Ease.None, "alpha", 0).TweenCoroutine;
	}

	public override IEnumerator Run()
	{
		yield break;
	}

	private void Update()
	{
		Vector3 localEulerAngles = m_rotatingGear.localEulerAngles;
		localEulerAngles.z -= 30f * Time.unscaledDeltaTime;
		m_rotatingGear.localEulerAngles = localEulerAngles;
	}
}
