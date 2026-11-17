using UnityEngine;

public static class EffectTrigger
{
	public static BaseEffect InvokeTrigger(EffectTriggerEntry entry, GameObject selfObj, GameObject otherObj, Vector3 selfObjMidpos, Vector3 otherObjMidpos, EffectTriggerDirection otherDirection, Animator animator = null)
	{
		if (!EffectManager.IsInitialized)
		{
			return null;
		}
		Vector3 effectPosition = entry.PositionOffset;
		float num = 1f;
		switch (entry.DeriveScale)
		{
		case EffectTargetType.Self:
			num = Mathf.Abs(selfObj.transform.lossyScale.x) / entry.BaseScaleToDerive;
			break;
		case EffectTargetType.Other:
			num = Mathf.Abs(otherObj.transform.lossyScale.x) / entry.BaseScaleToDerive;
			break;
		}
		effectPosition *= num;
		bool flag = false;
		float duration = ((entry.DurationType == EffectDurationType.CustomDuration) ? entry.Duration : ((float)entry.DurationType));
		BaseEffect baseEffect = null;
		switch (entry.FollowTargetType)
		{
		case EffectTargetType.Other:
		{
			IPivotObj component4 = otherObj.GetComponent<IPivotObj>();
			GameObject gameObject2 = ((component4 != null) ? component4.Pivot : otherObj);
			if (entry.OffsetType == EffectOffsetType.Midpoint)
			{
				gameObject2 = otherObj;
			}
			if (gameObject2.transform.lossyScale.x < 0f)
			{
				effectPosition.x = 0f - effectPosition.x;
			}
			if (entry.OffsetType == EffectOffsetType.Midpoint)
			{
				effectPosition += otherObjMidpos;
			}
			else
			{
				effectPosition += gameObject2.transform.position;
			}
			effectPosition.z = gameObject2.transform.position.z;
			if (!animator)
			{
				animator = otherObj.GetComponentInChildren<Animator>();
			}
			baseEffect = EffectManager.PlayEffect(gameObject2, animator, entry.EffectTypeName, effectPosition, duration, entry.EffectStopType, otherDirection);
			Vector3 localEulerAngles2 = gameObject2.transform.localEulerAngles;
			gameObject2.transform.localEulerAngles = Vector3.zero;
			baseEffect.transform.SetParent(gameObject2.transform, worldPositionStays: true);
			if (gameObject2.transform.localScale.x < 0f)
			{
				baseEffect.transform.SetLocalScaleX(baseEffect.transform.localScale.x * -1f);
			}
			gameObject2.transform.localEulerAngles = localEulerAngles2;
			IPreOnDisable component5 = otherObj.GetComponent<IPreOnDisable>();
			if (component5 != null)
			{
				baseEffect.AddDetachListener(component5);
			}
			break;
		}
		case EffectTargetType.Self:
		{
			IPivotObj component2 = selfObj.GetComponent<IPivotObj>();
			GameObject gameObject = ((component2 != null) ? component2.Pivot : selfObj);
			if (entry.OffsetType == EffectOffsetType.Midpoint)
			{
				gameObject = selfObj;
			}
			if (gameObject.transform.lossyScale.x < 0f)
			{
				effectPosition.x = 0f - effectPosition.x;
			}
			if (entry.OffsetType == EffectOffsetType.Midpoint)
			{
				effectPosition += selfObjMidpos;
			}
			else
			{
				effectPosition += gameObject.transform.position;
			}
			effectPosition.z = gameObject.transform.position.z;
			if (!animator)
			{
				animator = selfObj.GetComponentInChildren<Animator>();
			}
			baseEffect = EffectManager.PlayEffect(gameObject, animator, entry.EffectTypeName, effectPosition, duration, entry.EffectStopType, otherDirection);
			Vector3 localEulerAngles = gameObject.transform.localEulerAngles;
			gameObject.transform.localEulerAngles = Vector3.zero;
			baseEffect.transform.SetParent(gameObject.transform, worldPositionStays: true);
			if (gameObject.transform.localScale.x < 0f)
			{
				baseEffect.transform.SetLocalScaleX(baseEffect.transform.localScale.x * -1f);
			}
			gameObject.transform.localEulerAngles = localEulerAngles;
			IPreOnDisable component3 = selfObj.GetComponent<IPreOnDisable>();
			if (component3 != null)
			{
				baseEffect.AddDetachListener(component3);
			}
			break;
		}
		case EffectTargetType.None:
		{
			BaseCharacterController component = selfObj.GetComponent<BaseCharacterController>();
			if (((bool)component && !component.IsFacingRight) || (!component && selfObj.transform.localScale.x < 0f))
			{
				effectPosition.x = 0f - effectPosition.x;
				flag = true;
			}
			if (entry.OffsetType == EffectOffsetType.Midpoint)
			{
				effectPosition += selfObjMidpos;
			}
			else
			{
				effectPosition += selfObj.transform.position;
			}
			if (!animator)
			{
				animator = selfObj.GetComponentInChildren<Animator>();
			}
			baseEffect = EffectManager.PlayEffect(selfObj, animator, entry.EffectTypeName, effectPosition, duration, entry.EffectStopType, otherDirection);
			Vector3 vector = selfObjMidpos;
			Vector3 b = otherObjMidpos;
			if (entry.OffsetType == EffectOffsetType.Pivot)
			{
				vector = selfObj.transform.position;
				b = otherObj.transform.position;
			}
			Vector3 vector2 = Vector3.Lerp(vector, b, entry.StartingPosLerp);
			baseEffect.transform.Translate(vector2 - vector);
			break;
		}
		}
		Vector3 localScale = baseEffect.transform.localScale * num;
		localScale.z = baseEffect.transform.localScale.z;
		baseEffect.transform.localScale = localScale;
		Vector3 localEulerAngles3 = baseEffect.transform.localEulerAngles;
		switch (entry.DeriveRotation)
		{
		case EffectTargetType.Self:
		{
			if (entry.DeriveRotationFromHeading)
			{
				IHeading component8 = selfObj.GetComponent<IHeading>();
				if (!component8.IsNativeNull())
				{
					localEulerAngles3.z = CDGHelper.VectorToAngle(component8.Heading);
				}
				break;
			}
			Vector3 eulerAngles2 = selfObj.transform.eulerAngles;
			if (selfObj.transform.lossyScale.x < 0f && entry.FollowTargetType == EffectTargetType.None)
			{
				eulerAngles2.z -= 180f;
			}
			if (entry.OffsetType == EffectOffsetType.Pivot)
			{
				IPivotObj component9 = selfObj.GetComponent<IPivotObj>();
				if (component9 != null)
				{
					eulerAngles2 = component9.Pivot.transform.eulerAngles;
				}
			}
			localEulerAngles3 += eulerAngles2;
			break;
		}
		case EffectTargetType.Other:
		{
			if (entry.DeriveRotationFromHeading)
			{
				BaseCharacterController component6 = otherObj.GetComponent<BaseCharacterController>();
				localEulerAngles3.z = CDGHelper.VectorToAngle(component6.Heading);
				break;
			}
			Vector3 eulerAngles = otherObj.transform.eulerAngles;
			if (otherObj.transform.lossyScale.x < 0f && entry.FollowTargetType == EffectTargetType.None)
			{
				eulerAngles.z -= 180f;
			}
			if (entry.OffsetType == EffectOffsetType.Pivot)
			{
				IPivotObj component7 = otherObj.GetComponent<IPivotObj>();
				if (component7 != null)
				{
					eulerAngles = component7.Pivot.transform.eulerAngles;
				}
			}
			localEulerAngles3 += eulerAngles;
			break;
		}
		}
		baseEffect.transform.localEulerAngles = localEulerAngles3;
		if (entry.DeriveRotation != 0 && entry.FollowTargetType == EffectTargetType.None)
		{
			Vector3 position = baseEffect.transform.position;
			Vector3 vector3 = entry.PositionOffset * num;
			if (flag)
			{
				vector3.x *= -1f;
			}
			Vector3 vector4 = baseEffect.transform.position - vector3;
			Vector3 position2 = CDGHelper.RotatedPoint(position, vector4, localEulerAngles3.z);
			position2.z = position.z;
			baseEffect.transform.position = position2;
		}
		float num2 = 0f;
		bool flag2 = false;
		if ((otherDirection & EffectTriggerDirection.MovingLeft) != 0)
		{
			num2 += entry.LeftRotationZ;
			flag2 = entry.LeftFlip;
		}
		else if ((otherDirection & EffectTriggerDirection.MovingRight) != 0)
		{
			num2 += entry.RightRotationZ;
			flag2 = entry.RightFlip;
		}
		if ((otherDirection & EffectTriggerDirection.MovingDown) != 0)
		{
			num2 += entry.DownRotationZ;
			if (!flag2)
			{
				flag2 = entry.DownFlip;
			}
		}
		else if ((otherDirection & EffectTriggerDirection.MovingUp) != 0)
		{
			num2 += entry.UpRotationZ;
			if (!flag2)
			{
				flag2 = entry.UpFlip;
			}
		}
		if (flag2)
		{
			baseEffect.transform.localScale = Vector3.Scale(baseEffect.transform.localScale, new Vector3(-1f, 1f, 1f));
		}
		baseEffect.transform.localEulerAngles += new Vector3(0f, 0f, num2);
		baseEffect.AnimatorLayer = entry.AnimatorLayer;
		baseEffect.DisableDestroyOnRoomChange = !entry.DestroyOnRoomChange;
		return baseEffect;
	}

	public static EffectTriggerDirection GetEffectDirectionFromObject(GameObject obj, EffectTriggerDirection directionsToCheck)
	{
		EffectTriggerDirection effectTriggerDirection = EffectTriggerDirection.None;
		Vector2 vector = Vector2.zero;
		IEffectVelocity component = obj.GetComponent<IEffectVelocity>();
		if (component != null)
		{
			vector = component.EffectVelocity;
		}
		if (vector.x < 0f && (directionsToCheck & EffectTriggerDirection.MovingLeft) != 0)
		{
			effectTriggerDirection |= EffectTriggerDirection.MovingLeft;
		}
		else if (vector.x > 0f && (directionsToCheck & EffectTriggerDirection.MovingRight) != 0)
		{
			effectTriggerDirection |= EffectTriggerDirection.MovingRight;
		}
		if (vector.y < 0f && (directionsToCheck & EffectTriggerDirection.MovingDown) != 0)
		{
			effectTriggerDirection |= EffectTriggerDirection.MovingDown;
		}
		else if (vector.y > 0f && (directionsToCheck & EffectTriggerDirection.MovingUp) != 0)
		{
			effectTriggerDirection |= EffectTriggerDirection.MovingUp;
		}
		if (vector == Vector2.zero)
		{
			bool flag = obj.transform.localScale.x > 0f;
			BaseCharacterController component2 = obj.GetComponent<BaseCharacterController>();
			if ((bool)component2)
			{
				flag = component2.IsFacingRight;
			}
			effectTriggerDirection = ((!flag) ? (effectTriggerDirection | EffectTriggerDirection.MovingLeft) : (effectTriggerDirection | EffectTriggerDirection.MovingRight));
		}
		return effectTriggerDirection;
	}
}
