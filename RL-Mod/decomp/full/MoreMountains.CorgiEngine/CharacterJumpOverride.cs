using UnityEngine;

namespace MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Environment/Character Jump Override")]
public class CharacterJumpOverride : MonoBehaviour
{
	[Header("Jumps")]
	public float JumpHeight = 3.025f;

	public float JumpMinimumAirTime = 0.1f;

	public int NumberOfJumps = 3;

	public CharacterJump.JumpBehavior JumpRestrictions;

	public bool ResetNumberOfJumpsLeft = true;

	protected float _previousJumpHeight;

	protected float _previousJumpMinimumAirTime;

	protected int _previousNumberOfJumps;

	protected int _previousNumberOfJumpsLeft;

	protected CharacterJump.JumpBehavior _previousJumpRestrictions;

	protected virtual void OnTriggerEnter2D(Collider2D collider)
	{
		CharacterJump component = collider.GetComponent<CharacterJump>();
		if (!(component == null))
		{
			_previousJumpHeight = component.JumpHeight;
			_previousJumpMinimumAirTime = component.JumpMinimumAirTime;
			_previousNumberOfJumps = component.NumberOfJumps;
			_previousJumpRestrictions = component.JumpRestrictions;
			_previousNumberOfJumpsLeft = component.NumberOfJumpsLeft;
			component.JumpHeight = JumpHeight;
			component.JumpMinimumAirTime = JumpMinimumAirTime;
			component.NumberOfJumps = NumberOfJumps;
			component.JumpRestrictions = JumpRestrictions;
			if (ResetNumberOfJumpsLeft)
			{
				component.SetNumberOfJumpsLeft(NumberOfJumps);
			}
		}
	}

	protected virtual void OnTriggerExit2D(Collider2D collider)
	{
		CharacterJump component = collider.GetComponent<CharacterJump>();
		if (!(component == null))
		{
			component.JumpHeight = _previousJumpHeight;
			component.JumpMinimumAirTime = _previousJumpMinimumAirTime;
			component.NumberOfJumps = _previousNumberOfJumps;
			component.JumpRestrictions = _previousJumpRestrictions;
			if (ResetNumberOfJumpsLeft)
			{
				component.SetNumberOfJumpsLeft(component.NumberOfJumps);
			}
			else
			{
				component.SetNumberOfJumpsLeft(_previousNumberOfJumpsLeft);
			}
		}
	}
}
