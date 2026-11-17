namespace MoreMountains.CorgiEngine;

public class CharacterStates
{
	public enum CharacterConditions
	{
		Normal,
		ControlledMovement,
		Frozen,
		Paused,
		Stunned,
		DisableHorizontalMovement,
		Dead
	}

	public enum MovementStates
	{
		Null,
		Idle,
		Walking,
		Falling,
		Running,
		Dashing,
		DownStriking,
		Jumping,
		Pushing,
		DoubleJumping,
		WallJumping,
		Flying
	}
}
