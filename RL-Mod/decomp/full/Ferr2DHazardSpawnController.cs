using UnityEngine;

public class Ferr2DHazardSpawnController : HazardSpawnControllerBase
{
	private Ferr2DT_PathTerrain m_ferr2D;

	public override HazardCategory Category => HazardCategory.Ferr2D;

	protected override void Awake()
	{
		base.Awake();
		m_ferr2D = base.gameObject.GetComponent<Ferr2DT_PathTerrain>();
		base.gameObject.GetComponent<MeshRenderer>().enabled = false;
	}

	protected override IHazard GetHazard(HazardType hazardType)
	{
		return base.GetHazard(hazardType);
	}

	protected override void Reset()
	{
	}

	protected override void Spawn()
	{
		if (base.Type == HazardType.None)
		{
			return;
		}
		if (!base.Hazard.IsNativeNull())
		{
			if (base.Hazard is IFerr2DHazard && !base.Hazard.gameObject.activeSelf)
			{
				base.Hazard.gameObject.SetActive(value: true);
			}
			return;
		}
		if (!m_ferr2D)
		{
			m_ferr2D = base.gameObject.GetComponent<Ferr2DT_PathTerrain>();
		}
		CreateHazard();
		if (base.Hazard != null && base.Hazard is IFerr2DHazard)
		{
			IFerr2DHazard obj = base.Hazard as IFerr2DHazard;
			obj.gameObject.transform.SetParent(base.transform.parent);
			Ferr2DHazardArgs hazardArgs = new Ferr2DHazardArgs(StateID.One, base.transform.localPosition, m_ferr2D.PathData.GetPoints(0));
			obj.Initialize(hazardArgs);
		}
	}
}
