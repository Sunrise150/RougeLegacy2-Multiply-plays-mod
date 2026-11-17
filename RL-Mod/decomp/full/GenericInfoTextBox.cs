using System;
using TMPro;
using UnityEngine;

public class GenericInfoTextBox : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_headerText;

	[SerializeField]
	private TMP_Text m_subHeaderText;

	[SerializeField]
	private TMP_Text m_descriptionText;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	private float m_storedCanvasAlpha;

	private Action<MonoBehaviour, EventArgs> m_onPauseChange;

	public CanvasGroup CanvasGroup => m_canvasGroup;

	public TMP_Text HeaderText => m_headerText;

	public TMP_Text SubHeaderText => m_subHeaderText;

	public TMP_Text DescriptionText => m_descriptionText;

	public bool HideOnPause { get; set; }

	private void Awake()
	{
		m_onPauseChange = OnPauseChange;
	}

	private void OnEnable()
	{
		if (HideOnPause)
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.GamePauseStateChange, m_onPauseChange);
		}
	}

	private void OnDisable()
	{
		if (HideOnPause)
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.GamePauseStateChange, m_onPauseChange);
		}
	}

	private void OnPauseChange(object sender, EventArgs args)
	{
		if ((args as GamePauseStateChangeEventArgs).IsPaused)
		{
			if ((bool)CanvasGroup)
			{
				m_storedCanvasAlpha = CanvasGroup.alpha;
				CanvasGroup.alpha = 0f;
				TweenManager.SetPauseAllTweensContaining(CanvasGroup, isPaused: true);
			}
		}
		else if ((bool)CanvasGroup)
		{
			CanvasGroup.alpha = m_storedCanvasAlpha;
			TweenManager.SetPauseAllTweensContaining(CanvasGroup, isPaused: false);
		}
	}
}
