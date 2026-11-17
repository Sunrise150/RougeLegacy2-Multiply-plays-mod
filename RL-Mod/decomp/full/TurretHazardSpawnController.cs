using System.Runtime.CompilerServices;
using UnityEngine;

public class TurretHazardSpawnController : LineHazardSpawnController, IHazardSpawnController, IComplexSpawnController, ISpawnController, IRoomConsumer, IIDConsumer, IStateConsumer, ISetSpawnType, IHasProjectileNameArray, IWidthConsumer, IMirror, IPivotConsumer, ITurretSettingsConsumer
{
	[SerializeField]
	private bool m_useHalfLoopDelay;

	[SerializeField]
	private TurretLogicType m_logicType;

	[SerializeField]
	private float m_initialFireDelay;

	[SerializeField]
	private float m_loopFireDelay = 1f;

	[SerializeField]
	private float m_projectileSpeedMod = 1f;

	public override HazardCategory Category => HazardCategory.Turret;

	public float InitialFireDelay => m_initialFireDelay;

	public TurretLogicType LogicType => m_logicType;

	public float LoopFireDelay => m_loopFireDelay;

	public float ProjectileSpeedMod => m_projectileSpeedMod;

	public override string SpriteSheetPath => "Assets/Content/Textures/Hazards/SharedHazards_Packed/TurretHazard_Editor_Texture.png";

	public bool UseHalfLoopDelay => m_useHalfLoopDelay;

	public void SetInitialFireDelay(float value)
	{
	}

	public void SetLogic(TurretLogicType value)
	{
	}

	public void SetLoopFireDelay(float value)
	{
	}

	public void SetProjectileSpeedMod(float value)
	{
	}

	public void SetUseHalfLoopDelay(bool value)
	{
	}

	protected override void Spawn()
	{
		if (base.Type != HazardType.None)
		{
			CreateHazard();
			if (m_hazardArgs == null)
			{
				m_hazardArgs = new TurretHazardArgs(base.InitialState, LogicType, UseHalfLoopDelay, InitialFireDelay, LoopFireDelay, ProjectileSpeedMod);
			}
			(base.Hazard as Multi_Hazard).Initialize(base.PivotPoint, base.Width, m_hazardArgs);
		}
	}

	[SpecialName]
	GameObject ISpawnController.get_gameObject()
	{
		return base.gameObject;
	}
}
