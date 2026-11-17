using System.Collections;
using RL_Windows;
using UnityEngine;

public class GardenHubTownController : BaseSpecialRoomController
{
	[SerializeField]
	private GameObject m_castleParentObj;

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		StartCoroutine(FlipPlayer());
		if (!WindowManager.GetIsWindowLoaded(WindowID.SkillTree))
		{
			WindowManager.LoadWindow(WindowID.SkillTree);
		}
		WindowController windowController = WindowManager.GetWindowController(WindowID.SkillTree);
		if ((bool)windowController)
		{
			SkillTreeWindowController obj = windowController as SkillTreeWindowController;
			GameObject obj2 = obj.SkillTreeCastleParentObj.transform.GetChild(0).gameObject;
			obj2.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			obj2.transform.SetParent(m_castleParentObj.transform, worldPositionStays: false);
			obj2.SetLayerRecursively(26);
			EffectManager.AddAnimatorToDisableList(obj.CastleAnimator);
			obj.ForceUpdateSkillTreeAnimatorParams();
			obj.CastleAnimator.SetBool("StartingStairs", value: true);
			obj.CastleAnimator.SetBool("Entrance_1", value: true);
			obj.CastleAnimator.Update(1f);
			EffectManager.RemoveAnimatorFromDisableList(obj.CastleAnimator);
		}
	}

	protected override void OnPlayerExitRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerExitRoom(sender, eventArgs);
		if (m_castleParentObj.transform.childCount > 0)
		{
			WindowController windowController = WindowManager.GetWindowController(WindowID.SkillTree);
			GameObject obj = m_castleParentObj.transform.GetChild(0).gameObject;
			SkillTreeWindowController skillTreeWindowController = windowController as SkillTreeWindowController;
			obj.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			obj.transform.SetParent(skillTreeWindowController.SkillTreeCastleParentObj.transform, worldPositionStays: false);
			obj.SetLayerRecursively(5);
		}
	}

	private IEnumerator FlipPlayer()
	{
		yield return null;
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (playerController.IsFacingRight)
		{
			playerController.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: true, ignoreTurnParam: true);
		}
	}
}
