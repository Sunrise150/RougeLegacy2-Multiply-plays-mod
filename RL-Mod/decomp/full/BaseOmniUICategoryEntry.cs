using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BaseOmniUICategoryEntry : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
	[SerializeField]
	protected Image m_selectedSprite;

	[SerializeField]
	protected Image m_newSymbol;

	[SerializeField]
	protected Image m_upgradeSymbol;

	[SerializeField]
	protected Image m_iconSprite;

	private bool m_inputEnabled;

	private IOmniUIWindowController m_windowController;

	public IOmniUIWindowController WindowController => m_windowController;

	public bool IsInitialized { get; private set; }

	public int EntryIndex { get; private set; }

	public abstract void UpdateState();

	protected virtual void Initialize(int entryIndex, IOmniUIWindowController windowController)
	{
		EntryIndex = entryIndex;
		m_windowController = windowController;
		IsInitialized = true;
	}

	public virtual void OnSelect(BaseEventData eventData)
	{
		if (!m_selectedSprite.gameObject.activeSelf)
		{
			m_selectedSprite.gameObject.SetActive(value: true);
		}
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		if (m_selectedSprite.gameObject.activeSelf)
		{
			m_selectedSprite.gameObject.SetActive(value: false);
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			m_windowController.SetHighlightedCategory(this);
			m_windowController.SetSelectedCategoryIndex(m_windowController.HighlightedCategoryIndex, playSFX: true);
		}
	}
}
