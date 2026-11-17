using System;
using UnityEngine;

[Serializable]
public class EffectTriggerEntry
{
	public EffectCategoryType EffectCategory;

	public string EffectTypeName = "";

	public EffectOffsetType OffsetType;

	public Vector2 PositionOffset;

	public bool DestroyOnRoomChange = true;

	public EffectTriggerDirection TriggerDirection = EffectTriggerDirection.Anywhere;

	public float LeftRotationZ;

	public bool LeftFlip;

	public float RightRotationZ;

	public bool RightFlip;

	public float UpRotationZ;

	public bool UpFlip;

	public float DownRotationZ;

	public bool DownFlip;

	public EffectTargetType DeriveFacing = EffectTargetType.Other;

	public EffectTargetType DeriveScale;

	public EffectTargetType DeriveRotation;

	public bool DeriveRotationFromHeading;

	public float BaseScaleToDerive = 1f;

	public EffectTargetType FollowTargetType;

	public EffectDurationType DurationType;

	[Range(0f, 1f)]
	public float StartingPosLerp;

	public float Duration;

	public int AnimatorLayer;

	public EffectStartType EffectStartType;

	public float NormalizedStartTime;

	public EffectStopType EffectStopType = EffectStopType.Gracefully;
}
