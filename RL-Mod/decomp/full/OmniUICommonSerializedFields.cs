using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OmniUICommonSerializedFields : MonoBehaviour
{
	[Header("LetterBox")]
	[SerializeField]
	private CanvasGroup m_letterBoxGroup;

	[Header("Entries")]
	[SerializeField]
	protected CanvasGroup m_menuCanvasGroup;

	[SerializeField]
	private VerticalLayoutGroup m_categoryEntryLayoutGroup;

	[SerializeField]
	private VerticalLayoutGroup m_entryLayoutGroup;

	[SerializeField]
	private GameObject m_selectedCategoryIndicator;

	[SerializeField]
	private TMP_Text m_chooseCategoryText;

	[SerializeField]
	private CanvasGroup m_bgCanvasGroup;

	[Header("Models")]
	[SerializeField]
	private PlayerLookController m_playerModel;

	[SerializeField]
	private GameObject m_playerSpawnPosObj;

	[SerializeField]
	private GameObject m_NPCModel;

	[SerializeField]
	private GameObject m_npcSpawnPosObj;

	[SerializeField]
	private NPCController m_npcController;

	[Header("Description Boxes")]
	[SerializeField]
	protected CanvasGroup m_descriptionBox;

	[SerializeField]
	protected CanvasGroup m_descriptionBoxRaycastBlocker;

	[SerializeField]
	private CanvasGroup m_purchaseBox;

	[SerializeField]
	private GameObject m_resetTextbox;

	[Header("Scrollbar")]
	[SerializeField]
	private Scrollbar m_scrollBar;

	[SerializeField]
	private ScrollRect m_scrollRect;

	[SerializeField]
	private RectTransform m_contentViewport;

	[SerializeField]
	private GameObject m_topScrollArrow;

	[SerializeField]
	private Image m_topScrollNewSymbol;

	[SerializeField]
	private Image m_topScrollUpgradeSymbol;

	[SerializeField]
	private GameObject m_bottomScrollArrow;

	[SerializeField]
	private Image m_bottomScrollNewSymbol;

	[SerializeField]
	private Image m_bottomScrollUpgradeSymbol;

	[SerializeField]
	private GameObject m_warningMessageBox;

	public CanvasGroup LetterBoxGroup => m_letterBoxGroup;

	public CanvasGroup MenuCanvasGroup => m_menuCanvasGroup;

	public VerticalLayoutGroup CategoryEntryLayoutGroup => m_categoryEntryLayoutGroup;

	public VerticalLayoutGroup EntryLayoutGroup => m_entryLayoutGroup;

	public GameObject SelectedCategoryIndicator => m_selectedCategoryIndicator;

	public TMP_Text ChooseCategoryText => m_chooseCategoryText;

	public CanvasGroup BackgroundCanvasGroup => m_bgCanvasGroup;

	public PlayerLookController PlayerModel => m_playerModel;

	public GameObject PlayerSpawnPosObj => m_playerSpawnPosObj;

	public GameObject NPCModel => m_NPCModel;

	public GameObject NPCSpawnPosObj => m_npcSpawnPosObj;

	public NPCController NPCController => m_npcController;

	public CanvasGroup DescriptionBox => m_descriptionBox;

	public CanvasGroup DescriptionBoxRaycastBlocker => m_descriptionBoxRaycastBlocker;

	public CanvasGroup PurchaseBox => m_purchaseBox;

	public GameObject ResetTextbox => m_resetTextbox;

	public Scrollbar ScrollBar => m_scrollBar;

	public ScrollRect ScrollRect => m_scrollRect;

	public RectTransform ContentViewport => m_contentViewport;

	public GameObject TopScrollArrow => m_topScrollArrow;

	public Image TopScrollNewSymbol => m_topScrollNewSymbol;

	public Image TopScrollUpgradeSymbol => m_topScrollUpgradeSymbol;

	public GameObject BottomScrollArrow => m_bottomScrollArrow;

	public Image BottomScrollNewSymbol => m_bottomScrollNewSymbol;

	public Image BottomScrollUpgradeSymbol => m_bottomScrollUpgradeSymbol;

	public GameObject WarningMessageBox => m_warningMessageBox;
}
