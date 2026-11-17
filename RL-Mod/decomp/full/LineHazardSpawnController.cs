using System.Runtime.CompilerServices;
using UnityEngine;

public class LineHazardSpawnController : HazardSpawnControllerBase, IHazardSpawnController, IComplexSpawnController, ISpawnController, IRoomConsumer, IIDConsumer, IStateConsumer, ISetSpawnType, IHasProjectileNameArray, IWidthConsumer, IMirror, IPivotConsumer
{
	[SerializeField]
	[ReadOnly]
	private int m_width = 1;

	[SerializeField]
	[ReadOnly]
	private PivotPoint m_pivot;

	public const int MIN_WIDTH = 1;

	public const int MAX_WIDTH = 32;

	public override HazardCategory Category => HazardCategory.Line;

	public int Width
	{
		get
		{
			return m_width;
		}
		private set
		{
			m_width = value;
		}
	}

	public PivotPoint PivotPoint
	{
		get
		{
			return m_pivot;
		}
		private set
		{
			m_pivot = value;
		}
	}

	public virtual string SpriteSheetPath => "Assets/Content/Textures/Hazards/LineHazard_Editor_Texture.png";

	public HazardType GetLineHazardType(HazardType hazardType)
	{
		switch (hazardType)
		{
		case HazardType.SpikeTrap:
			hazardType = HazardType.Multi_SpikeTrap;
			break;
		case HazardType.WallTurret_Standard:
			hazardType = HazardType.Multi_StandardWallTurret;
			break;
		case HazardType.WallTurret_FlameThrower:
			hazardType = HazardType.Multi_FlameThrowerWallTurret;
			break;
		case HazardType.BreakableSpike:
			hazardType = HazardType.Multi_BreakableSpike;
			break;
		case HazardType.PressurePlate:
			hazardType = HazardType.PressurePlate_Trigger;
			break;
		case HazardType.RaycastTurret_Arrow:
			hazardType = HazardType.Multi_RaycastTurret_Arrow;
			break;
		case HazardType.SpringTrap:
			hazardType = HazardType.Multi_SpringTrap;
			break;
		case HazardType.Bodies:
			hazardType = HazardType.Multi_Bodies;
			break;
		case HazardType.BreakableSpikeTall:
			hazardType = HazardType.Multi_BreakableSpikeTall;
			break;
		case HazardType.RaycastTurret_Curse:
			hazardType = HazardType.Multi_RaycastTurret_Curse;
			break;
		}
		return hazardType;
	}

	protected override IHazard GetHazard(HazardType hazardType)
	{
		hazardType = GetLineHazardType(hazardType);
		return HazardManager.GetHazard(hazardType);
	}

	public void Mirror()
	{
		if (PivotPoint == PivotPoint.Left)
		{
			PivotPoint = PivotPoint.Right;
		}
		else if (PivotPoint == PivotPoint.Right)
		{
			PivotPoint = PivotPoint.Left;
		}
		base.transform.rotation = Quaternion.Euler(0f, 0f, -1f * base.transform.rotation.eulerAngles.z);
	}

	protected override void Reset()
	{
		if (base.Hazard != null && base.Hazard.gameObject != null)
		{
			Multi_Hazard multi_Hazard = base.Hazard as Multi_Hazard;
			if (multi_Hazard != null)
			{
				multi_Hazard.Reset();
			}
			base.Hazard.gameObject.SetActive(value: false);
		}
		base.Hazard = null;
	}

	public void SetPivot(PivotPoint pivotPoint)
	{
		PivotPoint = pivotPoint;
	}

	public void SetWidth(int width)
	{
	}

	protected override void Spawn()
	{
		if (base.Type == HazardType.None)
		{
			return;
		}
		CreateHazard();
		if (base.Hazard != null)
		{
			if (m_hazardArgs == null)
			{
				m_hazardArgs = new HazardArgs(base.InitialState);
			}
			Multi_Hazard multi_Hazard = base.Hazard as Multi_Hazard;
			if (multi_Hazard != null)
			{
				multi_Hazard.Initialize(PivotPoint, Width, m_hazardArgs);
				return;
			}
			Debug.LogFormat("<color=red>| {0} | Hazard ({1}) is <b>NOT</b> a <b>Multi Hazard</b>, but should be. Please add a bug report to Pivotal</color>", this, base.Hazard.gameObject.name);
		}
		else if (base.Room != null)
		{
			Debug.LogFormat("<color=red>| {0} | Hazard is null in Room <b>({1})</b>, but should not be. Please add a bug report to Pivotal</color>", this, base.Room.gameObject.name);
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Hazard is null, but should not be. Please add a bug report to Pivotal</color>", this);
		}
	}

	[SpecialName]
	GameObject ISpawnController.get_gameObject()
	{
		return base.gameObject;
	}
}
