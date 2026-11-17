using UnityEngine;

public class CurseRaycastTurret_Hazard : RaycastTurret_Hazard
{
	protected override bool AllTurretsFire => false;

	protected override float InitializationDelay => 1f;

	protected override float FireDelay => 0.5f;

	protected override float DelayBetweenShots => 8f;

	protected override float DetectedRange => 15f;

	protected override Vector2 FireOffset => new Vector2(0.5f, 0f);
}
