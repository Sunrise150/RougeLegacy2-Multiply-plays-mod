using System;
using UnityEngine;

namespace MoreMountains.CorgiEngine;

[Serializable]
[RequireComponent(typeof(Collider2D))]
public class CorgiControllerParameters
{
	[Header("Gravity")]
	public float Gravity = -30f;

	public float FallMultiplier = 1f;

	public float AscentMultiplier = 1f;

	[Header("Speed")]
	public Vector2 MaxVelocity = new Vector2(100f, 100f);

	public float SpeedAccelerationOnGround = 20f;

	public float SpeedAccelerationInAir = 5f;

	public float SpeedFactor = 1f;

	[Header("Slopes")]
	[Range(0f, 90f)]
	public float MaximumSlopeAngle = 30f;

	public AnimationCurve SlopeAngleSpeedFactor = new AnimationCurve(new Keyframe(-90f, 1f), new Keyframe(0f, 1f), new Keyframe(90f, 1f));

	[Header("Physics2D Interaction [Experimental]")]
	public bool Physics2DInteraction = true;

	public float Physics2DPushForce = 2f;

	[Header("Gizmos")]
	public bool DrawRaycastsGizmos = true;

	public bool DisplayWarnings = true;
}
