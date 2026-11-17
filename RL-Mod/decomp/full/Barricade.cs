using UnityEngine;

public class Barricade : MonoBehaviour
{
	private Ferr2DT_PathTerrain m_ferr2D;

	public Ferr2DT_PathTerrain Ferr2D
	{
		get
		{
			if (m_ferr2D == null)
			{
				m_ferr2D = GetComponent<Ferr2DT_PathTerrain>();
			}
			return m_ferr2D;
		}
	}

	public GameObject GameObject => base.gameObject;
}
