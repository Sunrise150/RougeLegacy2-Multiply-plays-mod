using System.Collections;
using FMODUnity;
using RL_Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathDefyWindowController : WindowController
{
	[SerializeField]
	private Image m_banner;

	[SerializeField]
	private CanvasGroup m_bannerCanvasGroup;

	[SerializeField]
	private CanvasGroup m_spotlightCanvasGroup;

	[SerializeField]
	private TMP_Text m_text;

	[SerializeField]
	private CanvasGroup m_bgCanvasGroup;

	[SerializeField]
	private StudioEventEmitter m_snapshotEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_deathLoopEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_reverseLoopEventEmitter;

	private WaitRL_Yield m_waitYield;

	private PlayerHUDVisibilityEventArgs m_hudArgs;

	public DeathDefiedType DeathDefiedType { get; set; }

	public override WindowID ID => WindowID.DeathDefy;

	private void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f, useUnscaledTime: true);
		m_hudArgs = new PlayerHUDVisibilityEventArgs(0.5f);
	}

	protected override void OnOpen()
	{
		CameraController.CinemachineBrain.enabled = false;
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HidePlayerHUD, null, m_hudArgs);
		m_windowCanvas.gameObject.SetActive(value: true);
		StartCoroutine(OnOpenCoroutine());
	}

	private IEnumerator OnOpenCoroutine()
	{
		m_snapshotEventEmitter.Play();
		m_deathLoopEventEmitter.Play();
		if (TraitManager.IsTraitActive(TraitType.UpsideDown))
		{
			(TraitManager.GetActiveTrait(TraitType.UpsideDown) as UpsideDown_Trait).ApplyDeathDefy(reset: false);
		}
		PlayerController player = PlayerManager.GetPlayerController();
		Vector3 localScale = m_banner.transform.localScale;
		localScale.x = 0f;
		localScale.y = 0f;
		m_banner.transform.localScale = localScale;
		RectTransform component = m_windowCanvas.GetComponent<RectTransform>();
		Vector3 vector = CameraController.GameCamera.WorldToScreenPoint(player.transform.position);
		vector = new Vector3(vector.x / m_windowCanvas.scaleFactor, vector.y / m_windowCanvas.scaleFactor, vector.z / m_windowCanvas.scaleFactor);
		vector -= new Vector3(component.sizeDelta.x / 2f, component.sizeDelta.y / 2f, 0f);
		m_bannerCanvasGroup.alpha = 1f;
		m_spotlightCanvasGroup.transform.localPosition = vector;
		m_spotlightCanvasGroup.alpha = 0f;
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		player.Animator.SetBool("Stunned", value: true);
		player.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
		player.CharacterHitResponse.AnimateHitEffectsOnUnscaledTime = true;
		Vector3 position = CameraController.GameCamera.transform.position;
		position.z = CameraController.SoloCam.transform.position.z;
		CameraController.SoloCam.transform.position = position;
		CameraController.SoloCam.gameObject.SetActive(value: true);
		CameraController.SoloCam.AddToCameraLayer(player.Visuals);
		CameraController.SoloCam.Camera.orthographicSize = CameraController.GameCamera.orthographicSize;
		yield return null;
		RevivePlayer(player);
		m_waitYield.CreateNew(0.5f, useUnscaledTime: true);
		yield return m_waitYield;
		m_bgCanvasGroup.alpha = 0f;
		m_bgCanvasGroup.gameObject.SetActive(value: true);
		Vector3 position2 = CameraController.GameCamera.transform.position;
		position2.z = m_bgCanvasGroup.transform.position.z;
		m_bgCanvasGroup.transform.position = position2;
		yield return TweenManager.TweenTo_UnscaledTime(m_bgCanvasGroup, 1f, Ease.None, "alpha", 1).TweenCoroutine;
		yield return new WaitForSecondsRealtime(0.25f);
		player.Animator.SetBool("Stunned", value: false);
		int num = Player_EV.PLAYER_DEATH_ANIM_OVERRIDE;
		if (num == -1 && (TraitManager.IsTraitActive(TraitType.Fart) || TraitManager.IsTraitActive(TraitType.SuperFart)))
		{
			num = 9;
		}
		if (SaveManager.PlayerSaveData.IsInHeirloom)
		{
			num = 8;
		}
		if (num > -1 && num < 12)
		{
			player.Animator.SetTrigger("Death" + num);
		}
		else
		{
			int num2 = Random.Range(0, 12);
			while (num2 == 9 || num2 == 8)
			{
				num2 = Random.Range(0, 12);
			}
			player.Animator.SetTrigger("Death" + num2);
		}
		yield return null;
		while (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			yield return null;
		}
		m_waitYield.CreateNew(1f, useUnscaledTime: true);
		yield return m_waitYield;
		player.SetHealth(player.CurrentHealth, additive: false, runEvents: true);
		EffectManager.PlayEffect(player.gameObject, player.Animator, "PlayerDeathDefyStart_Effect", player.transform.localPosition, 0f, EffectStopType.Gracefully);
		m_waitYield.CreateNew(0.5f, useUnscaledTime: true);
		yield return m_waitYield;
		m_reverseLoopEventEmitter.Play();
		player.Animator.SetFloat("Ability_Anim_Speed", -3f);
		while (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.1f)
		{
			yield return null;
		}
		m_reverseLoopEventEmitter.Stop();
		EffectManager.PlayEffect(player.gameObject, player.Animator, "PlayerDeathDefyEnd_Effect", player.transform.localPosition, 0f, EffectStopType.Gracefully);
		player.Animator.SetBool("DeathDefied", value: true);
		player.Animator.SetBool("Victory", value: true);
		player.Animator.SetFloat("Ability_Anim_Speed", 1f);
		TweenManager.TweenTo_UnscaledTime(m_spotlightCanvasGroup, 0.5f, Ease.None, "alpha", 1);
		yield return TweenManager.TweenTo_UnscaledTime(m_banner.transform, 0.5f, Ease.Back.EaseOut, "localScale.x", 1, "localScale.y", 1).TweenCoroutine;
		m_waitYield.CreateNew(1f, useUnscaledTime: true);
		yield return m_waitYield;
		StartCoroutine(OnCloseCoroutine());
	}

	private void RevivePlayer(PlayerController player)
	{
		switch (DeathDefiedType)
		{
		case DeathDefiedType.Death_Dodge:
			player.SetHealth(1f, additive: false, runEvents: false);
			break;
		case DeathDefiedType.ExtraLife:
		{
			float num = (float)player.ActualMaxHealth * 0.5f;
			num += 0f * player.ActualMagic;
			player.SetHealth(num, additive: false, runEvents: false);
			break;
		}
		case DeathDefiedType.ExtraLife_Unity:
		{
			float value = (float)player.ActualMaxHealth * 0.5f;
			player.SetHealth(value, additive: false, runEvents: false);
			break;
		}
		}
		PlayerHUDController.IgnoreHealthChangeEvents = true;
		SaveManager.SaveCurrentProfileGameData(SaveDataType.Player, SavingType.FileOnly, ignoreTimeRestriction: true);
		PlayerHUDController.IgnoreHealthChangeEvents = false;
	}

	private IEnumerator OnCloseCoroutine()
	{
		PlayerController player = PlayerManager.GetPlayerController();
		player.SetVelocity(0f, 0f, additive: false);
		TweenManager.TweenTo_UnscaledTime(m_spotlightCanvasGroup, 0.5f, Ease.None, "alpha", 0);
		TweenManager.TweenTo_UnscaledTime(m_bannerCanvasGroup, 0.5f, Ease.None, "alpha", 0);
		yield return TweenManager.TweenTo_UnscaledTime(m_bgCanvasGroup, 1f, Ease.None, "alpha", 0).TweenCoroutine;
		player.Visuals.gameObject.SetLayerRecursively(0, includeInactive: true);
		player.GetComponentInChildren<CharacterSortController>().ResetCharacterLayers();
		player.Animator.updateMode = AnimatorUpdateMode.Normal;
		player.Animator.SetBool("Victory", value: false);
		player.Animator.SetBool("DeathDefied", value: false);
		player.CharacterHitResponse.AnimateHitEffectsOnUnscaledTime = false;
		player.BlinkPulseEffect.UseUnscaledTime = false;
		if (TraitManager.IsTraitActive(TraitType.UpsideDown))
		{
			(TraitManager.GetActiveTrait(TraitType.UpsideDown) as UpsideDown_Trait).ApplyDeathDefy(reset: true);
		}
		m_deathLoopEventEmitter.Stop();
		m_snapshotEventEmitter.Stop();
		WindowManager.SetWindowIsOpen(WindowID.DeathDefy, isOpen: false);
	}

	protected override void OnFocus()
	{
	}

	protected override void OnLostFocus()
	{
	}

	protected override void OnClose()
	{
		m_bgCanvasGroup.gameObject.SetActive(value: false);
		CameraController.CinemachineBrain.enabled = true;
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayPlayerHUD, null, m_hudArgs);
		m_windowCanvas.gameObject.SetActive(value: false);
		CameraController.SoloCam.gameObject.SetActive(value: false);
	}
}
