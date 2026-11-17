namespace MoreMountains.CorgiEngine;

public class CorgiControllerState
{
	public float DistanceToLeftCollider;

	public float DistanceToRightCollider;

	public bool IsCollidingRight { get; set; }

	public bool IsCollidingLeft { get; set; }

	public bool IsCollidingAbove { get; set; }

	public bool IsCollidingBelow { get; set; }

	public bool HasCollisions
	{
		get
		{
			if (!IsCollidingRight && !IsCollidingLeft && !IsCollidingAbove)
			{
				return IsCollidingBelow;
			}
			return true;
		}
	}

	public float LateralSlopeAngle { get; set; }

	public float BelowSlopeAngle { get; set; }

	public bool SlopeAngleOK { get; set; }

	public bool OnAMovingPlatform { get; set; }

	public bool IsGrounded
	{
		get
		{
			if (IsCollidingBelow)
			{
				return !JustStartedJump;
			}
			return false;
		}
	}

	public bool IsFalling { get; set; }

	public bool WasGroundedLastFrame { get; set; }

	public bool WasTouchingTheCeilingLastFrame { get; set; }

	public bool JustGotGrounded { get; set; }

	public bool ColliderResized { get; set; }

	public bool JustStartedJump { get; set; }

	public virtual void Reset()
	{
		IsCollidingLeft = false;
		IsCollidingRight = false;
		IsCollidingAbove = false;
		DistanceToLeftCollider = -1f;
		DistanceToRightCollider = -1f;
		SlopeAngleOK = false;
		JustGotGrounded = false;
		IsFalling = true;
		LateralSlopeAngle = 0f;
		JustStartedJump = false;
	}

	public override string ToString()
	{
		return string.Format("(controller: r:{0} l:{1} a:{2} b:{3} down-slope:{4} up-slope:{5} angle: {6}", IsCollidingRight, IsCollidingLeft, IsCollidingAbove, IsCollidingBelow, LateralSlopeAngle, JustStartedJump);
	}
}
