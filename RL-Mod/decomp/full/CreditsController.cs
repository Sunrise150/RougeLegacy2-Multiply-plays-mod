using System;
using System.Collections;
using RLAudio;
using Rewired;
using SceneManagement_RL;
using UnityEngine;

public class CreditsController : MonoBehaviour
{
	public static bool IsEndingScroll;

	private const float SCROLL_SPEED = 150f;

	[SerializeField]
	private RectTransform m_creditsPanel;

	[SerializeField]
	private GameObject m_finalLogo;

	[SerializeField]
	private GameObject m_centrePoint;

	[SerializeField]
	private GameObject m_fastForwardObj;

	[SerializeField]
	private Canvas m_canvas;

	[Header("Animations")]
	[SerializeField]
	private PlayerLookController m_leftPlayer;

	[SerializeField]
	private PlayerLookController m_rightPlayer;

	[SerializeField]
	private Animator m_pizzaGirl;

	[SerializeField]
	private CanvasGroup m_leftOverlay;

	[SerializeField]
	private CanvasGroup m_rightOverlay;

	private Action<InputActionEventData> m_toggleSpeedUp;

	private Action<InputActionEventData> m_loadNextScene;

	private Vector3 m_leftPlayerStartingPos;

	private Vector3 m_rightPlayerStartingPos;

	private CharacterData m_leftCharData;

	private CharacterData m_rightCharData;

	private bool m_scrollingComplete;

	private int[] m_animArray;

	private int m_animIndex;

	private void Awake()
	{
		m_toggleSpeedUp = ToggleSpeedUp;
		m_loadNextScene = LoadNextScene;
		m_leftCharData = new CharacterData();
		m_rightCharData = new CharacterData();
		m_rightPlayer.transform.SetLocalScaleX(Mathf.Abs(m_rightPlayer.transform.localScale.x) * -1f);
		m_pizzaGirl.gameObject.SetActive(value: false);
		LookCreator.DisableAllWeaponGeo(m_leftPlayer);
		LookCreator.DisableAllWeaponGeo(m_rightPlayer);
		m_animArray = new int[11];
		for (int i = 0; i < m_animArray.Length; i++)
		{
			m_animArray[i] = i;
		}
	}

	private void OnEnable()
	{
		AnimBehaviourEventEmitter.DisableEmitter_STATIC = true;
		m_canvas.worldCamera = CameraController.UICamera;
		m_fastForwardObj.SetActive(value: false);
		RewiredMapController.SetMap(GameInputMode.Window);
		Rewired_RL.Player.AddInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		Rewired_RL.Player.AddInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Confirm");
		Rewired_RL.Player.AddInputEventDelegate(m_loadNextScene, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		m_leftOverlay.alpha = 1f;
		m_rightOverlay.alpha = 1f;
		StartCoroutine(CreditRoll());
	}

	private void OnDisable()
	{
		AnimBehaviourEventEmitter.DisableEmitter_STATIC = false;
		IsEndingScroll = false;
		if (!GameManager.IsApplicationClosing)
		{
			Rewired_RL.Player.RemoveInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			Rewired_RL.Player.RemoveInputEventDelegate(m_toggleSpeedUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Window_Confirm");
			Rewired_RL.Player.RemoveInputEventDelegate(m_loadNextScene, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		}
	}

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(0.2f);
		MusicManager.PlayMusic(SongID.Credits_ASITP, isOverride: false);
		RewiredMapController.SetMap(GameInputMode.Window);
	}

	private void ToggleSpeedUp(InputActionEventData eventData)
	{
		if (!m_scrollingComplete)
		{
			if (!eventData.GetButtonUp())
			{
				RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 10f);
				m_fastForwardObj.SetActive(value: true);
			}
			else
			{
				RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
				m_fastForwardObj.SetActive(value: false);
			}
		}
	}

	private void LoadNextScene(InputActionEventData eventData)
	{
		if (m_scrollingComplete)
		{
			if (!IsEndingScroll)
			{
				WorldBuilder.FirstBiomeOverride = BiomeType.None;
				SceneLoader_RL.LoadScene(SceneID.MainMenu, TransitionID.FadeToBlackNoLoading);
			}
			else
			{
				SceneLoader_RL.LoadScene(SceneID.Lineage, TransitionID.FadeToBlackWithLoading);
			}
		}
	}

	private IEnumerator CreditRoll()
	{
		m_scrollingComplete = false;
		Vector3 pos = m_creditsPanel.transform.localPosition;
		while (m_finalLogo.transform.position.y < m_centrePoint.transform.position.y)
		{
			float num = Time.deltaTime * 150f;
			pos.y += num;
			m_creditsPanel.localPosition = pos;
			yield return null;
		}
		float endDelay = Time.time + 3f;
		while (Time.time < endDelay)
		{
			yield return null;
		}
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
		m_fastForwardObj.SetActive(value: false);
		m_scrollingComplete = true;
	}

	private void InitializeLook(ClassType classType, bool useLeft)
	{
		CharacterData characterData = (useLeft ? m_leftCharData : m_rightCharData);
		PlayerLookController lookObj = (useLeft ? m_leftPlayer : m_rightPlayer);
		CharacterCreator.GenerateClass(classType, characterData);
		LookCreator.InitializeClassLook(characterData.ClassType, lookObj);
		LookCreator.InitializeCharacterLook(characterData, lookObj, generateRandomLook: true);
		LookCreator.DisableAllWeaponGeo(lookObj);
	}

	private IEnumerator PlayAnimation(Animator animator, int layerIndex, params string[] states)
	{
		foreach (string stateName in states)
		{
			animator.Play(stateName, layerIndex);
			yield return null;
			while (animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime < 1f)
			{
				yield return null;
			}
		}
	}

	private IEnumerator StartPlayerAnimations()
	{
		CDGHelper.Shuffle(m_animArray);
		m_animIndex = 0;
		float delay4 = UnityEngine.Random.Range(8f, 12f);
		delay4 += Time.time;
		while (Time.time < delay4)
		{
			yield return null;
		}
		m_leftPlayerStartingPos = m_leftPlayer.transform.position;
		m_rightPlayerStartingPos = m_rightPlayer.transform.position;
		while (m_finalLogo.transform.position.y < m_centrePoint.transform.position.y)
		{
			switch (m_animArray[m_animIndex])
			{
			case 0:
				yield return BowAnimation();
				break;
			case 1:
				yield return TwirlAnimation();
				break;
			case 2:
				yield return TwirlUpAnimation();
				break;
			case 3:
				yield return FireballAnimation();
				break;
			case 4:
				yield return RandomDeathAnimation();
				break;
			case 5:
				yield return RetireAnimation();
				break;
			case 6:
				yield return LookDistanceAnimation();
				break;
			case 7:
				yield return KickDownAnimation();
				break;
			case 8:
				yield return BowFartAnimation();
				break;
			case 9:
				yield return BoxingAnimation();
				break;
			case 10:
				yield return EatPizza();
				break;
			}
			m_animIndex++;
			delay4 = UnityEngine.Random.Range(5, 8);
			delay4 += Time.time;
			while (Time.time < delay4)
			{
				yield return null;
			}
		}
	}

	private IEnumerator BowAnimation()
	{
		InitializeLook(ClassType.SwordClass, useLeft: true);
		m_rightPlayer.transform.position = m_rightPlayerStartingPos;
		m_leftPlayer.transform.position = m_leftPlayerStartingPos;
		m_leftPlayer.Animator.Play("Idle");
		m_leftOverlay.alpha = 1f;
		m_rightOverlay.alpha = 1f;
		yield return TweenManager.TweenTo(m_leftOverlay, 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
		float delay2 = Time.time + 0.5f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		m_leftPlayer.Animator.Play("Bow");
		delay2 = Time.time + 2f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo(m_leftOverlay, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
	}

	private IEnumerator TwirlAnimation()
	{
		InitializeLook(ClassType.LuteClass, useLeft: false);
		m_leftPlayer.transform.position = new Vector3(-10000f, 0f, m_leftPlayerStartingPos.z);
		m_rightPlayer.transform.position = m_rightPlayerStartingPos;
		m_leftOverlay.alpha = 0f;
		m_rightOverlay.alpha = 1f;
		m_rightPlayer.Animator.SetBool("Bounce", value: true);
		m_rightPlayer.Animator.Play("Bounce");
		m_rightPlayer.Animator.speed = 0.5f;
		TweenManager.TweenBy(m_rightPlayer.transform, 3f, Ease.None, "position.x", -25);
		TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 0);
		float delay = Time.time + 2f;
		while (Time.time < delay)
		{
			yield return null;
		}
		TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 1);
		yield return TweenManager.TweenTo(m_leftOverlay, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
		m_rightPlayer.Animator.SetBool("Bounce", value: false);
		m_rightPlayer.Animator.speed = 1f;
	}

	private IEnumerator TwirlUpAnimation()
	{
		InitializeLook(ClassType.LuteClass, useLeft: false);
		m_leftPlayer.transform.position = new Vector3(-10000f, 0f, m_leftPlayerStartingPos.z);
		m_rightPlayer.transform.position = m_rightPlayerStartingPos;
		m_rightPlayer.transform.SetPositionY(m_rightPlayerStartingPos.y - 10f);
		m_leftOverlay.alpha = 0f;
		m_rightOverlay.alpha = 1f;
		m_rightPlayer.Animator.SetBool("Bounce", value: true);
		m_rightPlayer.Animator.Play("Bounce");
		m_rightPlayer.Animator.speed = 0.5f;
		TweenManager.TweenBy(m_rightPlayer.transform, 3f, Ease.None, "position.y", 25);
		TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 0);
		float delay = Time.time + 2f;
		while (Time.time < delay)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
		m_rightPlayer.Animator.SetBool("Bounce", value: false);
		m_rightPlayer.Animator.speed = 1f;
	}

	private IEnumerator FireballAnimation()
	{
		m_leftPlayer.transform.position = m_leftPlayerStartingPos;
		m_rightPlayer.transform.position = m_rightPlayerStartingPos;
		m_leftOverlay.alpha = 1f;
		m_rightOverlay.alpha = 1f;
		yield return TweenManager.TweenTo(m_leftOverlay, 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
		float delay3 = Time.time + 0.5f;
		while (Time.time < delay3)
		{
			yield return null;
		}
		m_leftPlayer.Animator.Play("SpellCast_Tell_Intro");
		yield return null;
		while (m_leftPlayer.Animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1f)
		{
			yield return null;
		}
		m_leftPlayer.Animator.Play("SpellCast_Tell_Hold");
		delay3 = Time.time + 1f;
		while (Time.time < delay3)
		{
			yield return null;
		}
		m_leftPlayer.Animator.Play("SpellCast_Attack_Intro");
		yield return null;
		while (m_leftPlayer.Animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1f)
		{
			yield return null;
		}
		m_leftPlayer.Animator.Play("SpellCast_Attack_Hold");
		yield return null;
		while (m_leftPlayer.Animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1f)
		{
			yield return null;
		}
		yield return null;
		m_leftPlayer.Animator.speed = 0f;
		delay3 = Time.time + 1f;
		while (Time.time < delay3)
		{
			yield return null;
		}
		Vector3 position = m_leftPlayer.transform.position;
		position.x += 1.5f;
		position.y += 1f;
		EffectManager.PlayEffect(m_leftPlayer.gameObject, null, "Fart_Effect", position, 0f, EffectStopType.Gracefully).gameObject.SetLayerRecursively(5);
		delay3 = Time.time + 2f;
		while (Time.time < delay3)
		{
			yield return null;
		}
		m_leftPlayer.Animator.speed = 1f;
		m_leftPlayer.Animator.Play("Empty", 1);
		m_leftPlayer.Animator.Play("Death_6");
		delay3 = Time.time + 2.5f;
		while (Time.time < delay3)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo(m_leftOverlay, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
	}

	private IEnumerator RandomDeathAnimation()
	{
		InitializeLook(ClassType.AxeClass, useLeft: false);
		m_leftPlayer.transform.position = m_leftPlayerStartingPos;
		m_rightPlayer.transform.position = m_rightPlayerStartingPos;
		m_leftOverlay.alpha = 1f;
		m_rightOverlay.alpha = 1f;
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
		float delay2 = Time.time + 1f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		float num = UnityEngine.Random.Range(0, 12);
		m_rightPlayer.Animator.Play("Death_" + num);
		delay2 = Time.time + 3f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
	}

	private IEnumerator RetireAnimation()
	{
		InitializeLook(ClassType.AxeClass, useLeft: false);
		m_leftPlayer.transform.position = m_leftPlayerStartingPos;
		m_rightPlayer.transform.position = m_rightPlayerStartingPos;
		m_leftOverlay.alpha = 1f;
		m_rightOverlay.alpha = 1f;
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
		float delay2 = Time.time + 1f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		m_rightPlayer.Animator.SetBool("Retire", value: true);
		m_rightPlayer.Animator.Play("Player_Retire");
		delay2 = Time.time + 3f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
		m_rightPlayer.Animator.SetBool("Retire", value: false);
	}

	private IEnumerator LookDistanceAnimation()
	{
		m_leftPlayer.transform.position = m_leftPlayerStartingPos;
		m_rightPlayer.transform.position = m_rightPlayerStartingPos;
		m_leftOverlay.alpha = 1f;
		m_rightOverlay.alpha = 1f;
		yield return TweenManager.TweenTo(m_leftOverlay, 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
		float delay2 = Time.time + 1f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		m_leftPlayer.Animator.SetBool("LookDistance", value: true);
		m_leftPlayer.Animator.Play("Player_LookDistance_Intro");
		delay2 = Time.time + 3f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo(m_leftOverlay, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
		m_leftPlayer.Animator.SetBool("LookDistance", value: false);
	}

	private IEnumerator KickDownAnimation()
	{
		m_rightPlayer.transform.position = new Vector3(-10000f, 0f, m_rightPlayerStartingPos.z);
		m_leftPlayer.transform.position = m_leftPlayerStartingPos;
		m_leftPlayer.transform.SetPositionY(m_leftPlayerStartingPos.y + 20f);
		m_leftOverlay.alpha = 1f;
		m_rightOverlay.alpha = 1f;
		m_leftPlayer.Animator.SetBool("Kick_Down", value: true);
		m_leftPlayer.Animator.Play("Kick_Down");
		TweenManager.TweenBy(m_leftPlayer.transform, 3f, Ease.None, "position.y", -40);
		TweenManager.TweenTo(m_leftOverlay, 0.5f, Ease.None, "alpha", 0);
		float delay = Time.time + 2f;
		while (Time.time < delay)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
		m_leftPlayer.Animator.SetBool("Kick_Down", value: false);
	}

	private IEnumerator BowFartAnimation()
	{
		InitializeLook(ClassType.SwordClass, useLeft: true);
		m_rightPlayer.transform.position = m_rightPlayerStartingPos;
		m_leftPlayer.transform.position = m_leftPlayerStartingPos;
		m_rightPlayer.Animator.Play("Idle");
		m_leftOverlay.alpha = 1f;
		m_rightOverlay.alpha = 1f;
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
		float delay2 = Time.time + 0.5f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		m_rightPlayer.Animator.Play("Bow");
		delay2 = Time.time + 0.8f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		Vector3 position = m_rightPlayer.transform.position;
		position.x += 1f;
		position.y += 0.5f;
		BaseEffect baseEffect = EffectManager.PlayEffect(m_rightPlayer.gameObject, null, "Fart_Effect", position, 0f, EffectStopType.Gracefully);
		baseEffect.gameObject.SetLayerRecursively(5);
		baseEffect.transform.SetLocalScaleX(baseEffect.transform.localScale.x * -1f);
		TweenManager.TweenBy(m_rightPlayer.transform, 0.5f, Ease.Quint.EaseOut, "position.x", -1);
		delay2 = Time.time + 1.2f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
	}

	private IEnumerator BoxingAnimation()
	{
		m_leftPlayer.transform.position = m_leftPlayerStartingPos;
		m_rightPlayer.transform.position = m_rightPlayerStartingPos;
		m_leftOverlay.alpha = 1f;
		m_rightOverlay.alpha = 1f;
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
		float delay2 = Time.time + 0.5f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		m_rightPlayer.Animator.speed = 2f;
		yield return PlayAnimation(m_rightPlayer.Animator, 1, "Punch1_Tell_Intro", "Punch1_Tell_Hold", "Punch1_Attack_Intro", "Punch1_Attack_Hold", "Punch1_Exit");
		yield return PlayAnimation(m_rightPlayer.Animator, 1, "Punch1_Tell_Intro", "Punch1_Tell_Hold", "Punch1_Attack_Intro", "Punch1_Attack_Hold", "Punch1_Exit");
		yield return PlayAnimation(m_rightPlayer.Animator, 1, "Punch2_Tell_Intro", "Punch2_Tell_Hold", "Punch2_Attack_Intro", "Punch2_Attack_Hold", "Punch2_Exit");
		yield return PlayAnimation(m_rightPlayer.Animator, 1, "Punch3_Tell_Intro", "Punch3_Tell_Hold", "Punch3_Attack_Intro", "Punch3_Attack_Hold", "Punch3_Exit");
		yield return PlayAnimation(m_rightPlayer.Animator, 1, "Uppercut_Tell_Intro", "Uppercut_Tell_Hold", "Uppercut_Attack_Intro", "Uppercut_Attack_Hold", "Uppercut_Exit");
		m_rightPlayer.Animator.speed = 0f;
		delay2 = Time.time + 1.5f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
		m_rightPlayer.Animator.speed = 1f;
		m_rightPlayer.Animator.Play("Empty", 1);
	}

	private IEnumerator EatPizza()
	{
		m_rightPlayer.gameObject.SetActive(value: false);
		m_leftPlayer.gameObject.SetActive(value: false);
		m_pizzaGirl.gameObject.SetActive(value: true);
		m_leftOverlay.alpha = 1f;
		m_rightOverlay.alpha = 1f;
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 0).TweenCoroutine;
		float delay2 = Time.time + 1f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		m_pizzaGirl.Play("EatPizza");
		delay2 = Time.time + 4f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo(m_rightOverlay, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
		m_rightPlayer.gameObject.SetActive(value: true);
		m_leftPlayer.gameObject.SetActive(value: true);
		m_pizzaGirl.gameObject.SetActive(value: false);
	}
}
