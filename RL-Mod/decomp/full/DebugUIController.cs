using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUIController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_panel;

	[SerializeField]
	private Toggle m_weaponHitBox;

	[SerializeField]
	private Toggle m_platformHitBox;

	[SerializeField]
	private Toggle m_bodyHitBox;

	[SerializeField]
	private Toggle m_hazardHitBox;

	[SerializeField]
	private Toggle m_worldInfo;

	[SerializeField]
	private Toggle m_roomInfo;

	[SerializeField]
	private Toggle m_dwarf;

	[SerializeField]
	private Toggle m_enlarge;

	[SerializeField]
	private Toggle m_invincible;

	[SerializeField]
	private TMP_Dropdown m_traitOneDropdown;

	[SerializeField]
	private TMP_Dropdown m_traitTwoDropdown;
}
