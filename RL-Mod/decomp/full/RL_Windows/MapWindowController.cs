using System;
using System.Collections;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RL_Windows;

public class MapWindowController : WindowController
{
	public static bool EnteredFromOtherSubmenu;

	[SerializeField]
	private Camera m_mapCamera;

	[SerializeField]
	private Ferr2DT_PathTerrain m_mapFerr2DObj;

	[SerializeField]
	private GameObject m_mapHorizontalDoor;

	[SerializeField]
	private GameObject m_mapVerticalDoor;

	[SerializeField]
	private GameObject m_mapEnemyIcon;

	[SerializeField]
	private GameObject m_legend;

	[Header("Animation Objects")]
	[SerializeField]
	private CanvasGroup m_shadowsCanvasGroup;

	[SerializeField]
	private CanvasGroup m_fadeBGCanvasGroup;

	[SerializeField]
	private CanvasGroup m_gridAndRTCanvasGroup;

	[SerializeField]
	private CanvasGroup m_legendCanvasGroup;

	[SerializeField]
	private RawImage m_uvGridImage;

	[SerializeField]
	private GameObject m_yellowStarsGO;

	private GameObject m_mapObj;

	private Action<InputActionEventData> m_onCancelButtonDown;

	private Action<InputActionEventData> m_moveMapHorizontal;

	private Action<InputActionEventData> m_moveMapVertical;

	private Action<InputActionEventData> m_onYButtonDown;

	private Action<InputActionEventData> m_onConfirmButtonDown;

	private Action<InputActionEventData> m_zoomMapVertical;

	public override WindowID ID => WindowID.Map;

	public Scene Scene => base.gameObject.scene;

	private bool IsMapMade { get; set; }

	private void Awake()
	{
		m_onCancelButtonDown = OnCancelButtonDown;
		m_moveMapHorizontal = MoveMapHorizontal;
		m_moveMapVertical = MoveMapVertical;
		m_onYButtonDown = OnYButtonDown;
		m_onConfirmButtonDown = OnConfirmButtonDown;
		m_zoomMapVertical = ZoomMapVertical;
	}

	protected override void OnOpen()
	{
		m_windowCanvas.gameObject.SetActive(value: true);
		MapController.MapWindowCamera.orthographicSize = 9f;
		MapController.MapWindowCamera.aspect = CameraController.GameCamera.aspect;
		MapController.MapWindowCamera.rect = CameraController.GameCamera.rect;
		MapController.MapWindowCamera.gameObject.SetActive(value: true);
		Rect uvRect = m_uvGridImage.uvRect;
		uvRect = new Rect(0f, 0f, 8.170213f, 8.170213f);
		m_uvGridImage.uvRect = uvRect;
		MapController.UpdatePlayerIconPosition();
		if (!TraitManager.IsTraitActive(TraitType.MapReveal))
		{
			MapController.CentreCameraAroundPlayerIcon();
		}
		else
		{
			MapController.SetMapCameraPosition(MapController.GetBiomeRect(PlayerManager.GetCurrentPlayerRoom().BiomeType).center);
		}
		MapController.SetCameraFollowIsOn(isOn: false);
		StartCoroutine(RunEnterAnimation());
	}

	protected override void OnClose()
	{
		EnteredFromOtherSubmenu = false;
		m_windowCanvas.gameObject.SetActive(value: false);
		MapController.SetCameraFollowIsOn(isOn: true);
		MapController.MapWindowCamera.orthographicSize = 2f;
		MapController.MapWindowCamera.gameObject.SetActive(value: false);
		UnsubscribeFromRewiredInputEvents();
	}

	protected IEnumerator RunEnterAnimation()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		float duration = 0.15f;
		if (!EnteredFromOtherSubmenu)
		{
			m_fadeBGCanvasGroup.alpha = 0f;
			TweenManager.TweenTo_UnscaledTime(m_fadeBGCanvasGroup, duration, Ease.Quad.EaseOut, "alpha", 0.66667f);
			m_shadowsCanvasGroup.transform.localScale = new Vector3(2f, 2f, 1f);
			m_shadowsCanvasGroup.alpha = 0f;
			TweenManager.TweenTo_UnscaledTime(m_shadowsCanvasGroup.transform, duration, Ease.Quad.EaseOut, "localScale.x", 1, "localScale.y", 1);
			TweenManager.TweenTo_UnscaledTime(m_shadowsCanvasGroup, duration, Ease.Quad.EaseOut, "alpha", 1);
		}
		else
		{
			m_fadeBGCanvasGroup.alpha = 1f;
			m_shadowsCanvasGroup.alpha = 1f;
			m_shadowsCanvasGroup.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		m_legendCanvasGroup.alpha = 0f;
		TweenManager.TweenTo_UnscaledTime(m_legendCanvasGroup, 0.15f, Ease.Quad.EaseOut, "alpha", 1);
		m_gridAndRTCanvasGroup.transform.localScale = new Vector3(2f, 2f, 1f);
		m_gridAndRTCanvasGroup.alpha = 0f;
		TweenManager.TweenTo_UnscaledTime(m_gridAndRTCanvasGroup.transform, duration, Ease.Quad.EaseOut, "localScale.x", 1, "localScale.y", 1);
		yield return TweenManager.TweenTo_UnscaledTime(m_gridAndRTCanvasGroup, duration, Ease.Quad.EaseOut, "alpha", 1).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	protected override void OnFocus()
	{
		SubscribeToRewiredInputEvents();
	}

	protected override void OnLostFocus()
	{
		UnsubscribeFromRewiredInputEvents();
	}

	protected override void OnPause()
	{
		throw new NotImplementedException();
	}

	protected override void OnUnpause()
	{
		throw new NotImplementedException();
	}

	protected virtual void OnCancelButtonDown(InputActionEventData obj)
	{
		if (WindowManager.GetIsWindowOpen(WindowID.Pause))
		{
			WindowManager.CloseAllOpenWindows();
		}
		else
		{
			WindowManager.SetWindowIsOpen(ID, isOpen: false);
		}
	}

	protected virtual void OnConfirmButtonDown(InputActionEventData obj)
	{
		StartCoroutine(TweenCameraToPlayer(0.25f));
	}

	protected virtual void OnYButtonDown(InputActionEventData obj)
	{
		ToggleLegend();
	}

	public override void Initialize()
	{
		base.Initialize();
	}

	protected virtual void SubscribeToRewiredInputEvents()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Select");
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.AddInputEventDelegate(m_moveMapHorizontal, UpdateLoopType.Update, InputActionEventType.AxisActive, "Window_Horizontal");
			base.RewiredPlayer.AddInputEventDelegate(m_moveMapVertical, UpdateLoopType.Update, InputActionEventType.AxisActive, "Window_Vertical");
			base.RewiredPlayer.AddInputEventDelegate(m_onYButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_R");
			base.RewiredPlayer.AddInputEventDelegate(m_onConfirmButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			base.RewiredPlayer.AddInputEventDelegate(m_zoomMapVertical, UpdateLoopType.Update, InputActionEventType.AxisActive, "Window_Vertical_RStick");
		}
	}

	protected virtual void UnsubscribeFromRewiredInputEvents()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Select");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.RemoveInputEventDelegate(m_moveMapHorizontal, UpdateLoopType.Update, InputActionEventType.AxisActive, "Window_Horizontal");
			base.RewiredPlayer.RemoveInputEventDelegate(m_moveMapVertical, UpdateLoopType.Update, InputActionEventType.AxisActive, "Window_Vertical");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onYButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_R");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			base.RewiredPlayer.RemoveInputEventDelegate(m_zoomMapVertical, UpdateLoopType.Update, InputActionEventType.AxisActive, "Window_Vertical_RStick");
		}
	}

	private void ToggleLegend()
	{
		m_legend.SetActive(!m_legend.activeSelf);
	}

	protected virtual void MoveMapHorizontal(InputActionEventData inputActionEventData)
	{
		float num = inputActionEventData.GetAxis();
		if (num == 0f)
		{
			num = 0f - inputActionEventData.GetAxisPrev();
		}
		Vector3 position = MapController.Camera.transform.position;
		float num2 = MapController.MapWindowCamera.orthographicSize / 9f;
		float num3 = num * num2 * 14f * Time.unscaledDeltaTime;
		position.x += num3;
		Rect uvRect = m_uvGridImage.uvRect;
		uvRect.x += num3 / (33f / uvRect.width);
		MapController.SetMapCameraPosition(position);
	}

	protected virtual void MoveMapVertical(InputActionEventData inputActionEventData)
	{
		float num = inputActionEventData.GetAxis();
		if (num == 0f)
		{
			num = 0f - inputActionEventData.GetAxisPrev();
		}
		Vector3 position = MapController.Camera.transform.position;
		float num2 = MapController.MapWindowCamera.orthographicSize / 9f;
		float num3 = num * num2 * 14f * Time.unscaledDeltaTime;
		position.y += num3;
		Rect uvRect = m_uvGridImage.uvRect;
		uvRect.y += num3 / (32f / uvRect.height);
		MapController.SetMapCameraPosition(position);
	}

	private void ZoomMapVertical(InputActionEventData eventData)
	{
		float num = 14f;
		if (eventData.IsCurrentInputSource(ControllerType.Mouse))
		{
			num *= 10f;
		}
		float num2 = 0f - eventData.GetAxis();
		float num3 = Mathf.Clamp(MapController.MapWindowCamera.orthographicSize + num2 * num * Time.unscaledDeltaTime, 6f, 24f);
		float num4 = MapController.MapWindowCamera.orthographicSize - num3;
		MapController.MapWindowCamera.orthographicSize = num3;
		Rect uvRect = m_uvGridImage.uvRect;
		uvRect.width -= num4;
		uvRect.x += num4 / 2f;
		uvRect.height -= num4;
		uvRect.y += num4 / 2f;
		m_uvGridImage.uvRect = uvRect;
	}

	private IEnumerator TweenCameraToPlayer(float duration)
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		Vector2 vector = MapController.PlayerIconPosition;
		if (TraitManager.IsTraitActive(TraitType.MapReveal))
		{
			vector = MapController.GetBiomeRect(PlayerManager.GetCurrentPlayerRoom().AppearanceBiomeType).center;
		}
		float num = vector.x - MapController.Camera.transform.position.x;
		num /= 5.4166665f;
		float num2 = vector.y - MapController.Camera.transform.position.y;
		num2 /= 5.4166665f;
		TweenManager.TweenBy_UnscaledTime(m_uvGridImage, duration, Ease.Quad.EaseInOut, "uvRect.x", num, "uvRect.y", num2);
		yield return TweenManager.TweenTo_UnscaledTime(MapController.Camera.transform, duration, Ease.Quad.EaseInOut, "position.x", vector.x, "position.y", vector.y).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}
}
