using RL_Windows;
using SceneManagement_RL;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
	public enum MainMenuSelectionType
	{
		None,
		Start,
		SelectProfile,
		Options,
		Credits,
		Quit,
		ViewNews
	}

	[SerializeField]
	private MainMenuSelectionType m_selectorType;

	[SerializeField]
	protected GameObject m_selectedIndicator;

	private MainMenuWindowController m_mainMenuWindow;

	public MainMenuButtonSelectedHandler MenuButtonSelected { get; set; }

	public MainMenuButtonSelectedHandler MenuButtonActivated { get; set; }

	public MainMenuSelectionType SelectorType => m_selectorType;

	public bool Interactable { get; set; }

	public int Index { get; private set; }

	public TMP_Text Text { get; private set; }

	private void Awake()
	{
		Text = GetComponentInChildren<TMP_Text>();
	}

	public void Initialize(MainMenuWindowController mainMenuWindow, int index)
	{
		m_selectedIndicator.SetActive(value: false);
		Index = index;
		m_mainMenuWindow = mainMenuWindow;
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			ExecuteButton();
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		OnSelect(eventData);
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		m_selectedIndicator.SetActive(value: true);
		if (MenuButtonSelected != null)
		{
			MenuButtonSelected(this);
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		m_selectedIndicator.SetActive(value: false);
	}

	public void ExecuteButton()
	{
		if (Interactable)
		{
			if (MenuButtonActivated != null)
			{
				MenuButtonActivated(this);
			}
			switch (m_selectorType)
			{
			case MainMenuSelectionType.Start:
				m_mainMenuWindow.LoadGame();
				break;
			case MainMenuSelectionType.SelectProfile:
				WindowManager.SetWindowIsOpen(WindowID.MainMenu, isOpen: false);
				WindowManager.SetWindowIsOpen(WindowID.ProfileSelect, isOpen: true);
				break;
			case MainMenuSelectionType.Options:
				WindowManager.SetWindowIsOpen(WindowID.MainMenu, isOpen: false);
				WindowManager.SetWindowIsOpen(WindowID.Options, isOpen: true);
				break;
			case MainMenuSelectionType.ViewNews:
				m_mainMenuWindow.SetEnablePatchNotes(enable: true);
				break;
			case MainMenuSelectionType.Credits:
				CreditsController.IsEndingScroll = false;
				SceneLoader_RL.LoadScene(SceneID.Credits, TransitionID.FadeToBlackNoLoading);
				break;
			case MainMenuSelectionType.Quit:
				Application.Quit(10);
				break;
			}
		}
	}
}
