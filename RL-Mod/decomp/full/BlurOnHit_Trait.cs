using System;
using System.Collections;
using UnityEngine;

public class BlurOnHit_Trait : BaseTrait
{
	[SerializeField]
	private GameObject m_blurGameObject;

	private WaitRL_Yield m_waitYield;

	private Tween m_blurTween;

	private Action<MonoBehaviour, EventArgs> m_onPlayerHit;

	public override TraitType TraitType => TraitType.BlurOnHit;

	protected override void Awake()
	{
		base.Awake();
		m_onPlayerHit = OnPlayerHit;
	}

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		m_waitYield = new WaitRL_Yield(0f);
		PlayerController playerController = PlayerManager.GetPlayerController();
		float y = playerController.Midpoint.y - playerController.transform.position.y;
		m_traitMask.transform.SetParent(playerController.transform);
		m_traitMask.transform.localPosition = new Vector3(0f, y, 0f);
		float num = 10f;
		m_traitMask.transform.localScale = new Vector3(num, num, 1f);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHit, m_onPlayerHit);
		m_blurGameObject.SetActive(value: false);
	}

	public override void AssignGreenMask()
	{
		base.AssignGreenMask();
		m_postProcessOverrideController.Profile.OverrideTintGreenChannel = true;
		m_postProcessOverrideController.Profile.TintGreenChannel = m_postProcessOverrideController.Profile.TintRedChannel;
		m_postProcessOverrideController.Profile.TintRedChannel = 0f;
		m_postProcessOverrideController.Profile.OverrideTintRedChannel = false;
	}

	private void OnPlayerHit(MonoBehaviour sender, EventArgs args)
	{
		StopAllCoroutines();
		StartCoroutine(BlurCoroutine());
	}

	private IEnumerator BlurCoroutine()
	{
		m_blurGameObject.SetActive(value: true);
		CameraController.ForegroundPostProcessing.TintBaselineAmount = 1f;
		if (!m_blurTween.IsNativeNull())
		{
			m_blurTween.StopTweenWithConditionChecks(runEndHandlers: false, CameraController.ForegroundPostProcessing);
		}
		m_waitYield.CreateNew(2.5f);
		yield return m_waitYield;
		m_blurTween = TweenManager.TweenTo(CameraController.ForegroundPostProcessing, 0.5f, Ease.None, "TintBaselineAmount", 0f);
		yield return m_blurTween.TweenCoroutine;
		CameraController.ForegroundPostProcessing.TintBaselineAmount = 0f;
		m_blurGameObject.SetActive(value: false);
	}

	public override void DisableOnCutscene()
	{
		StopAllCoroutines();
		m_blurGameObject.SetActive(value: false);
		CameraController.ForegroundPostProcessing.TintBaselineAmount = 0f;
		base.DisableOnCutscene();
	}

	public override void DisableOnDeath()
	{
		StopAllCoroutines();
		m_blurGameObject.SetActive(value: false);
		CameraController.ForegroundPostProcessing.TintBaselineAmount = 0f;
		base.DisableOnDeath();
	}

	private void OnDestroy()
	{
		if ((bool)m_traitMask)
		{
			UnityEngine.Object.Destroy(m_traitMask.gameObject);
		}
		if (!GameUtility.IsApplicationQuitting)
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHit, m_onPlayerHit);
		}
	}
}
