using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class PlayerMovementHelper
{
	public static void StopAllMovementInput()
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.ConditionState = CharacterStates.CharacterConditions.ControlledMovement;
			playerController.SetVelocity(0f, 0f, additive: false);
			playerController.CharacterMove.ResetHorizontalMovement();
		}
	}

	public static void ResumeAllMovementInput()
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController().ConditionState = CharacterStates.CharacterConditions.Normal;
		}
	}

	private static IEnumerator MovePlayerHorizontal(float speed, float duration)
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.DisableAbilitiesForXSeconds(0f);
			float totalDuration = Time.time + duration;
			while (Time.time <= totalDuration)
			{
				playerController.CharacterMove.SetHorizontalMove(speed);
				yield return null;
			}
			playerController.CharacterMove.SetHorizontalMove(0f);
		}
	}

	public static IEnumerator MovePlayerLeft(float duration)
	{
		yield return MovePlayerHorizontal(-1f, duration);
	}

	public static IEnumerator MovePlayerRight(float duration)
	{
		yield return MovePlayerHorizontal(1f, duration);
	}

	public static IEnumerator MoveTo(Vector2 position, bool checkXAxisOnly)
	{
		if (!PlayerManager.IsInstantiated)
		{
			yield break;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.DisableAbilitiesForXSeconds(0f);
		Vector2 startingDirectionVector = position - (Vector2)playerController.transform.localPosition;
		bool moveCharacter = true;
		while (moveCharacter)
		{
			Vector2 vector = position - (Vector2)playerController.transform.localPosition;
			vector.x = Mathf.Clamp(vector.x, -1f, 1f);
			vector.y = Mathf.Clamp(vector.y, -1f, 1f);
			if (Mathf.Abs(vector.x) >= 0.1f)
			{
				yield return MovePlayerHorizontal(vector.x, 0f);
			}
			else
			{
				moveCharacter = false;
			}
			Vector2 vector2 = position - (Vector2)playerController.transform.localPosition;
			if ((checkXAxisOnly && startingDirectionVector.x == 0f) || (!checkXAxisOnly && startingDirectionVector == Vector2.zero) || (vector2.x > 0f && startingDirectionVector.x < 0f) || (vector2.x < 0f && startingDirectionVector.x > 0f) || (!checkXAxisOnly && vector2.y > 0f && startingDirectionVector.y < 0f) || (!checkXAxisOnly && vector2.y < 0f && startingDirectionVector.y > 0f))
			{
				moveCharacter = false;
			}
		}
		if (checkXAxisOnly)
		{
			playerController.transform.position = new Vector3(position.x, playerController.transform.position.y, playerController.transform.position.z);
		}
		else
		{
			playerController.transform.position = new Vector3(position.x, position.y, playerController.transform.position.z);
		}
	}

	public static IEnumerator MoveTo_Old(Vector2 position, bool xAxisOnly)
	{
		if (!PlayerManager.IsInstantiated)
		{
			yield break;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.DisableAbilitiesForXSeconds(0f);
		float num = CDGHelper.DistanceBetweenPts(position, playerController.transform.position);
		if (xAxisOnly)
		{
			num = Mathf.Abs(playerController.transform.position.x - position.x);
		}
		while (num > 0.1f)
		{
			if (playerController.transform.position.x < position.x)
			{
				yield return MovePlayerRight(0f);
			}
			else
			{
				yield return MovePlayerLeft(0f);
			}
			num = (xAxisOnly ? Mathf.Abs(playerController.transform.position.x - position.x) : CDGHelper.DistanceBetweenPts(position, playerController.transform.position));
		}
	}

	public static IEnumerator JumpPlayer(float duration)
	{
		if (PlayerManager.IsInstantiated)
		{
			float timeStart = Time.time;
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.DisableAbilitiesForXSeconds(0f);
			playerController.CharacterJump.JumpStart();
			while (Time.time < timeStart + duration)
			{
				yield return null;
			}
			playerController.CharacterJump.JumpStop();
		}
	}
}
