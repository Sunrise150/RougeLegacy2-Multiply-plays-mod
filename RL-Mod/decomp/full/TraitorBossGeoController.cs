using UnityEngine;

public class TraitorBossGeoController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_armillarySphere;

	[SerializeField]
	private GameObject m_arrow;

	[SerializeField]
	private GameObject m_axe;

	[SerializeField]
	private GameObject m_bow;

	[SerializeField]
	private GameObject m_dualBladesL;

	[SerializeField]
	private GameObject m_dualBladesR;

	[SerializeField]
	private GameObject m_katana;

	[SerializeField]
	private GameObject m_ladle;

	[SerializeField]
	private GameObject m_pizza;

	[SerializeField]
	private GameObject m_scythe;

	[SerializeField]
	private GameObject m_staff;

	[SerializeField]
	private GameObject m_sword;

	private GameObject[] m_allGeo;

	public GameObject ArmillarySphere => m_armillarySphere;

	public GameObject Arrow => m_arrow;

	public GameObject Axe => m_axe;

	public GameObject Bow => m_bow;

	public GameObject DualBladesL => m_dualBladesL;

	public GameObject DualBladesR => m_dualBladesR;

	public GameObject Katana => m_katana;

	public GameObject Ladle => m_ladle;

	public GameObject Pizza => m_pizza;

	public GameObject Scythe => m_scythe;

	public GameObject Staff => m_staff;

	public GameObject Sword => m_sword;

	public GameObject[] AllWeaponGeo
	{
		get
		{
			if (m_allGeo == null)
			{
				m_allGeo = new GameObject[12]
				{
					ArmillarySphere, Arrow, Axe, Bow, DualBladesL, DualBladesR, Katana, Ladle, Pizza, Scythe,
					Staff, Sword
				};
			}
			return m_allGeo;
		}
	}
}
