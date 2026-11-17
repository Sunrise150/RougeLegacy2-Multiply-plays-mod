using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Utility Library")]
public class UtilityLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/UtilityLibrary";

	[SerializeField]
	private GameObject m_hbControllerBoxColliderGO;

	[SerializeField]
	private GameObject m_hbControllerCircleColliderGO;

	[SerializeField]
	private GameObject m_hbControllerCapsuleColliderGO;

	[SerializeField]
	private GameObject m_hbControllerPolygonColliderGO;

	private static UtilityLibrary m_instance;

	private static UtilityLibrary Instance
	{
		get
		{
			if (!m_instance)
			{
				m_instance = CDGResources.Load<UtilityLibrary>("Scriptable Objects/Libraries/UtilityLibrary");
			}
			return m_instance;
		}
	}

	public static GameObject GetHitControllerBoxColliderGO()
	{
		return Instance.m_hbControllerBoxColliderGO;
	}

	public static GameObject GetHitControllerCircleColliderGO()
	{
		return Instance.m_hbControllerCircleColliderGO;
	}

	public static GameObject GetHitControllerCapsuleColliderGO()
	{
		return Instance.m_hbControllerCapsuleColliderGO;
	}

	public static GameObject GetHitControllerPolygonColliderGO()
	{
		return Instance.m_hbControllerPolygonColliderGO;
	}
}
